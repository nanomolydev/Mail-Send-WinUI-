using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Mail_Send.Views;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
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
using static System.Net.Mime.MediaTypeNames;
using static Mail_Send.text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class folder_content : Page
{
    string connectionString;
    Int64 ind = 0;
    TextBlock folder_textblock;
    CollectionViewSource text_coll;
    public folder_content(Int64 index, TextBlock sharedel, CollectionViewSource text_collection)
    {
        ind = index;
        folder_textblock = sharedel;
        text_coll = text_collection;
        this.InitializeComponent();
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        folder_name.Text = Text_Sql.Get_Folder().FirstOrDefault(x=>x.id== index).name;
        List<Create_folder> all_text = Text_Sql.Get_Folder();
        Create_folder selected_text = all_text.FirstOrDefault(item => item.id == index);
        List<CreateNewTextBlock> selected_text_files = selected_text.texts;
        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
        collectionViewSource.Source = selected_text_files;
    }

    private void delete_texts_Clic(object sender, RoutedEventArgs e)
    {

    }

    private void folder_name_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBlock topic_name_ready = folder_textblock;
        topic_name_ready.Text = folder_name.Text;
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
                @"
                        UPDATE text_folders
                        SET name = $name
                        WHERE id = $id
                    ";
            command.Parameters.AddWithValue("$name", folder_name.Text);
            command.Parameters.AddWithValue("$id", ind);
            command.ExecuteNonQuery();
        }
    }

    private void add_new_folder_Click(object sender, RoutedEventArgs e)
    {
        MenuFlyout menuFlyout = new MenuFlyout();

        var deleteItem = new MenuFlyoutItem { Text = "Delete", Icon = new SymbolIcon(Symbol.Delete) };


        foreach(var text in Text_Sql.Get_Texts())
        {
            var text_el = new MenuFlyoutItem { Text = text.topic, Icon = new SymbolIcon(Symbol.Document) };
            text_el.Click += (s, args) =>
            {
                var index = menuFlyout.Items.IndexOf(text_el) + 1;
                List<CreateNewTextBlock> all_text = Text_Sql.Get_Texts();
                CreateNewTextBlock selected_text = all_text.FirstOrDefault(x=>x.id==index);
                List<CreateNewTextBlock> last_texts = new List<CreateNewTextBlock>();
                if (Text_Sql.Get_Folder().FirstOrDefault(x => x.id == ind).texts != null)
                {
                    last_texts = Text_Sql.Get_Folder().FirstOrDefault(x => x.id == ind).texts;
                }
                last_texts.Add(selected_text);
                for (var i = 1; i <= last_texts.Count; i++) {
                    last_texts[i-1].id = i;
                }
                Debug.WriteLine(JsonConvert.SerializeObject(last_texts)+" - - - вововово");
                string updatedJson = JsonConvert.SerializeObject(last_texts);
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
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




                    Debug.WriteLine(updatedJson + "- вот он");
                    command.Parameters.AddWithValue("$texts", updatedJson);
                    command.Parameters.AddWithValue("$id", ind);
                    command.ExecuteNonQuery();

                }
                
                using (var connection1 = new SqliteConnection(connectionString))
                {
                    connection1.Open();

                    var command1 = connection1.CreateCommand();
                    command1.CommandText = "DELETE FROM texts WHERE id = @id";
                    command1.Parameters.AddWithValue("@id", index);
                    command1.ExecuteNonQuery();
                    command1.CommandText = "UPDATE texts SET id = id - 1 WHERE id > @id";
                    command1.ExecuteNonQuery();
                }
                var collectionViewSource = (CollectionViewSource)Resources["text_list"];
                collectionViewSource.Source = last_texts;
                var all_texts = Text_Sql.Get_Texts();
                text_coll.Source = all_texts;
            };
            menuFlyout.Items.Add(text_el);
        }
        FrameworkElement senderElement = sender as FrameworkElement;
        menuFlyout.ShowAt(senderElement);
    }

    private void delete_texts_Click(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        Grid grid = (Grid)button.Parent;


        Int64 index = (Int64)grid.Tag;
        Debug.WriteLine(index+" - - -- - ");
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        List<Create_folder> all_folder = Text_Sql.Get_Folder();
        for (var i = 1; i <= all_folder.FirstOrDefault(item => item.id == ind).texts.Count; i++)
        {
            all_folder.FirstOrDefault(item => item.id == ind).texts[i - 1].id = i;
        }
        text.CreateNewTextBlock deleted_texts = all_folder.FirstOrDefault(item => item.id == ind).texts.FirstOrDefault(item => item.id == index);
        all_folder.FirstOrDefault(item => item.id == ind).texts.RemoveAll(item => item.id == index);
        for (int i = 0; i < all_folder.FirstOrDefault(item => item.id == ind).texts.Count; i++)
        {
            Debug.WriteLine(i + " - перемещённые id");
            all_folder.FirstOrDefault(item => item.id == ind).texts[i].id = i + 1;
        }
        var all_texts = Text_Sql.Get_Texts();
        if(all_texts.Count == 0)
        {
            deleted_texts.id = 1;
        }
        else
        {
            deleted_texts.id = all_texts.Count + 1;
        }
        all_texts.Add(deleted_texts);
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM texts";
            command.ExecuteNonQuery();
            foreach (text.CreateNewTextBlock text in all_texts)
            {

                Text_Sql.Add_Text(text.topic, text.text, JsonConvert.SerializeObject(text.files), text.id);
            }

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

        }

        Debug.WriteLine(index);

        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
        collectionViewSource.Source = all_folder.FirstOrDefault(item => item.id == ind).texts;
        text_coll.Source = all_texts;
    }
}
