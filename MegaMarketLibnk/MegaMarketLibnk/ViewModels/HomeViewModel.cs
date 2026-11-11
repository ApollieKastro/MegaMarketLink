using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using MegaMarketLibnk.Models;
using MegaMarketLibnk.Services;
using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class HomeViewModel : ViewModelBase
{
    private const int PageSize = 12;
    private const string BrandKey = "Бренд";
    private static readonly string[] TileColors =
        { "#2E3A46", "#3A2E46", "#2E4638", "#463A2E", "#462E3A", "#2E4644" };
    private bool loading;
    public bool IsLoading
    {
        get => loading;
        private set
        {
            this.RaiseAndSetIfChanged(ref loading, value);
            RaiseFeedState();
        }
    }
    private bool hasMore = true;
    public bool HasMore => hasMore;
    public bool ShowMoreButton => hasMore && !loading;
    public bool EndOfList => !hasMore && !loading && Items.Count > 0;
    private int index;
    private int version;
    private Category selectedCategory = null!;
    public Category SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (value == null || ReferenceEquals(selectedCategory, value))
                return;
            this.RaiseAndSetIfChanged(ref selectedCategory, value);
            Reset();
        }
    }
    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set
        {
            if (searchText == value)
                return;
            this.RaiseAndSetIfChanged(ref searchText, value);
            Reset();
        }
    }
    private string priceMin = string.Empty;
    public string PriceMin
    {
        get => priceMin;
        set
        {
            if (priceMin == value)
                return;
            this.RaiseAndSetIfChanged(ref priceMin, value);
            Reset();
        }
    }
    private string priceMax = string.Empty;
    public string PriceMax
    {
        get => priceMax;
        set
        {
            if (priceMax == value)
                return;
            this.RaiseAndSetIfChanged(ref priceMax, value);
            Reset();
        }
    }
    private string status = string.Empty;
    public string Status
    {
        get => status;
        private set
        {
            this.RaiseAndSetIfChanged(ref status, value);
            this.RaisePropertyChanged(nameof(HasStatus));
        }
    }
    public bool HasStatus => !string.IsNullOrEmpty(status);
    public ObservableCollection<FeedTile> Items { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<FilterOption> BrandFilters { get; } = new();
    public ObservableCollection<FilterOption> ParamFilters { get; } = new();
    public string UserName => SF.User?.Email ?? "Гость";
    public string CartCountText => CartService.Count > 0
        ? $"Корзина ({CartService.Count})"
        : "Корзина";
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public ReactiveCommand<FeedTile, Unit> OpenProductCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetFiltersCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenCartCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenCabinetCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenSellerCabinetCommand { get; }
    public bool IsSeller => SF.User?.Role?.Rolename?.ToLowerInvariant() == "seller" || SF.User?.Role?.Rolename?.ToLowerInvariant() == "продавец";
    public HomeViewModel()
    {
        LoadMoreCommand = ReactiveCommand.Create(LoadMore);
        OpenProductCommand = ReactiveCommand.Create<FeedTile>(OpenProduct);
        ResetFiltersCommand = ReactiveCommand.Create(ResetFilters);
        OpenCartCommand = ReactiveCommand.Create(OpenCart);
        OpenCabinetCommand = ReactiveCommand.Create(OpenCabinet);
        OpenSellerCabinetCommand = ReactiveCommand.Create(OpenSellerCabinet);
        LoadCategories();
        LoadFilters();
        LoadMore();
    }
    public void LoadMore()
    {
        if (loading || !hasMore)
            return;
        _ = LoadMoreAsync();
    }
    private async Task LoadMoreAsync()
    {
        var current = version;
        IsLoading = true;
        try
        {
            await Task.Delay(400);
            var page = FetchPage(index);
            if (current != version)
                return;
            index += page.Count;
            foreach (var tile in page)
                Items.Add(tile);
            hasMore = page.Count == PageSize;
            if (Items.Count == 0)
                Status = "Ничего не найдено";
            RaiseFeedState();
        }
        catch
        {
            hasMore = false;
            Status = "✗ Нет соединения с базой данных";
            RaiseFeedState();
        }
        finally
        {
            IsLoading = false;
            RaiseFeedState();
            if (current != version)
                LoadMore();
        }
    }
    private void Reset()
    {
        version++;
        Items.Clear();
        index = 0;
        hasMore = true;
        Status = string.Empty;
        RaiseFeedState();
        LoadMore();
    }
    private void LoadCategories()
    {
        try
        {
            var categories = SF.Context.Categories
                .Where(c => c.Isactive)
                .OrderBy(c => c.Name)
                .ToList();
            categories.Insert(0, new Category { Id = 0, Name = "Все товары" });
            foreach (var category in categories)
                Categories.Add(category);
            selectedCategory = Categories[0];
        }
        catch
        {
            Status = "✗ Нет соединения с базой данных";
        }
    }
    private void LoadFilters()
    {
        try
        {
            var characteristics = SF.Context.Productcharacteristics.ToList();
            foreach (var brand in characteristics
                .Where(c => c.Keyname == BrandKey && c.Value != null)
                .Select(c => c.Value!)
                .Distinct()
                .OrderBy(v => v))
                BrandFilters.Add(new FilterOption(brand, BrandKey, brand, Reset));
            foreach (var item in characteristics
                .Where(c => c.Keyname != BrandKey && c.Value != null)
                .GroupBy(c => new { c.Keyname, c.Value })
                .OrderBy(g => g.Key.Keyname)
                .ThenBy(g => g.Key.Value))
                ParamFilters.Add(new FilterOption(
                    $"{item.Key.Keyname}: {item.Key.Value}",
                    item.Key.Keyname,
                    item.Key.Value!,
                    Reset));
        }
        catch
        {
            Status = "✗ Нет соединения с базой данных";
        }
    }
    private void ResetFilters()
    {
        foreach (var option in BrandFilters)
            option.UncheckSilently();
        foreach (var option in ParamFilters)
            option.UncheckSilently();
        this.RaiseAndSetIfChanged(ref priceMin, string.Empty, nameof(PriceMin));
        this.RaiseAndSetIfChanged(ref priceMax, string.Empty, nameof(PriceMax));
        Reset();
    }
    private List<FeedTile> FetchPage(int skip)
    {
        var query = SF.Context.Products.AsQueryable();
        if (selectedCategory is { Id: not 0 })
            query = query.Where(p => p.Categoryid == selectedCategory.Id);
        var text = searchText.Trim();
        if (text.Length > 0)
            query = query.Where(p => p.Name.Contains(text));
        if (decimal.TryParse(priceMin, NumberStyles.Number, CultureInfo.CurrentCulture, out var minPrice))
            query = query.Where(p => p.Baseprice * (100 - p.Currentdiscount) / 100 >= minPrice);
        if (decimal.TryParse(priceMax, NumberStyles.Number, CultureInfo.CurrentCulture, out var maxPrice))
            query = query.Where(p => p.Baseprice * (100 - p.Currentdiscount) / 100 <= maxPrice);
        var brands = BrandFilters.Where(f => f.IsChecked).Select(f => f.Value).ToList();
        if (brands.Count > 0)
            query = query.Where(p => p.Productcharacteristics
                .Any(c => c.Keyname == BrandKey && c.Value != null && brands.Contains(c.Value)));
        foreach (var group in ParamFilters.Where(f => f.IsChecked).GroupBy(f => f.Key))
        {
            var key = group.Key;
            var values = group.Select(f => f.Value).ToList();
            query = query.Where(p => p.Productcharacteristics
                .Any(c => c.Keyname == key && c.Value != null && values.Contains(c.Value)));
        }
        var products = query.OrderBy(p => p.Id).Skip(skip).Take(PageSize).ToList();
        var tiles = new List<FeedTile>();
        for (var i = 0; i < products.Count; i++)
            tiles.Add(NewTile(index + i, products[i]));
        return tiles;
    }
    private FeedTile NewTile(int position, Product product)
    {
        var color = TileColors[position % TileColors.Length];
        if (product.Currentdiscount > 0 && product.Currentdiscount < 100)
        {
            var salePrice = Math.Round(product.Baseprice * (100 - product.Currentdiscount) / 100, 2);
            return new SaleTile
            {
                Productid = product.Id,
                FromDb = true,
                Title = product.Name,
                Price = salePrice,
                OldPrice = product.Baseprice,
                Color = color,
            };
        }
        return new ProductTile
        {
            Productid = product.Id,
            FromDb = true,
            Title = product.Name,
            Price = product.Baseprice,
            Color = color,
        };
    }
    private void OpenProduct(FeedTile? tile)
    {
        if (tile == null)
            return;
        MainWindowViewModel.Instance.CurrentModel = new ProductViewModel(tile, this);
    }
    private void OpenCart()
    {
        MainWindowViewModel.Instance.CurrentModel = new CartViewModel(this);
    }
    private void OpenCabinet()
    {
        MainWindowViewModel.Instance.CurrentModel = new BuyerCabinetViewModel();
    }
    private void OpenSellerCabinet()
    {
        MainWindowViewModel.Instance.CurrentModel = new SellerCabinetViewModel();
    }
    public void RefreshCart()
    {
        this.RaisePropertyChanged(nameof(CartCountText));
    }
    private void RaiseFeedState()
    {
        this.RaisePropertyChanged(nameof(HasMore));
        this.RaisePropertyChanged(nameof(ShowMoreButton));
        this.RaisePropertyChanged(nameof(EndOfList));
    }
}
