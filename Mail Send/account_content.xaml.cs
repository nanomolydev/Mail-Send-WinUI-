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
using static Mail_Send.setting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class account_content : Page
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
        public account_content(int index, TextBlock account_name_block)
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
        private async void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                home.log_sql.CreateBs();
                gms gms = new gms();
                gms.load_index_log((home.log_sql.Get_Logs()!=null) ? home.log_sql.Get_Logs().Count+1 : 1);
                DateTime currentDate = DateTime.Now;
                gms.load_created_datetime(currentDate);
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                var dbHandler = new SmtpSettingsDatabaseHandler(connectionString);

                // Инициализация базы данных
                dbHandler.InitializeDatabase();
                var smtpSettings = dbHandler.GetSmtpSettings();
                gms.smtp_load(smtpSettings.SmtpServer, smtpSettings.SmtpPort);
                gms.login_account(account_email_textbox.Text, account_app_password_textbox.Text);
                gms.Create_message(account_email_textbox.Text, "Mail Sendler", ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("check_account"));
                await gms.send_message(true);
                List<create_account> all_account = SqliteFunctions.SelectAllRows();
                create_account account = all_account.FirstOrDefault(item => item.id == ind);
                account.email = account_email_textbox.Text;
                account.is_verified = true;
                account.app_password = account_app_password_textbox.Text;
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
                reload_data_Click();
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
