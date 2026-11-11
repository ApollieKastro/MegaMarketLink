using System.Collections.ObjectModel;
using System.Reactive;
using MegaMarketLibnk.Models;
using MegaMarketLibnk.Services;
using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class CartViewModel : ViewModelBase
{
    private readonly HomeViewModel home;
    public ObservableCollection<CartGroupViewModel> Groups { get; } = new();
    private string promoText = string.Empty;
    public string PromoText
    {
        get => promoText;
        set => this.RaiseAndSetIfChanged(ref promoText, value);
    }
    private string promoStatus = string.Empty;
    public string PromoStatus
    {
        get => promoStatus;
        private set => this.RaiseAndSetIfChanged(ref promoStatus, value);
    }
    private string promoStatusColor = "#4ADE80";
    public string PromoStatusColor
    {
        get => promoStatusColor;
        private set => this.RaiseAndSetIfChanged(ref promoStatusColor, value);
    }
    private bool hasPromoStatus;
    public bool HasPromoStatus
    {
        get => hasPromoStatus;
        private set => this.RaiseAndSetIfChanged(ref hasPromoStatus, value);
    }
    private string itemsSummary = string.Empty;
    public string ItemsSummary
    {
        get => itemsSummary;
        private set => this.RaiseAndSetIfChanged(ref itemsSummary, value);
    }
    private string subtotalText = "0 ₽";
    public string SubtotalText
    {
        get => subtotalText;
        private set => this.RaiseAndSetIfChanged(ref subtotalText, value);
    }
    private string discountText = string.Empty;
    public string DiscountText
    {
        get => discountText;
        private set => this.RaiseAndSetIfChanged(ref discountText, value);
    }
    private bool hasDiscount;
    public bool HasDiscount
    {
        get => hasDiscount;
        private set => this.RaiseAndSetIfChanged(ref hasDiscount, value);
    }
    private string deliveryText = "0 ₽";
    public string DeliveryText
    {
        get => deliveryText;
        private set => this.RaiseAndSetIfChanged(ref deliveryText, value);
    }
    private string totalText = "0 ₽";
    public string TotalText
    {
        get => totalText;
        private set => this.RaiseAndSetIfChanged(ref totalText, value);
    }
    private bool isEmpty = true;
    public bool IsEmpty
    {
        get => isEmpty;
        private set => this.RaiseAndSetIfChanged(ref isEmpty, value);
    }
    public bool HasItems => !IsEmpty;
    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> ApplyPromoCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearCartCommand { get; }
    public ReactiveCommand<Unit, Unit> CheckoutCommand { get; }
    public CartViewModel(HomeViewModel home)
    {
        this.home = home;
        BackCommand = ReactiveCommand.Create(() =>
        {
            CartService.Changed -= Refresh;
            CartService.ClearPromo();
            MainWindowViewModel.Instance.CurrentModel = home;
            home.RefreshCart();
        });
        CheckoutCommand = ReactiveCommand.Create(() =>
        {
            if (CartService.Items.Count == 0)
                return;
            CartService.Changed -= Refresh;
            MainWindowViewModel.Instance.CurrentModel = new CheckoutViewModel(home);
        });
        ApplyPromoCommand = ReactiveCommand.Create(() =>
        {
            var (ok, message) = CartService.ApplyPromo(PromoText);
            PromoStatus = message;
            PromoStatusColor = ok ? "#4ADE80" : "#F87171";
            HasPromoStatus = true;
            if (ok)
                PromoText = string.Empty;
        });
        ClearCartCommand = ReactiveCommand.Create(() =>
        {
            CartService.Clear();
            PromoStatus = string.Empty;
            HasPromoStatus = false;
            PromoText = string.Empty;
        });
        if (CartService.AppliedPromo is { } promo)
        {
            PromoStatus = $"✓ Применён промокод {promo.Code}: {promo.Description}";
            PromoStatusColor = "#4ADE80";
            HasPromoStatus = true;
        }
        CartService.Changed += Refresh;
        Refresh();
    }
    private void Refresh()
    {
        Groups.Clear();
        foreach (var group in CartService.Items.GroupBy(i => i.SellerName))
            Groups.Add(new CartGroupViewModel(group.Key, group.ToList(), CartService.AppliedPromo));
        var subtotal = Groups.Sum(g => g.Subtotal);
        var promo = CartService.AppliedPromo;
        var discount = CartService.DiscountFor(promo, subtotal);
        var delivery = Groups.Sum(g => g.DeliveryCost);
        var total = subtotal - discount + delivery;
        var pieces = CartService.Count;
        var shipments = Groups.Count;
        ItemsSummary = $"{pieces} шт. · {shipments} "
            + CartService.Plural(shipments, "отправление", "отправления", "отправлений");
        SubtotalText = FeedTile.Money(subtotal) + " ₽";
        HasDiscount = discount > 0;
        DiscountText = "− " + FeedTile.Money(discount) + " ₽"
            + (promo != null && discount > 0 ? $" ({promo.Code})" : string.Empty);
        DeliveryText = promo is { FreeDelivery: true }
            ? "0 ₽ — бесплатно по промокоду"
            : FeedTile.Money(delivery) + " ₽";
        TotalText = FeedTile.Money(total) + " ₽";
        IsEmpty = Groups.Count == 0;
        this.RaisePropertyChanged(nameof(HasItems));
    }
}
