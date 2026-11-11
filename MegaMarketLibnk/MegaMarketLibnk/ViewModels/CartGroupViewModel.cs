using System.Collections.ObjectModel;
using MegaMarketLibnk.Models;
using MegaMarketLibnk.Services;
namespace MegaMarketLibnk.ViewModels;
public class CartGroupViewModel
{
    public CartGroupViewModel(string sellerName, IReadOnlyCollection<CartItem> items, Promo? promo)
    {
        SellerName = sellerName;
        Items = new ObservableCollection<CartItem>(items);
        Weight = items.Sum(i => i.Weight * i.Quantity);
        Volume = items.Sum(i => i.Volume * i.Quantity);
        Subtotal = items.Sum(i => i.Price * i.Quantity);
        var pieces = items.Sum(i => i.Quantity);
        ItemsSummary =
            $"{items.Count} {CartService.Plural(items.Count, "позиция", "позиции", "позиций")} · {pieces} шт.";
        WeightText = $"{CartService.Num(Weight)} кг";
        VolumeText = $"{CartService.Num(Volume)} дм³";
        BaseLine = $"Базовая ставка: {FeedTile.Money(CartService.BaseDelivery)} ₽";
        WeightLine =
            $"Вес: {CartService.Num(Weight)} кг × {CartService.Num(CartService.PerKg)} ₽/кг "
            + $"= {FeedTile.Money(Weight * CartService.PerKg)} ₽";
        VolumeLine =
            $"Объём: {CartService.Num(Volume)} дм³ × {CartService.Num(CartService.PerVolume)} ₽/дм³ "
            + $"= {FeedTile.Money(Volume * CartService.PerVolume)} ₽";
        var delivery = CartService.BaseDelivery + Weight * CartService.PerKg + Volume * CartService.PerVolume;
        IsFreeDelivery = promo?.FreeDelivery == true;
        DeliveryCost = IsFreeDelivery ? 0 : delivery;
        DeliveryText = IsFreeDelivery
            ? $"0 ₽ — бесплатно по промокоду {promo!.Code}"
            : $"{FeedTile.Money(DeliveryCost)} ₽";
        SubtotalText = FeedTile.Money(Subtotal) + " ₽";
        TotalText = FeedTile.Money(Subtotal + DeliveryCost) + " ₽";
    }
    public string SellerName { get; }
    public ObservableCollection<CartItem> Items { get; }
    public string ItemsSummary { get; }
    public decimal Weight { get; }
    public decimal Volume { get; }
    public decimal Subtotal { get; }
    public decimal DeliveryCost { get; }
    public bool IsFreeDelivery { get; }
    public string WeightText { get; }
    public string VolumeText { get; }
    public string BaseLine { get; }
    public string WeightLine { get; }
    public string VolumeLine { get; }
    public string DeliveryText { get; }
    public string SubtotalText { get; }
    public string TotalText { get; }
}
