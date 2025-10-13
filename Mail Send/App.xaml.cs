using System.Diagnostics;
using System.Globalization;
using Mail_Send.Activation;
using Mail_Send.Contracts.Services;
using Mail_Send.Core.Contracts.Services;
using Mail_Send.Core.Services;
using Mail_Send.Helpers;
using Mail_Send.Services;
using Mail_Send.ViewModels;
using Mail_Send.Views;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Newtonsoft.Json;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using WinUI3Localizer;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using static Mail_Send.account;
using static Mail_Send.email;
using static Mail_Send.text;
using Windows.ApplicationModel.DataTransfer;
using TextCopy;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Text.Json;
using DeviceId;
using static Mail_Send.setting;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using PayPal.Api;
using Windows.UI.Notifications;
namespace Mail_Send;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Microsoft.UI.Xaml.Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }
    public void SetSequentialIdsUsingReflection<T>(List<T> items)
    {
        PropertyInfo idProp = typeof(T).GetProperty("id");
        if (idProp == null || idProp.PropertyType != typeof(int))
        {
            throw new InvalidOperationException("Class does not have an integer Id property.");
        }

        for (int i = 0; i < items.Count; i++)
        {
            idProp.SetValue(items[i], i + 1);
        }
    }
    public LocalizedStrings localizedStrings = new LocalizedStrings();

    public class scan_clipboard
    {
        public  bool IsJsonArray(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return false;
            }

            try
            {
                var jsonElement = JsonDocument.Parse(jsonString).RootElement;
                return jsonElement.ValueKind == JsonValueKind.Array;
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }
        public bool ProcessClipboardContent(string type)
        {


            string clipboardText = ClipboardService.GetText();

            if (TryDeserialize<export>(clipboardText, out var account))
            {
                var export_data = JsonConvert.DeserializeObject<export>(clipboardText);
                if (type == "account" && export_data.type=="account")
                {
                    return true;
                }
                else if (type == "email" && export_data.type == "email")
                {
                    return true;
                }
                else if (type == "folder" && export_data.type == "folder")
                {
                    return true;
                }
                else if (type == "text" && export_data.type == "text")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        private bool TryDeserialize<T>(string json, out T result)
        {
            try
            {

                if(json == null || json=="") {
                    result = default;
                    return false;
                }
                else
                {
                    result = JsonConvert.DeserializeObject<T>(json);
                    return result != null;
                }
                
                
            }
            catch (Newtonsoft.Json.JsonException)
            {
                result = default;
                return false;
            }
        }
    }
    public class LocalizedStrings
    {
        private ILocalizer localizer;
        private string stringsFolderPath;

        public LocalizedStrings()
        {
            stringsFolderPath = Path.Combine(AppContext.BaseDirectory, "Strings");
            InitializeLocalizer().Wait(); // Замечание: Использование .Wait() в конструкторе может быть не лучшей практикой. Рассмотрите альтернативные подходы для асинхронной инициализации.
        }
        public static List<string> ExtractEmails(string text)
        {
            var emailList = new List<string>();
            string EmailPattern = @"[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}";

            foreach (Match match in Regex.Matches(text, EmailPattern))
            {
                emailList.Add(match.Value);
            }

            return emailList;
        }
        private async Task InitializeLocalizer()
        {
            Debug.WriteLine(stringsFolderPath+"--------");
            StorageFolder stringsFolder = await StorageFolder.GetFolderFromPathAsync(stringsFolderPath);
            localizer = await new LocalizerBuilder()
                .AddStringResourcesFolderForLanguageDictionaries(stringsFolderPath)
                .SetOptions(options =>
                {
                    options.DefaultLanguage = "en-US"; // Устанавливаем язык по умолчанию
                })
                .Build();
        }

        public void ChangeLanguage(string language)
        {
            localizer.SetLanguage(language); // Изменение языка с помощью WinUI3Localizer
        }

        public string GetString(string key)
        {
            Debug.WriteLine(key);
            return localizer.GetLocalizedString(key); // Получение строки с помощью WinUI3Localizer
        }
    }


    public async void ShowDialog()
    {
        // Создание экземпляра ContentDialog
        ContentDialog dialog = new ContentDialog
        {
            Title = "Title",
            Content = "Your message here",
            CloseButtonText = "Ok"
        };

        // Получение текущего активного окна
        Window currentWindow = Window.Current;

        if (currentWindow != null)
        {
            // Показать диалоговое окно
            await dialog.ShowAsync();
        }
    }
    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();
    public static UIElement? AppTitlebar { get; set; }
    public T GetFromCache<T>(string key)
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        if (localSettings.Values.ContainsKey(key))
        {
            return (T)localSettings.Values[key];
        }
        else
        {
            return default(T);
        }
    }
    public void SaveToCache(string key, object value)
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        localSettings.Values[key] = value;
    }
    public App()
    {
        InitializeComponent();
        SqliteFunctions.CreateTable();
        Email_Sql.CreateBs();
        setting_Sql.CreateBs();
        Text_Sql.CreateFolderBs();
        var connectionString = "server=188.225.18.53;user=defaultuser;database=default_db;port=3306;password=bbnn7788";
        var mySqlHelper = new MySqlHelper(connectionString);

        // Создайте таблицу shoped в базе данных MySQL.
        GlobalVariables.MyVariable = mySqlHelper.GetShopedData();
        string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
        var selected_shoped = GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);
        if(selected_shoped == null)
        {
            var all_account = SqliteFunctions.SelectAllRows();
            if(all_account.Count > 1) {
                all_account.RemoveRange(1, all_account.Count-1);
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM accounts";
                    command.ExecuteNonQuery();
                    foreach (create_account acc1 in all_account)
                    {

                        SqliteFunctions.Add_Account(acc1.name, acc1.email, acc1.app_password, acc1.id, acc1.is_verified);
                    }

                }
            }
            var all_email = Email_Sql.Get_email();
            if (all_email.Count > 1)
            {
                all_email.RemoveRange(1, all_email.Count - 1);
                if (all_email[0].email != null)
                {
                    List<string> strings = ExtractEmails(all_email[0].email);
                    if (strings.Count > 50)
                    {
                        strings.RemoveRange(50, strings.Count-1);
                    }
                    all_email[0].email = string.Join("\n", strings); ;

                }
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM all_email";
                    command.ExecuteNonQuery();
                    foreach (Create_email acc1 in all_email)
                    {

                        Email_Sql.Add_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, false);
                    }

                }

            }
            SaveToCache("threads", "1");
        }
        if (GetFromCache<string>("threads") == null)
        {
            SaveToCache("threads", "1");
        }
        if (GetFromCache<string>("auto") == null && GetFromCache<string>("type_setting")==null)
        {
            var all_account = SqliteFunctions.SelectAllRows();
            var all_account_in_setting = new List<automatic_account>();
            foreach (var account in all_account)
            {
                automatic_account item = new automatic_account()
                {
                    id = account.id,
                    name = account.name,
                    is_enabled = true
                };
                all_account_in_setting.Add(item);
            }
            SaveToCache("type_setting", "auto");
            SaveToCache("auto", JsonConvert.SerializeObject(all_account_in_setting));
        }
        if (GetFromCache<string>("language") != null) {
            Debug.WriteLine(GetFromCache<string>("language"));
            localizedStrings.ChangeLanguage(GetFromCache<string>("language"));
        };
        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<Microsoft.UI.Xaml.LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }
    public static XamlRoot ActivateWindowRoot
    {
        get; private set;
    }
    public class export
    {
        public string type
        {
            get; set;
        }
        public List<object> data
        {
            get; set;
        }
    }
    public void generate_import_bar(Grid save_bar, TranslateTransform save_transform, CollectionViewSource source)
    {
        var accounts = new List<create_account>();
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        var connectionString = $"Filename={dbpath}";
        Storyboard storyboard = new Storyboard();
        DoubleAnimation animation = new DoubleAnimation();
        animation.From = 90;
        animation.To = -70;
        animation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
        Storyboard.SetTarget(animation, save_transform);
        Storyboard.SetTargetProperty(animation, "Y");
        storyboard.Children.Add(animation);
        storyboard.Begin();
        // Создание Grid
        Grid grid = new Grid
        {
            Height = 40,
            Width = 400,
            VerticalAlignment = VerticalAlignment.Bottom,
            CornerRadius = new CornerRadius(5),
            Background = new SolidColorBrush(Colors.LightGray) { Opacity = 0.5 }
        };

        // Создание TextBlock
        TextBlock textBlock = new TextBlock
        {
            Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("import_text"),
            Foreground = new SolidColorBrush(Colors.Black),
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 8,
            Padding = new Thickness(20, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        grid.Children.Add(textBlock);

        // Создание StackPanel
        StackPanel stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Padding = new Thickness(0, 0, 5, 0)
        };
        grid.Children.Add(stackPanel);

        // Создание кнопки "Отменить"
        Button cancelButton = new Button
        {
            Content = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("cancel"),
            Width = 70,
            FontSize = 10,
            Foreground = new SolidColorBrush(Colors.Black),
            CornerRadius = new CornerRadius(5)
        };
        cancelButton.Click += (s, args) => {
        }; // Обработчик события нажатия кнопки "Отменить"
        stackPanel.Children.Add(cancelButton);

        // Создание кнопки "Сохранить"
        Button saveButton = new Button
        {
            Content = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("import"),
            Width = 70,
            FontSize = 10,
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Colors.Orange),
            CornerRadius = new CornerRadius(5)
        };
        saveButton.Click += (s, args) => {
            try
            {
                scan_clipboard scan = new scan_clipboard();
                string clipboardText = ClipboardService.GetText();
                var export_object = JsonConvert.DeserializeObject<export>(clipboardText);
                if (export_object.type == "account")
                {
                    var all_folder = SqliteFunctions.SelectAllRows();
                    List<create_account> all_acc = JsonConvert.DeserializeObject<List<create_account>>(JsonConvert.SerializeObject(export_object.data));

                    foreach (var acc in all_acc)
                    {
                        all_folder.Add(acc);
                    }
                    for (var i = 0; i < all_folder.Count; i++)
                    {
                        all_folder[i].id = i + 1;
                    }
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM accounts";
                        command.ExecuteNonQuery();
                        foreach (create_account acc in all_folder)
                        {

                            SqliteFunctions.Add_Account(acc.name, acc.email, acc.app_password, acc.id, acc.is_verified);
                        }

                        var collectionViewSource = source;
                        collectionViewSource.Source = SqliteFunctions.SelectAllRows();
                    }
                }
                else if (export_object.type == "text")
                {
                    var all_folder = Text_Sql.Get_Texts();
                    List<CreateNewTextBlock> all_acc = JsonConvert.DeserializeObject<List<CreateNewTextBlock>>(JsonConvert.SerializeObject(export_object.data));
                    Debug.WriteLine(JsonConvert.SerializeObject(all_acc));
                    foreach (var acc in all_acc)
                    {
                        all_folder.Add(acc);
                    }
                    for (var i = 0; i < all_folder.Count; i++)
                    {
                        all_folder[i].id = i + 1;
                    }
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM texts";
                        command.ExecuteNonQuery();
                        foreach (CreateNewTextBlock acc in all_folder)
                        {

                            Text_Sql.Add_Text(acc.topic, acc.text, JsonConvert.SerializeObject(acc.files), acc.id);
                        }

                        var collectionViewSource = source;
                        collectionViewSource.Source = Text_Sql.Get_Texts();
                    }
                }
                else if (export_object.type == "folder")
                {
                    var all_folder = Text_Sql.Get_Folder();
                    List<Create_folder> all_acc = JsonConvert.DeserializeObject<List<Create_folder>>(JsonConvert.SerializeObject(export_object.data));

                    foreach (var acc in all_acc)
                    {
                        all_folder.Add(acc);
                    }
                    for (var i = 0; i < all_folder.Count; i++)
                    {
                        all_folder[i].id = i + 1;
                    }
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM text_folders";
                        command.ExecuteNonQuery();
                        foreach (Create_folder acc in all_folder)
                        {

                            Text_Sql.Add_folder(acc.name, acc.texts, (int)acc.id);
                        }

                        var collectionViewSource = source;
                        collectionViewSource.Source = Text_Sql.Get_Folder();
                    }
                }
                else if (export_object.type == "email")
                {
                    var all_folder = Email_Sql.Get_email();
                    List<Create_email> all_acc = JsonConvert.DeserializeObject<List<Create_email>>(JsonConvert.SerializeObject(export_object.data));

                    foreach (var acc in all_acc)
                    {
                        all_folder.Add(acc);
                    }
                    for (var i = 0; i < all_folder.Count; i++)
                    {
                        all_folder[i].id = i + 1;
                    }
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM all_email";
                        command.ExecuteNonQuery();
                        foreach (Create_email acc in all_folder)
                        {

                            Email_Sql.Add_email(acc.name, acc.email, acc.folders, acc.texts,(int)acc.id, acc.is_random);
                        }

                        var collectionViewSource = source;
                        collectionViewSource.Source = Email_Sql.Get_email();
                    }
                }
            }
            catch
            {

            }
            
            
        };
        stackPanel.Children.Add(saveButton);
        save_bar.Children.Add(grid);
    }
    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }
    public static class GlobalVariables
    {
        public static List<ShopedData> MyVariable = new List<ShopedData>();
    }
    private void PaymentSucces()
    {
        // Создать уведомление о том, что фоновая задача запущена
        var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
        var textNodes = toastXml.GetElementsByTagName("text");
        textNodes[0].AppendChild(toastXml.CreateTextNode("ВЫ ОПЛАТИЛИ"));
        var toast = new ToastNotification(toastXml);
        ToastNotificationManager.CreateToastNotifier().Show(toast);
    }
    private void PaymentError()
    {
        // Создать уведомление о том, что фоновая задача запущена
        var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
        var textNodes = toastXml.GetElementsByTagName("text");
        textNodes[0].AppendChild(toastXml.CreateTextNode("Оплата не прошла"));
        var toast = new ToastNotification(toastXml);
        ToastNotificationManager.CreateToastNotifier().Show(toast);
    }
    protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {

        var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
        if (activatedEventArgs.Kind == ExtendedActivationKind.Protocol)
        {    
            var d = activatedEventArgs.Data as IProtocolActivatedEventArgs;
            if (d != null)
            {
                var uri = d.Uri;
                var uriString = uri.AbsoluteUri;
                if (uriString.Contains("payment_check/")){
                    var query = HttpUtility.ParseQueryString(uri.Query); 
                    var paymentId = query["id"];
                    var apiContext = new APIContext(new OAuthTokenCredential("AQfeE7A-Bkzl8q3uOQVSPofVajYeUT2s94Iuln7xcVjqUvejukQunoBigrYmbYvI1DFSJ8jZiVhKvBJn", "ENxIKDL3lQ_6qjdBP5ZaHeakRV8P-rgQHk5KDzdcaYXujxjwc_cFt29qLr2SKrhGva94d1etXvaIK5ag").GetAccessToken());
                    Payment getPaymentDetails = Payment.Get(apiContext, paymentId);
                    Debug.WriteLine(getPaymentDetails.state);
                    if (getPaymentDetails.state == "approved")
                    {
                        PaymentSucces();
                    }
                    else
                    {
                        PaymentError();
                    }
                }

                

                // do something...
            }
        }

        
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

}
