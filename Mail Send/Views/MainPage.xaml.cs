using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon.Core;
using Mail_Send.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DeviceId;
namespace Mail_Send.Views;
partial class starter
{
    [RelayCommand]
    public static void start_algorithm()
    {
        Debug.WriteLine("dddd");
    }
}
public sealed partial class MainPage : Page
{
    
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();

    }
    
    private bool isshow;
    [RelayCommand]
    public void ShowHideWindow()
    {
        MainWindow window = App.MainWindow as MainWindow;
        if(window == null)
        {
            return;
        }
        if (window.Visible)
        {
            window.Hide();
        }
        else
        {
            window.Show();
        }
        isshow = window.Visible;
    }
    [RelayCommand]
    public void ExitApplication()
    {
        App.MainWindow?.Close();
    }
    private void Visibility_window_Click(object sender, RoutedEventArgs e) => Debug.WriteLine("dddddd");

    public async Task ShowErrorDialog(string message)
    {
        ContentDialog errorDialog = new ContentDialog
        {
            Title = "Ошибка",
            Content = message,
            CloseButtonText = "ОК"
        };
        errorDialog.XamlRoot = this.XamlRoot;

        await errorDialog.ShowAsync();
    }


    private async void visibility_window_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        
    }

    private  async void TaskbarIcon_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        await ShowErrorDialog("sss");
    }
}
