using ReactiveUI;
namespace MegaMarketLibnk.ViewModels;
public class FilterOption : ViewModelBase
{
    private readonly Action changed;
    private bool isChecked;
    public FilterOption(string name, string key, string value, Action changed)
    {
        Name = name;
        Key = key;
        Value = value;
        this.changed = changed;
    }
    public string Name { get; }
    public string Key { get; }
    public string Value { get; }
    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value)
                return;
            this.RaiseAndSetIfChanged(ref isChecked, value);
            changed();
        }
    }
    public void UncheckSilently()
    {
        if (!isChecked)
            return;
        isChecked = false;
        this.RaisePropertyChanged(nameof(IsChecked));
    }
}
