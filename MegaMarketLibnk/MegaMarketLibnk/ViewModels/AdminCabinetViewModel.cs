using System.Collections.ObjectModel;
using System.Linq;
using MegaMarketLibnk.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace MegaMarketLibnk.ViewModels;

public class AdminUserRow
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsBanned { get; set; }
}

public class AdminProductRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public int StatusId { get; set; }
}

public class AdminAddressRow
{
    public int Id { get; set; }
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public string Street { get; set; } = "";
}

public class AdminCabinetViewModel : ViewModelBase
{
    public string UserName => SF.User?.Email ?? "Гость";
    public ObservableCollection<AdminProductRow> PendingProducts { get; } = new();
    public ObservableCollection<AdminUserRow> Users { get; } = new();
    public ObservableCollection<AdminAddressRow> Addresses { get; } = new();

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> GoHomeCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> LogoutCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RefreshCommand { get; }
    public ReactiveCommand<AdminProductRow, System.Reactive.Unit> ApproveCommand { get; }
    public ReactiveCommand<AdminProductRow, System.Reactive.Unit> RejectCommand { get; }
    public ReactiveCommand<AdminUserRow, System.Reactive.Unit> ToggleBanCommand { get; }

    public AdminCabinetViewModel()
    {
        GoHomeCommand = ReactiveUI.ReactiveCommand.Create(GoHome);
        LogoutCommand = ReactiveUI.ReactiveCommand.Create(Logout);
        RefreshCommand = ReactiveUI.ReactiveCommand.Create(Load);
        ApproveCommand = ReactiveUI.ReactiveCommand.Create<AdminProductRow>(Approve);
        RejectCommand = ReactiveUI.ReactiveCommand.Create<AdminProductRow>(Reject);
        ToggleBanCommand = ReactiveUI.ReactiveCommand.Create<AdminUserRow>(ToggleBan);
        Load();
    }

    private void Load()
    {
        PendingProducts.Clear();
        SF.Context.ChangeTracker.Clear();
        foreach (var p in SF.Context.Products.AsNoTracking().Where(p => p.Statusid == 0).ToList())
            PendingProducts.Add(new AdminProductRow { Id = p.Id, Name = p.Name, Sku = p.Sku, StatusId = p.Statusid });

        Users.Clear();
        foreach (var u in SF.Context.Users.Include(x => x.Role).ToList())
            Users.Add(new AdminUserRow { Id = u.Id, Email = u.Email, Role = u.Role?.Rolename ?? "", IsBanned = u.Isbanned });

        Addresses.Clear();
        foreach (var a in SF.Context.Addresses.ToList())
            Addresses.Add(new AdminAddressRow { Id = a.Id, City = a.City, Region = a.Region ?? "", Street = a.Street ?? "" });
    }

    private void Approve(AdminProductRow row)
    {
        var p = SF.Context.Products.FirstOrDefault(x => x.Id == row.Id);
        if (p == null) return;
        p.Statusid = 1;
        SF.Context.SaveChanges();
        Load();
    }

    private void Reject(AdminProductRow row)
    {
        var p = SF.Context.Products.FirstOrDefault(x => x.Id == row.Id);
        if (p == null) return;
        p.Statusid = 2;
        SF.Context.SaveChanges();
        Load();
    }

    private void ToggleBan(AdminUserRow row)
    {
        var u = SF.Context.Users.FirstOrDefault(x => x.Id == row.Id);
        if (u == null) return;
        u.Isbanned = !u.Isbanned;
        SF.Context.SaveChanges();
        Load();
    }

    public void GoHome() => MainWindowViewModel.Instance.CurrentModel = new HomeViewModel();
    public void Logout() { SF.User = null; MainWindowViewModel.Instance.CurrentModel = new AuthViewModel(); }
}
