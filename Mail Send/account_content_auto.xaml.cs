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
using static Mail_Send.account;
using Windows.Storage;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using Org.BouncyCastle.Asn1.Cms;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class account_content_auto : Page
    {
        TextBlock sharedElement;
        int ind = 0;
        public string ReadSetting(string key)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            object value = localSettings.Values[key];
            string result = (value != null) ? value.ToString() : "Not Found";
            return result;
        }

        public void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[key] = value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error writing app settings: " + ex.Message);
            }
        }

        string connectionString;
        public account_content_auto(int index, TextBlock account_name_block)
        {
            ind = index;
            sharedElement = account_name_block;
            this.InitializeComponent();
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            SqliteFunctions.CreateTable();
            List<create_account> all_account = SqliteFunctions.SelectAllRows();
            foreach (var account in all_account)
            {
                if (account.id == index)
                {
                    account_name_textbox.Text = account.name;
                    account_email_textbox.Text = account.email;
                    account_app_password_textbox.Text = account.app_password;
                }
            }

        }


        private void account_name_changed(object sender, TextChangedEventArgs e)
        {
            TextBlock topic_name_ready = sharedElement;
            topic_name_ready.Text = account_name_textbox.Text;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        UPDATE accounts
                        SET name = $name
                        WHERE id = $id
                    ";
                command.Parameters.AddWithValue("$name", account_name_textbox.Text);
                command.Parameters.AddWithValue("$id", ind);
                command.ExecuteNonQuery();
            }
            //AddUpdateAppSettings("account_name_" + index.ToString(), account_name_textbox.Text);
        }

        private void email_write_open(object sender, RoutedEventArgs e)
        {
            Frame frame = this.Frame;

        }

        private void close_content(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("status_account", "close");
            Frame mainFrame = Window.Current.Content as Frame;
            MainPage Mainpage = mainFrame.Content as MainPage;
            Frame max_size_frame = Mainpage.FindName("max_size_frame") as Frame;
            max_size_frame.Margin = new Thickness(200, 0, 0, 0);
            Frame right_panel = Mainpage.FindName("right_panel") as Frame;
            right_panel.Content = null;
        }

        private void account_email_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<create_account> all_account = SqliteFunctions.SelectAllRows();
            create_account account = all_account.FirstOrDefault(item=>item.id==ind);
            account.email = account_email_textbox.Text;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM accounts";
                command.ExecuteNonQuery();
                foreach (create_account acc in all_account)
                {

                    SqliteFunctions.Add_Account(acc.name, acc.email, acc.app_password, acc.id);
                }
            }
            
        }

        private void account_app_password_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<create_account> all_account = SqliteFunctions.SelectAllRows();
            create_account account = all_account.FirstOrDefault(item => item.id == ind);
            account.app_password = account_app_password_textbox.Text;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM accounts";
                command.ExecuteNonQuery();
                foreach (create_account acc in all_account)
                {

                    SqliteFunctions.Add_Account(acc.name, acc.email, acc.app_password, acc.id);
                }
            }
        }
        public class login_account
        {
            public string email
            {
                get; set;
            }
            public string password
            {
                get; set;
            }

        }
        public class AppCredentials
        {
            public string app_password
            {
                get; set;
            }
        }

        private async void save_Click(object sender, RoutedEventArgs e)
        {
            string pwd = "C:\\mlsend\\";
            Debug.WriteLine(pwd);
            string finalString = System.IO.Path.Combine(pwd, "server-win.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C start \"\" \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "login.bat")}\"",
                UseShellExecute = false,
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            Thread.Sleep(2000);
            var client = new HttpClient();
            var message = new login_account() { email = account_email_textbox.Text, password = account_app_password_textbox.Text };
            var json = JsonConvert.SerializeObject(message);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            Console.WriteLine(JsonConvert.SerializeObject(json));
            var response = await client.PostAsync("http://localhost:3000/message", data);
            string result1 = "";
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    result1 = await reader.ReadToEndAsync();
                }
            }
            AppCredentials credentials = JsonConvert.DeserializeObject<AppCredentials>(result1);

            try
            {
                gms gms = new gms();
                gms.login_account(account_email_textbox.Text, credentials.app_password);
                gms.Create_message(account_email_textbox.Text, "Mail Sendler", ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("check_account"));
                await gms.send_message(false);
                List<create_account> all_account = SqliteFunctions.SelectAllRows();
                create_account account = all_account.FirstOrDefault(item => item.id == ind);
                account.email = account_email_textbox.Text;
                account.is_verified = true;
                account.app_password = credentials.app_password;
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
                ContentDialog dialog = new ContentDialog();

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("saved");
                dialog.CloseButtonText = "Ok";
                dialog.DefaultButton = ContentDialogButton.Primary;

                var result = await dialog.ShowAsync();

            }
            catch
            {
                ContentDialog dialog = new ContentDialog();

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("error_log_in");
                dialog.CloseButtonText = "Ok";
                dialog.DefaultButton = ContentDialogButton.Primary;

                var result = await dialog.ShowAsync();
            }
            
        }
    }
}
