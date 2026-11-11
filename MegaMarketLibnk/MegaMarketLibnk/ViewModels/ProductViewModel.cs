using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using MegaMarketLibnk.Models;
using MegaMarketLibnk.Services;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class CharacteristicItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
public class ReviewItem
{
    public string Author { get; set; } = string.Empty;
    public string Stars { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}
public class GalleryItem
{
    public string Color { get; set; } = string.Empty;
    public int Index { get; set; }
}
public class ProductViewModel : ViewModelBase
{
    private string status = string.Empty;
    public string Status
    {
        get => status;
        private set => this.RaiseAndSetIfChanged(ref status, value);
    }
    public string Title { get; private set; }
    public string PriceText { get; private set; }
    public string CategoryName { get; private set; }
    public string SellerName { get; private set; }
    public string Description { get; private set; }
    private decimal weight = 1m;
    private decimal volume = 1m;
    private int selectedTab;
    public int SelectedTab
    {
        get => selectedTab;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedTab, value);
            this.RaisePropertyChanged(nameof(IsDescriptionTab));
            this.RaisePropertyChanged(nameof(IsCharacteristicsTab));
            this.RaisePropertyChanged(nameof(IsReviewsTab));
        }
    }
    public bool IsDescriptionTab => SelectedTab == 0;
    public bool IsCharacteristicsTab => SelectedTab == 1;
    public bool IsReviewsTab => SelectedTab == 2;
    public ObservableCollection<CharacteristicItem> Characteristics { get; } = new();
    public ObservableCollection<ReviewItem> Reviews { get; } = new();
    public bool HasCharacteristics => Characteristics.Count > 0;
    public bool HasReviews => Reviews.Count > 0;
    private string ratingText = string.Empty;
    public string RatingText
    {
        get => ratingText;
        private set
        {
            this.RaiseAndSetIfChanged(ref ratingText, value);
            this.RaisePropertyChanged(nameof(HasRating));
        }
    }
    public bool HasRating => ratingText.Length > 0;
    private string reviewsTabText = "Отзывы";
    public string ReviewsTabText
    {
        get => reviewsTabText;
        private set => this.RaiseAndSetIfChanged(ref reviewsTabText, value);
    }
    public ObservableCollection<GalleryItem> Gallery { get; } = new();
    private int currentImage;
    public int CurrentImage
    {
        get => currentImage;
        set
        {
            if (Gallery.Count == 0)
                return;
            var count = Gallery.Count;
            var next = ((value % count) + count) % count;
            this.RaiseAndSetIfChanged(ref currentImage, next);
            this.RaisePropertyChanged(nameof(CurrentColor));
            this.RaisePropertyChanged(nameof(ImageCounterText));
        }
    }
    public string CurrentColor => Gallery.Count > 0 ? Gallery[CurrentImage].Color : "#2E2E36";
    public string ImageCounterText => $"{CurrentImage + 1} / {Gallery.Count}";
    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> AddToCartCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenCartCommand { get; }
    public ReactiveCommand<string, Unit> SelectTabCommand { get; }
    public ReactiveCommand<Unit, Unit> NextImageCommand { get; }
    public ReactiveCommand<Unit, Unit> PrevImageCommand { get; }
    public ReactiveCommand<int, Unit> SelectImageCommand { get; }
    public ProductViewModel(FeedTile tile, HomeViewModel home)
    {
        BackCommand = ReactiveCommand.Create(() =>
        {
            MainWindowViewModel.Instance.CurrentModel = home;
            home.RefreshCart();
        });
        OpenCartCommand = ReactiveCommand.Create(() =>
        {
            MainWindowViewModel.Instance.CurrentModel = new CartViewModel(home);
        });
        AddToCartCommand = ReactiveCommand.Create(() =>
        {
            CartService.Add(new CartItem
            {
                Productid = tile.Productid,
                Title = Title,
                Price = tile.Price,
                SellerName = SellerName,
                Color = tile.Color,
                Weight = weight,
                Volume = volume,
            });
            Status = $"✓ Товар добавлен в корзину · {CartService.Count} шт.";
        });
        SelectTabCommand = ReactiveCommand.Create<string>(tab =>
        {
            SelectedTab = int.Parse(tab);
        });
        NextImageCommand = ReactiveCommand.Create(() =>
        {
            CurrentImage++;
        });
        PrevImageCommand = ReactiveCommand.Create(() =>
        {
            CurrentImage--;
        });
        SelectImageCommand = ReactiveCommand.Create<int>(index =>
        {
            CurrentImage = index;
        });
        Title = tile.Title;
        PriceText = tile.PriceText;
        CategoryName = home.SelectedCategory?.Name ?? "Все товары";
        SellerName = "Продавец MegaMarket";
        Description = "Описание пока не добавлено.";
        BuildGallery(tile.Color);
        if (!tile.FromDb)
            return;
        try
        {
            var product = SF.Context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Productcharacteristics)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Customer)
                .FirstOrDefault(p => p.Id == tile.Productid);
            if (product == null)
                return;
            Title = product.Name;
            if (tile is not SaleTile)
                PriceText = FeedTile.Money(product.Baseprice);
            Description = product.Description ?? Description;
            CategoryName = product.Category?.Name ?? CategoryName;
            SellerName = product.Seller?.Companyname ?? SellerName;
            weight = ReadMeasure(product, "Вес", "кг", "г", 1m);
            volume = ReadMeasure(product, "Объём", "л", "мл", 1m);
            foreach (var characteristic in product.Productcharacteristics.OrderBy(c => c.Id))
            {
                Characteristics.Add(new CharacteristicItem
                {
                    Key = characteristic.Keyname,
                    Value = characteristic.Value ?? "—",
                });
            }
            foreach (var review in product.Reviews.OrderByDescending(r => r.Createdat))
            {
                var rating = Math.Clamp((int)review.Rating, 0, 5);
                Reviews.Add(new ReviewItem
                {
                    Author = review.Customer?.Firstname ?? "Покупатель",
                    Stars = new string('★', rating) + new string('☆', 5 - rating),
                    Text = review.Textreview ?? string.Empty,
                    Date = review.Createdat.ToString("dd.MM.yyyy"),
                });
            }
            if (Reviews.Count > 0)
            {
                var average = product.Reviews.Average(r => r.Rating);
                RatingText = $"★ {average.ToString("0.0", CultureInfo.GetCultureInfo("ru-RU"))} · {Plural(Reviews.Count)}";
                ReviewsTabText = $"Отзывы ({Reviews.Count})";
            }
            this.RaisePropertyChanged(nameof(HasCharacteristics));
            this.RaisePropertyChanged(nameof(HasReviews));
        }
        catch
        {
            Status = "✗ Нет соединения с базой данных";
        }
    }
    private void BuildGallery(string baseColor)
    {
        var factors = new[] { 1.0, 1.45, 0.6, 1.2 };
        for (var i = 0; i < factors.Length; i++)
        {
            Gallery.Add(new GalleryItem
            {
                Color = i == factors.Length - 1 ? Tint(baseColor) : Shade(baseColor, factors[i]),
                Index = i,
            });
        }
    }
    private static string Shade(string hex, double factor)
    {
        var r = Parse(hex, 1);
        var g = Parse(hex, 3);
        var b = Parse(hex, 5);
        return $"#{Clamp(r * factor):X2}{Clamp(g * factor):X2}{Clamp(b * factor):X2}";
    }
    private static string Tint(string hex)
    {
        const string accent = "#5B4FE0";
        var r = (Parse(hex, 1) + Parse(accent, 1)) / 2;
        var g = (Parse(hex, 3) + Parse(accent, 3)) / 2;
        var b = (Parse(hex, 5) + Parse(accent, 5)) / 2;
        return $"#{Clamp(r):X2}{Clamp(g):X2}{Clamp(b):X2}";
    }
    private static int Parse(string hex, int start) =>
        hex.Length >= start + 2 ? Convert.ToInt32(hex.Substring(start, 2), 16) : 46;
    private static int Clamp(double value) => (int)Math.Clamp(value, 0, 255);
    private static string Plural(int count)
    {
        var lastTwo = count % 100;
        var last = count % 10;
        if (lastTwo is >= 11 and <= 14)
            return $"{count} отзывов";
        return last switch
        {
            1 => $"{count} отзыв",
            2 or 3 or 4 => $"{count} отзыва",
            _ => $"{count} отзывов",
        };
    }
    private static decimal ReadMeasure(Product product, string key, string bigUnit, string smallUnit, decimal fallback)
    {
        var text = product.Productcharacteristics
            .FirstOrDefault(c => c.Keyname == key && c.Value != null)?.Value;
        if (!string.IsNullOrWhiteSpace(text))
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2
                && decimal.TryParse(parts[0].Replace(',', '.'), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var value))
            {
                return parts[1].StartsWith(smallUnit, StringComparison.Ordinal)
                    ? value / 1000m
                    : value;
            }
        }
        if (product.Id > 0)
            return fallback + product.Id % 5 * 0.5m;
        return fallback;
    }
}
