using System.Diagnostics;
using Mail_Send.Contracts.Services;
using Mail_Send.Helpers;
using Mail_Send.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Windows.System;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.Activation;
using CommunityToolkit.Mvvm.Input;
using DeviceId;

namespace Mail_Send.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }
    private static string connectionString = "Filename==database.db";
    public static ShellPage Current { get; private set; }
    private bool isshow;
    [RelayCommand]
    public void ShowHideWindow()
    {
        MainWindow window = App.MainWindow as MainWindow;
        if (window == null)
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
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        
        InitializeComponent();
        string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
        var selected_shoped = App.GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);

        if (selected_shoped != null)
        {

        }
        else
        {
            background.Visibility = Visibility.Collapsed;
        }
        setting.setting_Sql.CreateBs();
        account.SqliteFunctions.CreateTable();
        email.Email_Sql.CreateBs();
        List<setting.automatic_account> auto_sett = new List<setting.automatic_account>();
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                        INSERT INTO settings (threads, type_setting)
                        VALUES ($threads, $type_setting)
                    ";
            command.Parameters.AddWithValue("$threads", 1);
            command.Parameters.AddWithValue("$type_setting", "auto");

            command.ExecuteNonQuery();
        }
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                            UPDATE settings
                            SET auto = $auto
                        ";
            command.Parameters.AddWithValue("$auto", JsonConvert.SerializeObject(auto_sett));
            command.ExecuteNonQuery();
        }
        Current = this; 
        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
        
    }
    public Frame AppFrame { get { return this.NavigationFrame; } }
    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
        NavigationFrame.Navigate(new home().GetType());
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }
    public async void ChangeView(Type page )
    {
        NavigationFrame.Navigate(page);
    }
    
    private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        NavigationViewItem el = (NavigationViewItem)NavigationViewControl.SelectedItem;

        Debug.WriteLine(el.Name + " - hfjghdfkg");
        if (el.Name == "home_navigation")
        {
            NavigationFrame.Navigate(typeof(home));
        }
        else if (el.Name == "account_navigation")
        {
            NavigationFrame.Navigate(typeof(account));
        }
        else if (el.Name == "text_navigation")
        {
            NavigationFrame.Navigate(new text().GetType());
        }
        else if (el.Name == "SettingsItem")
        {
            NavigationFrame.Navigate(new setting().GetType());
        }
        else if (el.Name == "email_navigation")
        {
            NavigationFrame.Navigate(new email().GetType());
        }
        else if (el.Name == "chart_navigation")
        {
            NavigationFrame.Navigate(new statistic().GetType());
        }
        else if (el.Name == "oplata_navigation")
        {
            NavigationFrame.Navigate(new shop().GetType());
        }
        else if (el.Name == "info")
        {
            NavigationFrame.Navigate(new info().GetType());
        }
        else if (el.Name == "shop")
        {
            NavigationFrame.Navigate(new shop().GetType());
        }
    }
}
