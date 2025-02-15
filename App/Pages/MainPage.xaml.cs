using App.Pages.Menus;

namespace App.Pages;

public sealed partial class MainPage : Page
{
    private static MainPage s_instance;

    public MainPage()
    {
        s_instance = this;

        InitializeComponent();

        MainNavigationView.SelectedItem = MainNavigationView.MenuItems.First();
    }

    public static void Navigate(Type pageType, object parameter = null)
    {
        s_instance.MainFrame.Navigate(pageType, parameter);
        s_instance.MainFrame.BackStack.Clear();
    }

    public static void GoBack()
    {
        if (s_instance.MainFrame.CanGoBack)
        {
            s_instance.MainFrame.GoBack();
        }
    }

    public static void ShowLoading(string message = null)
    {
        s_instance?.DispatcherQueue.TryEnqueue(() =>
        {
            if (string.IsNullOrEmpty(message)) s_instance.LoadingTextBlock.Visibility = Visibility.Collapsed;
            else
            {
                s_instance.LoadingTextBlock.Text = message;
                s_instance.LoadingTextBlock.Visibility = Visibility.Visible;
            }

            s_instance.LoadingGrid.Visibility = Visibility.Visible;
            s_instance.MainFrame.IsEnabled = false;
        });
    }

    public static void HideLoading()
    {
        s_instance?.DispatcherQueue.TryEnqueue(() =>
        {
            s_instance.MainFrame.IsEnabled = true;
            s_instance.LoadingGrid.Visibility = Visibility.Collapsed;
        });
    }

    private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            Navigate(typeof(SettingsPage));
            return;
        }

        var selectedItem = sender.SelectedItem as NavigationViewItem;

        if (selectedItem == ItemsNavigationViewItem) Navigate(typeof(ItemsPage));
        else if (selectedItem == ManageNavigationViewItem) Navigate(typeof(ManagePage));
        else if (selectedItem == ManageRecordsNavigationViewItem) Navigate(typeof(ManageRecordsPage));
    }
}
