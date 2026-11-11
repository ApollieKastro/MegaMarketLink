using System.Reactive;
using MegaMarketLibnk.Helpers;
using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public abstract class FormViewModelBase : ViewModelBase
{
    private string email = string.Empty;
    private string password = string.Empty;
    private string captchaCode = Validation.GenerateCaptchaCode();
    private string captchaInput = string.Empty;
    public string Email
    {
        get => email;
        set
        {
            this.RaiseAndSetIfChanged(ref email, value);
            this.RaisePropertyChanged(nameof(EmailHint));
        }
    }
    public string Password
    {
        get => password;
        set
        {
            this.RaiseAndSetIfChanged(ref password, value);
            this.RaisePropertyChanged(nameof(PasswordHint));
        }
    }
    public string CaptchaCode => captchaCode;
    public string CaptchaInput
    {
        get => captchaInput;
        set
        {
            this.RaiseAndSetIfChanged(ref captchaInput, value);
            this.RaisePropertyChanged(nameof(CaptchaHint));
        }
    }
    public string EmailHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Email)) return "Формат: name@mail.ru";
            if (Validation.IsValidEmail(Email)) return "✓ Email корректен";
            return "✗ Неверный email";
        }
    }
    public string PasswordHint
    {
        get
        {
            if (string.IsNullOrEmpty(Password)) return "Минимум 6 символов, буквы и цифры";
            if (Validation.IsValidPassword(Password)) return "✓ Пароль надёжный";
            return "✗ Нужны буквы и цифры, минимум 6 символов";
        }
    }
    public string CaptchaHint
    {
        get
        {
            if (string.IsNullOrEmpty(CaptchaInput)) return "Введите код с картинки";
            if (IsCaptchaValid) return "✓ Код верный";
            return "✗ Код не совпадает";
        }
    }
    protected bool IsCaptchaValid =>
        !string.IsNullOrWhiteSpace(CaptchaInput) &&
        string.Equals(CaptchaInput.Trim(), captchaCode, StringComparison.OrdinalIgnoreCase);
    public ReactiveCommand<Unit, Unit> RefreshCaptchaCommand { get; }
    protected FormViewModelBase()
    {
        RefreshCaptchaCommand = ReactiveCommand.Create(() =>
        {
            captchaCode = Validation.GenerateCaptchaCode();
            this.RaisePropertyChanged(nameof(CaptchaCode));
            CaptchaInput = string.Empty;
        });
    }
}
