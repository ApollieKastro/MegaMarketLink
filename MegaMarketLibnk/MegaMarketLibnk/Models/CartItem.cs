using System.Globalization;
using System.Reactive;
using MegaMarketLibnk.Services;
using MegaMarketLibnk.ViewModels;
using ReactiveUI;
namespace MegaMarketLibnk.Models;
public class CartItem : ViewModelBase
{
    public int Productid { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public string Color { get; init; } = "#2E2E36";
    public decimal Weight { get; init; }
    public decimal Volume { get; init; }
    private int quantity = 1;
    public int Quantity
    {
        get => quantity;
        set
        {
            this.RaiseAndSetIfChanged(ref quantity, value);
            this.RaisePropertyChanged(nameof(LineTotalText));
        }
    }
    public string PriceText => FeedTile.Money(Price) + " ₽";
    public string LineTotalText => FeedTile.Money(Price * Quantity) + " ₽";
    public string UnitParamsText =>
        $"{CartService.Num(Weight)} кг · {CartService.Num(Volume)} дм³ за шт.";
    public ReactiveCommand<Unit, Unit> IncreaseCommand { get; }
    public ReactiveCommand<Unit, Unit> DecreaseCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
    public CartItem()
    {
        IncreaseCommand = ReactiveCommand.Create(() => CartService.ChangeQuantity(this, +1));
        DecreaseCommand = ReactiveCommand.Create(() => CartService.ChangeQuantity(this, -1));
        RemoveCommand = ReactiveCommand.Create(() => CartService.Remove(this));
    }
}
