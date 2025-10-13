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
using Newtonsoft.Json;
using System.Diagnostics;
using Windows.Storage;
using static Mail_Send.account;
using WinRT;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI;
using System.Reflection.PortableExecutable;
using DeviceId;
using MySql.Data.MySqlClient;
using static Mail_Send.setting;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using Syncfusion.UI.Xaml.Core;
using gm;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;
using System.Net;
using Windows.System;
using System.Net.Http.Headers;
using Microsoft.UI.Xaml.Automation.Peers;
using static Mail_Send.statistic;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class setting : Page
    {
        private static string connectionString = "Filename==database.db";
        public class create_setting
        {
            public int threads
            {
                get; set;
            }
            public string type_setting
            {
                get; set;
            }
            public List<automatic_account> auto
            {
                get; set;
            }
            public List<manual_account> manual
            {
                get; set;
            }
            public string language
            {
                get; set;
            }

        }
        public class automatic_account
        {

            public int id
            {
                get; set;
            }
            public string name
            {
                get; set;
            }
            public bool is_enabled
            {
                get; set;
            }
        }
        public class manual_account
        {

            public int id
            {
                get; set;
            }
            public string name
            {
                get; set;
            }
            public bool is_enabled
            {
                get; set;
            }
            public int id_last
            {
                get; set;
            }
            public bool is_all_email
            {
                get; set;
            }
        }
        public class ShopedData
        {
            public string deviceid
            {
                get; set;
            }
            public DateTime shoped_time
            {
                get; set;
            }
        }
        public class SmtpSettings
        {
            public string SmtpServer
            {
                get; set;
            }
            public int SmtpPort
            {
                get; set;
            }
            public bool Custom
            {
                get; set;
            }

            public SmtpSettings(string smtpServer, int smtpPort, bool custom)
            {
                SmtpServer = smtpServer;
                SmtpPort = smtpPort;
                Custom = custom;
            }
        }

        public class SmtpSettingsDatabaseHandler
        {
            private readonly string _connectionString;

            public SmtpSettingsDatabaseHandler(string connectionString)
            {
                _connectionString = connectionString;
            }

            public void InitializeDatabase()
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText =
                        @"CREATE TABLE IF NOT EXISTS smtp (
                    id INTEGER PRIMARY KEY,
                    smtp_server TEXT NOT NULL,
                    smtp_port INTEGER NOT NULL,
                    custom BOOLEAN NOT NULL DEFAULT FALSE
                )";
                    tableCmd.ExecuteNonQuery();
                    var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM smtp";
                    var count = (long)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        var deafult_value = connection.CreateCommand();
                        tableCmd.CommandText = @"INSERT INTO smtp (smtp_server, smtp_port)
                SELECT 'smtp.gmail.com', 587
                WHERE NOT EXISTS (SELECT 1 FROM smtp WHERE id = 1);";
                        deafult_value.ExecuteNonQuery();
                    }
                    

                }
            }

            public SmtpSettings GetSmtpSettings()
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    var selectCmd = connection.CreateCommand();
                    selectCmd.CommandText = "SELECT smtp_server, smtp_port, custom FROM smtp LIMIT 1";

                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SmtpSettings(
                                smtpServer: reader.GetString(0),
                                smtpPort: reader.GetInt32(1),
                                custom: reader.GetBoolean(2));
                        }
                    }
                }

                // Возвращаем значения по умолчанию, если в таблице нет строк
                return new SmtpSettings("smtp.gmail.com", 587, false);
            }

            public void SetSmtpSettings(SmtpSettings settings)
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM smtp";
                    var count = (long)checkCmd.ExecuteScalar();

                    SqliteCommand command;

                    if (count == 0)
                    {
                        // Вставляем строку, если её нет
                        command = connection.CreateCommand();
                        command.CommandText =
                            "INSERT INTO smtp (smtp_server, smtp_port, custom) VALUES (@server, @port, @custom)";
                    }
                    else
                    {
                        // Обновляем существующую строку
                        command = connection.CreateCommand();
                        command.CommandText =
                            "UPDATE smtp SET smtp_server = @server, smtp_port = @port, custom = @custom";
                    }

                    command.Parameters.AddWithValue("@server", settings.SmtpServer);
                    command.Parameters.AddWithValue("@port", settings.SmtpPort);
                    command.Parameters.AddWithValue("@custom", settings.Custom);
                    command.ExecuteNonQuery();
                }
            }
        }
        public class MySqlHelper
        {
            private readonly string connectionString;

            public MySqlHelper(string connectionString)
            {
                this.connectionString = connectionString;
            }

            public void CreateShopedTable()
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS shoped (deviceid TEXT, shoped_time DATETIME)", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }


            public List<ShopedData> GetShopedData()
            {
                var shopedDataList = new List<ShopedData>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
            SELECT deviceid, shoped_time FROM shoped
            UNION ALL
            SELECT device_id AS deviceid, date_added AS shoped_time FROM trial_keys";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var shopedData = new ShopedData
                                {
                                    deviceid = reader.GetString("deviceid"),
                                    shoped_time = reader.GetDateTime("shoped_time")
                                };

                                shopedDataList.Add(shopedData);
                            }
                        }
                    }
                }

                return shopedDataList;
            }

        }
        public static class GlobalVariables
        {
            public static List<ShopedData> MyVariable = new List<ShopedData>();
        }
        public List<ShopedData> shopedDataList;
        public setting()
        {
            setting_Sql.CreateBs();
            Button str = new Button();
            


            this.InitializeComponent();
            Package package = Package.Current;

            // Получаем версию пакета
            PackageVersion version = package.Id.Version;
            string versionString = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            now_version.Text = "current version: " + versionString;
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();

            GlobalVariables.MyVariable = App.GlobalVariables.MyVariable;

            var selected_shoped = GlobalVariables.MyVariable.FirstOrDefault(x=>x.deviceid==deviceId);
            //if (selected_shoped != null)
            //{
            //    number_thread.Maximum = 10;
            //}
            //else
            //{
            //    number_thread.Maximum = 1;
                
            //}
            SqliteFunctions.CreateTable();
            var settings = setting_Sql.Get_setting();
            Debug.WriteLine(JsonConvert.SerializeObject(settings));

            if (settings != null)
            {
                var type_setting = "auto";
                if (GetFromCache<string>("type_setting") != null && GetFromCache<string>("type_setting") != "")
                {
                    type_setting = GetFromCache<string>("type_setting");
                }
                if (type_setting == "auto")
                {
                    auto.IsChecked = true;
                }
                else if (type_setting == "manual")
                {
                    manual.IsChecked = true;
                }

                Debug.WriteLine(settings.threads + " - -- - - ");
                var value = GetFromCache<string>("threads");
                if (value != null)
                {
                    Debug.WriteLine($"{value}  -- = == ");
                    number_thread.Value = int.Parse(value);
                }
                
            }
            
        }
        public static class setting_Sql
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
                        CREATE TABLE IF NOT EXISTS settings (
                            threads INT NULL,
                            type_setting TEXT NULL,
                            auto TEXT NULL,
                            manual TEXT NULL,
                            language TEXT NULL
                        );
                    ";
                    command.ExecuteNonQuery();

                }
            }


            public static create_setting Get_setting()
            {
                var result = new create_setting();
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
                        FROM settings
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var setting = new create_setting();
                            if (reader.GetInt32(reader.GetOrdinal("threads")) != 0)
                            {
                                setting = new create_setting()
                                {
                                    threads = reader.GetInt32(reader.GetOrdinal("threads")),
                                    type_setting = null
                                };
                            }
                            else
                            {
                                setting = new create_setting()
                                {
                                    threads = 0,
                                    type_setting = null
                                };
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("type_setting")))
                            {
                                setting.type_setting = reader.GetString(reader.GetOrdinal("type_setting"));

                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("language")))
                            {
                                setting.language = reader.GetString(reader.GetOrdinal("language"));
                            }

                            List<automatic_account> data = new List<automatic_account>();
                            List<manual_account> data1 = new List<manual_account>();
                            if (!reader.IsDBNull(reader.GetOrdinal("auto")))
                            {
                                Debug.WriteLine(reader.GetString(reader.GetOrdinal("auto"))+ " - ffffffffffff ");
                                data = JsonConvert.DeserializeObject<List<automatic_account>>(reader.GetString(reader.GetOrdinal("auto")));
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("manual")))
                            {
                                data1 = JsonConvert.DeserializeObject<List<manual_account>>(reader.GetString(reader.GetOrdinal("manual")));
                            }
                            setting.auto = data;
                            setting.manual = data1;
                            result = setting;
                            Debug.WriteLine(reader.GetInt32(reader.GetOrdinal("threads")) + "nu int je");
                            if (reader.GetInt32(reader.GetOrdinal("threads")) != 0)
                            {
                                
                                setting.threads = reader.GetInt32(reader.GetOrdinal("threads"));
                                
                            }



                            // Добавьте объект text в список или используйте его по своему усмотрению
                        }
                    }
                }
                Debug.WriteLine(JsonConvert.SerializeObject(result) + "--------d");
                if (result.threads != 0)
                {

                    return result;
                }
                else
                {
                    Debug.WriteLine("null--------d");
                    return null;

                }

            }

        }
        public void generate_save_bar(string status)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 90;
            animation.To = -10;
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
                Text = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("not_saved"),
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 15,
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
                Foreground = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(5)
            };
            cancelButton.Click += (s, args) => {
            }; // Обработчик события нажатия кнопки "Отменить"
            stackPanel.Children.Add(cancelButton);

            // Создание кнопки "Сохранить"
            Button saveButton = new Button
            {
                Content = ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("save"),
                Width = 70,
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Orange),
                CornerRadius = new CornerRadius(5)
            };
            saveButton.Click += (s, args) => {
                if (status == "threads")
                {
                    SaveToCache("threads", number_thread.Value.ToString());


                }
                else if (status == "auto")
                {
                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                    connectionString = $"Filename={dbpath}";
                    var settings = setting_Sql.Get_setting();
                    if (settings != null)
                    {
                        if(settings.auto == null || settings.auto.Count == 0)
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
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.auto = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "auto");
                        SaveToCache("auto", JsonConvert.SerializeObject(settings.auto));

                    }
                    else
                    {
                        if (settings == null || settings.auto == null || settings.auto.Count == 0)
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
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.auto = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "auto");
                        SaveToCache("auto", JsonConvert.SerializeObject(settings.auto));
                    }
                    
                }
                else if (status == "manual")
                {
                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                    connectionString = $"Filename={dbpath}";
                    var settings = setting_Sql.Get_setting();
                    if (settings != null)
                    {
                        if (settings.auto == null || settings.auto.Count == 0)
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            var all_account_in_setting = new List<manual_account>();
                            foreach (var account in all_account)
                            {
                                manual_account item = new manual_account()
                                {
                                    id = account.id,
                                    id_last = account.id,
                                    name = account.name,
                                    is_enabled = true
                                };
                                all_account_in_setting.Add(item);
                            }
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.manual = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "manual");
                        SaveToCache("manual", JsonConvert.SerializeObject(settings.manual));

                    }
                    else
                    {
                        if (settings == null || settings.auto == null || settings.auto.Count == 0)
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            var all_account_in_setting = new List<manual_account>();
                            foreach (var account in all_account)
                            {
                                manual_account item = new manual_account()
                                {
                                    id = account.id,
                                    name = account.name,
                                    is_enabled = true
                                };
                                all_account_in_setting.Add(item);
                            }
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.manual = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "manual");
                        SaveToCache("manual", JsonConvert.SerializeObject(settings.manual));
                    }
                }
                Storyboard storyboard = new Storyboard();
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = -10;
                animation.To = 90;
                animation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                Storyboard.SetTarget(animation, save_transform);
                Storyboard.SetTargetProperty(animation, "Y");
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }; // Обработчик события нажатия кнопки "Сохранить"
            stackPanel.Children.Add(saveButton);
            save_bar.Children.Add(grid);
        }
        static string ExtractCharacters(string s)
        {
            // Проверка на минимальную длину строки
            if (s.Length < 10)
            {
                return s;
            }

            // Получение первых и последних пяти символов
            string firstFive = s.Substring(0, 5);
            string lastFive = s.Substring(s.Length - 4);

            return firstFive + ".." + lastFive;
        }
        private void auto_Checked(object sender, RoutedEventArgs e)
        {
            reload_data_Click();
            manual.Visibility = Visibility.Collapsed;
            manual.Height = 0;
            auto.Visibility = Visibility.Visible;
            manual.Visibility = Visibility.Collapsed;
            acc_scroll.Visibility = Visibility.Visible;
            open_all_manual.Visibility = Visibility.Collapsed;
            var settings = new create_setting();
            var auto1 = GetFromCache<string>("auto");
            if (auto1 != null && auto1 != "")
            {
                Debug.WriteLine(auto1);
                settings.auto = JsonConvert.DeserializeObject<List<automatic_account>>(auto1);
            }
                            
            if (settings==null|| settings.auto == null || settings.auto.Count==0)
            {
                var item1 = GetFromCache<string>("auto");
                if (item1!=null&& item1 != "")
                {
                    settings.auto = JsonConvert.DeserializeObject<List<automatic_account>>(item1);
                }
                else
                {
                    var all_account = SqliteFunctions.SelectAllRows();
                    var all_account_in_setting = new List<automatic_account>();
                    foreach (var account in all_account)
                    {
                        if (account.is_verified == true)
                        {
                            automatic_account item = new automatic_account()
                            {
                                id = account.id,
                                name = account.name,
                                is_enabled = true
                            };
                            all_account_in_setting.Add(item);
                        }
                    }
                    if (settings == null)
                    {
                        settings = new create_setting();
                    }
                    Debug.WriteLine($"{all_account_in_setting} - ты попутал ");
                    settings.auto = all_account_in_setting;
                }
                
                
            }
            Debug.WriteLine(JsonConvert.SerializeObject(settings.auto));
            foreach(var aut in settings.auto)
            {
                aut.name = ExtractCharacters(aut.name);
            }
            var collectionViewSource = (CollectionViewSource)Resources["automatic_list"];
            collectionViewSource.Source = settings.auto;
            if (settings != null)
            {
                if (GetFromCache<string>("type_setting") != "auto")
                {
                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                    connectionString = $"Filename={dbpath}";
                    if (settings != null)
                    {
                        if (settings.auto == null || settings.auto.Count == 0)
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            var all_account_in_setting = new List<automatic_account>();
                            foreach (var account in all_account)
                            {
                                if (account.is_verified == true){
                                    automatic_account item = new automatic_account()
                                    {
                                        id = account.id,
                                        name = account.name,
                                        is_enabled = true
                                    };
                                    all_account_in_setting.Add(item);
                                }
                                
                            }
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.auto = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "auto");
                        SaveToCache("auto", JsonConvert.SerializeObject(settings.auto));

                    }
                    else
                    {
                        if (settings == null || settings.auto == null || settings.auto.Count == 0)
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            var all_account_in_setting = new List<automatic_account>();
                            foreach (var account in all_account)
                            {
                                if (account.is_verified == true)
                                {
                                    automatic_account item = new automatic_account()
                                    {
                                        id = account.id,
                                        name = account.name,
                                        is_enabled = true
                                    };
                                    all_account_in_setting.Add(item);
                                }
                            }
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.auto = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "auto");
                        SaveToCache("auto", JsonConvert.SerializeObject(settings.auto));
                    }
                }
            }
        }

        private void manual_Checked(object sender, RoutedEventArgs e)
        {
            auto.Visibility = Visibility.Collapsed;
            manual.Visibility = Visibility.Visible;
            auto.Height = 0;
            acc_scroll.Visibility = Visibility.Collapsed;
            manual_scroll.Visibility = Visibility.Visible;
            open_all_manual.Visibility = Visibility.Visible;
            var settings = setting_Sql.Get_setting();
            var manual1 = GetFromCache<string>("manual");
            if (manual1 != null && manual1 != "")
            {
                settings.manual = JsonConvert.DeserializeObject<List<manual_account>>(manual1);
            }
            else
            {

            }
            if (settings == null || settings.manual == null ||settings.manual.Count == 0)
            {
                var all_account = SqliteFunctions.SelectAllRows();
                var all_account_in_setting = new List<manual_account>();
                foreach (var account in all_account)
                {
                    manual_account item = new manual_account()
                    {
                        id = account.id,
                        name = account.name,
                        is_enabled = true,
                        is_all_email = true,
                    };
                    all_account_in_setting.Add(item);
                }
                if (settings == null)
                {
                    settings = new create_setting();
                }
                settings.manual = all_account_in_setting;
            }
            Debug.WriteLine(JsonConvert.SerializeObject(settings.manual) + "------- couynt");
            foreach (var aut in settings.manual)
            {
                aut.name = ExtractCharacters(aut.name);
            }
            var collectionViewSource = (CollectionViewSource)Resources["manual_list"];
            collectionViewSource.Source = settings.manual;
            if (settings != null)
            {
                if (GetFromCache<string>("type_setting") != "manual")
                {
                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                    connectionString = $"Filename={dbpath}";

                    if (settings != null)
                    {
                        if (settings.auto == null || settings.auto.Count == 0)
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            var all_account_in_setting = new List<manual_account>();
                            foreach (var account in all_account)
                            {
                                manual_account item = new manual_account()
                                {
                                    id = account.id,
                                    id_last = account.id,
                                    name = account.name,
                                    is_enabled = true
                                };
                                all_account_in_setting.Add(item);
                            }
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.manual = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "manual");
                        SaveToCache("manual", JsonConvert.SerializeObject(settings.manual));

                    }
                    else
                    {
                        if (settings == null || settings.auto == null || settings.auto.Count == 0)
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            var all_account_in_setting = new List<manual_account>();
                            foreach (var account in all_account)
                            {
                                manual_account item = new manual_account()
                                {
                                    id = account.id,
                                    name = account.name,
                                    is_enabled = true
                                };
                                all_account_in_setting.Add(item);
                            }
                            if (settings == null)
                            {
                                settings = new create_setting();
                            }
                            settings.manual = all_account_in_setting;
                        }
                        SaveToCache("type_setting", "manual");
                        SaveToCache("manual", JsonConvert.SerializeObject(settings.manual));
                    }
                }
            }
            

            
        }
        public void SaveToCache(string key, object value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
        }
        public T GetFromCache<T>(string key)
        {
            var localSettings = Windows.Storage.ApplicationData .Current.LocalSettings;
            if (localSettings.Values.ContainsKey(key))
            {
                return (T)localSettings.Values[key];
            }
            else
            {
                return default(T);
            }
        }

        private async void number_thread_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (config_grid.Visibility == Visibility.Visible)
            {
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                
                Debug.WriteLine(shopedDataList);
                var mySqlHelper = new MySqlHelper("server=188.225.18.53;user=defaultuser;database=default_db;port=3306;password=bbnn7788");
                string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                var selected_shoped = GlobalVariables.MyVariable.FirstOrDefault(x => x.deviceid == deviceId);
                if (selected_shoped != null)
                {
                    if (setting_Sql.Get_setting() != null)
                    {
                        if (number_thread.Value != 1 && (int)setting_Sql.Get_setting().threads != (int)number_thread.Value)
                        {
                            SaveToCache("threads", number_thread.Value.ToString());
                        }
                    }
                    else
                    {
                        if (number_thread.Value != 1)
                        {
                            SaveToCache("threads", number_thread.Value.ToString());
                        }
                    }
                }
                else
                {
                    if (number_thread.Value != 1)
                    {
                        ShowProFeaturesDialog();

                        number_thread.Value = 1;
                    }


                }
            }
            
            
        }

        private void auto_Unchecked(object sender, RoutedEventArgs e)
        {
            auto.IsChecked = false;
            manual.Visibility = Visibility.Visible;
            manual_scroll.Visibility = Visibility.Collapsed;
            acc_scroll.Visibility = Visibility.Collapsed;
            open_all_manual.Visibility = Visibility.Collapsed;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                            UPDATE settings
                            SET type_setting = $type_setting
                        ";
                command.Parameters.AddWithValue("$type_setting", "null");
                command.ExecuteNonQuery();

            }
            var collectionViewSource = (CollectionViewSource)Resources["automatic_list"];
            collectionViewSource.Source = null;
        }

        private void manual_Unchecked(object sender, RoutedEventArgs e)
        {
            manual.IsChecked = false;
            auto.Visibility = Visibility.Visible;
            manual_scroll.Visibility = Visibility.Collapsed;
            acc_scroll.Visibility = Visibility.Collapsed;
            open_all_manual.Visibility= Visibility.Collapsed;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                            UPDATE settings
                            SET type_setting = $type_setting
                        ";
                command.Parameters.AddWithValue("$type_setting", "null");
                command.ExecuteNonQuery();

            }
            var collectionViewSource = (CollectionViewSource)Resources["manual_list"];
            collectionViewSource.Source  = new List<manual_account>();
        }

        private void account_toggle_switch_toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch tg = sender as ToggleSwitch;

            Grid grid = tg.Parent as Grid;
            if (grid == null)
            {

            }
            else
            {
                var all_sett = setting_Sql.Get_setting();
                var item1 = GetFromCache<string>("auto");
                if (item1 != null && item1 != "")
                {
                    all_sett.auto = JsonConvert.DeserializeObject<List<automatic_account>>(item1);
                }
                else
                {

                }



                if (all_sett.auto == null || all_sett.auto == new List<automatic_account>())
                {
                    all_sett.auto = new List<automatic_account>() {
                        new automatic_account() {id=(int)grid.Tag,is_enabled=true,name=SqliteFunctions.SelectAllRows().FirstOrDefault(x=>x.id==(int)grid.Tag).name }
                    };
                }
                else
                {
                    try
                    {
                        all_sett.auto.FirstOrDefault(x => x.id == (int)grid.Tag).is_enabled = tg.IsOn;
                    }
                    catch
                    {

                    }

                }
                SaveToCache("auto", JsonConvert.SerializeObject(all_sett.auto));



            }

        }
        private void manual_toggle_switch_toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch tg = sender as ToggleSwitch;

            Grid grid = tg.Parent as Grid;
            if (grid == null)
            {

            }
            else
            {
                var all_sett = setting_Sql.Get_setting();
                var item1 = GetFromCache<string>("manual");
                if (item1 != null && item1 != "")
                {
                    all_sett.manual = JsonConvert.DeserializeObject<List<manual_account>>(item1);
                }
                else
                {

                }
                

                Debug.WriteLine((all_sett.manual.Count + "---ddddddd"));
                if (all_sett.manual == null || all_sett.manual == new List<manual_account>())
                {
                    all_sett.manual = new List<manual_account>() {
                        new manual_account() {id=(int)grid.Tag,id_last=(int)grid.Tag,is_all_email=true,is_enabled=true,name=SqliteFunctions.SelectAllRows().FirstOrDefault(x=>x.id==(int)grid.Tag).name }
                    };
                }
                else
                {
                    try
                    {
                         all_sett.manual.FirstOrDefault(x => x.id_last == (int)grid.Tag).is_enabled = tg.IsOn;
                    }
                    catch
                    {

                    }
                    
                }
                SaveToCache("manual", JsonConvert.SerializeObject(all_sett.manual));



            }

        }
        static void ПереместитьЭлемент(int индекс, int индексПеремещения, List<manual_account> список)
        {
            manual_account элемент = список[индекс];
            список.RemoveAt(индекс);
            список.Insert(индексПеремещения, элемент);
        }


        public void UpdateIds(List<manual_account> list)
        {
            int id = 1;
            foreach (var item in list)
            {
                item.id = id++;
            }
        }
        private void manual_panel_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            Debug.WriteLine("---");
            var cc = args.Items;
            var all_setting = setting_Sql.Get_setting();
            var item1 = GetFromCache<string>("manual");
            if (item1 != null && item1 != "")
            {
                all_setting.manual = JsonConvert.DeserializeObject<List<manual_account>>(item1);
            }
            else
            {

            }
            manual_account lastitem = (manual_account)cc.First();
            int id_to = manual_panel.Items.IndexOf(cc.First()) + 1;
            List<manual_account> all_account = all_setting.manual;
            var l = lastitem.id;
            var t = id_to;
            Debug.WriteLine(l);
            Debug.WriteLine(t);
            ПереместитьЭлемент(l - 1, t - 1, all_account);
            UpdateIds(all_account);
            //debug.writeline(all_setting.manual[lastitem.id - 1].id + " - айди которое было");
            //all_setting.manual[lastitem.id - 1].id_last = lastitem.id;
            //all_setting.manual[lastitem.id - 1].id = id_to;
            //debug.writeline(all_setting.manual[id_to - 1].id + " - айди которое стало");
            //if (id_to - lastitem.id < 0)
            //{
            //    for (var i = id_to; i < lastitem.id; i++)
            //    {
            //        all_setting.manual[i - 1].id = all_setting.manual[i - 1].id + 1;
            //    }
            //}
            //else
            //{
            //    for (var i = lastitem.id + 1; i <= id_to; i++)
            //    {
            //        all_setting.manual[i - 1].id = all_setting.manual[i - 1].id - 1;
            //    }
            //}
            Debug.WriteLine(JsonConvert.SerializeObject(all_account)+" -ghg  ");
            SaveToCache("manual", JsonConvert.SerializeObject(all_account));
            foreach (var aut in all_account)
            {
                aut.name = ExtractCharacters(aut.name);
            }
            var collectionViewSource = (CollectionViewSource)Resources["manual_list"];
            collectionViewSource.Source = all_account;

        }
        private ContentDialog dialog;
        private async void manual_setting_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Grid grid = (Grid)button.Parent;
            dialog = new ContentDialog
            {
                Title = "Настройка аккаунта",
                MaxWidth = this.ActualWidth // Установка максимальной ширины диалогового окна
            };
            dialog.XamlRoot = this.XamlRoot;

            // Создание элементов для диалогового окна
            StackPanel panel = new StackPanel();
            
            CheckBox checkBox = new CheckBox();
            var all_sett = setting_Sql.Get_setting();
            checkBox.IsChecked = all_sett.manual.FirstOrDefault(x => x.id == (int)grid.Tag).is_all_email;
            checkBox.Tag = grid.Tag;
            checkBox.Content = "Отправлять все почты";
            checkBox.Checked += manual_all_email_checked;
            checkBox.Unchecked += manual_all_email_unhecked;
            panel.Children.Add(checkBox);

            // Добавление элементов в диалоговое окно
            dialog.Content = panel;

            // Отображение диалогового окна
            await dialog.ShowAsync();
        }
        private void manual_all_email_checked(object sender, RoutedEventArgs e)
        {

            CheckBox tg = sender as CheckBox;


            var all_sett = setting_Sql.Get_setting();

            Debug.WriteLine((all_sett.manual.Count + "---ddddddd"));
            all_sett.manual.FirstOrDefault(x => x.id == (int)tg.Tag).is_all_email = true;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                            UPDATE settings
                            SET manual = $manual
                        ";
                command.Parameters.AddWithValue("$manual", JsonConvert.SerializeObject(all_sett.manual));
                command.ExecuteNonQuery();

            }
        }
        private void manual_all_email_unhecked(object sender, RoutedEventArgs e)
        {

            CheckBox tg = sender as CheckBox;


            var all_sett = setting_Sql.Get_setting();

            Debug.WriteLine((all_sett.manual.Count + "---ddddddd"));
            all_sett.manual.FirstOrDefault(x => x.id == (int)tg.Tag).is_all_email = false;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                            UPDATE settings
                            SET manual = $manual
                        ";
                command.Parameters.AddWithValue("$manual", JsonConvert.SerializeObject(all_sett.manual));
                command.ExecuteNonQuery();

            }
        }

        private async void setting_catalog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (setting_catalog.SelectedItem != null)
            {
                ListViewItem selectedItem = (ListViewItem)setting_catalog.SelectedItem;
                if (selectedItem.Name == "conf")
                {
                    config_grid.Visibility = Visibility.Visible;
                    app_grid.Visibility = Visibility.Collapsed;
                    smtp_grid.Visibility = Visibility.Collapsed;
                }
                if (selectedItem.Name == "server")
                {
                    smtp_grid.Visibility = Visibility.Visible;
                    app_grid.Visibility = Visibility.Collapsed;
                    config_grid.Visibility = Visibility.Collapsed;
                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                    connectionString = $"Filename={dbpath}";
                    var dbHandler = new SmtpSettingsDatabaseHandler(connectionString);

                    // Инициализация базы данных
                    dbHandler.InitializeDatabase();

                    // Получение текущих настроек SMTP
                    // Retrieve and display the SMTP settings
                    var smtpSettings = dbHandler.GetSmtpSettings();
                    if (smtpSettings.SmtpServer != null)
                    {
                        smtp_server.Text = smtpSettings.SmtpServer;
                    }
                    if (smtpSettings.SmtpPort != null)
                    {
                        smtp_port.Text = smtpSettings.SmtpPort.ToString();
                    }
                    if (smtpSettings.Custom != null)
                    {
                        enable_user_smtp.IsChecked = smtpSettings.Custom;
                        if (smtpSettings.Custom == false)
                        {
                            enable_default_smtp.IsChecked = true;
                        }
                    }
                }
                if (selectedItem.Name == "app")
                {
                    smtp_grid.Visibility = Visibility.Collapsed;
                    config_grid.Visibility = Visibility.Collapsed;
                    app_grid.Visibility = Visibility.Visible;

                }

                // Теперь вы можете что-то сделать с выбранным элементом
            }
        }
        private void RestartApplication()
        {
            // Получение пути к файлу исполняемой программы
            var exePath = Process.GetCurrentProcess().MainModule.FileName;

            // Запуск нового экземпляра приложения
            Process.Start(exePath);

            // Закрытие текущего экземпляра
            Application.Current.Exit();
        }

        private void language_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)language_combobox.SelectedItem;

            if (item.Name.ToString()=="en")
            {
                ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.ChangeLanguage("en-US");
                SaveToCache("language", "en-US");
                RestartApplication();
            }
            if (item.Name.ToString() == "ru")
            {
                Debug.WriteLine("ddddd");
                ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.ChangeLanguage("ru-RU");
                SaveToCache("language", "ru-RU");
                RestartApplication();
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
            
            dialog.XamlRoot = config_grid.XamlRoot;
            _currentDialog = dialog;
            try
            {
                await _currentDialog.ShowAsync();
            }
            catch
            {

            }
            
        }
        public async Task ShowErrorDialog(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Ошибка",
                Content = message,
                CloseButtonText = "ОК"
            };
            errorDialog.XamlRoot = deviceid_grid.XamlRoot;

            await errorDialog.ShowAsync();
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
        private void open_pane_Click(object sender, RoutedEventArgs e)
        {
            main_split.IsPaneOpen = true;
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
            if(type_setting == "auto")
            {
                var all_account = SqliteFunctions.SelectAllRows();
                List<automatic_account> accounts = new List<automatic_account>();
                if(GetFromCache<string>("auto") != null && GetFromCache<string>("auto") != "[]" && GetFromCache<string>("auto") != "")
                {
                    List<automatic_account> auto = JsonConvert.DeserializeObject<List<automatic_account>>(GetFromCache<string>("auto"));
                    accounts = auto;
                    RemoveItemsNotInSecondList(auto, all_account);
                    var all_non_detected = FindElementsNotInFirstList(auto, all_account);
                    foreach(var item in all_non_detected)
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
                    foreach(var at in accounts)
                    {
                        create_account selected = all_account.FirstOrDefault(x=>x.id == at.id);
                        if(selected.name != at.name)
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
                            if(account.is_verified == true)
                            {
                                automatic_account auto = new automatic_account() { id = account.id, is_enabled = true, name = account.name };
                                accounts.Add(auto);
                            }
                            
                        }
                    }
                }
                var collectionViewSource = (CollectionViewSource)Resources["automatic_list"];
                collectionViewSource.Source = accounts;
                SaveToCache("auto", JsonConvert.SerializeObject(accounts));
                
            }
            else if(type_setting=="manual")
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
                var collectionViewSource = (CollectionViewSource)Resources["manual_list"];
                collectionViewSource.Source = accounts;
                SaveToCache("manual", JsonConvert.SerializeObject(accounts));
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
            ShowErrorDialog(deviceId);
        }

        private void open_all_manual_Click(object sender, RoutedEventArgs e)
        {
            var collectionViewSource = (CollectionViewSource)Resources["manual_list"];
            all_manual.Content = new manual_setting(collectionViewSource, all_manual);
        }
        public async Task ShowCopySuccess(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Успех",
                Content = message,
                CloseButtonText = "ОК"
            };
            errorDialog.XamlRoot = deviceid_grid.XamlRoot;

            await errorDialog.ShowAsync();
        }
        private void copy_deviceid_Click(object sender, RoutedEventArgs e)
        {
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
            var dataPackage = new DataPackage();
            dataPackage.SetText(deviceId);
            Clipboard.SetContent(dataPackage);
            ShowCopySuccess("Скопировано");
        }
        public async void ShowUpdate(string appData, string version)
        {
            if (_currentDialog != null && _currentDialog.IsLoaded)
            {
                _currentDialog.Hide();
            }
            ContentDialog dialog = new ContentDialog
            {
                CloseButtonText = "Отмена",
                PrimaryButtonText = "Обновить",
                PrimaryButtonCommand = new RelayCommand(async () => { Update(appData); }),

                Content = new StackPanel
                {
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uECC5"
                        },
                        new TextBlock
                        {
                            Text = "v"+version,
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
        private async void Update(string appData)
        {
            text_download.Text = "Ожидание загрузки";
            check_update.IsEnabled = false;


            // Проверка наличия файла
            
            try
            {
                string downloadsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(downloadsFolderPath);
                StorageFile file = await downloadFolder.CreateFileAsync("update.appx", CreationCollisionOption.ReplaceExisting);
                await file.DeleteAsync();
            }
            catch
            {
                // Файл не существует, выполняем соответствующие действия
                // Например, выводим сообщение об ошибке или предупреждение
                // или выполняем другую логику при необходимости
            }

            string fileUrl = appData;
            string savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Downloads", "update.appx");



            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
                    client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");

                    HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        string downloadsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                        StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(downloadsFolderPath);
                        StorageFile file = await downloadFolder.CreateFileAsync("update.appx", CreationCollisionOption.ReplaceExisting);

                        using (Stream fileStream = await file.OpenStreamForWriteAsync())
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            long totalBytesRead = 0;
                            long totalBytes = response.Content.Headers.ContentLength ?? 0;

                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                text_download.Text = "Загрузка";
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                progressBar.Value = (double)totalBytesRead / totalBytes * 100;
                            }
                        }
                    }
                    text_download.Text = "Загрузка завершена";
                    check_update.IsEnabled = true;
                    Debug.WriteLine("Файл успешно скачан и сохранен.");
                    string downloadsFolderPath3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    StorageFolder downloadFolder1 = await StorageFolder.GetFolderFromPathAsync(downloadsFolderPath3);
                    await Launcher.LaunchFileAsync(await downloadFolder1.GetFileAsync("update.appx"));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Произошла ошибка при скачивании файла: " + e.Message);
                }
            }
        }

        // Класс для RelayCommand
        
        private async void check_update_Click(object sender, RoutedEventArgs e)
        {
            string connStr = "server=188.225.18.53;user=defaultuser;database=default_db;port=3306;password=bbnn7788";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                // Получаем последнюю версию приложения из базы данных
                string getVersionSql = "SELECT version, app FROM AppUpdates ORDER BY version DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(getVersionSql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string dbVersion = reader.GetString(0);
                        string appData = (string)reader[1];

                        // Получаем текущую версию приложения
                        PackageVersion version = Package.Current.Id.Version;
                        string appVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                        Debug.WriteLine(dbVersion);
                        // Сравниваем версии
                        if (string.Compare(dbVersion, appVersion) > 0)
                        {
                            ShowUpdate(appData, dbVersion);
                            
                        }
                    }
                }
            }
        }

        private void enable_user_smtp_Checked(object sender, RoutedEventArgs e)
        {
            enable_default_smtp.IsChecked = false;
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            var dbHandler = new SmtpSettingsDatabaseHandler(connectionString);
            var smtpSettingsToSet = new SmtpSettings(smtp_server.Text, int.Parse(smtp_port.Text), true);
            dbHandler.SetSmtpSettings(smtpSettingsToSet);
        }

        private void enable_default_smtp_Checked(object sender, RoutedEventArgs e)
        {
            enable_user_smtp.IsChecked = false;
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            var dbHandler = new SmtpSettingsDatabaseHandler(connectionString);
            var smtpSettingsToSet = new SmtpSettings("smtp.gmail.com", 587, false);
            dbHandler.SetSmtpSettings(smtpSettingsToSet);
        }

        private void smtp_server_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            var dbHandler = new SmtpSettingsDatabaseHandler(connectionString);
            var smtpSettings = dbHandler.GetSmtpSettings();

            var smtpSettingsToSet = new SmtpSettings(smtp_server.Text, smtpSettings.SmtpPort, smtpSettings.Custom);
            dbHandler.SetSmtpSettings(smtpSettingsToSet);
        }

        private void smtp_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            var dbHandler = new SmtpSettingsDatabaseHandler(connectionString);
            var smtpSettings = dbHandler.GetSmtpSettings();

            var smtpSettingsToSet = new SmtpSettings(smtpSettings.SmtpServer, int.Parse(smtp_port.Text), smtpSettings.Custom);
            dbHandler.SetSmtpSettings(smtpSettingsToSet);
        }

        private void config_grid_Loaded(object sender, RoutedEventArgs e)
        {
            reload_data_Click();
        }
    }
}
