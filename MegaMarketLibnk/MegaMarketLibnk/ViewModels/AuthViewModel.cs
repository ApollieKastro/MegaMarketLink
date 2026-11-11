using System.Reactive;
using MegaMarketLibnk.Helpers;
using MegaMarketLibnk.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class AuthViewModel : FormViewModelBase
{
    private string info = string.Empty;
    public string Info
    {
        get => info;
        private set => this.RaiseAndSetIfChanged(ref info, value);
    }
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToRegisterCommand { get; }
    public AuthViewModel(string info = "")
    {
        Info = info;
        LoginCommand = ReactiveCommand.Create(Login);
        GoToRegisterCommand = ReactiveCommand.Create(() =>
        {
            MainWindowViewModel.Instance.CurrentModel = new RegisterViewModel();
        });
    }
    private void Login()
    {
        if (!Validation.IsValidEmail(Email))
        {
            Info = "✗ Проверьте email";
            return;
        }
        if (string.IsNullOrEmpty(Password))
        {
            Info = "✗ Введите пароль";
            return;
        }
        if (!IsCaptchaValid)
        {
            Info = "✗ Капча введена неверно";
            return;
        }
        try
        {
            SF.User = SF.Context.Users
                .Include(x => x.Role)
                .Include(x => x.Sellerprofile)
                .FirstOrDefault(x => x.Email == Email.Trim() && x.Passwordhash == Password);
        }
        catch
        {
            Info = "✗ Нет соединения с базой данных";
            return;
        }
        if (SF.User == null)
            Info = "✗ Неверный email или пароль";
        else if (SF.User.Isbanned)
            Info = "✗ Аккаунт заблокирован";
        else if (SF.User.Role?.Rolename == "Продавец")
        {
            Info = "✓ Вход выполнен";
            MainWindowViewModel.Instance.CurrentModel = new SellerCabinetViewModel();
        }
        else if (SF.User.Role?.Rolename == "Администратор")
        {
            Info = "✓ Вход выполнен";
            MainWindowViewModel.Instance.CurrentModel = new AdminCabinetViewModel();
        }
        else
            MainWindowViewModel.Instance.CurrentModel = new HomeViewModel();
    }
}
