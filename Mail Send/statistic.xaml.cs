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
using Newtonsoft.Json;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using Syncfusion.UI.Xaml.Charts;
using System.Diagnostics;
using Windows.UI;
using Microsoft.UI;
using Newtonsoft.Json.Linq;
using System.Threading;
using Windows.UI.Core;
using static Mail_Send.email;
using static Mail_Send.home;
using System.Drawing.Printing;
using System.Threading.Tasks;
using static WinUI3Localizer.LanguageDictionary;
using PayPal.Api;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx.Messaging;
using MySql.Data.MySqlClient;
using DeviceId;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Mail_Send
{
    

    public sealed partial class statistic : Page
    {
        static CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;
        public class ListItem
        {
            public string Date
            {
                get; set;
            }
            public int Count
            {
                get; set;
            }
        }
        public static List<ListItem> FilterByTime(List<logs> inputList, string timeFrame)
        {
            DateTime now = DateTime.Now;
            List<ListItem> resultList = new List<ListItem>();

            switch (timeFrame)
            {
                case "today":
                    var groupedByHour = inputList.Where(item => item.date.Date == now.Date && !item.error)
                                                 .GroupBy(item => item.date.Hour)
                                                 .Select(group => new ListItem { Date = group.Key.ToString(), Count = group.Count() })
                                                 .ToList();
                    resultList.AddRange(groupedByHour);
                    break;
                case "week":
                    var groupedByDayOfWeek = inputList.Where(item => item.date >= now.Date.AddDays(-(int)now.DayOfWeek) &&
                                                                      item.date < now.Date.AddDays(7 - (int)now.DayOfWeek) && !item.error)
                                                      .GroupBy(item => item.date.Day)
                                                      .Select(group => new ListItem { Date = group.Key.ToString(), Count = group.Count() })
                                                      .ToList();
                    resultList.AddRange(groupedByDayOfWeek);
                    break;
                case "month":
                    var groupedByDayOfMonth = inputList.Where(item => item.date.Year == now.Year && item.date.Month == now.Month && !item.error)
                                                       .GroupBy(item => item.date.Day)
                                                       .Select(group => new ListItem { Date = group.Key.ToString(), Count = group.Count() })
                                                       .ToList();
                    resultList.AddRange(groupedByDayOfMonth);
                    break;
                case "year":
                    var groupedByMonthOfYear = inputList.Where(item => item.date.Year == now.Year && !item.error)
                                                        .GroupBy(item => item.date.Month)
                                                        .Select(group => new ListItem { Date = group.Key.ToString(), Count = group.Count() })
                                                        .ToList();
                    resultList.AddRange(groupedByMonthOfYear);
                    break;
            }

            return resultList;
        }
        public class ViewModel
        {
            public List<ListItem> Data_days
            {
                get; set;
            }
            public List<ListItem> Data_week
            {
                get; set;
            }
            public List<ListItem> Data_month
            {
                get; set;
            }
            public List<ListItem> Data_year
            {
                get; set;
            }
            public ViewModel()
            {


                List<logs> all_logs = home.log_sql.Get_Logs(); // Ваш исходный список
                Data_days = FilterByTime(all_logs, "today");
                Data_week = FilterByTime(all_logs, "week");
                Data_month = FilterByTime(all_logs, "month");
                Data_year = FilterByTime(all_logs, "year");



            }

        }
        public class test
        {
            public string date
            {
                get; set;
            }
            public double value
            {

                get; set;
            }
            public string tooltip
            {
                get; set;
            }
        }

        
        public class Person
        {
            public string Name
            {
                get; set;
            }

            public double Height
            {
                get; set;
            }
        }
        public class convert_log
        {
            public string icon
            {
                get; set;
            }
            public SolidColorBrush color
            {
                get;set;
            }
            public string to_send
            {
                get; set;
            }
            public string time
            {
                get;set;
            }
            public string tooltip
            {
                get;set;
            }
        }
        public List<convert_log> Items
        {
            get; set;
        }
        public void RemoveItemsNotInSecondList(List<LogData> firstList, List<logs> secondList)
        {
            firstList.RemoveAll(item => !secondList.Any(x => $"{x.date_created.Hour}:{x.date_created.Minute}:{x.date_created.Second} {x.date_created.Day}.{x.date_created.Month}.{x.date_created.Year}" == $"{item.Date.Hour}:{item.Date.Minute}:{item.Date.Second} {item.Date.Day}.{item.Date.Month}.{item.Date.Year}"));
        }
        public void DeleteAndInsert(List<LogData> logs, string deviceId)
        {
            string connectionString = "server=188.225.18.53;user=defaultuser;database=log;port=3306;password=bbnn7788;";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Optional: Use a transaction
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;

                    // Delete operation
                    string deleteQuery = "DELETE FROM log WHERE deviceid = @deviceId";
                    command.CommandText = deleteQuery;
                    command.Parameters.AddWithValue("@deviceId", deviceId);
                    command.ExecuteNonQuery();

                    // Insert operation
                    string insertQuery = "INSERT INTO log (log_id, deviceid, date) VALUES (@logId, @deviceId, @date)";
                    foreach (LogData log in logs)
                    {
                        command.Parameters.Clear();
                        command.CommandText = insertQuery;
                        command.Parameters.AddWithValue("@logId", log.LogId);
                        command.Parameters.AddWithValue("@deviceId", deviceId); // deviceId is same, could be optimized
                        command.Parameters.AddWithValue("@date", log.Date);
                        command.ExecuteNonQuery();
                    }

                    // Optional: Commit the transaction
                    transaction.Commit();
                }
            }
        }

        async Task read_search()
        {
            
        }
        public statistic()
        {
            this.InitializeComponent();
            logsss.ItemsSource = items;
            


        }

        private void statistic_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)statistic_select.SelectedItem;
            
            if (item.Content.ToString() == ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("day"))
            {
                Debug.WriteLine(item.Content);
                series.ItemsSource = (new ViewModel()).Data_days;
            }
            if (item.Content.ToString() == ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("week"))
            {
                Debug.WriteLine(item.Content);
                series.ItemsSource = (new ViewModel()).Data_week;
            }
            if (item.Content.ToString() == ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("month"))
            {
                Debug.WriteLine(item.Content);
                series.ItemsSource = (new ViewModel()).Data_month;
            }
            if (item.Content.ToString() == ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("year"))
            {
                Debug.WriteLine(item.Content);
                series.ItemsSource = (new ViewModel()).Data_year;
            }


        }
        static void MyTask()
        {
            // Выполняем задачу
            // ...
        }
        void   gets_logs(Action<bool, logs> callback)
        {
            var all_logs = home.log_sql.Get_Logs();
            do
            {

                logs log = all_logs[all_logs.Count - 1] as logs;
                if (log.error)
                {
                    callback(true, log);
                    //if (this.DispatcherQueue.HasThreadAccess)
                    //{
                    //    convert_log logs = new convert_log
                    //    {
                    //        icon = "\uE711",
                    //        color = new SolidColorBrush(Colors.Red),
                    //        to_send = log.to_send,
                    //        time = log.date.ToString("dd.MM.yyyy HH:mm")
                    //    };
                    //    //List<convert_log> last_logs = logsss.ItemsSource as List<convert_log>;

                    //    //if (last_logs != null)
                    //    //{

                    //    //    last_logs.Add(logs);
                    //    //}
                    //    //else
                    //    //{
                    //    //    last_logs = new List<convert_log> { logs };
                    //    //}
                        


                    //}
                    //else
                    //{
                    //    bool isQueued = this.DispatcherQueue.TryEnqueue(
                    //    Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                    //    () =>
                    //    {
                    //        convert_log logs = new convert_log
                    //        {
                    //            icon = "\uE711",
                    //            color = new SolidColorBrush(Colors.Red),
                    //            to_send = log.to_send,
                    //            time = log.date.ToString("dd.MM.yyyy HH:mm")
                    //        };
                    //        callback(logs);




                    //    });

                    //}
                }
                else
                {
                    callback(false, log);
                    //if (this.DispatcherQueue.HasThreadAccess)
                    //{
                    //    convert_log logs = new convert_log
                    //    {
                    //        icon = "\uE73E",
                    //        color = new SolidColorBrush(Colors.Green),
                    //        to_send = log.to_send,
                    //        time = log.date.ToString("dd.MM.yyyy HH:mm")
                    //    };
                    //    callback(logs);


                    //    //collectionViewSource.Source = collectionViewSource.Source;
                    //}
                    //else
                    //{
                    //    bool isQueued = this.DispatcherQueue.TryEnqueue(
                    //    Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                    //    () =>
                    //    {
                    //        convert_log logs = new convert_log
                    //        {
                    //            icon = "\uE73E",
                    //            color = new SolidColorBrush(Colors.Green),
                    //            to_send = log.to_send,
                    //            time = log.date.ToString("dd.MM.yyyy HH:mm")
                    //        };
                    //        callback(logs);



                    //    });

                    //}
                    //if (this.DispatcherQueue.HasThreadAccess)
                    //{
                    //    all_time_count.Text = (Convert.ToInt32(all_time_count.Text) + 1).ToString();
                    //}
                    //else
                    //{
                    //    bool isQueued = this.DispatcherQueue.TryEnqueue(
                    //    Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                    //    () =>
                    //    {
                    //        all_time_count.Text = (Convert.ToInt32(all_time_count.Text) + 1).ToString();
                    //    });
                    //}
                }

                all_logs.RemoveAt(all_logs.Count - 1);

            } while (all_logs.Count != 0);
        }
        public class LogData
        {
            public int LogId
            {
                get; set;
            }

            public DateTime Date
            {
                get; set;
            }
        }

        public List<LogData> GetDataByDeviceId(string deviceId)
        {
            string connectionString = "server=188.225.18.53;user=defaultuser;database=log;port=3306;password=bbnn7788;";
            var logData = new List<LogData>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var command = new MySqlCommand("SELECT log_id, date FROM log WHERE deviceid = @deviceid", connection);
                command.Parameters.AddWithValue("@deviceid", deviceId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = new LogData
                        {
                            LogId = reader.GetInt32("log_id"),
                            Date = DateTime.Parse(reader.GetString("date"))
                        };

                        logData.Add(data);
                    }
                }
            }

            return logData;
        }


        public static class Pause
        {
            // Static variable that must be initialized at run time.
            public static bool is_all_pause;
            public static List<logs> logs;
        }
        public void MyMethod()
        {
            // Вызов метода Cancel() для отмены задачи
            source.Cancel();

            // Создание нового объекта CancellationTokenSource для сброса токена
            source = new CancellationTokenSource();
            token = source.Token;
        }
        public async void UpdateLog(int id, bool error, string to_send, DateTime date, DateTime date_created, string from_send, string message, bool is_read)
        {
            // Создаем строку подключения к базе данных
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            string connectionString = $"Filename={dbpath}";

            // Открываем подключение к базе данных
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.OpenAsync();

                // Создаем команду для обновления строк в таблице logs
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE logs SET error = @error, to_send = @to_send, date = @date, date_created = @date_created, from_send = @from_send, message = @message, is_read = @is_read WHERE id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@error", error);
                command.Parameters.AddWithValue("@to_send", to_send);
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@date_created", date_created);
                command.Parameters.AddWithValue("@from_send", from_send);
                command.Parameters.AddWithValue("@message", message);
                command.Parameters.AddWithValue("@is_read", is_read);

                // Выполняем команду
                command.ExecuteNonQueryAsync();
            }
        }
        public async void all_load(List<logs> all_logs)
        {
            MyMethod();
            if (this.DispatcherQueue.HasThreadAccess)
            {
                all_read_count.Text = "0";
            }
            else
            {
                bool isQueued = this.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                async () => {
                    all_read_count.Text = "0";

                });
            }
            
            var all_email = Email_Sql.Get_email();
            if (all_email != null)
            {
                try
                {
                    if (all_email[0] != null)
                    {
                        if (this.DispatcherQueue.HasThreadAccess)
                        {
                            will_send.Text = email.ExtractEmails(all_email[0].email).Count.ToString();
                        }
                        else
                        {
                            bool isQueued = this.DispatcherQueue.TryEnqueue(
                            Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                            async () => {
                                will_send.Text = email.ExtractEmails(all_email[0].email).Count.ToString();

                            });
                        }
                        
                    }
                }
                catch
                {

                }

            }

            var all_log = all_logs;
            if (Pause.logs != null && Pause.logs.Count != 0)
            {
                if (this.DispatcherQueue.HasThreadAccess)
                {
                    all_read_count.Text = all_log.Count(item => item.is_read).ToString();
                }
                else
                {
                    bool isQueued = this.DispatcherQueue.TryEnqueue(
                    Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                    async () => {
                        all_read_count.Text = all_log.Count(item => item.is_read).ToString();

                    });
                }
                
            }
            else
            {
                string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                var all_read = GetDataByDeviceId(deviceId);
                foreach (var log in all_log)
                {
                    foreach (var item in all_read)
                    {
                        if (item.LogId == log.id)
                        {
                            if ($"{item.Date.Hour}:{item.Date.Minute}:{item.Date.Second} {item.Date.Day}.{item.Date.Month}.{item.Date.Year}" == $"{log.date_created.Hour}:{log.date_created.Minute}:{log.date_created.Second} {item.Date.Day}.{item.Date.Month}.{item.Date.Year}")
                            {
                                log.is_read = true;
                                if (this.DispatcherQueue.HasThreadAccess)
                                {
                                    all_read_count.Text = (int.Parse(all_read_count.Text) + 1).ToString();
                                }
                                else
                                {
                                    bool isQueued = this.DispatcherQueue.TryEnqueue(
                                    Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                    async () => {
                                        all_read_count.Text = (int.Parse(all_read_count.Text) + 1).ToString();

                                    });
                                }
                                
                            }

                        }
                    }

                }
                RemoveItemsNotInSecondList(all_read, all_log);
                DeleteAndInsert(all_read, deviceId);
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                string connectionString = $"Filename={dbpath}";
                foreach (var log in all_log)
                {
                    UpdateLog(id: log.id, log.error, log.to_send, log.date, log.date_created, log.from_send, log.message, log.is_read);
                }
                Pause.logs = all_log.ToList();

            }
            
            
            if (all_logs.Count != 0)
            {
                do
                {
                    logs log = all_logs[all_logs.Count - 1] as logs;
                    if (log.error)
                    {
                        if (this.DispatcherQueue.HasThreadAccess)
                        {
                            convert_log logs = new convert_log
                            {
                                icon = "\ue5cd",
                                tooltip = $"С аккаунта: {log.from_send}\nСостояние отправки: {(log.error ? "Не доставленно" : "Доставленно")}\nТекст Ошибки: {log.message}",
                                color = new SolidColorBrush(Colors.Red),
                                to_send = log.to_send,
                                time = log.date.ToString("dd.MM.yyyy HH:mm")
                            };
                            items.Add(logs);
                        }
                        else
                        {
                            bool isQueued = this.DispatcherQueue.TryEnqueue(
                            Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                            async () => {
                                convert_log logs = new convert_log
                                {
                                    icon = "\ue5cd",
                                    tooltip = $"С аккаунта: {log.from_send}\nСостояние отправки: {(log.error ? "Не доставленно" : "Доставленно")}\nТекст Ошибки: {log.message}",
                                    color = new SolidColorBrush(Colors.Red),
                                    to_send = log.to_send,
                                    time = log.date.ToString("dd.MM.yyyy HH:mm")
                                };
                                items.Add(logs);
                                
                                

                            });
                        }
                        all_logs.RemoveAt(all_logs.Count - 1);
                        await Task.Delay(1);
                    }
                    else
                    {
                        if (log.is_read)
                        {
                            if (this.DispatcherQueue.HasThreadAccess)
                            {
                                convert_log logs = new convert_log
                                {
                                    icon = "\ue877",
                                    tooltip = $"С аккаунта: {log.from_send}\nСостояние отправки: {(log.error ? "Не доставленно" : "Доставленно")}\nТекст Ошибки: {log.message}",
                                    color = new SolidColorBrush(Colors.Cyan),
                                    to_send = log.to_send,
                                    time = log.date.ToString("dd.MM.yyyy HH:mm")
                                };
                                items.Add(logs);

                                all_time_count.Text = (Convert.ToInt32(all_time_count.Text) + 1).ToString();
                                
                            }
                            else
                            {
                                bool isQueued = this.DispatcherQueue.TryEnqueue(
                                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                async () => {
                                    convert_log logs = new convert_log
                                    {
                                        icon = "\ue877",
                                        tooltip = $"С аккаунта: {log.from_send}\nСостояние отправки: {(log.error ? "Не доставленно" : "Доставленно")}\nТекст Ошибки: {log.message}",
                                        color = new SolidColorBrush(Colors.Cyan),
                                        to_send = log.to_send,
                                        time = log.date.ToString("dd.MM.yyyy HH:mm")
                                    };
                                    items.Add(logs);

                                    all_time_count.Text = (Convert.ToInt32(all_time_count.Text) + 1).ToString();

                                });
                            }
                            all_logs.RemoveAt(all_logs.Count - 1);
                            await Task.Delay(1);
                        }
                        else
                        {
                            if (this.DispatcherQueue.HasThreadAccess)
                            {
                                convert_log logs = new convert_log
                                {
                                    icon = "\ue876",
                                    tooltip = $"С аккаунта: {log.from_send}\nСостояние отправки: {(log.error ? "Не доставленно" : "Доставленно")}\nТекст Ошибки: {log.message}",
                                    color = new SolidColorBrush(Colors.Green),
                                    to_send = log.to_send,
                                    time = log.date.ToString("dd.MM.yyyy HH:mm")
                                };
                                items.Add(logs);

                                all_time_count.Text = (Convert.ToInt32(all_time_count.Text) + 1).ToString();

                            }
                            else
                            {
                                bool isQueued = this.DispatcherQueue.TryEnqueue(
                                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                async () => {
                                    convert_log logs = new convert_log
                                    {
                                        icon = "\ue876",
                                        tooltip = $"С аккаунта: {log.from_send}\nСостояние отправки: {(log.error ? "Не доставленно" : "Доставленно")}\nТекст Ошибки: {log.message}",
                                        color = new SolidColorBrush(Colors.Green),
                                        to_send = log.to_send,
                                        time = log.date.ToString("dd.MM.yyyy HH:mm")
                                    };
                                    items.Add(logs);

                                    all_time_count.Text = (Convert.ToInt32(all_time_count.Text) + 1).ToString();

                                });
                            }
                            all_logs.RemoveAt(all_logs.Count - 1);
                            await Task.Delay(1);
                        }

                    }
                } while (all_logs.Count != 0 && Pause.is_all_pause);
            }
            
        }
        public async Task loaded()
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
            var all_read = GetDataByDeviceId(deviceId);


            if (Pause.logs != null && Pause.logs.Count != 0)
            {
                all_load(Pause.logs);
            }
            else
            {
                var all_log = home.log_sql.Get_Logs();
                all_load(all_log);
            }
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Pause.is_all_pause = true;
            var taskWithState = Task.Factory.StartNew(async () =>
            {

                await loaded();


            }, token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();


        }
        class log_group
        {
            public string icon
                {
                    get; set;
                }
                public SolidColorBrush color
                {
                    get; set;
                }
                public List<logs> to_send
                {
                    get; set;
                }
                public string date
                {
                    get; set;
                }
                public int count
                {
                get; set;
                }
        }
        ObservableCollection<convert_log> items = new ObservableCollection<convert_log>();
        public async Task ShowCopySuccess(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Успех",
                Content = message,
                CloseButtonText = "ОК"
            };
            errorDialog.XamlRoot = this.XamlRoot;

            await errorDialog.ShowAsync();
        }
        public List<logs> FilterByDateRange(List<logs> items, DateTime startDate, DateTime endDate)
        {
            return items.Where(item => item.date >= startDate && item.date <= endDate).ToList();
        }
        public async void ShowDateRangeSelectionDialog(bool is_good)
        {
            // Создание DatePicker для начальной даты
            DatePicker startDatePicker = new DatePicker
            {
                Header = "Начальная дата"
            };

            // Создание DatePicker для конечной даты
            DatePicker endDatePicker = new DatePicker
            {
                Header = "Конечная дата"
            };

            // Создание StackPanel для упорядочивания DatePicker элементов
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(startDatePicker);
            stackPanel.Children.Add(endDatePicker);

            // Создание ContentDialog
            ContentDialog dateRangeDialog = new ContentDialog
            {
                Content = stackPanel,
                Title = "Выберите диапазон дат",
                PrimaryButtonText = "ОК",
                CloseButtonText = "Отмена"
            };
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Успех",
                Content = "Скопировано",
                CloseButtonText = "ОК"
            };
            
            dateRangeDialog.XamlRoot = this.XamlRoot;
            errorDialog.XamlRoot = this.XamlRoot;
            // Обработчик для кнопки ОК
            dateRangeDialog.PrimaryButtonClick += async (sender, args) =>
            {
                DateTime startDate = startDatePicker.Date.DateTime;
                DateTime endDate = endDatePicker.Date.DateTime;
                var all_logs = new List<logs>();
                if (Pause.logs != null && Pause.logs.Count != 0)
                {
                    all_logs = Pause.logs;
                }
                else
                {
                    all_logs = home.log_sql.Get_Logs();
                }
                var all_good_email_log = all_logs.FindAll(x => x.error != is_good);
                var filteredItems = FilterByDateRange(all_good_email_log, startDate, endDate);
                List<string> strings = filteredItems.Select(x => x.to_send).ToList();
                var dataPackage = new DataPackage();
                dataPackage.SetText(string.Join("\n", strings));
                Clipboard.SetContent(dataPackage);

                dateRangeDialog.Hide();
                await errorDialog.ShowAsync();
                // Обработка выбранного диапазона дат
                // Например, сохранение их в переменные класса или использование непосредственно здесь
            };

            // Показать диалог
            await dateRangeDialog.ShowAsync();
        }
        private async void copy_good_email_Click(object sender, RoutedEventArgs e)
        {
            ShowDateRangeSelectionDialog(true);

        }
        private void DoSomeWork()
        {
            // Создать уведомление о том, что фоновая задача запущена
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode("Мало данных"));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        private void copy_bad_email_Click(object sender, RoutedEventArgs e)
        {
            ShowDateRangeSelectionDialog(false);
        }
        public List<string> GetUniqueParameters(List<logs> items, string mode)
        {
            if (mode == "from_send")
            {
                return items.Select(item => item.from_send) // Выбираем MyParameter из каждого объекта
                    .Distinct()                        // Убираем дубликаты
                    .ToList();
            }
            else if(mode=="to_send")
            {
                return items.Select(item => item.to_send) // Выбираем MyParameter из каждого объекта
                       .Distinct()                        // Убираем дубликаты
                       .ToList();
            }
            else
            {
                return new List<string>();
            }
                                    // Преобразуем в List
        }
        private void sort_stat_Click(object sender, RoutedEventArgs e)
        {
            // Создаем основной контейнер для Flyout
            StackPanel panel = new StackPanel();
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            // Создаем CheckBox и ComboBox для первого пункта
            CheckBox checkBox1 = new CheckBox { Content = "По отправителям" };
            ComboBox comboBox1 = new ComboBox { Visibility = Visibility.Collapsed }; // Изначально скрыт
            checkBox1.Checked += (s, args) => {
                comboBox1.Visibility = Visibility.Visible;
            };
            checkBox1.Unchecked += async (s, args) => {
                items.Clear();
                comboBox1.Visibility = Visibility.Collapsed;
                Pause.is_all_pause = false;
                all_time_count.Text = "0";
                await Task.Delay(10);
                Pause.is_all_pause = true;
                var all_logs = new List<logs>();
                if (Pause.logs != null && Pause.logs.Count != 0)
                {
                    all_logs = Pause.logs;
                    all_load(all_logs);
                }
                else
                {
                    all_logs = home.log_sql.Get_Logs();
                    all_load(all_logs);
                }
                
            };
            var all_logs = new List<logs>();
            if (Pause.logs != null && Pause.logs.Count != 0)
            {
                all_logs = Pause.logs;
                List<string> uniqueParameters = GetUniqueParameters(all_logs, "from_send");
                foreach (string item in uniqueParameters)
                {
                    if (item != null)
                    {
                        comboBox1.Items.Add(item);
                    }

                }
            }
            else
            {
                all_logs = home.log_sql.Get_Logs();
                List<string> uniqueParameters = GetUniqueParameters(all_logs, "from_send");
                foreach (string item in uniqueParameters)
                {
                    if (item != null)
                    {
                        comboBox1.Items.Add(item);
                    }

                }
            }
            
            comboBox1.SelectionChanged += async (s, args) => {

                if (comboBox1.SelectedItem != null)
                {
                    try
                    {
                        items.Clear();
                        string selectedOption = comboBox1.SelectedItem.ToString();
                        var all_logs = new List<logs>();
                        if (Pause.logs != null && Pause.logs.Count != 0)
                        {
                            all_logs = Pause.logs;
                            var all_filtred = all_logs.FindAll(x => x.from_send == selectedOption);
                            Pause.is_all_pause = false;
                            all_time_count.Text = "0";
                            await Task.Delay(10);
                            Pause.is_all_pause = true;
                            all_load(all_filtred);
                        }
                        else
                        {
                            all_logs = home.log_sql.Get_Logs();
                            var all_filtred = all_logs.FindAll(x => x.from_send == selectedOption);
                            Pause.is_all_pause = false;
                            all_time_count.Text = "0";
                            await Task.Delay(10);
                            Pause.is_all_pause = true;
                            all_load(all_filtred);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        DoSomeWork();
                    }
                    
                }
            }; // Обработчик выбора
            panel.Children.Add(checkBox1);
            panel.Children.Add(comboBox1);



            CheckBox checkBox3 = new CheckBox { Content = "По получателям" };
            ComboBox comboBox3 = new ComboBox { Visibility = Visibility.Collapsed }; // Изначально скрыт
            checkBox3.Checked += (s, args) => {
                comboBox3.Visibility = Visibility.Visible;
            };
            checkBox3.Unchecked += async (s, args) => {
                items.Clear();
                comboBox3.Visibility = Visibility.Collapsed;
                Pause.is_all_pause = false;
                all_time_count.Text = "0";
                await Task.Delay(10);
                Pause.is_all_pause = true;
                var all_logs = new List<logs>();
                if (Pause.logs != null && Pause.logs.Count != 0)
                {
                    all_logs = Pause.logs;
                    all_load(all_logs);
                }
                else
                {
                    all_logs = home.log_sql.Get_Logs();
                    all_load(all_logs);
                }
                
            };
            List<string> uniqueParameters_to = GetUniqueParameters(all_logs, "to_send");
            Debug.WriteLine(JsonConvert.SerializeObject(uniqueParameters_to));
            foreach (string item in uniqueParameters_to)
            {
                if (item != null)
                {
                    comboBox3.Items.Add(item);
                }

            }
            comboBox3.SelectionChanged += async (s, args) => {

                if (comboBox3.SelectedItem != null)
                {
                    try
                    {
                        items.Clear();
                        string selectedOption = comboBox3.SelectedItem.ToString();
                        var all_logs = new List<logs>();
                        if (Pause.logs != null && Pause.logs.Count != 0)
                        {
                            all_logs = Pause.logs;
                            var all_filtred = all_logs.FindAll(x => x.to_send == selectedOption);
                            Pause.is_all_pause = false;
                            all_time_count.Text = "0";
                            await Task.Delay(10);
                            Pause.is_all_pause = true;
                            all_load(all_filtred);
                        }
                        else
                        {
                            all_logs = home.log_sql.Get_Logs();
                            var all_filtred = all_logs.FindAll(x => x.to_send == selectedOption);
                            Pause.is_all_pause = false;
                            all_time_count.Text = "0";
                            await Task.Delay(10);
                            Pause.is_all_pause = true;
                            all_load(all_filtred);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        DoSomeWork();
                    }
                    
                }
            }; // Обработчик выбора
            panel.Children.Add(checkBox3);
            panel.Children.Add(comboBox3);

            // Создаем CheckBox и DatePicker для второго пункта
            CheckBox checkBox2 = new CheckBox { Content = "По дате" };
            DatePicker start_datepicker = new DatePicker { Visibility = Visibility.Collapsed }; // Изначально скрыт
            DatePicker end_datepicker = new DatePicker { Visibility = Visibility.Collapsed }; // Изначально скрыт
            checkBox2.Checked += (s, args) => { 
                start_datepicker.Visibility = Visibility.Visible;
                end_datepicker.Visibility = Visibility.Visible;
            };
            checkBox2.Unchecked += async (s, args) => {
                items.Clear();
                start_datepicker.Visibility = Visibility.Collapsed;
                end_datepicker.Visibility = Visibility.Collapsed;
                Pause.is_all_pause = false;
                all_time_count.Text = "0";
                await Task.Delay(10);
                Pause.is_all_pause = true;
                var all_logs = new List<logs>();
                if (Pause.logs != null && Pause.logs.Count != 0)
                {
                    all_logs = Pause.logs;
                    all_load(all_logs);
                }
                else
                {
                    all_logs = home.log_sql.Get_Logs();
                    all_load(all_logs);
                }
                
            };
            start_datepicker.DateChanged += async (s, args) => {
                try
                {
                    items.Clear();
                    startTime = start_datepicker.Date.DateTime;
                    var all_logs = new List<logs>();
                    if (Pause.logs != null && Pause.logs.Count != 0)
                    {
                        all_logs = Pause.logs;
                        var filteredItems = FilterByDateRange(all_logs, startTime, endTime);
                        Pause.is_all_pause = false;
                        all_time_count.Text = "0";
                        await Task.Delay(10);
                        Pause.is_all_pause = true;
                        all_load(filteredItems);
                    }
                    else
                    {
                        all_logs = home.log_sql.Get_Logs();
                        var filteredItems = FilterByDateRange(all_logs, startTime, endTime);
                        Pause.is_all_pause = false;
                        all_time_count.Text = "0";
                        await Task.Delay(10);
                        Pause.is_all_pause = true;
                        all_load(filteredItems);
                    }
                    
                }
                catch (Exception ex)
                {
                    DoSomeWork();
                }
                
            }; // Обработчик выбора даты
            end_datepicker.DateChanged +=  async (s, args) => {
                try
                {
                    items.Clear();
                    endTime = end_datepicker.Date.DateTime;
                    var all_logs = new List<logs>();
                    if (Pause.logs != null && Pause.logs.Count != 0)
                    {
                        all_logs = Pause.logs;
                        var filteredItems = FilterByDateRange(all_logs, startTime, endTime);
                        Pause.is_all_pause = false;
                        all_time_count.Text = "0";
                        await Task.Delay(10);
                        Pause.is_all_pause = true;
                        all_load(filteredItems);
                    }
                    else
                    {
                        all_logs = home.log_sql.Get_Logs();
                        var filteredItems = FilterByDateRange(all_logs, startTime, endTime);
                        Pause.is_all_pause = false;
                        all_time_count.Text = "0";
                        await Task.Delay(10);
                        Pause.is_all_pause = true;
                        all_load(filteredItems);
                    }
                    
                }
                catch
                {
                    DoSomeWork();
                }
                
            }; // Обработчик выбора даты
            panel.Children.Add(checkBox2);
            panel.Children.Add(start_datepicker);
            panel.Children.Add(end_datepicker);
            // Создаем Flyout с нашим пользовательским содержимым
            Flyout flyout = new Flyout
            {
                Content = panel
            };

            // Отображаем Flyout
            flyout.ShowAt(sender as FrameworkElement);
        }
    }

}
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
