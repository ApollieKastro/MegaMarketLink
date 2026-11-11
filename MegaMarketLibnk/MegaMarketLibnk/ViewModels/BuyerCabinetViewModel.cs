using System.Collections.ObjectModel;
using System.Linq;
using MegaMarketLibnk.Models;
using ReactiveUI;

namespace MegaMarketLibnk.ViewModels;

public class BuyerOrderViewModel : ViewModelBase
{
    public Order Order { get; }
    public string Status { get; }
    private const string Active = "#FF3B30";
    private const string Inactive = "#04D16B";

    public int Step { get; }
    public string Seg0 => Active;           
    public string Seg1 => Step >= 1 ? Active : Inactive;
    public string Seg2 => Step >= 2 ? Active : Inactive;

    public BuyerOrderViewModel(Order order)
    {
        Order = order;
        var last = order.Orderstatuses.OrderBy(s => s.Changedat).LastOrDefault();
        Status = (last?.Status ?? "сборка").Trim();
        Step = Status.ToLowerInvariant() switch
        {
            "в пути" => 1,
            "доставлено" => 2,
            _ => 0
        };
    } 
    public string OrderDateText => Order.Orderdate.ToString("dd.MM.yyyy HH:mm");
    public string TotalText => $"{Order.Totalprice:0.00} ₽";
}

public class BuyerCabinetViewModel : ViewModelBase
{
    public string UserName => SF.User?.Email ?? "Гость";
    public string FullName { get; }
    public string Email => SF.User?.Email ?? "";
    public ObservableCollection<BuyerOrderViewModel> Orders { get; } = new();

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> GoHomeCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> LogoutCommand { get; }
    public BuyerCabinetViewModel()
    {
        var user = SF.User;
        var profile = user == null ? null : SF.Context.Customerprofiles.FirstOrDefault(c => c.Userid == user.Id);
        GoHomeCommand = ReactiveUI.ReactiveCommand.Create(GoHome);
        LogoutCommand = ReactiveUI.ReactiveCommand.Create(Logout);
        FullName = profile == null ? (user?.Email ?? "") : $"{profile.Lastname} {profile.Firstname} {profile.Middlename}".Trim();
        if (profile != null)
        {
            foreach (var order in SF.Context.Orders
                .Where(o => o.Customerid == profile.Id)
                .OrderByDescending(o => o.Orderdate)
                .ToList())
            {
                SF.Context.Entry(order).Collection(o => o.Orderstatuses).Load();
                Orders.Add(new BuyerOrderViewModel(order));
            }
        }
    }

    public void GoHome() => MainWindowViewModel.Instance.CurrentModel = new HomeViewModel();
    public void Logout() { SF.User = null; MainWindowViewModel.Instance.CurrentModel = new AuthViewModel(); }
}
