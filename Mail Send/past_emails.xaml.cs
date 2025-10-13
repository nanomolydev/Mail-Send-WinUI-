using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using Windows.System;
using static Mail_Send.account;
using static Mail_Send.text;
using static Mail_Send.email;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.UI;

namespace Mail_Send;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class past_emails : Page
{
     private static string connectionString = "Filename==database.db";
    public past_emails()
    {
        this.InitializeComponent();
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        SqliteFunctions.CreateTable();
        Text_Sql.CreateFolderBs();

        Email_Sql.CreateBs();
        var all_email = Email_Sql.Get_past_email();
        var collectionViewSource = (CollectionViewSource)Resources["past_list"];
        collectionViewSource.Source = all_email;
    }
    private void dele_text_block(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;

        long pre_index = (long)button.Tag;
        int index = (int)pre_index;
        ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
        connectionString = $"Filename={dbpath}";
        List<Create_email> all_email = Email_Sql.Get_past_email();
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
            command.CommandText = "DELETE FROM all_past_email";
            command.ExecuteNonQuery();
            foreach (Create_email acc in all_email)
            {

                Email_Sql.Add_past_email(acc.name, acc.email, acc.folders, acc.texts, (int)acc.id, false);
            }

        }

        Debug.WriteLine(index);

        var collectionViewSource = (CollectionViewSource)Resources["past_list"];
        collectionViewSource.Source = all_email;

    }
    private void copy_text_block(object sender, RoutedEventArgs e)
    {
        // Code to execute when the second item is clicked
    }

    private void rename_text_block(object sender, RoutedEventArgs e)
    {
        // Code to execute when the third item is clicked
    }
    public void ResetIdsInOrder<T>(List<T> items) where T : home.IHasId
    {
        int currentId = 1;
        foreach (var item in items)
        {
            item.id = currentId++;
        }
    }
    private void open_email_content_RightTapped(object sender, RightTappedRoutedEventArgs e)
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
        secondItem.Click += copy_text_block;
        MenuFlyoutItem thirdItem = new MenuFlyoutItem { Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("return_email_block") };
        thirdItem.Icon = new SymbolIcon(Symbol.Undo);
        thirdItem.CornerRadius = new CornerRadius(5);
        thirdItem.Click+= (button, args) => {
            Email_Sql.CreateBs();
            Button button1 = sender as Button;

            long pre_index = (long)button1.Tag;
            int index = (int)pre_index;
            var all_email1 = Email_Sql.Get_past_email();
            var selected_email = all_email1.FirstOrDefault(x => x.id == index);
            Debug.WriteLine(JsonConvert.SerializeObject(selected_email));
            all_email1.RemoveAll(x => x.id == index);
            for (var i = 0; i < all_email1.Count; i++)
            {
                all_email1[i].id = i + 1;
            }
            var all_past_email = Email_Sql.Get_email();
            if (all_past_email.Count == 0)
            {
                Email_Sql.Add_email(selected_email.name, selected_email.email, selected_email.folders, selected_email.texts, 1, selected_email.is_random);
            }
            else
            {
                Email_Sql.Add_email(selected_email.name, selected_email.email, selected_email.folders, selected_email.texts, all_past_email.Count + 1, selected_email.is_random);
            }
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM past_email";
                command.ExecuteNonQuery();
                foreach (Create_email acc1 in all_email1)
                {

                    Email_Sql.Add_past_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                }

            }
            var collectionViewSource = (CollectionViewSource)Resources["past_list"];
            collectionViewSource.Source = all_email1;
        };
        myFlyout.Items.Add(firstItem);
        myFlyout.Items.Add(secondItem);
        myFlyout.Items.Add(thirdItem);
        FrameworkElement senderElement = sender as FrameworkElement;
        myFlyout.ShowAt(senderElement);
    }

    private void open_email_content_Click(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        TextBlock textBlock = (TextBlock)button.FindName("email_name");
        Page page = new past_emails_content((Int64)button.Tag, textBlock);
        text_content.IsPaneOpen = true;
        text_content_frame.Content = page;
    }



}
