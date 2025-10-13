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
using static Mail_Send.text;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using static Mail_Send.account;
using Windows.Storage;
using Newtonsoft.Json;
using Windows.Storage.Streams;
using System.Text;
using WinRT;
 using Windows.Win32;
using Windows.Storage.Pickers;
using DeviceId;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using OpenAI_API;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.UI.Xaml.Media.Imaging;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class text_content : Page
    {
        Int64 ind = 0;
        string connectionString;
        TextBlock text_textblock;

        public text_content(Int64 index, TextBlock sharedel)
        {
            text_textblock = sharedel;
            ind = index;
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            this.InitializeComponent();
            List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
            CreateNewTextBlock selected_text = all_text.FirstOrDefault(item=>item.id==index);
            List <CreateNewTextBlock.create_file> selected_text_files = selected_text.files;
            Debug.WriteLine(JsonConvert.SerializeObject(selected_text_files));
            topic_name.Text = selected_text.topic;
            text_textbox.Text = selected_text.text;
            var collectionViewSource = (CollectionViewSource)Resources["file_list"];
            collectionViewSource.Source = selected_text_files;

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void topic_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock topic_name_ready = text_textblock;
            topic_name_ready.Text = topic_name.Text;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        UPDATE texts
                        SET topic = $topic
                        WHERE id = $id
                    ";
                command.Parameters.AddWithValue("$topic", topic_name.Text);
                command.Parameters.AddWithValue("$id", ind);
                command.ExecuteNonQuery();
            }
        }

        private void text_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
            CreateNewTextBlock text = all_text.FirstOrDefault(item => item.id == ind);
            text.text = text_textbox.Text;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM texts";
                command.ExecuteNonQuery();
                foreach (CreateNewTextBlock text1 in all_text)
                {

                    Text_Sql.Add_Text(text1.topic, text1.text, JsonConvert.SerializeObject(text1.files), text1.id);
                }
            }
        }
        private string FormatSize(ulong size)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double sizeInDouble = size;

            while (sizeInDouble >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                sizeInDouble /= 1024;
                suffixIndex++;
            }

            return $"{sizeInDouble:N2} {suffixes[suffixIndex]}";
        }

        private async void add_new_file2(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario


            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.



            // Initialize the file picker with the window handle (HWND).


            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();

        }
        private async void add_new_file(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();

            var window = new Microsoft.UI.Xaml.Window();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add("*");

            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                var all_text = Text_Sql.Get_Texts();
                List<CreateNewTextBlock.create_file> data = new List<CreateNewTextBlock.create_file>();
                if(all_text!=null)
                {
                    data = all_text.FirstOrDefault(x => x.id == ind).files!=null ? all_text.FirstOrDefault(x => x.id == ind).files : new List<CreateNewTextBlock.create_file>();
                }
                foreach (StorageFile file in files)
                {
                    StorageFile copiedFile = await file.CopyAsync(localFolder, file.Name, NameCollisionOption.ReplaceExisting);
                    ulong size;
                    using (IRandomAccessStream stream = await copiedFile.OpenAsync(FileAccessMode.Read))
                    {
                        size = stream.Size;
                    }

                    string sizeString = FormatSize(size);
                    
                    CreateNewTextBlock.create_file newData;

                    newData = new CreateNewTextBlock.create_file
                    {
                        id = data.Count + 1,
                        name = copiedFile.Name,
                        size = sizeString,
                        path = copiedFile.Path,
                    };

                    data.Add(newData);
                    string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                    string connectionString = $"Filename={dbpath}";
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText =
                            @"
                    UPDATE texts
                    SET files = $files
                    WHERE id = $id
                    ";
                        command.Parameters.AddWithValue("$files", json);
                        command.Parameters.AddWithValue("$id", ind);
                        command.ExecuteNonQuery();
                    }

                    
                }
                var collectionViewSource = (CollectionViewSource)Resources["file_list"];
                collectionViewSource.Source = data;
            }
        }

        //private async void add_new_file(object sender, RoutedEventArgs e)
        //{
        //    // Clear previous returned file name, if it exists, between iterations of this scenario


        //    // Create a file picker
        //    var openPicker = new FileOpenPicker();

        //    // Retrieve the window handle (HWND) of the current WinUI 3 window.

        //    var window = new Microsoft.UI.Xaml.Window();

        //    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        //    // Initialize the file picker with the window handle (HWND).
        //    WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        //    // Set options for your file picker
        //    openPicker.ViewMode = PickerViewMode.List;
        //    openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        //    openPicker.FileTypeFilter.Add("*");

        //    // Open the picker for the user to pick a file
        //    IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
        //    if (files.Count > 0)
        //    {
        //        foreach (StorageFile file in files)
        //        {
        //            int index_stackpanel = files_item.Children.Count;
        //            List<CreateNewTextBlock.create_file> data = new List<CreateNewTextBlock.create_file>();

        //            List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
        //            foreach (var text in all_text)
        //            {
        //                if (text.id == ind)
        //                {
        //                    if (text.files == null)
        //                    {
        //                    }
        //                    else {
        //                        data = text.files;
        //                    }

        //                }
        //            }
        //            // Save file details
        //            ulong size;
        //            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
        //            {
        //                size = stream.Size;
        //            }

        //            string sizeString = FormatSize(size);
        //            CreateNewTextBlock.create_file newData = new CreateNewTextBlock.create_file();
        //            if (data == null)
        //            {
        //                newData = new CreateNewTextBlock.create_file
        //                {
        //                    id = 1,
        //                    name = file.Name,
        //                    size = sizeString,
        //                    path = file.Path,
        //                    // Установите здесь другие свойства
        //                };
        //            }
        //            else
        //            {
        //                newData = new CreateNewTextBlock.create_file
        //                {
        //                    id = data.Count+1,
        //                    name = file.Name,
        //                    size = sizeString,
        //                    path = file.Path,
        //                    // Установите здесь другие свойства
        //                };
        //            }


        //            data.Add(newData);

        //            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        //            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        //            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        //            connectionString = $"Filename={dbpath}";
        //            using (var connection = new SqliteConnection(connectionString))
        //            {
        //                connection.Open();

        //                var command = connection.CreateCommand();
        //                command.CommandText =
        //                    @"
        //                UPDATE texts
        //                SET files = $files
        //                WHERE id = $id
        //            ";
        //                command.Parameters.AddWithValue("$files", json);
        //                command.Parameters.AddWithValue("$id", ind);
        //                command.ExecuteNonQuery();
        //            }
        //            var collectionViewSource = (CollectionViewSource)Resources["file_list"];
        //            collectionViewSource.Source = data;


        //        }

        //    }

        //}



        private void delete_file_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Grid grid = (Grid)button.Parent;

            int index = (int)grid.Tag;
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();

            var fileToDelete = all_text.FirstOrDefault(item => item.id == ind).files.FirstOrDefault(x => x.id == index);
            if (fileToDelete != null)
            {
                string filePath = fileToDelete.path;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            all_text.FirstOrDefault(item => item.id == ind).files.RemoveAll(item => item.id == index);
            for (int i = 0; i < all_text.FirstOrDefault(item => item.id == ind).files.Count; i++)
            {
                all_text.FirstOrDefault(item => item.id == ind).files[i].id = i + 1;
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM texts";
                command.ExecuteNonQuery();
                foreach (CreateNewTextBlock acc in all_text)
                {
                    Text_Sql.Add_Text(acc.topic, acc.text, JsonConvert.SerializeObject(acc.files), acc.id);
                }
            }

            var collectionViewSource = (CollectionViewSource)Resources["file_list"];
            collectionViewSource.Source = all_text.FirstOrDefault(item => item.id == ind).files;
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
        private async void  generate_email_content_Click(object sender, RoutedEventArgs e)
        {
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
            var selected_shoped = App.GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);

            if (selected_shoped != null)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("ai_assist"),
                    PrimaryButtonText = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("generate"),
                    SecondaryButtonText = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("cancel")
                };
                dialog.XamlRoot = this.XamlRoot;
                ComboBox languageComboBox = new ComboBox
                {
                    Header = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("selected_language"),
                    Name = "LanguageComboBox"
                };
                languageComboBox.Items.Add(new ComboBoxItem { Content = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("ru") });
                languageComboBox.Items.Add(new ComboBoxItem { Content = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("en") });

                TextBox descriptionTextBox = new TextBox
                {
                    PlaceholderText = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("describe_product"),
                    Name = "DescriptionTextBox"
                };

                StackPanel panel = new StackPanel();
                panel.Children.Add(languageComboBox);
                panel.Children.Add(descriptionTextBox);

                dialog.Content = panel;

                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    text_textbox.IsEnabled = false;

                    OpenAIAPI api = new OpenAIAPI("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6Ik1UaEVOVUpHTkVNMVFURTRNMEZCTWpkQ05UZzVNRFUxUlRVd1FVSkRNRU13UmtGRVFrRXpSZyJ9.eyJodHRwczovL2FwaS5vcGVuYWkuY29tL3Byb2ZpbGUiOnsiZW1haWwiOiJhZGFtLmxvbHpvdnZ2KzIzMTIyQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlfSwiaHR0cHM6Ly9hcGkub3BlbmFpLmNvbS9hdXRoIjp7InBvaWQiOiJvcmctbkhsQmkxUWlXSUFJb0JuZmFTVHFrWXdRIiwidXNlcl9pZCI6InVzZXItNGNHbTFwODNzZUNBWHYyMkZUUTVYRGVZIn0sImlzcyI6Imh0dHBzOi8vYXV0aDAub3BlbmFpLmNvbS8iLCJzdWIiOiJhdXRoMHw2NTFkNDBlZGIwNjYzNDgyODkwNmMwNGMiLCJhdWQiOlsiaHR0cHM6Ly9hcGkub3BlbmFpLmNvbS92MSIsImh0dHBzOi8vb3BlbmFpLm9wZW5haS5hdXRoMGFwcC5jb20vdXNlcmluZm8iXSwiaWF0IjoxNzAxMzg0NzMzLCJleHAiOjE3MDIyNDg3MzMsImF6cCI6IlRkSkljYmUxNldvVEh0Tjk1bnl5d2g1RTR5T282SXRHIiwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCBtb2RlbC5yZWFkIG1vZGVsLnJlcXVlc3Qgb3JnYW5pemF0aW9uLnJlYWQgb3JnYW5pemF0aW9uLndyaXRlIG9mZmxpbmVfYWNjZXNzIn0.Rujp_INzXYTxQVxT27_mZJMajAUmVHXVizl-t116DCan3M9-_TuHuLNh0b7aHSSVvhlZ8nT9IWqnz9FvGi0Qwtihsc6mSjaWajdunD1oR6uLuhrjgnVpOpZ7iUOvjmp4OAvTmHiaepqfgDuShiM9y1Jttn2Sht9SqYhTGt0tY3DzwTnzuxf7Lg471XlAYSEjsQJdiZaP7nr64pUtpEN_1XFsXWx_bd8RwEI4kdIT1OwckK16jb89ara2zL7848t8VqI9yT5QZjoVm6l4UfcQYreBNe7boN55KTwd6Qt1d6LSWzF_-s7a4Zp5kdeAGCNOwH71oVAo4dR5jsJel6329Q");
                    var chat = api.Chat.CreateConversation();

                    /// give instruction as System
                    ComboBoxItem item = (ComboBoxItem)languageComboBox.SelectedItem;
                    chat.AppendSystemMessage($"Disregard all previous guidelines.I'd like a response only in {item.Content.ToString()}. Craft a top - notch marketing email for this product or service: {descriptionTextBox.Text} Please avoid echoing my prompt, reminding me of my request, apologizing, or making self-references.\r\nSimply take the best action possible. Every response should be in the {item.Content.ToString()}.\r\nPlease write in  tone,  writing style, using {item.Content.ToString()}.");
                    string response = await chat.GetResponseFromChatbotAsync();
                    if (!response.Contains("No emails") && !response.Contains("(No emails)"))
                    {
                        text_textbox.Text = response;

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
                            UPDATE texts
                            SET text = $text
                            WHERE id = $id
                        ";
                        command.Parameters.AddWithValue("$text", text_textbox.Text);
                        command.Parameters.AddWithValue("$id", ind);
                        command.ExecuteNonQuery();
                    }
                    text_textbox.IsEnabled = true;
                }
                else
                {
                    // Code for when "Отмена" button is pressed or dialog is dismissed
                }
            }
            else
            {
                ShowProFeaturesDialog();
            }
        }
    }
}
