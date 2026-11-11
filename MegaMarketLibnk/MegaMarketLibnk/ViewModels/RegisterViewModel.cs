using System.Reactive;
using MegaMarketLibnk.Helpers;
using MegaMarketLibnk.Models;
using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class RegisterViewModel : FormViewModelBase
{
    private bool isSeller;
    public bool IsSeller
    {
        get => isSeller;
        set
        {
            this.RaiseAndSetIfChanged(ref isSeller, value);
            this.RaisePropertyChanged(nameof(IsSellerView));
            this.RaisePropertyChanged(nameof(IsBuyerView));
            this.RaisePropertyChanged(nameof(InnHint));
        }
    }
    public bool IsSellerView => IsSeller;
    public bool IsBuyerView => !IsSeller;
    private string passwordConfirm = string.Empty;
    public string PasswordConfirm
    {
        get => passwordConfirm;
        set
        {
            this.RaiseAndSetIfChanged(ref passwordConfirm, value);
            this.RaisePropertyChanged(nameof(PasswordConfirmHint));
        }
    }
    private string phone = string.Empty;
    public string Phone
    {
        get => phone;
        set
        {
            this.RaiseAndSetIfChanged(ref phone, value);
            this.RaisePropertyChanged(nameof(PhoneHint));
        }
    }
    private string firstName = string.Empty;
    public string FirstName
    {
        get => firstName;
        set
        {
            this.RaiseAndSetIfChanged(ref firstName, value);
            this.RaisePropertyChanged(nameof(FirstNameHint));
        }
    }
    private string lastName = string.Empty;
    public string LastName
    {
        get => lastName;
        set
        {
            this.RaiseAndSetIfChanged(ref lastName, value);
            this.RaisePropertyChanged(nameof(LastNameHint));
        }
    }
    private string companyName = string.Empty;
    public string CompanyName
    {
        get => companyName;
        set
        {
            this.RaiseAndSetIfChanged(ref companyName, value);
            this.RaisePropertyChanged(nameof(CompanyNameHint));
        }
    }
    private string inn = string.Empty;
    public string Inn
    {
        get => inn;
        set
        {
            this.RaiseAndSetIfChanged(ref inn, value);
            this.RaisePropertyChanged(nameof(InnHint));
        }
    }
    private string innConfirm = string.Empty;
    public string InnConfirm
    {
        get => innConfirm;
        set
        {
            this.RaiseAndSetIfChanged(ref innConfirm, value);
            this.RaisePropertyChanged(nameof(InnConfirmHint));
        }
    }
    private string status = string.Empty;
    public string Status
    {
        get => status;
        private set => this.RaiseAndSetIfChanged(ref status, value);
    }
    public string FirstNameHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FirstName)) return "Обязательное поле";
            return "✓";
        }
    }
    public string LastNameHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(LastName)) return "Обязательное поле";
            return "✓";
        }
    }
    public string CompanyNameHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CompanyName)) return "Название для карточки продавца";
            return "✓";
        }
    }
    public string PasswordConfirmHint
    {
        get
        {
            if (string.IsNullOrEmpty(PasswordConfirm)) return "Повторите пароль";
            if (PasswordConfirm == Password) return "✓ Пароли совпадают";
            return "✗ Пароли не совпадают";
        }
    }
    public string PhoneHint
    {
        get
        {
            if (string.IsNullOrEmpty(Phone)) return "Формат: +7 (999) 123-45-67";
            if (Validation.IsValidPhone(Phone)) return "✓ Телефон корректен";
            return "✗ Введите 11 цифр";
        }
    }
    public string InnHint
    {
        get
        {
            if (!IsSeller) return "ИНН нужен только продавцу";
            if (string.IsNullOrEmpty(Inn)) return "10 или 12 цифр";
            if (Validation.IsValidInn(Inn)) return "✓ ИНН корректен";
            return "✗ Нужно 10 или 12 цифр";
        }
    }
    public string InnConfirmHint
    {
        get
        {
            if (string.IsNullOrEmpty(InnConfirm)) return "Повторите ИНН";
            if (InnConfirm == Inn) return "✓ ИНН совпадают";
            return "✗ ИНН не совпадают";
        }
    }
    public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> CheckInnCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectBuyerCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectSellerCommand { get; }
    public ReactiveCommand<Unit, Unit> BackToAuthCommand { get; }
    public RegisterViewModel()
    {
        RegisterCommand = ReactiveCommand.Create(Register);
        CheckInnCommand = ReactiveCommand.Create(CheckInn);
        SelectBuyerCommand = ReactiveCommand.Create(() => { IsSeller = false; });
        SelectSellerCommand = ReactiveCommand.Create(() => { IsSeller = true; });
        BackToAuthCommand = ReactiveCommand.Create(() =>
        {
            MainWindowViewModel.Instance.CurrentModel = new AuthViewModel();
        });
    }
    private void Register()
    {
        if (!Validation.IsValidEmail(Email))
        {
            Status = "✗ Проверьте email";
            return;
        }
        if (!Validation.IsValidPassword(Password))
        {
            Status = "✗ Пароль: минимум 6 символов, буквы и цифры";
            return;
        }
        if (Password != PasswordConfirm)
        {
            Status = "✗ Пароли не совпадают";
            return;
        }
        if (!Validation.IsValidPhone(Phone))
        {
            Status = "✗ Телефон: нужно 11 цифр";
            return;
        }
        if (!IsCaptchaValid)
        {
            Status = "✗ Капча введена неверно";
            return;
        }
        if (!IsSeller && (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName)))
        {
            Status = "✗ Укажите имя и фамилию";
            return;
        }
        if (IsSeller && string.IsNullOrWhiteSpace(CompanyName))
        {
            Status = "✗ Укажите название компании";
            return;
        }
        if (IsSeller && !Validation.IsValidInn(Inn))
        {
            Status = "✗ ИНН: 10 или 12 цифр";
            return;
        }
        if (IsSeller && Inn != InnConfirm)
        {
            Status = "✗ ИНН не совпадают";
            return;
        }
        try
        {
            var db = SF.Context;
            var roleName = IsSeller ? "Продавец" : "Покупатель";
            var role = db.Roles.FirstOrDefault(x => x.Rolename == roleName);
            if (role == null)
            {
                role = new Role { Rolename = roleName };
                db.Roles.Add(role);
            }
            var user = new User
            {
                Email = Email.Trim(),
                Phone = Phone,
                Passwordhash = Password,
                Salt = string.Empty,
                Createdat = DateTime.Now,
                Role = role,
            };
            if (IsSeller)
            {
                user.Sellerprofile = new Sellerprofile
                {
                    Companyname = CompanyName.Trim(),
                    Inn = Inn.Trim(),
                };
            }
            else
            {
                user.Customerprofile = new Customerprofile
                {
                    Firstname = FirstName.Trim(),
                    Lastname = LastName.Trim(),
                };
            }
            db.Users.Add(user);
            db.SaveChanges();
            MainWindowViewModel.Instance.CurrentModel =
                new AuthViewModel("✓ Регистрация завершена — войдите в аккаунт");
        }
        catch
        {
            Status = "✗ Ошибка записи в БД (возможно, email уже занят)";
        }
    }
    private void CheckInn()
    {
        if (!IsSeller)
        {
            Status = "✗ ИНН нужен только продавцу";
            return;
        }
        if (!Validation.IsValidInn(Inn))
        {
            Status = "✗ ИНН: 10 или 12 цифр";
            return;
        }
        if (Inn != InnConfirm)
        {
            Status = "✗ ИНН не совпадают";
            return;
        }
        Status = "✓ ИНН принят";
    }
}
