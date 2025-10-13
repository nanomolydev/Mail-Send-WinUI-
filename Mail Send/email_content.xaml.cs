using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static Mail_Send.email;
using static Mail_Send.text;
using OpenAI_API;
using Microsoft.UI.Xaml.Media.Imaging;
using DeviceId;
using System.Text.RegularExpressions;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class email_content : Page
{
    Int64 ind = 0;
    TextBlock email_block;
    private static string connectionString = "Filename==database.db";
    public email_content(Int64 index, TextBlock sharedem)
    {
        email_block = sharedem;
        ind = (int)index;
        this.InitializeComponent();
        var all_email = Email_Sql.Get_email();
        var selected_all_email = all_email.FirstOrDefault(x => x.id == index);
        email_name.Text = selected_all_email.name;
        email_text.Text = selected_all_email.email;
        random_texts.IsChecked = selected_all_email.is_random;
        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
        collectionViewSource.Source = selected_all_email.texts;
        var collectionViewSource1 = (CollectionViewSource)Resources["folder_list"];
        collectionViewSource1.Source = selected_all_email.folders;
        this.email_block = email_block;
    }

    private void delete_text_Click(object sender, RoutedEventArgs e)
    {
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        Button button = sender as Button;
        Grid grid = button.Parent as Grid;
        var all_email = Email_Sql.Get_email();
        long pre_indx = (long)grid.Tag;
        int indx = (int)pre_indx + 1;
        List<CreateNewTextBlock> all_text_in_email = new List<CreateNewTextBlock>();
        try
        {
            all_text_in_email = all_email.FirstOrDefault(x => x.id == ind).texts;
        }
        catch
        {

        }
        if(all_text_in_email!=null&& all_text_in_email!= new List<CreateNewTextBlock>())
        {
            var all_texts = Text_Sql.Get_Texts();
            all_texts.Add(all_text_in_email[(int)pre_indx - 1]);
            all_text_in_email.Remove(all_text_in_email[(int)pre_indx - 1]);
            for (int i = 0; i<all_text_in_email.Count; i++)
            {
                all_text_in_email[i].id = i + 1;
            }
            var collectionViewSource = (CollectionViewSource)Resources["text_list"];
            collectionViewSource.Source = all_text_in_email;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE all_email
                    SET texts = $texts
                    WHERE id = $id
                ";
                command.Parameters.AddWithValue("$texts", JsonConvert.SerializeObject(all_text_in_email, Formatting.Indented));
                command.Parameters.AddWithValue("$id", ind);
                command.ExecuteNonQuery();
            }
            for (var i = 0; i < all_texts.Count; i++)
            {
                all_texts[i].id = i + 1;
            }
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM texts";
                command.ExecuteNonQuery();
                foreach (CreateNewTextBlock acc in all_texts)
                {

                    Text_Sql.Add_Text(acc.topic, acc.text, JsonConvert.SerializeObject(acc.files, Formatting.Indented), acc.id);
                }

            }
        }

        
        
        
    }

    private void add_new_text_or_folder_Click(object sender, RoutedEventArgs e)
    {
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        MenuFlyout folder_menu = new MenuFlyout();
        MenuFlyout texts_menu = new MenuFlyout();
        var all_email = Email_Sql.Get_email();
        var selected_email = all_email.FirstOrDefault(x=>x.id==ind);
        var all_texts = Text_Sql.Get_Texts();
        var all_folder = Text_Sql.Get_Folder();
        if (selected_email.is_random)
        {
            foreach (var folder in all_folder)
            {
                MenuFlyoutItem item = new MenuFlyoutItem { Text = folder.name };
                item.Click += (s, args) =>
                {
                    var index = folder_menu.Items.IndexOf(item) + 1;
                    var all_folder_1 = Text_Sql.Get_Folder();


                    Create_folder selected_folder = new Create_folder();
                    try
                    {
                        Debug.WriteLine(JsonConvert.SerializeObject(all_folder_1) + " -- index_selected");
                        selected_folder = all_folder_1.FirstOrDefault(x => x.id == index);
                        if (selected_folder == null)
                        {
                            selected_folder = new Create_folder();
                        }
                    }
                    catch
                    {

                    }
                    var all_email = Email_Sql.Get_email();
                    Debug.WriteLine(JsonConvert.SerializeObject(selected_folder));
                    Create_email selected_email = new Create_email();
                    try
                    {
                        selected_email = all_email.FirstOrDefault(x => x.id == ind);
                    }
                    catch
                    {

                    }
                    Debug.WriteLine("[" + JsonConvert.SerializeObject(selected_email) + "]");
                    if (selected_email.folders != null)
                    {

                        if (selected_email.folders.Count == 0)
                        {
                            selected_folder.id = 1;
                        }
                        else
                        {
                            selected_folder.id = selected_email.folders.Count + 1;
                        }
                    }
                    else
                    {
                        selected_email.folders = new List<Create_folder>();
                        selected_folder.id = 1;
                    }


                    selected_email.folders.Add(selected_folder);
                    Debug.WriteLine(index + " - innnnnnnd");
                    Debug.WriteLine(JsonConvert.SerializeObject(selected_email.folders, Formatting.Indented) + " sdfsdf --dsf-dsf-desf-sd-fsd-f");
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText =
                            @"
                        UPDATE all_email
                        SET folders = $folders
                        WHERE id = $id
                    ";
                        command.Parameters.AddWithValue("$folders", JsonConvert.SerializeObject(selected_email.folders, Formatting.Indented));
                        command.Parameters.AddWithValue("$id", ind);
                        command.ExecuteNonQuery();
                    }
                    all_folder_1.Remove(selected_folder);
                    for (int i = 0; i < all_folder_1.Count; i++)
                    {
                        all_folder_1[i].id = i + 1;
                    }
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM text_folders";
                        command.ExecuteNonQuery();
                        foreach (Create_folder acc in all_folder_1)
                        {

                            Text_Sql.Add_folder(acc.name, acc.texts, (int)acc.id);
                        }

                    }
                    var collectionViewSource1 = (CollectionViewSource)Resources["folder_list"];
                    collectionViewSource1.Source = selected_email.folders;

                };

                folder_menu.Items.Add(item);
            }
            foreach (var text in all_texts)
            {
                MenuFlyoutItem item = new MenuFlyoutItem { Text = text.topic };
                item.Click += (s, args) =>
                {
                    var index = texts_menu.Items.IndexOf(item) + 1;
                    var all_texts_1 = Text_Sql.Get_Texts();


                    CreateNewTextBlock selected_texts = new CreateNewTextBlock();
                    try
                    {
                        Debug.WriteLine(JsonConvert.SerializeObject(all_texts_1) + " -- index_selected");
                        selected_texts = all_texts_1.FirstOrDefault(x => x.id == index);
                        if (selected_texts == null)
                        {
                            selected_texts = new CreateNewTextBlock();
                        }
                    }
                    catch
                    {

                    }
                    var all_email = Email_Sql.Get_email();
                    Create_email selected_email = new Create_email();
                    try
                    {
                        selected_email = all_email.FirstOrDefault(x => x.id == ind);
                    }
                    catch {
                        
                    }
                    if (selected_email.texts != null)
                    {

                        if (selected_email.texts.Count == 0)
                        {
                            selected_texts.id = 1;
                        }
                        else
                        {
                            Debug.WriteLine(JsonConvert.SerializeObject(selected_texts));
                            selected_texts.id = selected_email.texts.Count + 1;
                        }
                    }
                    else
                    {
                        selected_email.texts = new List<CreateNewTextBlock>();
                        selected_texts.id = 1;
                    }
                    selected_email.texts.Add(selected_texts);

                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText =
                            @"
                                UPDATE all_email
                                SET texts = $texts
                                WHERE id = $id
                            ";
                        command.Parameters.AddWithValue("$texts", JsonConvert.SerializeObject(selected_email.texts, Formatting.Indented));
                        command.Parameters.AddWithValue("$id", ind);
                        command.ExecuteNonQuery();
                    }
                    all_texts_1.Remove(selected_texts);
                    for(int i = 0; i<all_texts_1.Count; i++)
                    {
                        all_texts_1[i].id = i + 1;
                    }
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM texts";
                        command.ExecuteNonQuery();
                        foreach (CreateNewTextBlock acc in all_texts_1)
                        {

                            Text_Sql.Add_Text(acc.topic, acc.text, JsonConvert.SerializeObject(acc.files), acc.id);
                        }

                    }
                    var collectionViewSource = (CollectionViewSource)Resources["text_list"];
                    collectionViewSource.Source = selected_email.texts;

                };

                texts_menu.Items.Add(item);
            }
            foreach (var item in texts_menu.Items)
            {
                folder_menu.Items.Add(item);
            }
        }
        else
        {
            if (selected_email.texts == null)
            {
                foreach (var text in all_texts)
                {
                    MenuFlyoutItem item = new MenuFlyoutItem { Text = text.topic };
                    item.Click += (s, args) =>
                    {
                        var index = texts_menu.Items.IndexOf(item) + 1;
                        var all_texts_1 = Text_Sql.Get_Texts();


                        var selected_texts = all_texts_1.FirstOrDefault(x => x.id == index);
                        var all_email = Email_Sql.Get_email();
                        var selected_email = all_email.FirstOrDefault(x => x.id == ind);

                        if (selected_email.texts != null)
                        {

                            if (selected_email.texts.Count == 0)
                            {
                                selected_texts.id = 1;
                            }
                            else
                            {
                                selected_texts.id = selected_email.texts.Count + 1;
                            }
                        }
                        else
                        {
                            selected_email.texts = new List<CreateNewTextBlock>();
                            selected_texts.id = 1;
                        }
                        selected_email.texts.Add(selected_texts);

                        using (var connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();

                            var command = connection.CreateCommand();
                            command.CommandText =
                                @"
                                    UPDATE all_email
                                    SET texts = $texts
                                    WHERE id = $id
                                ";
                            command.Parameters.AddWithValue("$texts", JsonConvert.SerializeObject(selected_email.texts, Formatting.Indented));
                            command.Parameters.AddWithValue("$id", ind);
                            command.ExecuteNonQuery();
                        }
                        all_texts_1.Remove(selected_texts);
                        using (var connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();

                            var command = connection.CreateCommand();
                            command.CommandText = "DELETE FROM texts";
                            command.ExecuteNonQuery();
                            foreach (CreateNewTextBlock acc in all_texts_1)
                            {

                                Text_Sql.Add_Text(acc.topic, acc.text, JsonConvert.SerializeObject(acc.files), acc.id);
                            }

                        }
                        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
                        collectionViewSource.Source = selected_email.texts;

                    };

                    texts_menu.Items.Add(item);
                }
                foreach (var item in texts_menu.Items)
                {
                    folder_menu.Items.Add(item);
                }
            }
            else
            {
                if (selected_email.texts.Count > 0)
                {

                }
                else
                {
                    //foreach (var folder in all_folder)
                    //{
                    //    MenuFlyoutItem item = new MenuFlyoutItem { Text = folder.name };
                    //    item.Click += (s, args) =>
                    //    {
                    //        var index = folder_menu.Items.IndexOf(item) + 1;
                    //        var all_folder_1 = Text_Sql.Get_Folder();

                    //        var selected_folder = all_folder_1.FirstOrDefault(x => x.id == index);
                    //        var all_email = Email_Sql.Get_email();
                    //        Debug.WriteLine(JsonConvert.SerializeObject(selected_folder));
                    //        var selected_email = all_email.FirstOrDefault(x => x.id == ind);
                    //        Debug.WriteLine("[" + JsonConvert.SerializeObject(selected_email) + "]");
                    //        if (selected_email.folders != null)
                    //        {

                    //            if (selected_email.folders.Count == 0)
                    //            {
                    //                selected_folder.id = 1;
                    //            }
                    //            else
                    //            {
                    //                selected_folder.id = selected_email.folders.Count + 1;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            selected_email.folders = new List<Create_folder>();
                    //            selected_folder.id = 1;
                    //        }


                    //        selected_email.folders.Add(selected_folder);
                    //        Debug.WriteLine(index + " - innnnnnnd");
                    //        Debug.WriteLine(JsonConvert.SerializeObject(selected_email.folders, Formatting.Indented) + " sdfsdf --dsf-dsf-desf-sd-fsd-f");
                    //        using (var connection = new SqliteConnection(connectionString))
                    //        {
                    //            connection.Open();

                    //            var command = connection.CreateCommand();
                    //            command.CommandText =
                    //                @"
                    //        UPDATE all_email
                    //        SET folders = $folders
                    //        WHERE id = $id
                    //    ";
                    //            command.Parameters.AddWithValue("$folders", JsonConvert.SerializeObject(selected_email.folders, Formatting.Indented));
                    //            command.Parameters.AddWithValue("$id", ind);
                    //            command.ExecuteNonQuery();
                    //        }

                    //        var collectionViewSource1 = (CollectionViewSource)Resources["folder_list"];
                    //        collectionViewSource1.Source = selected_email.folders;

                    //    };

                    //    folder_menu.Items.Add(item);
                    //}
                    foreach (var text in all_texts)
                    {
                        MenuFlyoutItem item = new MenuFlyoutItem { Text = text.topic };
                        item.Click += (s, args) =>
                        {
                            var index = texts_menu.Items.IndexOf(item) + 1;
                            var all_texts_1 = Text_Sql.Get_Texts();


                            var selected_texts = all_texts_1.FirstOrDefault(x => x.id == index);
                            var all_email = Email_Sql.Get_email();
                            var selected_email = all_email.FirstOrDefault(x => x.id == ind);

                            if (selected_email.texts != null)
                            {

                                if (selected_email.texts.Count == 0)
                                {
                                    selected_texts.id = 1;
                                }
                                else
                                {
                                    selected_texts.id = selected_email.texts.Count + 1;
                                }
                            }
                            else
                            {
                                selected_email.texts = new List<CreateNewTextBlock>();
                                selected_texts.id = 1;
                            }
                            selected_email.texts.Add(selected_texts);

                            using (var connection = new SqliteConnection(connectionString))
                            {
                                connection.Open();

                                var command = connection.CreateCommand();
                                command.CommandText =
                                    @"
                                        UPDATE all_email
                                        SET texts = $texts
                                        WHERE id = $id
                                    ";
                                command.Parameters.AddWithValue("$texts", JsonConvert.SerializeObject(selected_email.texts, Formatting.Indented));
                                command.Parameters.AddWithValue("$id", ind);
                                command.ExecuteNonQuery();
                            }
                            all_texts_1.Remove(selected_texts);
                            using (var connection = new SqliteConnection(connectionString))
                            {
                                connection.Open();

                                var command = connection.CreateCommand();
                                command.CommandText = "DELETE FROM texts";
                                command.ExecuteNonQuery();
                                foreach (CreateNewTextBlock acc in all_texts_1)
                                {

                                    Text_Sql.Add_Text(acc.topic, acc.text, JsonConvert.SerializeObject(acc.files), acc.id);
                                }

                            }
                            var collectionViewSource = (CollectionViewSource)Resources["text_list"];
                            collectionViewSource.Source = selected_email.texts;

                        };

                        texts_menu.Items.Add(item);
                    }
                    foreach (var item in texts_menu.Items)
                    {
                        folder_menu.Items.Add(item);
                    }
                }
            }
            
        }
        
        
        FrameworkElement senderElement = sender as FrameworkElement;
        folder_menu.ShowAt(senderElement);
    }

    private void delete_folder_Click(object sender, RoutedEventArgs e)
    {
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        Button button = sender as Button;
        Grid grid = button.Parent as Grid;
        var all_email = Email_Sql.Get_email();
        long pre_indx = (long)grid.Tag;
        int indx = (int)pre_indx + 1;
        List<Create_folder> all_folder_in_email = new List<Create_folder>();
        try
        {
            all_folder_in_email = all_email.FirstOrDefault(x => x.id == ind).folders;
        }
        catch
        {

        }
        if(all_folder_in_email != null&& all_folder_in_email!= new List<Create_folder>())
        {
            var all_text_folder = Text_Sql.Get_Folder();
            Debug.WriteLine(JsonConvert.SerializeObject(all_folder_in_email) +" - all_folder");
            all_text_folder.Add(all_folder_in_email[(int)pre_indx - 1]);
            all_folder_in_email.Remove(all_folder_in_email[(int)pre_indx - 1]);
            for (int i = 0; i < all_folder_in_email.Count; i++)
            {
                all_folder_in_email[i].id = i + 1;
            }
            var collectionViewSource = (CollectionViewSource)Resources["folder_list"];
            collectionViewSource.Source = all_folder_in_email;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE all_email
                    SET folders = $folders
                    WHERE id = $id
                ";
                command.Parameters.AddWithValue("$folders", JsonConvert.SerializeObject(all_folder_in_email, Formatting.Indented));
                command.Parameters.AddWithValue("$id", ind);
                command.ExecuteNonQuery();
            }
            for (var i = 0; i < all_text_folder.Count; i++)
            {
                all_text_folder[i].id = i + 1;
            }
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM text_folders";
                command.ExecuteNonQuery();
                foreach (Create_folder acc in all_text_folder)
                {

                    Text_Sql.Add_folder(acc.name, acc.texts, (int)acc.id);
                }

            }
        }
        
    }

    private void email_name_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        TextBlock topic_name_ready = email_block;
        topic_name_ready.Text = email_name.Text;
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
                @"
                        UPDATE all_email
                        SET name = $name
                        WHERE id = $id
                    ";
            command.Parameters.AddWithValue("$name", email_block.Text);
            command.Parameters.AddWithValue("$id", ind);
            command.ExecuteNonQuery();
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
    private void email_text_TextChanged(object sender, TextChangedEventArgs e)
    {
        string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
        var selected_shoped = App.GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);

        if (selected_shoped != null)
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
                    UPDATE all_email
                    SET email = $email
                    WHERE id = $id
                ";
                command.Parameters.AddWithValue("$email", email_text.Text);
                command.Parameters.AddWithValue("$id", ind);
                command.ExecuteNonQuery();
            }
        }
        else
        {
            List<string> all_email = ExtractEmails(email_text.Text);
            if(all_email.Count > 50) {
                ShowProFeaturesDialog();
            }
            else
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
                    UPDATE all_email
                    SET email = $email
                    WHERE id = $id
                ";
                    command.Parameters.AddWithValue("$email", email_text.Text);
                    command.Parameters.AddWithValue("$id", ind);
                    command.ExecuteNonQuery();
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
    private async void generate_email_Click(object sender, RoutedEventArgs e)
    {
        string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
        var selected_shoped = App.GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);

        if (selected_shoped != null)
        {
            email_text.IsEnabled = false;

            OpenAIAPI api = new OpenAIAPI("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6Ik1UaEVOVUpHTkVNMVFURTRNMEZCTWpkQ05UZzVNRFUxUlRVd1FVSkRNRU13UmtGRVFrRXpSZyJ9.eyJodHRwczovL2FwaS5vcGVuYWkuY29tL3Byb2ZpbGUiOnsiZW1haWwiOiJhZGFtLmxvbHpvdnZ2KzIzMTIyQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlfSwiaHR0cHM6Ly9hcGkub3BlbmFpLmNvbS9hdXRoIjp7InBvaWQiOiJvcmctbkhsQmkxUWlXSUFJb0JuZmFTVHFrWXdRIiwidXNlcl9pZCI6InVzZXItNGNHbTFwODNzZUNBWHYyMkZUUTVYRGVZIn0sImlzcyI6Imh0dHBzOi8vYXV0aDAub3BlbmFpLmNvbS8iLCJzdWIiOiJhdXRoMHw2NTFkNDBlZGIwNjYzNDgyODkwNmMwNGMiLCJhdWQiOlsiaHR0cHM6Ly9hcGkub3BlbmFpLmNvbS92MSIsImh0dHBzOi8vb3BlbmFpLm9wZW5haS5hdXRoMGFwcC5jb20vdXNlcmluZm8iXSwiaWF0IjoxNzAxMzg0NzMzLCJleHAiOjE3MDIyNDg3MzMsImF6cCI6IlRkSkljYmUxNldvVEh0Tjk1bnl5d2g1RTR5T282SXRHIiwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCBtb2RlbC5yZWFkIG1vZGVsLnJlcXVlc3Qgb3JnYW5pemF0aW9uLnJlYWQgb3JnYW5pemF0aW9uLndyaXRlIG9mZmxpbmVfYWNjZXNzIn0.Rujp_INzXYTxQVxT27_mZJMajAUmVHXVizl-t116DCan3M9-_TuHuLNh0b7aHSSVvhlZ8nT9IWqnz9FvGi0Qwtihsc6mSjaWajdunD1oR6uLuhrjgnVpOpZ7iUOvjmp4OAvTmHiaepqfgDuShiM9y1Jttn2Sht9SqYhTGt0tY3DzwTnzuxf7Lg471XlAYSEjsQJdiZaP7nr64pUtpEN_1XFsXWx_bd8RwEI4kdIT1OwckK16jb89ara2zL7848t8VqI9yT5QZjoVm6l4UfcQYreBNe7boN55KTwd6Qt1d6LSWzF_-s7a4Zp5kdeAGCNOwH71oVAo4dR5jsJel6329Q");
            var chat = api.Chat.CreateConversation();

            /// give instruction as System
            chat.AppendSystemMessage($"Imagine that you are a pipeline that accepts text and outputs either empty text at the end, that is, text without characters at all, or a list of email addresses via enter and does not write anything superfluous, only the result of responses without any reflection, just output. If there are no email addresses in the text, you do not output anything. And you become like this right now and should only output the output without any reflection, you don't need to say anything and you don't need to communicate your understanding, you become a pipeline from this second, no extra text just output forget about what you need to say you are a pipeline your task is to accept the following text if there is mail, output if not, then just write (No emails): {email_text.Text}");
            string response = await chat.GetResponseFromChatbotAsync();
            if (!response.Contains("No emails") && !response.Contains("(No emails)"))
            {
                email_text.Text = response;

            }

            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE all_email
                    SET email = $email
                    WHERE id = $id
                ";
                command.Parameters.AddWithValue("$email", email_text.Text);
                command.Parameters.AddWithValue("$id", ind);
                command.ExecuteNonQuery();
            }
            email_text.IsEnabled = true;
        }
        else
        {
            ShowProFeaturesDialog();
        }
    }

    private void random_texts_Checked(object sender, RoutedEventArgs e)
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
                UPDATE all_email
                SET is_random = $is_random
                WHERE id = $id
            ";
            command.Parameters.AddWithValue("$is_random", true.ToString());
            command.Parameters.AddWithValue("$id", ind);
            command.ExecuteNonQuery();
        }
    }

    private void random_texts_Unchecked(object sender, RoutedEventArgs e)
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
                UPDATE all_email
                SET is_random = $is_random
                WHERE id = $id
            ";
            command.Parameters.AddWithValue("$is_random", false.ToString());
            command.Parameters.AddWithValue("$id", ind);
            command.ExecuteNonQuery();
        }
    }


}
