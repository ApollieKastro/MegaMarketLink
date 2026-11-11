using System.Collections.ObjectModel;
using System.Globalization;
using MegaMarketLibnk.Models;
namespace MegaMarketLibnk.Services;
public sealed record Promo(
    string Code,
    string Description,
    decimal Percent,
    decimal Amount,
    bool FreeDelivery);
public static class CartService
{
    public const decimal BaseDelivery = 300m;
    public const decimal PerKg = 40m;
    public const decimal PerVolume = 20m;
    private static readonly Dictionary<string, Promo> Promos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["СКИДКА10"] = new("СКИДКА10", "скидка 10% на товары", 10m, 0m, false),
        ["500"] = new("500", "скидка 500 ₽ на товары", 0m, 500m, false),
        ["ФРИШИП"] = new("ФРИШИП", "бесплатная доставка", 0m, 0m, true),
    };
    public static ObservableCollection<CartItem> Items { get; } = new();
    public static Promo? AppliedPromo { get; private set; }
    public static event Action? Changed;
    public static int Count => Items.Sum(i => i.Quantity);
    public static CartItem Add(CartItem item)
    {
        var existing = Items.FirstOrDefault(i => i.Productid == item.Productid);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
            Changed?.Invoke();
            return existing;
        }
        Items.Add(item);
        Changed?.Invoke();
        return item;
    }
    public static void Remove(CartItem item)
    {
        Items.Remove(item);
        Changed?.Invoke();
    }
    public static void ChangeQuantity(CartItem item, int delta)
    {
        item.Quantity = Math.Max(1, item.Quantity + delta);
        Changed?.Invoke();
    }
    public static void Clear()
    {
        Items.Clear();
        AppliedPromo = null;
        Changed?.Invoke();
    }
    public static void ClearPromo()
    {
        if (AppliedPromo == null)
            return;
        AppliedPromo = null;
        Changed?.Invoke();
    }
    public static decimal DiscountFor(Promo? promo, decimal subtotal)
    {
        if (promo == null || subtotal <= 0)
            return 0;
        if (promo.Percent > 0)
            return Math.Round(subtotal * promo.Percent / 100, 2);
        if (promo.Amount > 0)
            return Math.Min(promo.Amount, subtotal);
        return 0;
    }
    public static (bool Ok, string Message) ApplyPromo(string? code)
    {
        var trimmed = code?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return (false, "✗ Введите промокод");
        if (!Promos.TryGetValue(trimmed, out var promo))
            return (false, $"✗ Промокод «{trimmed}» не найден");
        AppliedPromo = promo;
        Changed?.Invoke();
        return (true, $"✓ Промокод применён: {promo.Description}");
    }
    public static string Num(decimal value) =>
        value.ToString("0.##", CultureInfo.GetCultureInfo("ru-RU"));
    public static string Plural(int count, string one, string few, string many)
    {
        var lastTwo = count % 100;
        var last = count % 10;
        if (lastTwo is >= 11 and <= 14)
            return many;
        return last switch
        {
            1 => one,
            2 or 3 or 4 => few,
            _ => many,
        };
    }
}
