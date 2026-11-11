using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase currentModel;
    public ViewModelBase CurrentModel
    {
        get => currentModel;
        set => this.RaiseAndSetIfChanged(ref currentModel, value);
    }
    public static MainWindowViewModel Instance { get; private set; } = null!;
    public MainWindowViewModel()
    {
        Instance = this;
        currentModel = new RegisterViewModel();
    }
}
