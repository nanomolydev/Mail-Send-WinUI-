using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Mail_Send.Views;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using TextCopy;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI.Notifications;
using static Mail_Send.account;
using static Mail_Send.App;
using static Mail_Send.email;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class text : Page
{
    private static string connectionString = "Filename==database.db";
    public text()
    {
        
        this.InitializeComponent();
        scan_clipboard scan = new scan_clipboard();
        if (scan.ProcessClipboardContent("text"))
        {
            ((App)Microsoft.UI.Xaml.Application.Current).generate_import_bar(save_bar, save_transform, (CollectionViewSource)Resources["text_list"]);
        }
        else if (scan.ProcessClipboardContent("folder"))
        {
            ((App)Microsoft.UI.Xaml.Application.Current).generate_import_bar(save_bar, save_transform, (CollectionViewSource)Resources["folder_list"]);
        }
        SqliteFunctions.CreateTable();
        Text_Sql.CreateFolderBs();
        List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
        collectionViewSource.Source = all_text;
        Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
        List<Create_folder> all_folder = Text_Sql.Get_Folder();
        var collectionViewSource1 = (CollectionViewSource)Resources["folder_list"];
        collectionViewSource1.Source = all_folder;


    }
    public class export
    {
        public string type
        {
        get; set; }
        public List<object> data
        {
            get; set;
        }
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

        text_view.SelectionMode = ListViewSelectionMode.Multiple;
        folder_view.SelectionMode = ListViewSelectionMode.Multiple;
        if (text_view.SelectedItems.Count > 0)
        {
            folder_view.SelectionMode = ListViewSelectionMode.None;
            var all_folder = Text_Sql.Get_Texts();
            List<CreateNewTextBlock> result = new List<CreateNewTextBlock>();
            foreach (CreateNewTextBlock create_Folder in text_view.SelectedItems)
            {
                result.Add(all_folder.FirstOrDefault(x => x.id == create_Folder.id));
            }
            var export_object = new export()
            {
                type = "text",
                data = new List<Object>(result)
            };
            SaveDataToJsonFile(export_object);
        }
        else if(folder_view.SelectedItems.Count > 0)
        {
            text_view.SelectionMode = ListViewSelectionMode.None;
            
            var all_folder = Text_Sql.Get_Folder();
            List<Create_folder> result = new List<Create_folder>();
            foreach(Create_folder create_Folder in folder_view.SelectedItems)
            {
                result.Add(all_folder.FirstOrDefault(x=>x.id== create_Folder.id));
            }
            var export_object = new export()
            {
                type = "folder",
                data = new List<Object>(result)
            };
            SaveDataToJsonFile(export_object);
        }

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
    private async void export_file_Click(object sender, RoutedEventArgs e)
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
                if (export_object.type == "folder")
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

                        var collectionViewSource = (CollectionViewSource)Resources["folder_list"];
                        collectionViewSource.Source = Text_Sql.Get_Folder();
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

                        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
                        collectionViewSource.Source = Text_Sql.Get_Texts();
                    }
                }
            }
            catch
            {

            }
            
        }
    }
    public class Create_folder
    {
        public Int64 id
        {
            get; set;
        }
        public string name
        {
            get; set;
        }
        public List<CreateNewTextBlock> texts
        {
            get; set;
        }

    }
    public class CreateNewTextBlock
    {
        public Int64 id
        {
            get; set;
        }
        public string topic
        {
            get; set;
        }
        public string text
        {
            get; set;
        }
        public List<CreateNewTextBlock.create_file> files
        {
            get; set;
        }
        public class create_file
        {
            public int id
            {
                get; set;
            }
            public string name
            {
                get; set;
            }
            public string size
            {
                get; set;
            }
            public string path
            {
                get; set;
            }
        }
        // Добавьте здесь другие свойства
    }
    public static class Text_Sql
    {
        public static void CreateFolderBs()
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
                        CREATE TABLE IF NOT EXISTS text_folders (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NULL,
                            texts TEXT NULL
                        );
                    ";
                command.ExecuteNonQuery();

            }
        }
        public static void Add_Text(string? topic = null, string? text = null, string? files = null, Int64? id = null)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                Debug.WriteLine(id);
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                        INSERT INTO texts (id, topic, text, files)
                        VALUES ($id, $topic, $text, $files)
                    ";
                command.Parameters.AddWithValue("$id", id.ToString());
                command.Parameters.AddWithValue("$topic", string.IsNullOrEmpty(topic) ? "" : topic ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$text", string.IsNullOrEmpty(text) ? "" : text ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$files", string.IsNullOrEmpty(files) ? "" : files ?? (object)DBNull.Value);

                 command.ExecuteNonQuery();
            }
        }
        public static void Add_folder(string name, List<CreateNewTextBlock> texts, int id)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                        INSERT INTO text_folders (id,name, texts)
                        VALUES ($id,$name, $texts)
                    ";
                command.Parameters.AddWithValue("$id", id);
                command.Parameters.AddWithValue("$name", name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$texts", JsonConvert.SerializeObject(texts, Formatting.Indented));

                command.ExecuteNonQuery();
            }
        }
        public static List<Create_folder> Get_Folder()
        {
            var result = new List<Create_folder>();
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
                        FROM text_folders
                    ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        var folder = new Create_folder
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            name = reader.GetString(reader.GetOrdinal("name"))

                        };
                        Debug.WriteLine(reader.GetString(reader.GetOrdinal("texts")));
                        List<CreateNewTextBlock> data_text = JsonConvert.DeserializeObject<List<CreateNewTextBlock>>(reader.GetString(reader.GetOrdinal("texts")));
                        folder.texts = data_text;

                        result.Add(folder);
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
                        List<CreateNewTextBlock.create_file> data_files = new List<CreateNewTextBlock.create_file> { };
                        if (reader.IsDBNull(3))
                        {
                            

                        }
                        else
                        {
                            data_files = JsonConvert.DeserializeObject<List<CreateNewTextBlock.create_file>>(reader.GetString(reader.GetOrdinal("files")));
                        }
                         
                        text.files = data_files;
                        result.Add(text);
                        // Добавьте объект text в список или используйте его по своему усмотрению
                    }
                }
            }

            return result;
        }

    }

    private void folder_rigth_tapped(object sender, RightTappedRoutedEventArgs e)
    {

    }
    private void account_rigth_tapped(object sender, RightTappedRoutedEventArgs e)
    {

    }

    private void create_new_block_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
            CreateNewTextBlock acc = new CreateNewTextBlock();
            if (all_text.Count == 0)
            {
                acc = new CreateNewTextBlock()
                {
                    id = 1,
                    topic = create_new_block.Text,
                    text = "",
                    files = null
                };
            }
            else
            {
                acc = new CreateNewTextBlock()
                {
                    id = all_text.Count + 1,
                    topic = create_new_block.Text,
                    text = "",
                    files = null
                };
            }
            all_text.Add(acc);
            Debug.WriteLine(JsonConvert.SerializeObject(all_text));
            var collectionViewSource = (CollectionViewSource)Resources["text_list"];
            collectionViewSource.Source = all_text;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM texts";
                command.ExecuteNonQuery();
                foreach (CreateNewTextBlock acc1 in all_text)
                {

                    Text_Sql.Add_Text(acc1.topic, acc1.text, JsonConvert.SerializeObject(acc1.files), (Int64)acc1.id);
                }

            }
        }
    }

    private void create_new_block_TextChanged(object sender, TextChangedEventArgs e)
    {

    }

    private void account_content_PaneOpened(SplitView sender, object args)
    {

    }

    private void account_content_PaneClosed(SplitView sender, object args)
    {

    }

    private void open_folder(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        Grid grid = button.Parent as Grid;
        TextBlock textBlock = (TextBlock)grid.FindName("folder_name");
        Debug.WriteLine((Int64)grid.Tag + "--- id");
        Page page = new folder_content((Int64)grid.Tag, textBlock, (CollectionViewSource)Resources["text_list"]);
        text_content.IsPaneOpen = true;
        text_content_frame.Content = page;
    }
    private void open_text(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        Grid grid = button.Parent as Grid;
        Debug.WriteLine((Int64)grid.Tag + "--- id");
        TextBlock textBlock = (TextBlock)grid.FindName("text_name");
        Page page = new text_content((Int64)grid.Tag, textBlock);
        text_content.IsPaneOpen = true;
        text_content_frame.Content = page;
    }

    private void text_content_PaneOpened(SplitView sender, object args)
    {

    }

    private void text_content_PaneClosed(SplitView sender, object args)
    {

    }

    private void Button_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {

    }

    private async void text_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        MenuFlyout menuFlyout = new MenuFlyout();

        var deleteItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("delete_button"), Icon = new SymbolIcon(Symbol.Delete) };
        Grid grid = (Grid)sender;
        var index_text_block = (Int64)grid.Tag;
        Debug.WriteLine(index_text_block + " - индекс текстового блока с которого мы нажали");
        deleteItem.Click += (s, args) => dele_text_block(grid, args);
        deleteItem.Foreground = new SolidColorBrush(Colors.Red);
        menuFlyout.Items.Add(deleteItem);

        var renameItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("rename_button"), Icon = new SymbolIcon(Symbol.Rename) };
        menuFlyout.Items.Add(renameItem);

        var copyItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("copy_button"), Icon = new SymbolIcon(Symbol.Copy) };
        copyItem.Click += (s, args) => copy_text_block(grid, args);
        menuFlyout.Items.Add(copyItem);

        var addToFolderItem = new MenuFlyoutSubItem { Text = "Add to Folder", Icon = new SymbolIcon(Symbol.Add) };
        var all_folder = Text_Sql.Get_Folder();
        foreach (var folder in all_folder)
        {
            if (folder.id == 0)
            {

            }
            else
            {
                var new_folder = new MenuFlyoutItem { Text = folder.name };
                new_folder.Click += (s, args) => {
                    var index = addToFolderItem.Items.IndexOf(new_folder) + 1;
                    Debug.WriteLine(index + " - индекс на какую папку нажали");
                    List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
                    for (var i = 1; i <= all_text.Count; i++)
                    {
                        all_text[i - 1].id = i;
                    }
                    List<Create_folder> all_folder = Text_Sql.Get_Folder();
                    Debug.WriteLine(JsonConvert.SerializeObject(all_text)+" - - выооооо");
                    
                    CreateNewTextBlock selected_text = all_text.FirstOrDefault(x => x.id == index_text_block);
                    List<CreateNewTextBlock> last_texts = new List<CreateNewTextBlock>();
                    if (Text_Sql.Get_Folder().FirstOrDefault(x => x.id == index).texts != null)
                    {
                        last_texts = Text_Sql.Get_Folder().FirstOrDefault(x => x.id == index).texts;
                    }
                    last_texts.Add(selected_text);
                    if (all_folder.FirstOrDefault(x => x.id == index).texts==null|| all_folder.FirstOrDefault(x => x.id == index).texts.Count==0)
                    {

                        selected_text.id = 1;
                    }
                    else
                    {
                        
                        selected_text.id = all_folder.FirstOrDefault(x => x.id == index).texts.Count;
                    }

                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();
                        command.CommandText =
                            @"
                                    UPDATE text_folders
                                    SET texts = $texts
                                    WHERE id = $id
                                ";

                        //foreach (var file in data_files)
                        //{
                        //    var fille = new CreateNewTextBlock.create_file
                        //    {
                        //        id = file.id,
                        //        name = file.name,
                        //        size = file.size,
                        //        path = file.path,
                        //    };
                        //    newData.files.Add(fille);

                        //}



                        string updatedJson = JsonConvert.SerializeObject(last_texts);
                        Debug.WriteLine(updatedJson+"- вот он");
                        command.Parameters.AddWithValue("$texts", updatedJson);
                        command.Parameters.AddWithValue("$id", index);
                        command.ExecuteNonQuery();

                    }
                    using (var connection1 = new SqliteConnection(connectionString))
                    {
                        connection1.Open();

                        var command1 = connection1.CreateCommand();
                        command1.CommandText = "DELETE FROM texts WHERE id = @id";
                        command1.Parameters.AddWithValue("@id", index_text_block);
                        command1.ExecuteNonQuery();
                        command1.CommandText = "UPDATE texts SET id = id - 1 WHERE id > @id";
                        command1.ExecuteNonQuery();
                    }
                    Debug.WriteLine((int)index_text_block - 1 + " - - - DDDD");
                    all_text.Remove(selected_text);
                    for (var i = 1; i <= all_text.Count; i++)
                    {
                        all_text[i - 1].id = i;
                    }
                    var collectionViewSource = (CollectionViewSource)Resources["text_list"];
                    collectionViewSource.Source = all_text;

                };
                addToFolderItem.Items.Add(new_folder);

            }


        }
        menuFlyout.Items.Add(addToFolderItem);

        FrameworkElement senderElement = sender as FrameworkElement;
        menuFlyout.ShowAt(senderElement);
    }
    private void dele_text_block(object sender, RoutedEventArgs e)
    {
        Grid grid = (Grid)sender;


        Int64 index = (Int64)grid.Tag;
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
        for (var i = 1; i <=all_text.Count; i++)
        {
            all_text[i - 1].id = i;
        }
        all_text.RemoveAll(item => item.id == index);
        Debug.WriteLine(JsonConvert.SerializeObject(all_text));
        for (int i = 0; i < all_text.Count; i++)
        {
            Debug.WriteLine(i + " - перемещённые id");
            all_text[i].id = i + 1;
        }
        Debug.WriteLine(JsonConvert.SerializeObject(all_text));
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

        Debug.WriteLine(index);

        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
        collectionViewSource.Source = all_text;
        Frame.Navigate(Frame.CurrentSourcePageType);
        Frame.BackStack.Remove(Frame.BackStack.Last());
        //AddUpdateAppSettings("colvo_account_block", (text_panel.Children.Count - 1).ToString());


        //if (text_panel.Children.Count == 0)
        //{
        //    account_oversize.Visibility = Visibility.Visible;
        //}
    }

    private void copy_text_block(object sender, RoutedEventArgs e)
    {
        Grid grid = (Grid)sender;


        Int64 index = (Int64)grid.Tag;
        List<CreateNewTextBlock> pre_copy = new List<CreateNewTextBlock>();
        pre_copy.Add(Text_Sql.Get_Texts().FirstOrDefault(x => x.id == index));
        var copy_text = new export()
        {
            type = "text",
            data = new List<object>(pre_copy)
        };

        ClipboardService.SetText(JsonConvert.SerializeObject(copy_text));
    }
    private void copy_folder_block(object sender, RoutedEventArgs e)
    {
        Grid grid = (Grid)sender;


        Int64 index = (Int64)grid.Tag;
        List<Create_folder> pre_copy = new List<Create_folder>();
        pre_copy.Add(Text_Sql.Get_Folder().FirstOrDefault(x => x.id == index));
        var copy_text = new export()
        {
            type = "folder",
            data = new List<object>(pre_copy)
        };

        ClipboardService.SetText(JsonConvert.SerializeObject(copy_text));
    }
    private void rename_text_block(object sender, RoutedEventArgs e)
    {
        // Code to execute when the third item is clicked
    }
    private ContentDialog dialog;
    private async void create_folder_Click(object sender, RoutedEventArgs e)
    {
        dialog = new ContentDialog
        {
            Title = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("Create_folder"),
            MaxWidth = this.ActualWidth // Установка максимальной ширины диалогового окна
        };
        dialog.XamlRoot = this.XamlRoot;

        // Создание элементов для диалогового окна
        StackPanel panel = new StackPanel();

        TextBox textBox = new TextBox { PlaceholderText = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("write_name_folder") };
        textBox.KeyDown += create_name_folder;
        panel.Children.Add(textBox);

        // Добавление элементов в диалоговое окно
        dialog.Content = panel;

        // Отображение диалогового окна
        await dialog.ShowAsync();
    }
    private void create_name_folder(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            TextBox name = sender as TextBox;
            add_new_folder(name.Text, false);
            dialog.Hide(); // Закрытие диалогового окна
        }
    }
    private void add_new_folder(string name, bool is_not_bs_update)
    {
        List<Create_folder> all_folder = new List<Create_folder>();
        Create_folder new_folder = new Create_folder();
        if (Text_Sql.Get_Folder() != null)
        {
            all_folder = Text_Sql.Get_Folder();
            new_folder = new Create_folder()
            {
                id = Text_Sql.Get_Folder().Count + 1,
                name = name,
                texts = null
            };
            all_folder.Add(new_folder);
            Text_Sql.Add_folder(name, null, Text_Sql.Get_Folder().Count + 1);
        }
        else
        {
            new_folder = new Create_folder()
            {
                id = 1,
                name = name,
                texts = null
            };
            all_folder.Add(new_folder);
            Text_Sql.Add_folder(name, null,1);
        }
        
        var collectionViewSource = (CollectionViewSource)Resources["folder_list"];
        collectionViewSource.Source = all_folder;
    }
    private void dele_folder_block(object sender, RoutedEventArgs e)
    {
        Grid grid = (Grid)sender;


        Int64 index = (Int64)grid.Tag;
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        List<Create_folder> all_folder = Text_Sql.Get_Folder();
        for (var i = 1; i <= all_folder.Count; i++)
        {
            all_folder[i - 1].id = i;
        }
        all_folder.RemoveAll(item => item.id == index);
        Debug.WriteLine(JsonConvert.SerializeObject(all_folder));
        for (int i = 0; i < all_folder.Count; i++)
        {
            Debug.WriteLine(i + " - перемещённые id");
            all_folder[i].id = i + 1;
        }
        Debug.WriteLine(JsonConvert.SerializeObject(all_folder));
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

        }

        Debug.WriteLine(index);

        var collectionViewSource = (CollectionViewSource)Resources["folder_list"];
        collectionViewSource.Source = all_folder;
        Frame.Navigate(Frame.CurrentSourcePageType);
        Frame.BackStack.Remove(Frame.BackStack.Last());
        //AddUpdateAppSettings("colvo_account_block", (text_panel.Children.Count - 1).ToString());


        //if (text_panel.Children.Count == 0)
        //{
        //    account_oversize.Visibility = Visibility.Visible;
        //}
    }
    private async void folder_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        MenuFlyout menuFlyout = new MenuFlyout();

        var deleteItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("delete_button"), Icon = new SymbolIcon(Symbol.Delete) };
        Grid grid = (Grid)sender;
        var index_text_block = (Int64)grid.Tag;
        Debug.WriteLine(index_text_block + " - индекс текстового блока с которого мы нажали");
        deleteItem.Click += (s, args) => dele_folder_block(grid, args);
        deleteItem.Foreground = new SolidColorBrush(Colors.Red);
        menuFlyout.Items.Add(deleteItem);

        var renameItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("rename_button"), Icon = new SymbolIcon(Symbol.Rename) };
        menuFlyout.Items.Add(renameItem);

        var copyItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("copy_button"), Icon = new SymbolIcon(Symbol.Copy) };
        copyItem.Click += (s, args) => copy_folder_block(grid, args);
        menuFlyout.Items.Add(copyItem);



        FrameworkElement senderElement = sender as FrameworkElement;
        menuFlyout.ShowAt(senderElement);
    }

    private void folder_view_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (folder_view.SelectedItems != null && folder_view.SelectedItems.Count > 0)
        {
            text_view.SelectionMode = ListViewSelectionMode.None;
        }
        else
        {
            Console.WriteLine("Список пуст или количество элементов равно 0.");
        }
        
    }

    private void text_view_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (text_view.SelectedItems != null && text_view.SelectedItems.Count > 0)
        {
            folder_view.SelectionMode = ListViewSelectionMode.None;
        }
        else
        {
            Console.WriteLine("Список пуст или количество элементов равно 0.");
        }
    }
}
