using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using Windows.Storage;
using Microsoft.UI;
using Windows.System;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Newtonsoft.Json;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Mail_Send.home;
using Microsoft.UI.Xaml.Media.Animation;
using static Mail_Send.App;
using Microsoft.UI.Xaml.Documents;
using TextCopy;
using Windows.UI.Notifications;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Newtonsoft.Json.Linq;
using static Mail_Send.email;
using static Mail_Send.text;
using Microsoft.UI.Xaml.Media.Imaging;
using static Mail_Send.setting;
using DeviceId;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class account : Page
    {

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
        public  account()
        {
            setting.setting_Sql.CreateBs();
            SqliteFunctions.CreateTable();
            this.InitializeComponent();
            GetFromCache<string>("type_setting");
            scan_clipboard scan = new scan_clipboard();
            if (scan.ProcessClipboardContent("account"))
            {
                ((App)Microsoft.UI.Xaml.Application.Current).generate_import_bar(save_bar, save_transform, (CollectionViewSource)Resources["account_list"]);
            }
            
            List<create_account> all_account = SqliteFunctions.SelectAllRows();

            var collectionViewSource = (CollectionViewSource)Resources["account_list"];
            collectionViewSource.Source = all_account;
        }

        int open_account_id;
        TextBlock account_text_box;

        public class create_account
        {
            public int id
            {
                get; set;
            }
            public string name
            {
                get; set;
            }
            public string email
            {
                get; set;
            }
            public string app_password
            {
                get; set;
            }
            public bool is_verified
            {
                get; set; 
            }
        }
        private static string connectionString = "Filename==database.db";
        public static class SqliteFunctions
        {



            public static void CreateTable()
            {
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                Debug.WriteLine(connectionString);
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS accounts (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NULL,
                            email TEXT NULL,
                            app_password TEXT NULL,
                            is_verified TEXT NULL
                        );
                    ";
                    command.ExecuteNonQuery();
                    command.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS texts (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            topic TEXT NULL,
                            text TEXT NULL,
                            files TEXT NULL
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }
            public static void Add_Account(string? name = null, string? email = null, string? app_password = null, int? id = null, bool? is_verified = null)
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO accounts (id, name, email, app_password, is_verified)
                        VALUES ($id, $name, $email, $app_password, $is_verified)
                    ";
                    command.Parameters.AddWithValue("$id", id ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$name", string.IsNullOrEmpty(name)?"": name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$email", string.IsNullOrEmpty(email) ? "" : email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$app_password", string.IsNullOrEmpty(app_password) ? "" : app_password ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$is_verified", is_verified.ToString());
                    command.ExecuteNonQuery();
                }
            }
            public static List<create_account> SelectAllRows()
            {
                var accounts = new List<create_account>();
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT * FROM accounts
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var account = new create_account
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                name = reader.GetString(reader.GetOrdinal("name")),
                                email = reader.GetString(reader.GetOrdinal("email")),
                                app_password = reader.GetString(reader.GetOrdinal("app_password")),
                                is_verified = Boolean.Parse(reader.GetString(reader.GetOrdinal("is_verified"))),
                            };


                            accounts.Add(account);
                        }
                    }
                }

                return accounts;
            }

            public static void Get_All_Account()
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT * FROM accounts
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.IsDBNull(0) ? "" : reader.GetInt32(0).ToString();
                            var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            var email = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            var app_password = reader.IsDBNull(3) ? "" : reader.GetString(3);

                            Debug.WriteLine($"id: {id}, name: {name}, email: {email}, app_password: {app_password}");
                        }
                    }
                }
            }


        }
        private ContentDialog _currentDialog;
        public async void ShowProFeaturesDialog()
        {
            if (_currentDialog != null && _currentDialog.IsLoaded)
            {
                _currentDialog.Hide();
            }
            ContentDialog dialog = new ContentDialog
            {
                CloseButtonText = "Отмена",
                PrimaryButtonText = "Написать в Telegram",
                PrimaryButtonCommand = new RelayCommand(() => { OpenTelegramLink(); }),
                SecondaryButtonText = "PayPal",
                SecondaryButtonCommand = new RelayCommand(() => { /* Действие при нажатии на вторую кнопку */ }),
                Content = new StackPanel
                {
                    Children =
                    {
                        new Microsoft.UI.Xaml.Controls.Image
                        {
                            Source = new SvgImageSource
                            {
                                UriSource = new Uri("ms-appx:///Assets/star-06-svgrepo-com.svg"),

                            },
                            Width = 52,
                            Height = 52,

                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        },
                        new TextBlock
                        {
                            Text = "Разблокируйте Pro функции",
                            FontSize = 28,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    },
                    Spacing = 5
                }
            };

            dialog.XamlRoot = this.XamlRoot;
            _currentDialog = dialog;
            try
            {
                await _currentDialog.ShowAsync();
            }
            catch
            {

            }

        }
        private void OpenTelegramLink()
        {
            // Замените URL на актуальный адрес вашего канала или чата в Telegram
            string telegramUrl = "https://t.me/yourChannelOrChat";
            Process.Start(new ProcessStartInfo
            {
                FileName = telegramUrl,
                UseShellExecute = true
            });
        }
        public class RelayCommand : System.Windows.Input.ICommand
        {
            private Action _execute;
            private Func<bool> _canExecute;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute();
            }

            public void Execute(object parameter)
            {
                _execute();
            }
        }
        public static class GlobalVariables
        {
            public static List<ShopedData> MyVariable = new List<ShopedData>();
        }
        
        private void create_new_block_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
            var selected_shoped = App.GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);

            if (selected_shoped != null)
            {
                if (e.Key == VirtualKey.Enter)
                {

                    List<create_account> all_account = SqliteFunctions.SelectAllRows();
                    Debug.WriteLine(all_account.Count);
                    create_account acc = new create_account();
                    if (all_account.Count == 0)
                    {
                        acc = new create_account()
                        {
                            id = 1,
                            name = create_new_block.Text,
                            is_verified = false
                        };
                    }
                    else
                    {
                        acc = new create_account()
                        {
                            id = all_account.Count + 1,
                            name = create_new_block.Text,
                            is_verified = false
                        };
                    }
                    setting.automatic_account auto_acc = new setting.automatic_account()
                    {
                        id = acc.id,
                        name = acc.name,
                        is_enabled = true
                    };
                    setting.manual_account manual_acc = new setting.manual_account()
                    {
                        id = acc.id,
                        id_last = acc.id,
                        name = acc.name,
                        is_enabled = true,
                        is_all_email = false
                    };
                    setting.create_setting all_sett = setting.setting_Sql.Get_setting();
                    all_sett.auto.Add(auto_acc);
                    all_sett.manual.Add(manual_acc);
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText =
                        @"
                        UPDATE settings
                        SET auto = $auto, manual = $manual
                    ";
                        command.Parameters.AddWithValue("$auto", JsonConvert.SerializeObject(all_sett.auto));
                        command.Parameters.AddWithValue("$manual", JsonConvert.SerializeObject(all_sett.manual));
                        command.ExecuteNonQuery();

                    }
                    all_account.Add(acc);
                    Debug.WriteLine(JsonConvert.SerializeObject(all_account));
                    var collectionViewSource = (CollectionViewSource)Resources["account_list"];
                    collectionViewSource.Source = all_account;
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
            }
            else
            {
                List<create_account> all_account = SqliteFunctions.SelectAllRows();
                if (all_account.Count == 1)
                {
                    if (e.Key == VirtualKey.Enter)
                    {
                        ShowProFeaturesDialog();
                    }
                }
                else
                {
                    if (e.Key == VirtualKey.Enter)
                    {


                        Debug.WriteLine(all_account.Count);
                        create_account acc = new create_account();
                        if (all_account.Count == 0)
                        {
                            acc = new create_account()
                            {
                                id = 1,
                                name = create_new_block.Text,
                                is_verified = false
                            };
                        }
                        else
                        {
                            acc = new create_account()
                            {
                                id = all_account.Count + 1,
                                name = create_new_block.Text,
                                is_verified = false
                            };
                        }
                        setting.automatic_account auto_acc = new setting.automatic_account()
                        {
                            id = acc.id,
                            name = acc.name,
                            is_enabled = true
                        };
                        setting.manual_account manual_acc = new setting.manual_account()
                        {
                            id = acc.id,
                            id_last = acc.id,
                            name = acc.name,
                            is_enabled = true,
                            is_all_email = false
                        };
                        setting.create_setting all_sett = setting.setting_Sql.Get_setting();
                        all_sett.auto.Add(auto_acc);
                        all_sett.manual.Add(manual_acc);
                        using (var connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();

                            var command = connection.CreateCommand();
                            command.CommandText =
                            @"
                        UPDATE settings
                        SET auto = $auto, manual = $manual
                    ";
                            command.Parameters.AddWithValue("$auto", JsonConvert.SerializeObject(all_sett.auto));
                            command.Parameters.AddWithValue("$manual", JsonConvert.SerializeObject(all_sett.manual));
                            command.ExecuteNonQuery();

                        }
                        all_account.Add(acc);
                        Debug.WriteLine(JsonConvert.SerializeObject(all_account));
                        var collectionViewSource = (CollectionViewSource)Resources["account_list"];
                        collectionViewSource.Source = all_account;
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
                }
                
            }
            
        }
        private void create_new_block_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void account_right_click(object sender, RightTappedRoutedEventArgs e)
        {
            MenuFlyout myFlyout = new MenuFlyout();
            MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("delete_button") };
            firstItem.Icon = new SymbolIcon(Symbol.Delete);
            firstItem.Foreground = new SolidColorBrush(Colors.Red);
            firstItem.CornerRadius = new CornerRadius(5);
            Grid grid = (Grid)sender;
            firstItem.Click += (s, args) => dele_text_block(grid, args);
            MenuFlyoutItem secondItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("copy_button") };
            secondItem.Icon = new SymbolIcon(Symbol.Copy);
            secondItem.CornerRadius = new CornerRadius(5);
            secondItem.Click += (s, args) => copy_text_block(grid, args);
            MenuFlyoutItem thirdItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("rename_button") };
            thirdItem.Icon = new SymbolIcon(Symbol.Rename);
            thirdItem.CornerRadius = new CornerRadius(5);
            thirdItem.Click += rename_text_block;
            myFlyout.Items.Add(firstItem);
            myFlyout.Items.Add(secondItem);
            myFlyout.Items.Add(thirdItem);
            FrameworkElement senderElement = sender as FrameworkElement;
            myFlyout.ShowAt(senderElement);
        }
        private void dele_text_block(object sender, RoutedEventArgs e)
        {
            Grid grid = (Grid)sender;


            int index = (int)grid.Tag;
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            List<create_account> all_account = SqliteFunctions.SelectAllRows();
            Debug.WriteLine(JsonConvert.SerializeObject(all_account));
            all_account.RemoveAll(item=>item.id == index);
            Debug.WriteLine(JsonConvert.SerializeObject(all_account));
            for (int i = 0; i < all_account.Count; i++)
            {
                Debug.WriteLine(i+" - перемещённые id");
                all_account[i].id = i+1;
            }
            Debug.WriteLine(JsonConvert.SerializeObject(all_account));
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM accounts";
                command.ExecuteNonQuery();
                foreach (create_account acc in all_account)
                {

                    SqliteFunctions.Add_Account(acc.name, acc.email, acc.app_password, acc.id, acc.is_verified);
                }

            }

            Debug.WriteLine(index);

            var collectionViewSource = (CollectionViewSource)Resources["account_list"];
            collectionViewSource.Source = all_account;
            reload_data_Click();
            //AddUpdateAppSettings("colvo_account_block", (text_panel.Children.Count - 1).ToString());


            //if (text_panel.Children.Count == 0)
            //{
            //    account_oversize.Visibility = Visibility.Visible;
            //}
        }

        private void copy_text_block(object sender, RoutedEventArgs e)
        {
            Grid grid = (Grid)sender;


            int index = (int)grid.Tag;
            List<create_account> pre_copy = new List<create_account>();
            pre_copy.Add(SqliteFunctions.SelectAllRows().FirstOrDefault(x => x.id == index));
            var copy_text = new export() { 
                type = "account",
                data = new List<object>(pre_copy)
            };

            ClipboardService.SetText(JsonConvert.SerializeObject(copy_text));
        }

        private void rename_text_block(object sender, RoutedEventArgs e)
        {
            // Code to execute when the third item is clicked
        }

        private void account_content_PaneOpened(SplitView sender, object args)
        {

        }

        private void account_content_PaneClosed(SplitView sender, object args)
        {

        }
        private void open_account(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Grid grid = button.Parent as Grid;
            open_account_id = (int)grid.Tag;
            account_text_box = (TextBlock)grid.FindName("account_name");
            TextBlock textBlock = (TextBlock)grid.FindName("account_name");
            Debug.WriteLine((int)grid.Tag + "--- id");
            Page page = new account_content((int)grid.Tag, textBlock);
            account_content.IsPaneOpen = true;
            account_content_frame.Content = page;
        }
        private void DoSomeWork(string all_acc)
        {
            // Создать уведомление о том, что фоновая задача запущена
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode(all_acc));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        private async void SaveDataToJsonFile(object data)
        {
            // Преобразование данных в JSON
            string json = JsonConvert.SerializeObject(data);
            
            // Создание и настройка FileSavePicker
            FileSavePicker savePicker = new FileSavePicker();
            var window = new Microsoft.UI.Xaml.Window();

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON File", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "NewData";

            // Показ диалога пользователю и получение файла для сохранения
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Предотвращение проблем с доступом к файлу
                CachedFileManager.DeferUpdates(file);

                // Запись данных в файл
                await FileIO.WriteTextAsync(file, json);

                // Завершение сохранения файла
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    DoSomeWork("Файл сохранен");
                }
                else
                {
                    DoSomeWork("Ошибка сохранения");
                }
            }
        }
        

        private void import_file_Click(object sender, RoutedEventArgs e)
        {
            account_listView.SelectionMode = ListViewSelectionMode.Multiple;
            if (account_listView.SelectedItems.Count > 0)
            {
                var all_folder = SqliteFunctions.SelectAllRows();
                List<create_account> result = new List<create_account>();
                foreach (create_account create_Folder in account_listView.SelectedItems)
                {
                    result.Add(all_folder.FirstOrDefault(x => x.id == create_Folder.id));
                }
                var export_object = new export()
                {
                    type = "account",
                    data = new List<Object>(result)
                };
                SaveDataToJsonFile(export_object);
            }
        }

        public void SaveToCache(string key, object value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
        }
        public void RemoveItemsNotInSecondList_manual(List<manual_account> firstList, List<create_account> secondList)
        {
            firstList.RemoveAll(item => !secondList.Any(x => x.id == item.id_last));
        }
        public List<create_account> FindElementsNotInFirstList_manual(List<manual_account> firstList, List<create_account> secondList)
        {
            HashSet<int> firstListIds = new HashSet<int>(firstList.Select(x => x.id_last));
            List<create_account> notInFirstList = secondList.Where(x => !firstListIds.Contains(x.id)).ToList();

            return notInFirstList;
        }

        public void RemoveItemsNotInSecondList(List<automatic_account> firstList, List<create_account> secondList)
        {
            firstList.RemoveAll(item => !secondList.Any(x => x.id == item.id));
        }
        public List<create_account> FindElementsNotInFirstList(List<automatic_account> firstList, List<create_account> secondList)
        {
            HashSet<int> firstListIds = new HashSet<int>(firstList.Select(x => x.id));
            List<create_account> notInFirstList = secondList.Where(x => !firstListIds.Contains(x.id)).ToList();

            return notInFirstList;
        }
        private void reload_data_Click()
        {
            var type_setting = "auto";
            if (GetFromCache<string>("type_setting") != null && GetFromCache<string>("type_setting") != "[]" && GetFromCache<string>("type_setting") != "")
            {
                type_setting = GetFromCache<string>("type_setting");
            }
            if (type_setting == "auto")
            {
                var all_account = SqliteFunctions.SelectAllRows();
                List<automatic_account> accounts = new List<automatic_account>();
                if (GetFromCache<string>("auto") != null && GetFromCache<string>("auto") != "[]" && GetFromCache<string>("auto") != "")
                {
                    List<automatic_account> auto = JsonConvert.DeserializeObject<List<automatic_account>>(GetFromCache<string>("auto"));
                    accounts = auto;
                    RemoveItemsNotInSecondList(auto, all_account);
                    var all_non_detected = FindElementsNotInFirstList(auto, all_account);
                    foreach (var item in all_non_detected)
                    {
                        if (item.is_verified)
                        {
                            automatic_account auto_item = new automatic_account()
                            {
                                id = item.id,
                                is_enabled = true,
                                name = item.name,

                            };
                            accounts.Add(auto_item);
                        }

                    }
                    foreach (var at in accounts)
                    {
                        create_account selected = all_account.FirstOrDefault(x => x.id == at.id);
                        if (selected.name != at.name)
                        {
                            at.name = selected.name;
                        }
                    }
                    //foreach (var account in all_account)
                    //{
                    //    automatic_account selected_auto = auto.FirstOrDefault(x => x.id == account.id);
                    //    automatic_account auto1 = new automatic_account() { id = account.id, is_enabled = selected_auto.is_enabled, name = account.name };
                    //    accounts.Add(auto1);
                    //}
                }
                else
                {
                    all_account = SqliteFunctions.SelectAllRows();
                    if (all_account != null)
                    {
                        foreach (var account in all_account)
                        {
                            if (account.is_verified == true)
                            {
                                automatic_account auto = new automatic_account() { id = account.id, is_enabled = true, name = account.name };
                                accounts.Add(auto);
                            }

                        }
                    }
                }
                SaveToCache("auto", JsonConvert.SerializeObject(accounts));


            }
            else if (type_setting == "manual")
            {
                var all_account = SqliteFunctions.SelectAllRows();
                List<manual_account> accounts = new List<manual_account>();
                if (GetFromCache<string>("manual") != null && GetFromCache<string>("manual") != "[]" && GetFromCache<string>("manual") != "")
                {
                    List<manual_account> auto = JsonConvert.DeserializeObject<List<manual_account>>(GetFromCache<string>("manual"));
                    accounts = auto;
                    RemoveItemsNotInSecondList_manual(auto, all_account);
                    var all_non_detected = FindElementsNotInFirstList_manual(auto, all_account);
                    foreach (var item in all_non_detected)
                    {
                        if (item.is_verified)
                        {
                            manual_account auto_item = new manual_account()
                            {
                                id = item.id,
                                id_last = item.id,
                                is_enabled = true,
                                name = item.name,

                            };
                            accounts.Add(auto_item);
                        }

                    }
                    foreach (var at in accounts)
                    {
                        create_account selected = all_account.FirstOrDefault(x => x.id == at.id_last);
                        if (selected.name != at.name)
                        {
                            at.name = selected.name;
                        }
                    }
                }
                else
                {
                    all_account = SqliteFunctions.SelectAllRows();
                    if (all_account != null)
                    {
                        foreach (var account in all_account)
                        {
                            if (account.is_verified)
                            {
                                manual_account auto = new manual_account() { id = account.id, id_last = account.id, is_enabled = true, name = account.name };
                                accounts.Add(auto);
                            }

                        }
                    }
                }
                SaveToCache("manual", JsonConvert.SerializeObject(accounts));

            }
        }
        private async void export_file_Click(object sender, RoutedEventArgs e)
        {
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
            var selected_shoped = App.GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);

            if (selected_shoped != null)
            {
                // Создание и настройка FileOpenPicker
                FileOpenPicker openPicker = new FileOpenPicker();
                var window = new Microsoft.UI.Xaml.Window();

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                // Initialize the file picker with the window handle (HWND).
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".json");

                // Показ диалога пользователю и получение файла для загрузки
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    // Чтение текста из файла
                    string json = await FileIO.ReadTextAsync(file);

                    try
                    {
                        var export_object = JsonConvert.DeserializeObject<export>(json);
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

                                var collectionViewSource = (CollectionViewSource)Resources["account_list"];
                                collectionViewSource.Source = SqliteFunctions.SelectAllRows();
                                reload_data_Click();
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                List<create_account> all_account = SqliteFunctions.SelectAllRows();
                if (all_account.Count == 1)
                {
                    ShowProFeaturesDialog();
                }
                else if(all_account.Count == 0){
                    // Создание и настройка FileOpenPicker
                    FileOpenPicker openPicker = new FileOpenPicker();
                    var window = new Microsoft.UI.Xaml.Window();

                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                    // Initialize the file picker with the window handle (HWND).
                    WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
                    openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    openPicker.FileTypeFilter.Add(".json");

                    // Показ диалога пользователю и получение файла для загрузки
                    StorageFile file = await openPicker.PickSingleFileAsync();
                    if (file != null)
                    {
                        // Чтение текста из файла
                        string json = await FileIO.ReadTextAsync(file);

                        try
                        {
                            var export_object = JsonConvert.DeserializeObject<export>(json);
                            if (export_object.type == "account")
                            {
                                var all_folder = SqliteFunctions.SelectAllRows();
                                List<create_account> all_acc = JsonConvert.DeserializeObject<List<create_account>>(JsonConvert.SerializeObject(export_object.data));
                                if (all_acc.Count > 1)
                                {
                                    ShowProFeaturesDialog();
                                }
                                else
                                {
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

                                        var collectionViewSource = (CollectionViewSource)Resources["account_list"];
                                        collectionViewSource.Source = SqliteFunctions.SelectAllRows();
                                    }
                                }
                                
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private void open_account_page_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem el = (NavigationViewItem)open_account_page.SelectedItem;
            Debug.WriteLine(el.Name + " - hfjghdfkg");
            if (el.Name == "manual_open")
            {
                Page page = new account_content(open_account_id, account_text_box);
                account_content.IsPaneOpen = true;
                account_content_frame.Content = page;
            }
            else
            {
                Page page = new account_content_auto(open_account_id, account_text_box);
                account_content.IsPaneOpen = true;
                account_content_frame.Content = page;
            }
        }
    }
}
