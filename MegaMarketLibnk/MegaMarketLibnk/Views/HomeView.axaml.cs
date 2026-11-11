using Avalonia;
using Avalonia.Controls;
using MegaMarketLibnk.ViewModels;
namespace MegaMarketLibnk.Views;
public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        FeedScroll.PropertyChanged += OnFeedScrollChanged;
    }
    private void OnFeedScrollChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ScrollViewer.OffsetProperty &&
            e.Property != ScrollViewer.ExtentProperty &&
            e.Property != ScrollViewer.ViewportProperty)
            return;
        if (DataContext is not HomeViewModel viewModel)
            return;
        var visibleBottom = FeedScroll.Offset.Y + FeedScroll.Viewport.Height;
        if (visibleBottom >= FeedScroll.Extent.Height - 400)
            viewModel.LoadMore();
    }
}
