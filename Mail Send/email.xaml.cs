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
using Mail_Send.Views;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Newtonsoft.Json;
using static Mail_Send.text;
using System.Diagnostics;
using Windows.Storage;
using Windows.System;
using static Mail_Send.account;
using static Mail_Send.App;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Notifications;
using TextCopy;
using static Mail_Send.setting;
using DeviceId;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class email : Page
    {
        private static string connectionString = "Filename==database.db";
        public email()
        {
            this.InitializeComponent();
            scan_clipboard scan = new scan_clipboard();
            if (scan.ProcessClipboardContent("email"))
            {
                ((App)Microsoft.UI.Xaml.Application.Current).generate_import_bar(save_bar, save_transform, (CollectionViewSource)Resources["email_list"]);
            }
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            SqliteFunctions.CreateTable();
            Text_Sql.CreateFolderBs();

            Email_Sql.CreateBs();
            var all_email = Email_Sql.Get_email();
            var collectionViewSource = (CollectionViewSource)Resources["email_list"];
            collectionViewSource.Source = all_email;
            //foreach (var email in all_email)
            //{
            //    all_email_stack.Children.Add(generate_new_email_block(email.name, false, i));
            //    i++;
            //}
        }


        public class Create_email : home.IHasId
        {
            public Int64 id
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
            public List<Create_folder> folders
            {
                get; set;
            }
            public List<CreateNewTextBlock> texts
            {
                get; set;
            }
            public bool is_random {get; set;}

        }

        public static class Email_Sql
        {
            public static void CreateBs()
            {
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS all_email (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NULL,
                            email TEXT NULL,
                            folders TEXT NULL,
                            texts TEXT NULL,
                            is_random TEXT NULL
                        );
                    ";
                    command.ExecuteNonQuery();

                }
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS past_email (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NULL,
                            email TEXT NULL,
                            folders TEXT NULL,
                            texts TEXT NULL,
                            is_random TEXT NULL
                        );
                    ";
                    command.ExecuteNonQuery();

                }
            }
            public static void Add_past_email(string name, string email, List<Create_folder> folders, List<CreateNewTextBlock> texts, int id, bool is_random)
            {
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO past_email (id, email, name, folders, texts, is_random)
                        VALUES ($id, $email, $name, $folders, $texts, $is_random)
                    ";
                    command.Parameters.AddWithValue("$id", id);
                    command.Parameters.AddWithValue("$email", email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$name", name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$folders", JsonConvert.SerializeObject(folders, Formatting.Indented));
                    command.Parameters.AddWithValue("$texts", JsonConvert.SerializeObject(texts, Formatting.Indented));
                    command.Parameters.AddWithValue("$is_random", is_random.ToString());
                    command.ExecuteNonQuery();
                }
            }
            public static void Add_email(string name, string email, List<Create_folder> folders, List<CreateNewTextBlock> texts, int id, bool is_random)
            {
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO all_email (id, email, name, folders, texts, is_random)
                        VALUES ($id, $email, $name, $folders, $texts, $is_random)
                    ";
                    command.Parameters.AddWithValue("$id", id);
                    command.Parameters.AddWithValue("$email", email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$name", name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$folders", JsonConvert.SerializeObject(folders, Formatting.Indented));
                    command.Parameters.AddWithValue("$texts", JsonConvert.SerializeObject(texts, Formatting.Indented));
                    command.Parameters.AddWithValue("$is_random", is_random.ToString());
                    command.ExecuteNonQuery();
                }
            }
            public static List<Create_email> Get_past_email()
            {
                var result = new List<Create_email>();
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT *
                        FROM past_email
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            var email = new Create_email
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                name = reader.GetString(reader.GetOrdinal("name")),
                                is_random = Boolean.Parse(reader.GetString(reader.GetOrdinal("is_random")))
                            };

                            if (reader.IsDBNull(2))
                            {
                                email.email = null;
                            }
                            else
                            {
                                email.email = reader.GetString(reader.GetOrdinal("email"));
                            }
                            List<Create_folder> data_folders = new List<Create_folder>();
                            List<CreateNewTextBlock> data_texts = new List<CreateNewTextBlock>();
                            if (reader.IsDBNull(3))
                            {


                            }
                            else
                            {
                                data_folders = JsonConvert.DeserializeObject<List<Create_folder>>(reader.GetString(reader.GetOrdinal("folders")));
                            }
                            email.folders = data_folders;
                            if (reader.IsDBNull(4))
                            {


                            }
                            else
                            {
                                data_texts = JsonConvert.DeserializeObject<List<CreateNewTextBlock>>(reader.GetString(reader.GetOrdinal("texts")));
                            }
                            email.texts = data_texts;
                            result.Add(email);


                            // Добавьте объект text в список или используйте его по своему усмотрению
                        }
                    }
                }

                return result;
            }

            public static List<Create_email> Get_email()
            {
                var result = new List<Create_email>();
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT *
                        FROM all_email
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            var email = new Create_email
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                name = reader.GetString(reader.GetOrdinal("name")),
                                is_random = Boolean.Parse(reader.GetString(reader.GetOrdinal("is_random")))
                            };

                            if (reader.IsDBNull(2))
                            {
                                email.email = null;
                            }
                            else
                            {
                                email.email = reader.GetString(reader.GetOrdinal("email"));
                            }
                            List<Create_folder> data_folders = new List<Create_folder>();
                            List<CreateNewTextBlock> data_texts = new List<CreateNewTextBlock>();
                            if (reader.IsDBNull(3))
                            {


                            }
                            else {
                                data_folders = JsonConvert.DeserializeObject<List<Create_folder>>(reader.GetString(reader.GetOrdinal("folders")));
                            }
                            email.folders = data_folders;
                            if (reader.IsDBNull(4))
                            {


                            }
                            else
                            {
                                data_texts = JsonConvert.DeserializeObject<List<CreateNewTextBlock>>(reader.GetString(reader.GetOrdinal("texts")));
                            }
                            email.texts = data_texts;
                            result.Add(email);


                            // Добавьте объект text в список или используйте его по своему усмотрению
                        }
                    }
                }

                return result;
            }
            public static List<CreateNewTextBlock> Get_Texts()
            {
                var result = new List<CreateNewTextBlock>();
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT *
                        FROM texts
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            var text = new CreateNewTextBlock
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                topic = reader.GetString(reader.GetOrdinal("topic")),
                                text = reader.GetString(reader.GetOrdinal("text")),

                            };
                            List<CreateNewTextBlock.create_file> data_files = JsonConvert.DeserializeObject<List<CreateNewTextBlock.create_file>>(reader.GetString(reader.GetOrdinal("files")));
                            text.files = data_files;
                            result.Add(text);
                            // Добавьте объект text в список или используйте его по своему усмотрению
                        }
                    }
                }

                return result;
            }

        }

        private void open_email_content_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TextBlock textBlock = (TextBlock)button.FindName("email_name");
            Page page = new email_content((Int64)button.Tag, textBlock);
            text_content.IsPaneOpen = true;
            text_content_frame.Content = page;
        }

        private void create_new_block_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
            var selected_shoped = App.GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);

            if (selected_shoped != null)
            {
                if (e.Key == VirtualKey.Enter)
                {
                    List<Create_email> all_email = Email_Sql.Get_email();
                    Create_email email = new Create_email();
                    if (all_email.Count == 0)
                    {
                        email = new Create_email()
                        {
                            id = 1,
                            name = create_new_block.Text,
                            texts = null,
                            folders = null
                        };
                    }
                    else
                    {
                        email = new Create_email()
                        {
                            id = all_email.Count + 1,
                            name = create_new_block.Text,
                            texts = null,
                            folders = null
                        };
                    }
                    all_email.Add(email);
                    Debug.WriteLine(JsonConvert.SerializeObject(all_email));
                    var collectionViewSource = (CollectionViewSource)Resources["email_list"];
                    collectionViewSource.Source = all_email;
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
            }
            else
            {
                List<Create_email> all_account = Email_Sql.Get_email();
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
                        List<Create_email> all_email = Email_Sql.Get_email();
                        Create_email email = new Create_email();
                        if (all_email.Count == 0)
                        {
                            email = new Create_email()
                            {
                                id = 1,
                                name = create_new_block.Text,
                                texts = null,
                                folders = null
                            };
                        }
                        else
                        {
                            email = new Create_email()
                            {
                                id = all_email.Count + 1,
                                name = create_new_block.Text,
                                texts = null,
                                folders = null
                            };
                        }
                        all_email.Add(email);
                        Debug.WriteLine(JsonConvert.SerializeObject(all_email));
                        var collectionViewSource = (CollectionViewSource)Resources["email_list"];
                        collectionViewSource.Source = all_email;
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
                        new Image
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

        // Класс для RelayCommand
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
        private void create_new_block_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void dele_text_block(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            long pre_index = (long)button.Tag;
            int index = (int)pre_index;
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            List<Create_email> all_email = Email_Sql.Get_email();
            all_email.RemoveAll(item => item.id == index);
            Debug.WriteLine(JsonConvert.SerializeObject(all_email));
            for (int i = 0; i < all_email.Count; i++)
            {
                Debug.WriteLine(i + " - перемещённые id");
                all_email[i].id = i + 1;
            }
            Debug.WriteLine(JsonConvert.SerializeObject(all_email));
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM all_email";
                command.ExecuteNonQuery();
                foreach (Create_email acc in all_email)
                {

                    Email_Sql.Add_email(acc.name, acc.email, acc.folders, acc.texts, (int)acc.id, false);
                }

            }

            Debug.WriteLine(index);

            var collectionViewSource = (CollectionViewSource)Resources["email_list"];
            collectionViewSource.Source = all_email;

        }
        private void copy_text_block(object sender, RoutedEventArgs e)
        {
            Button grid = (Button)sender;


            Int64 index = (Int64)grid.Tag;
            List<Create_email> pre_copy = new List<Create_email>();
            pre_copy.Add(Email_Sql.Get_email().FirstOrDefault(x => x.id == index));
            var copy_text = new export()
            {
                type = "email",
                data = new List<object>(pre_copy)
            };

            ClipboardService.SetText(JsonConvert.SerializeObject(copy_text));
        }

        private void rename_text_block(object sender, RoutedEventArgs e)
        {
            // Code to execute when the third item is clicked
        }
        private async void open_email_content_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

            MenuFlyout myFlyout = new MenuFlyout();
            MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("delete_button") };
            firstItem.Icon = new SymbolIcon(Symbol.Delete);
            firstItem.Foreground = new SolidColorBrush(Colors.Red);
            firstItem.CornerRadius = new CornerRadius(5);
            Button button = sender as Button;
            firstItem.Click += (s, args) => dele_text_block(button, args);
            MenuFlyoutItem secondItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("copy_button") };
            secondItem.Icon = new SymbolIcon(Symbol.Copy);
            secondItem.CornerRadius = new CornerRadius(5);
            secondItem.Click += (s, args) => copy_text_block(button, args);
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

        private void past_email_open(object sender, RoutedEventArgs e)
        {
            ShellPage.Current.ChangeView(new past_emails().GetType());
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
        private void import_file_Click(object sender, RoutedEventArgs e)
        {
            email_listView.SelectionMode = ListViewSelectionMode.Multiple;
            if (email_listView.SelectedItems.Count > 0)
            {
                var all_folder = Email_Sql.Get_email();
                List<Create_email> result = new List<Create_email>();
                foreach (Create_email create_Folder in email_listView.SelectedItems)
                {
                    result.Add(all_folder.FirstOrDefault(x => x.id == create_Folder.id));
                }
                var export_object = new export()
                {
                    type = "email",
                    data = new List<Object>(result)
                };
                SaveDataToJsonFile(export_object);
            }
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
                        if (export_object.type == "email")
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

                                    Email_Sql.Add_email(acc.name, acc.email, acc.folders, acc.texts, (int)acc.id, acc.is_random);
                                }

                                var collectionViewSource = (CollectionViewSource)Resources["email_list"];
                                collectionViewSource.Source = Email_Sql.Get_email();
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
                List<Create_email> all_account = Email_Sql.Get_email();
                if (all_account.Count == 1)
                {
                    ShowProFeaturesDialog();
                }
                else if(all_account.Count==0)
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
                            if (export_object.type == "email")
                            {
                                var all_folder = Email_Sql.Get_email();
                                List<Create_email> all_acc = JsonConvert.DeserializeObject<List<Create_email>>(JsonConvert.SerializeObject(export_object.data));
                                foreach (Create_email item in all_acc)
                                {
                                    if (ExtractEmails(item.email).Count > 50)
                                    {
                                        all_acc.Remove(item);
                                    }
                                }
                                if (all_acc.Count == 0)
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
                                        command.CommandText = "DELETE FROM all_email";
                                        command.ExecuteNonQuery();
                                        foreach (Create_email acc in all_folder)
                                        {

                                            Email_Sql.Add_email(acc.name, acc.email, acc.folders, acc.texts, (int)acc.id, acc.is_random);
                                        }

                                        var collectionViewSource = (CollectionViewSource)Resources["email_list"];
                                        collectionViewSource.Source = Email_Sql.Get_email();
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
        static void ПереместитьЭлемент(int индекс, int индексПеремещения, List<Create_email> список)
        {
            Create_email элемент = список[индекс];
            список.RemoveAt(индекс);
            список.Insert(индексПеремещения, элемент);
        }
        public void UpdateIds(List<Create_email> list)
        {
            int id = 1;
            foreach (var item in list)
            {
                item.id = id++;
            }
        }
        private void email_listView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var cc = args.Items;
            Create_email lastitem = (Create_email)cc.First();
            int id_to = email_listView.Items.IndexOf(cc.First()) + 1;
            int l = (int)lastitem.id;
            var t = id_to;
            Debug.WriteLine(l);
            Debug.WriteLine(t);
            List<Create_email> all_account = Email_Sql.Get_email();
            ПереместитьЭлемент(l - 1, t - 1, all_account);
            UpdateIds(all_account);

            var collectionViewSource = (CollectionViewSource)Resources["email_list"];
            collectionViewSource.Source = all_account;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM all_email";
                command.ExecuteNonQuery();
                foreach (Create_email acc1 in all_account)
                {

                    Email_Sql.Add_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                }

            }
        }
    }

    
}
