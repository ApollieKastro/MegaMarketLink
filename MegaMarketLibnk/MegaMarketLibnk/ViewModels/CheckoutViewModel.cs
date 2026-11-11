using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Media.Imaging;
using MegaMarketLibnk.Helpers;
using MegaMarketLibnk.Models;
using MegaMarketLibnk.Services;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class CheckoutViewModel : ViewModelBase
{
    private readonly HomeViewModel home;
    private readonly Customerprofile? profile;
    private string status = string.Empty;
    public string Status
    {
        get => status;
        private set => this.RaiseAndSetIfChanged(ref status, value);
    }
    private string statusColor = "#4ADE80";
    public string StatusColor
    {
        get => statusColor;
        private set => this.RaiseAndSetIfChanged(ref statusColor, value);
    }
    private bool isPaid;
    public bool IsPaid
    {
        get => isPaid;
        private set
        {
            this.RaiseAndSetIfChanged(ref isPaid, value);
            this.RaisePropertyChanged(nameof(IsFormVisible));
        }
    }
    private const string QrUrl =
        "https://files.donationalerts.com/uploads/qr/16115162/qr_e9bdc8c0230c2e87fdc1e51bb987903d.png";
    private bool isQrVisible;
    public bool IsQrVisible
    {
        get => isQrVisible;
        private set
        {
            this.RaiseAndSetIfChanged(ref isQrVisible, value);
            this.RaisePropertyChanged(nameof(IsFormVisible));
        }
    }
    public bool IsFormVisible => !IsPaid && !IsQrVisible;
    private Bitmap? qrImage;
    public Bitmap? QrImage
    {
        get => qrImage;
        private set
        {
            this.RaiseAndSetIfChanged(ref qrImage, value);
            this.RaisePropertyChanged(nameof(HasQrImage));
        }
    }
    public bool HasQrImage => QrImage != null;
    private string qrStatus = string.Empty;
    public string QrStatus
    {
        get => qrStatus;
        private set => this.RaiseAndSetIfChanged(ref qrStatus, value);
    }
    private string qrStatusColor = "#B9B9C3";
    public string QrStatusColor
    {
        get => qrStatusColor;
        private set => this.RaiseAndSetIfChanged(ref qrStatusColor, value);
    }
    public ObservableCollection<CartGroupViewModel> Groups { get; } = new();
    public decimal Subtotal { get; }
    public decimal PromoDiscount { get; }
    public decimal DeliverySum { get; }
    public bool HasPromoDiscount => PromoDiscount > 0;
    public string PromoDiscountText => "− " + FeedTile.Money(PromoDiscount) + " ₽";
    public string SubtotalText => FeedTile.Money(Subtotal) + " ₽";
    public string DeliverySumText => FeedTile.Money(DeliverySum) + " ₽";
    public ObservableCollection<string> AddressOptions { get; } = new();
    private int addressMode;
    public int AddressMode
    {
        get => addressMode;
        set
        {
            this.RaiseAndSetIfChanged(ref addressMode, value);
            this.RaisePropertyChanged(nameof(IsManualVisible));
            this.RaisePropertyChanged(nameof(IsSavedVisible));
            RaiseValid();
        }
    }
    public bool HasSavedAddress { get; }
    public bool IsManualVisible => !HasSavedAddress || AddressMode == 1;
    public bool IsSavedVisible => HasSavedAddress && AddressMode == 0;
    public string SavedAddressText { get; } = string.Empty;
    private Address? savedAddress;
    private string postcode = string.Empty;
    public string Postcode
    {
        get => postcode;
        set { this.RaiseAndSetIfChanged(ref postcode, value); RaiseValid(); }
    }
    private string city = string.Empty;
    public string City
    {
        get => city;
        set { this.RaiseAndSetIfChanged(ref city, value); RaiseValid(); }
    }
    private string street = string.Empty;
    public string Street
    {
        get => street;
        set { this.RaiseAndSetIfChanged(ref street, value); RaiseValid(); }
    }
    private string house = string.Empty;
    public string House
    {
        get => house;
        set { this.RaiseAndSetIfChanged(ref house, value); RaiseValid(); }
    }
    private string apartment = string.Empty;
    public string Apartment
    {
        get => apartment;
        set { this.RaiseAndSetIfChanged(ref apartment, value); RaiseValid(); }
    }
    private string recipient = string.Empty;
    public string Recipient
    {
        get => recipient;
        set { this.RaiseAndSetIfChanged(ref recipient, value); RaiseValid(); }
    }
    private string phone = string.Empty;
    public string Phone
    {
        get => phone;
        set { this.RaiseAndSetIfChanged(ref phone, value); RaiseValid(); }
    }
    public string PostcodeHint => Hint(Postcode, "Индекс: 6 цифр", ValidPostcode, "Ровно 6 цифр");
    public string CityHint => Hint(City, "Город", ValidCity, "Только буквы, минимум 2");
    public string StreetHint => Hint(Street, "Улица", ValidStreet, "Минимум 2 символа");
    public string HouseHint => Hint(House, "Дом", ValidHouse, "Укажите номер дома");
    public string ApartmentHint => string.IsNullOrWhiteSpace(Apartment)
        ? "Квартира (необязательно)"
        : ValidApartment(Apartment) ? "✓" : "✗ Только цифры";
    public string RecipientHint => Hint(Recipient, "ФИО получателя", ValidRecipient, "Минимум 2 символа");
    public string PhoneHint => Hint(Phone, "Формат: 7XXXXXXXXXX", Validation.IsValidPhone, "Нужно 11 цифр");
    public bool IsPostcodeInvalid => IsInvalid(Postcode, ValidPostcode);
    public bool IsCityInvalid => IsInvalid(City, ValidCity);
    public bool IsStreetInvalid => IsInvalid(Street, ValidStreet);
    public bool IsHouseInvalid => IsInvalid(House, ValidHouse);
    public bool IsApartmentInvalid => IsInvalid(Apartment, v => string.IsNullOrWhiteSpace(v) || ValidApartment(v));
    public bool IsRecipientInvalid => IsInvalid(Recipient, ValidRecipient);
    public bool IsPhoneInvalid => IsInvalid(Phone, Validation.IsValidPhone);
    public int BonusAvailable { get; private set; }
    public string BonusAvailableText =>
        $"Доступно: {BonusAvailable} баллов · 1 балл = 1 ₽";
    private string bonusText = string.Empty;
    public string BonusText
    {
        get => bonusText;
        set
        {
            this.RaiseAndSetIfChanged(ref bonusText, value);
            this.RaisePropertyChanged(nameof(BonusHint));
            this.RaisePropertyChanged(nameof(IsBonusInvalid));
            this.RaisePropertyChanged(nameof(TotalText));
            this.RaisePropertyChanged(nameof(HasBonusDiscount));
            this.RaisePropertyChanged(nameof(BonusDiscountText));
            RaiseValid();
        }
    }
    private int BonusMax => Math.Min(BonusAvailable, (int)Math.Floor(PayableBeforeBonus));
    private int BonusUsed =>
        int.TryParse(BonusText.Trim(), out var value) && value > 0
            ? Math.Min(value, BonusMax)
            : 0;
    private decimal PayableBeforeBonus => Subtotal - PromoDiscount + DeliverySum;
    public string BonusHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(BonusText))
                return $"Списать до {BonusMax} баллов";
            if (!int.TryParse(BonusText.Trim(), out var value))
                return "✗ Только целые числа";
            if (value < 0)
                return "✗ От 0 и выше";
            if (value > BonusMax)
                return $"✗ Максимум {BonusMax} баллов";
            return $"✓ Спишем {value} баллов";
        }
    }
    public bool IsBonusInvalid => !string.IsNullOrWhiteSpace(BonusText) && BonusUsed != BonusRaw();
    private int BonusRaw() => int.TryParse(BonusText.Trim(), out var v) ? v : -1;
    public bool HasBonusDiscount => BonusUsed > 0;
    public string BonusDiscountText => "− " + FeedTile.Money(BonusUsed) + " ₽";
    public string TotalText => FeedTile.Money(PayableBeforeBonus - BonusUsed) + " ₽";
    private bool isValid;
    public bool IsValid
    {
        get => isValid;
        private set => this.RaiseAndSetIfChanged(ref isValid, value);
    }
    private bool ManualValid =>
        ValidPostcode(Postcode) && ValidCity(City) && ValidStreet(Street)
        && ValidHouse(House) && ValidApartment(Apartment);
    private void RaiseValid()
    {
        this.RaisePropertyChanged(nameof(PostcodeHint));
        this.RaisePropertyChanged(nameof(CityHint));
        this.RaisePropertyChanged(nameof(StreetHint));
        this.RaisePropertyChanged(nameof(HouseHint));
        this.RaisePropertyChanged(nameof(ApartmentHint));
        this.RaisePropertyChanged(nameof(RecipientHint));
        this.RaisePropertyChanged(nameof(PhoneHint));
        this.RaisePropertyChanged(nameof(IsPostcodeInvalid));
        this.RaisePropertyChanged(nameof(IsCityInvalid));
        this.RaisePropertyChanged(nameof(IsStreetInvalid));
        this.RaisePropertyChanged(nameof(IsHouseInvalid));
        this.RaisePropertyChanged(nameof(IsApartmentInvalid));
        this.RaisePropertyChanged(nameof(IsRecipientInvalid));
        this.RaisePropertyChanged(nameof(IsPhoneInvalid));
        this.RaisePropertyChanged(nameof(IsBonusInvalid));
        IsValid = HasItems() && ValidRecipient(Recipient) && Validation.IsValidPhone(Phone)
            && BonusValid && (!IsManualVisible || ManualValid);
    }
    private bool BonusValid =>
        string.IsNullOrWhiteSpace(BonusText)
        || (int.TryParse(BonusText.Trim(), out var v) && v >= 0 && v <= BonusMax);
    private static bool HasItems() => CartService.Items.Count > 0;
    private static bool ValidPostcode(string v) => IsDigits(v.Trim(), 6);
    private static bool ValidCity(string v) => v.Trim().Length >= 2 && v.Trim().All(c => char.IsLetter(c) || c is ' ' or '-');
    private static bool ValidStreet(string v) => v.Trim().Length >= 2;
    private static bool ValidHouse(string v) => v.Trim().Length >= 1;
    private static bool ValidApartment(string v) => v.Trim().All(char.IsDigit);
    private static bool ValidRecipient(string v) => v.Trim().Length >= 2;
    private static bool IsDigits(string value, int length) =>
        value.Length == length && value.All(char.IsDigit);
    private static bool IsInvalid(string value, Func<string, bool> valid) =>
        !string.IsNullOrWhiteSpace(value) && !valid(value);
    private static string Hint(string value, string neutral, Func<string, bool> valid, string error) =>
        string.IsNullOrWhiteSpace(value) ? neutral
        : valid(value) ? "✓"
        : "✗ " + error;
    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> PayCommand { get; }
    public ReactiveCommand<Unit, Unit> ConfirmPayCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelQrCommand { get; }
    public ReactiveCommand<Unit, Unit> GoHomeCommand { get; }
    public CheckoutViewModel(HomeViewModel home)
    {
        this.home = home;
        var promo = CartService.AppliedPromo;
        foreach (var group in CartService.Items.GroupBy(i => i.SellerName))
            Groups.Add(new CartGroupViewModel(group.Key, group.ToList(), promo));
        Subtotal = Groups.Sum(g => g.Subtotal);
        PromoDiscount = CartService.DiscountFor(promo, Subtotal);
        DeliverySum = Groups.Sum(g => g.DeliveryCost);
        CartService.ClearPromo();
        try
        {
            if (SF.User != null)
            {
                profile = SF.Context.Customerprofiles
                    .Include(p => p.Mainaddress)
                    .FirstOrDefault(p => p.Userid == SF.User.Id);
                BonusAvailable = profile?.Bonuspointsbalance ?? 0;
            }
        }
        catch
        {
            profile = null;
        }
        if (profile?.Mainaddress is { } address)
        {
            savedAddress = address;
            HasSavedAddress = true;
            AddressOptions.Add("Из профиля: " + Format(address));
            AddressOptions.Add("Ввести другой адрес");
            SavedAddressText = Format(address);
            addressMode = 0;
        }
        else
        {
            AddressOptions.Add("Ввести адрес вручную");
            addressMode = 0;
        }
        if (Groups.Count == 0)
        {
            Status = "✗ Корзина пуста";
            StatusColor = "#F87171";
        }
        BackCommand = ReactiveCommand.Create(() =>
        {
            MainWindowViewModel.Instance.CurrentModel = new CartViewModel(home);
        });
        GoHomeCommand = ReactiveCommand.Create(() =>
        {
            MainWindowViewModel.Instance.CurrentModel = home;
            home.RefreshCart();
        });
        var canPay = this.WhenAnyValue(x => x.IsValid);
        PayCommand = ReactiveCommand.CreateFromTask(ShowQrAsync, canPay);
        ConfirmPayCommand = ReactiveCommand.Create(Pay);
        CancelQrCommand = ReactiveCommand.Create(() =>
        {
            IsQrVisible = false;
            QrStatus = string.Empty;
        });
        RaiseValid();
    }
    private static string Format(Address a)
    {
        var line = $"{a.City}, {a.Street ?? ""}, д. {a.House ?? ""}";
        if (!string.IsNullOrWhiteSpace(a.Apartment))
            line += $", кв. {a.Apartment}";
        if (!string.IsNullOrWhiteSpace(a.Postcode))
            line += $" ({a.Postcode})";
        return line;
    }
    private async System.Threading.Tasks.Task ShowQrAsync()
    {
        IsQrVisible = true;
        QrImage = null;
        QrStatusColor = "#B9B9C3";
        QrStatus = "Загрузка QR-кода…";
        try
        {
            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(QrUrl);
            QrImage = new Bitmap(new MemoryStream(bytes));
            QrStatus = "Отсканируйте код в приложении банка";
        }
        catch
        {
            QrStatusColor = "#F87171";
            QrStatus = "✗ Не удалось загрузить QR-код — нажмите «Я оплатил» после оплаты по реквизитам";
        }
    }
    private void Pay()
    {
        if (SF.User == null)
        {
            Fail("✗ Войдите в аккаунт, чтобы оформить заказ");
            return;
        }
        if (profile == null)
        {
            Fail("✗ У аккаунта нет профиля покупателя");
            return;
        }
        var items = CartService.Items.Where(i => i.Productid > 0).ToList();
        if (items.Count == 0)
        {
            Fail("✗ Корзина пуста");
            return;
        }
        try
        {
            var db = SF.Context;
            Address address;
            if (IsSavedVisible && savedAddress != null)
            {
                address = savedAddress;
            }
            else
            {
                address = new Address
                {
                    Postcode = Postcode.Trim(),
                    City = City.Trim(),
                    Street = Street.Trim(),
                    House = House.Trim(),
                    Apartment = string.IsNullOrWhiteSpace(Apartment) ? null : Apartment.Trim(),
                };
                db.Addresses.Add(address);
            }
            var productIds = items.Select(i => i.Productid).ToList();
            var sellers = db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p.Sellerid);
            var order = new Order
            {
                Customerid = profile.Id,
                Orderdate = DateTime.Now,
                Deliveryaddress = address,
                Totalprice = PayableBeforeBonus - BonusUsed,
                Deliverycost = DeliverySum,
                Statusid = 1,
            };
            foreach (var item in items)
            {
                if (!sellers.TryGetValue(item.Productid, out var sellerId))
                    continue;
                order.Orderitems.Add(new Orderitem
                {
                    Productid = item.Productid,
                    Quantity = item.Quantity,
                    Priceatpurchase = item.Price,
                    Sellerid = sellerId,
                });
            }
            order.Orderstatuses.Add(new Orderstatus
            {
                Status = "Оплачен",
                Changedat = DateTime.Now,
            });
            if (BonusUsed > 0)
                profile.Bonuspointsbalance -= BonusUsed;
            db.Orders.Add(order);
            db.SaveChanges();
            CartService.Clear();
            IsQrVisible = false;
            QrImage = null;
            var bonusNote = BonusUsed > 0 ? $" · списано {BonusUsed} баллов" : string.Empty;
            Status = $"✓ Заказ №{order.Id} оформлен и оплачен{bonusNote}!";
            StatusColor = "#4ADE80";
            IsPaid = true;
        }
        catch
        {
            Fail("✗ Нет соединения с базой данных");
        }
    }
    private void Fail(string message)
    {
        Status = message;
        StatusColor = "#F87171";
    }
}
