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
using System;
using System.Text;
using PayPal.Api;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;
using Mail_Send.Views;
using Microsoft.UI.Xaml.Documents;
using static Mail_Send.account;
using static Mail_Send.email;
using static Mail_Send.setting;
using Mail_Send;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenAI_API;
using gm;
using Main_Algorithm;
using WinUIEx.Messaging;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Windows.UI.Core;

using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using PayPal.Api;
using Windows.ApplicationModel.Activation;
using Windows.System;
using System.Globalization;
using static Mail_Send.home;
using System.Net.NetworkInformation;
using static Mail_Send.App;
using System.Threading;
using Microsoft.UI;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Mail_Send.ViewModels;
using Newtonsoft.Json.Linq;
using Google.Protobuf.WellKnownTypes;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Microsoft.UI.Xaml.Automation.Peers;
using MySql.Data.MySqlClient;
using Windows.ApplicationModel;
using DeviceId;
using static System.Runtime.InteropServices.JavaScript.JSType;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send
{
    public sealed class MyBackgroundTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Получить деферрал, чтобы предотвратить завершение фоновой задачи
            var deferral = taskInstance.GetDeferral();

            // Выполнить некоторую работу в фоне
            DoSomeWork();

            // Освободить деферрал, чтобы сообщить о завершении фоновой задачи
            deferral.Complete();
        }

        private void DoSomeWork()
        {
            // Создать уведомление о том, что фоновая задача запущена
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode("Завершенно"));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    ///


    public sealed partial class home : Page
    {
        

        static CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;
        public void MyMethod()
        {
            // Вызов метода Cancel() для отмены задачи
            source.Cancel();

            // Создание нового объекта CancellationTokenSource для сброса токена
            source = new CancellationTokenSource();
            token = source.Token;
        }
        private static string connectionString = "Filename==database.db";
        public class ProgressInfo
        {
            public int Id
            {
                get; set;
            }
            public double Percentage
            {
                get; set;
            }
            public DateTime Timestamp
            {
                get; set;
            }
        }

        public class ProgressTrackerDB
        {
            private readonly string connectionString;

            public ProgressTrackerDB(string dbPath)
            {
                connectionString = $"Filename={dbPath}";
                InitializeDatabase();
            }

            private void InitializeDatabase()
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var createProgressTableCommand = connection.CreateCommand();
                    createProgressTableCommand.CommandText =
                    @"
                CREATE TABLE IF NOT EXISTS ProgressInfo (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Percentage DOUBLE NOT NULL,
                    Timestamp TEXT NOT NULL
                );
            ";
                    createProgressTableCommand.ExecuteNonQuery();

                    var createStartTableCommand = connection.CreateCommand();
                    createStartTableCommand.CommandText =
                    @"
                CREATE TABLE IF NOT EXISTS StartInfo (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    IsStart INTEGER NOT NULL
                );
            ";
                    createStartTableCommand.ExecuteNonQuery();
                }
            }

            // Метод добавления состояния начала в базу данных
            public void AddStartInfo(StartInfo startInfo)
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                INSERT INTO StartInfo (IsStart)
                VALUES ($isStart);
            ";

                    command.Parameters.AddWithValue("$isStart", startInfo.IsStart ? 1 : 0);

                    command.ExecuteNonQuery();
                }
            }
            public void UpdateStartInfo(StartInfo startInfo)
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                UPDATE StartInfo
                SET IsStart = $isStart
                WHERE Id = $id;
            ";

                    command.Parameters.AddWithValue("$isStart", startInfo.IsStart ? 1 : 0);
                    command.Parameters.AddWithValue("$id", startInfo.Id);

                    var result = command.ExecuteNonQuery();

                    // Если не было обновлено ни одной записи (потому что Id не существует), добавить новую запись
                    if (result == 0)
                    {
                        AddStartInfo(startInfo);
                    }
                }
            }

            // Метод получения всех записей о состоянии начала
            public List<StartInfo> GetAllStartInfo()
            {
                var startInfoList = new List<StartInfo>();

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, IsStart FROM StartInfo;";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var startInfo = new StartInfo
                            {
                                Id = reader.GetInt32(0),
                                IsStart = reader.GetInt32(1) == 1
                            };
                            startInfoList.Add(startInfo);
                        }
                    }
                }

                return startInfoList;
            }

            private static readonly SemaphoreSlim databaseAccessSemaphore = new SemaphoreSlim(1, 1);

            public async Task AddProgress(ProgressInfo progress)
            {
                var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
                var allProgressInfo = trackerDB.GetAllProgress(); // Получение списка записей


                using (var connection = new SqliteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        if (allProgressInfo.Any()) // Проверка на наличие элементов
                        {
                            // Обновление существующих записей
                            command.CommandText =
                            @"
                        UPDATE ProgressInfo
                        SET Percentage = $percentage, Timestamp = $timestamp;";
                        }
                        else
                        {
                            // Вставка новой записи
                            command.CommandText =
                            @"
                        INSERT INTO ProgressInfo (Percentage, Timestamp)
                        VALUES ($percentage, $timestamp);
                    ";
                        }

                        command.Parameters.AddWithValue("$percentage", (double)progress.Percentage);
                        command.Parameters.AddWithValue("$timestamp", progress.Timestamp.ToString("o"));

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            public  List<ProgressInfo> GetAllProgress()
            {
                var progressList = new List<ProgressInfo>();
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                string connectionString = $"Filename={dbpath}";

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM ProgressInfo;";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var progress = new ProgressInfo
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Percentage = reader.GetInt32(reader.GetOrdinal("Percentage")),
                                Timestamp = DateTime.Parse(reader.GetString(reader.GetOrdinal("Timestamp")))
                            };
                            progressList.Add(progress);
                        }
                    }
                }

                return progressList;
            }
        }
        private home_model _Model;
        public class StartInfo
        {
            public int Id
            {
                get; set;
            }
            public bool IsStart
            {
                get; set;
            }
        }
        public async void progress_restore(Action<ProgressInfo> callback)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
                var allProgress = trackerDB.GetAllProgress();

                // Нахождение самого нового элемента прогресса
                var latestProgress = allProgress.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                callback(latestProgress);
                
            }
        }
        private void search_update(string version)
        {
            // Создать уведомление о том, что фоновая задача запущена
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode("Найдено обновление: v"+version));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        public home()
        {
            this.InitializeComponent();
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
                            search_update(dbVersion);

                        }
                    }
                }
            }
            _Model = new home_model();
            this.DataContext = _Model;
            setting_Sql.CreateBs();

            var settings = setting_Sql.Get_setting();

            log_sql.CreateBs();
            var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
            var allStartInfo = trackerDB.GetAllStartInfo();
            foreach (var startInfo in allStartInfo)
            {
                if (startInfo.IsStart)
                {
                    Action<ProgressInfo> callback = (latestProgress) =>
                    {
                        if (latestProgress != null)
                        {

                            if (this.DispatcherQueue.HasThreadAccess)
                            {
                                this.send_progress_ring.Value = latestProgress.Percentage;
                            }
                            else
                            {
                                bool isQueued = this.DispatcherQueue.TryEnqueue(
                                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                                () => this.send_progress_ring.Value = latestProgress.Percentage);
                            }

                        }
                        else
                        {
                            if (this.DispatcherQueue.HasThreadAccess)
                            {
                                this.send_progress_ring.Value = 0;
                            }
                            else
                            {
                                bool isQueued = this.DispatcherQueue.TryEnqueue(
                                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                                () => this.send_progress_ring.Value = 0);
                            }
                        }
                    };
                    start.Visibility = Visibility.Collapsed;
                    send_progress.Visibility = Visibility.Visible;

                    Thread thread = new Thread(new ThreadStart(() => progress_restore(callback)));
                    thread.Start();

                }
            }
        }
        private static home instance;

        public static home Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new home();
                }
                return instance;
            }
        }

        public class log_sql
        {
            private static string connectionString = "Filename==database.db";
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
                        CREATE TABLE IF NOT EXISTS logs (
                            id INTEGER PRIMARY KEY,
                            error TEXT NULL,
                            from_send TEXT NULL,
                            to_send TEXT NULL,
                            message TEXT NULL,
                            date TEXT NULL,
                            date_created TEXT NULL,
                            is_read TEXT NULL
                        );
                    ";
                    command.ExecuteNonQuery();

                }
            }
            public static void Delete_logs()
            {
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                connectionString = $"Filename={dbpath}";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM logs";
                    command.ExecuteNonQuery();

                }
            }
            public async static void Add_Logs(int id,bool error, string to_send, DateTime date, DateTime date_created, string? from_send = null, string? message = null, bool? is_read = null)
            {
                ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists).AsTask().Wait();
                string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                var connectionString = $"Filename={dbpath}";

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO logs (id, error, to_send, date, date_created, from_send, message, is_read)
                        VALUES ($id, $error, $to_send, $date, $date_created, $from_send, $message, $is_read);
                    ";
                    command.Parameters.AddWithValue("$id", id); // Boolean передаётся напрямую
                    command.Parameters.AddWithValue("$error", error.ToString()); // Boolean передаётся напрямую
                    command.Parameters.AddWithValue("$to_send", to_send ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$date", date.ToString("o")); // Форматирование в ISO 8601
                    command.Parameters.AddWithValue("$date_created", date_created.ToString("o")); // Форматирование в ISO 8601
                    command.Parameters.AddWithValue("$from_send", from_send ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$message", message ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$is_read", is_read.ToString() ?? (object)DBNull.Value);
                    command.ExecuteNonQueryAsync();

                }
            }


            public static List<logs> Get_Logs()
            {
                var result = new List<logs>();
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
                        FROM logs
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Debug.WriteLine(reader.GetOrdinal("error"));
                            var log = new logs
                            {
                                
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                error = bool.Parse(reader.GetString(reader.GetOrdinal("error"))),
                                to_send = reader.GetString(reader.GetOrdinal("to_send")),
                                date = reader.GetDateTime(reader.GetOrdinal("date")),
                                date_created = reader.GetDateTime(reader.GetOrdinal("date_created")),
                                is_read = bool.Parse(reader.GetString(reader.GetOrdinal("is_read"))),

                            };

                            if (reader.IsDBNull(2))
                            {
                                log.from_send = null;
                            }
                            else
                            {
                                log.from_send = reader.GetString(reader.GetOrdinal("from_send"));
                            }
                            if (reader.IsDBNull(4))
                            {

                                log.message = null;
                            }
                            else
                            {

                                log.message = reader.GetString(reader.GetOrdinal("message"));
                            }

                            result.Add(log);


                            // Добавьте объект text в список или используйте его по своему усмотрению
                        }
                    }
                }

                return result;
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
            errorDialog.XamlRoot = this.XamlRoot;

            await errorDialog.ShowAsync();
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
        public DateTime GetCurrentDate()
        {
            DateTime currentDate = DateTime.Now;
            return currentDate;
        }
        public interface IHasId
        {
            public Int64 id
            {
                get; set;
            }
        }

        public void ResetIdsInOrder<T>(List<T> items) where T : IHasId
        {
            int currentId = 1;
            foreach (var item in items)
            {
                item.id = currentId++;
            }
        }

        
        int int_thread = 0;
        private static readonly object _lock = new object();
        static void AppendTextToFile(string text)
        {
            string path = $"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "log.txt")}"; // Путь к файлу

            lock (_lock) // Блокировка для синхронизации доступа к файлу
            {
                try
                {
                    // Добавляем текст в файл
                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        writer.WriteLine(text);
                    }

                    Console.WriteLine($"Текст '{text}' успешно добавлен.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Произошла ошибка при записи в файл: " + e.Message);
                }
            }
        }
        public async Task UpdateEmailById(int id, string newEmail)
        {
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE all_email SET email = @newEmail WHERE id = @id";
                command.Parameters.AddWithValue("@newEmail", newEmail);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task EditEmail(List<Create_email> all_email)
        {

            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM all_email";
                await command.ExecuteNonQueryAsync();
                foreach (Create_email acc1 in all_email)
                {

                    Email_Sql.Add_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                }

            }
        }
        public static class Pause
        {
            // Static variable that must be initialized at run time.
            public static bool is_pause;
        }
        public static class logid
        {
            public static List<int> all_removed;
            public static List<string> all_email;
            // Static variable that must be initialized at run time.
            public static int id_log;
        }
        static List<string> NormalizeIds(List<string> items)
        {
            Debug.WriteLine(JsonConvert.SerializeObject(items));
            // Разбиваем строки и создаем список пар (текст, id)
            var splitItems = items.Select(item => {
                var parts = item.Split('&');
                return new { Text = parts[0], Id = int.Parse(parts[1]) };
            }).ToList();

            // Сортируем список по Id
            splitItems.Sort((a, b) => a.Id.CompareTo(b.Id));

            // Нормализуем Id и собираем обратно в строки
            for (int i = 0; i < splitItems.Count; i++)
            {
                splitItems[i] = new { splitItems[i].Text, Id = i + 1 };
            }

            return splitItems.Select(item => $"{item.Text}&{item.Id}").ToList();
        }
        public async Task check_email_removed(CancellationToken token)
        {
            
            while (true)
            {
                List<string> all_email = logid.all_email;
                List<int> all_id = logid.all_removed;
                List<string> selected_item = new List<string>();
                if (all_id!=null && all_id.Count != 0)
                {
                    foreach (string item in all_email)
                    {
                        if (all_id.Contains(int.Parse(item.Split("&")[1])))
                        {

                        }
                        else
                        {
                            selected_item.Add(item);
                        }
                    }
                }
                else
                {
                    selected_item = all_email;
                }
                
                logid.all_email = selected_item;
                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", selected_item))));
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }
        public async Task start_thread(List<object> account, List<object> result, bool reverse, double progress, int remainder, bool is_random, CancellationToken token)
        {
            
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            var dbHandler = new SmtpSettingsDatabaseHandler(connectionString);

            // Инициализация базы данных
            dbHandler.InitializeDatabase();

            // Получение текущих настроек SMTP
            // Retrieve and display the SMTP settings
            var smtpSettings = dbHandler.GetSmtpSettings();

            var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
            if (reverse)
            {
                List<create_account> acc = result.Select(o => o as create_account).Where(o => o != null).ToList();
                List<string> res = account.Cast<string>().ToList();
                int index_log = logid.id_log + 1;
                logid.id_log = logid.id_log + res.Count;
                for (var i = 0; i < acc.Count; i++)
                {

                    if (is_random)
                    {
                        List<email.Create_email> all_email = Email_Sql.Get_email();
                        email.Create_email email = all_email.FirstOrDefault(x => x.id == 1);
                        List<text.CreateNewTextBlock> all_text = new List<text.CreateNewTextBlock>();
                        if (email.texts != null && email.texts.Count != 0)
                        {
                            foreach (var text in email.texts)
                            {
                                all_text.Add(text);
                            };
                        }
                        if (email.folders != null)
                        {
                            if (email.folders.Count != 0)
                            {
                                foreach (var folder in email.folders)
                                {
                                    foreach (var text in folder.texts)
                                    {
                                        all_text.Add(text);
                                    }
                                }
                            }

                        }
                        Random random = new Random();
                        text.CreateNewTextBlock random_texts = all_text[random.Next(0, all_text.Count)];
                        gms gms = new gms();
                        gms.load_index_log(index_log);
                        DateTime currentDate = DateTime.Now;
                        gms.load_created_datetime(currentDate);
                        gms.smtp_load(smtpSettings.SmtpServer, smtpSettings.SmtpPort);
                        gms.login_account(acc[i].email, acc[i].app_password);

                        string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                        
                        gms.Create_message(res[i].Split("&")[0], random_texts.topic, $"<html><body>{random_texts.text}<img src='http://178.253.40.60:5050/tracking_image?device_id={deviceId}&log_id={index_log}&date={currentDate}' alt='' /></body></html>");

                        if (random_texts.files != null)
                        {
                            foreach (var files in random_texts.files)
                            {
                                gms.Add_Attachment(files.path);
                            }
                        }
                        await gms.send_message(false);
                        List<string> sort_email = ExtractEmails(email.email);
                        if (res[i].Contains("&"))
                        {
                            logid.all_email.RemoveAll(x => x.Split("&")[1] == res[i].Split("&")[1]);
                            await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                        }
                        else
                        {
                            logid.all_email.RemoveAll(x => x.Split("&")[1] == "0");
                            await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                        }
                        
                        //email.email = string.Join("\n", sort_email);
                        //await EditEmail(all_email);
                        
                        try
                        {
                            if (this.DispatcherQueue.HasThreadAccess)
                            {
                                double resul1t = (double)_Model.progress.Value + progress;
                                Debug.WriteLine($"VALUE RING: {(double)_Model.progress.Value}, progress: {progress}");
                                await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));
                            }
                            else
                            {
                                bool isQueued = this.DispatcherQueue.TryEnqueue(
                                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                async () => {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));

                                });
                            }
                        }
                        catch (Exception )
                        {
                            var allProgress = trackerDB.GetAllProgress();

                            // Нахождение самого нового элемента прогресса
                            var latestProgress = allProgress.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                            if (latestProgress != null)
                            {
                                await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)latestProgress.Percentage + (double)progress), Timestamp = DateTime.Now });
                                _Model.progress.UpdateValueAsync((double)((double)latestProgress.Percentage + (double)progress));
                            }
                            else
                            {
                                await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)progress, Timestamp = DateTime.Now });
                                _Model.progress.UpdateValueAsync(progress);
                            }

                        }

                        index_log =+ 1;
                        if (token.IsCancellationRequested)
                        {
                            Debug.WriteLine("конец");
                            if (!Pause.is_pause)
                            {
                                trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                            }




                            return;
                        }
                    }
                    else
                    {
                        List<email.Create_email> all_email = Email_Sql.Get_email();
                        email.Create_email email = all_email.FirstOrDefault(x => x.id == 1);
                        text.CreateNewTextBlock texts = email.texts[0];
                        gms gms = new gms();
                        gms.load_index_log(index_log);
                        DateTime currentDate = DateTime.Now;
                        gms.load_created_datetime(currentDate);
                        gms.smtp_load(smtpSettings.SmtpServer, smtpSettings.SmtpPort);
                        gms.login_account(acc[i].email, acc[i].app_password);
                        
                        string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                        
                        gms.Create_message(res[i].Split("&")[0], texts.topic, $"<html><body>{texts.text}<img src='http://178.253.40.60:5050/tracking_image?device_id={deviceId}&log_id={index_log}&date={currentDate}' alt='' /></body></html>");
                        if (texts.files != null)
                        {
                            foreach (var files in texts.files)
                            {
                                gms.Add_Attachment(files.path);
                            }
                        }
                        await gms.send_message(false);
                        List<string> sort_email = ExtractEmails(email.email);
                        if (res[i].Contains("&"))
                        {
                            logid.all_email.RemoveAll(x => x.Split("&")[1] == res[i].Split("&")[1]);
                            await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                        }
                        else
                        {
                            logid.all_email.RemoveAll(x => x.Split("&")[1] == "0");
                            await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                        }

                        try
                        {
                            if (this.DispatcherQueue.HasThreadAccess)
                            {
                                double resul1t = (double)_Model.progress.Value + progress;
                                Debug.WriteLine($"VALUE RING: {(double)_Model.progress.Value}, progress: {progress}");
                                await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));
                            }
                            else
                            {
                                bool isQueued = this.DispatcherQueue.TryEnqueue(
                                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                async () => {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));

                                });
                            }
                        }
                        catch
                        {
                            var allProgress = trackerDB.GetAllProgress();

                            // Нахождение самого нового элемента прогресса
                            var latestProgress = allProgress.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                            if (latestProgress != null)
                            {
                                await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)latestProgress.Percentage + (double)progress), Timestamp = DateTime.Now });
                                _Model.progress.UpdateValueAsync((double)((double)latestProgress.Percentage + (double)progress));
                            }
                            else
                            {
                                await trackerDB.AddProgress(new ProgressInfo { Percentage = progress, Timestamp = DateTime.Now });
                                _Model.progress.UpdateValueAsync(progress);
                            }

                        }
                        index_log = +1;
                        if (token.IsCancellationRequested)
                        {
                            Debug.WriteLine("конец");
                            if (!Pause.is_pause)
                            {
                                trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                            }




                            return;
                        }
                    }
                    
                }
                int pre_progress = 0;
                if (this.DispatcherQueue.HasThreadAccess)
                {
                    pre_progress = (int)this.send_progress_ring.Value;
                }
                else
                {
                    bool isQueued = this.DispatcherQueue.TryEnqueue(
                    Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                    () => pre_progress = (int)this.send_progress_ring.Value);

                }
                if (pre_progress + remainder == 100)
                {

                    trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });



                }
            }
            else
            {
                List<create_account> acc = account.Select(o => o as create_account).Where(o => o != null).ToList();
                List<string> res = result.Cast<string>().ToList();
                int index_log = logid.id_log + 1;
                logid.id_log = logid.id_log + res.Count;
                Debug.WriteLine(JsonConvert.SerializeObject(acc) + " - -- - - --JJJJJJJJJJJJJ");
                Debug.WriteLine(JsonConvert.SerializeObject(res));
                if (acc.Count == 1)
                {
                    for (var i = 0; i < res.Count; i++)
                    {

                        if (is_random)
                        {
                            List<email.Create_email> all_email = Email_Sql.Get_email();
                            email.Create_email email = all_email.FirstOrDefault(x => x.id == 1);
                            List<text.CreateNewTextBlock> all_text = new List<text.CreateNewTextBlock>();
                            if (email.texts != null && email.texts.Count != 0)
                            {
                                foreach (var text in email.texts)
                                {
                                    all_text.Add(text);
                                };
                            }
                            if (email.folders != null)
                            {
                                if (email.folders.Count != 0)
                                {
                                    foreach (var folder in email.folders)
                                    {
                                        foreach (var text in folder.texts)
                                        {
                                            all_text.Add(text);
                                        }
                                    }
                                }

                            }
                            Random random = new Random();
                            Debug.WriteLine($"{JsonConvert.SerializeObject(all_text)} - ИНТЕРЕСНО");
                            text.CreateNewTextBlock random_texts = all_text[random.Next(0, all_text.Count)];

                            gms gms = new gms();
                            gms.load_index_log(index_log);
                            DateTime currentDate = DateTime.Now;
                            gms.load_created_datetime(currentDate);
                            gms.smtp_load(smtpSettings.SmtpServer, smtpSettings.SmtpPort);
                            gms.login_account(acc[0].email, acc[0].app_password);
                            
                            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                            
                            gms.Create_message(res[i].Split("&")[0], random_texts.topic, $"<html><body>{random_texts.text}<img src='http://178.253.40.60:5050/tracking_image?device_id={deviceId}&log_id={index_log}&date={currentDate}' alt='' /></body></html>");
                            if (random_texts.files != null)
                            {
                                foreach (var files in random_texts.files)
                                {
                                    gms.Add_Attachment(files.path);
                                }
                            }
                            await gms.send_message(false);
                            List<string> sort_email = ExtractEmails(email.email);
                            if (res[i].Contains("&"))
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == res[i].Split("&")[1]);
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }
                            else
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == "0");
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }
                            try
                            {
                                if (this.DispatcherQueue.HasThreadAccess)
                                {
                                    double resul1t = (double)_Model.progress.Value + progress;
                                    Debug.WriteLine($"VALUE RING: {(double)_Model.progress.Value}, progress: {progress}");
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));
                                }
                                else
                                {
                                    bool isQueued = this.DispatcherQueue.TryEnqueue(
                                    Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                    async () => {
                                        await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                        _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));

                                    });
                                }
                            }
                            catch
                            {
                                var allProgress = trackerDB.GetAllProgress();

                                // Нахождение самого нового элемента прогресса
                                var latestProgress = allProgress.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                                if (latestProgress != null)
                                {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)latestProgress.Percentage + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)latestProgress.Percentage + (double)progress));
                                }
                                else
                                {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = progress, Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync(progress);
                                }

                            }
                            index_log += 1;
                            if (token.IsCancellationRequested)
                            {
                                Debug.WriteLine("конец");
                                if (!Pause.is_pause)
                                {
                                    trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                }




                                return;
                            }
                        }
                        else
                        {
                            List<email.Create_email> all_email = Email_Sql.Get_email();
                            email.Create_email email = all_email[0];
                            text.CreateNewTextBlock texts = email.texts[0];
                            gms gms = new gms();
                            gms.load_index_log(index_log);
                            DateTime currentDate = DateTime.Now;
                            gms.load_created_datetime(currentDate);
                            gms.smtp_load(smtpSettings.SmtpServer, smtpSettings.SmtpPort);
                            gms.login_account(acc[0].email, acc[0].app_password);
                            
                            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                            
                            gms.Create_message(res[i].Split("&")[0], texts.topic, $"<html><body>{texts.text}<img src='http://178.253.40.60:5050/tracking_image?device_id={deviceId}&log_id={index_log}&date={currentDate}' alt='' /></body></html>");
                            if (texts.files != null)
                            {
                                foreach (var files in texts.files)
                                {
                                    gms.Add_Attachment(files.path);
                                }
                            }
                            await gms.send_message(false);
                            
                            List<string> sort_email = ExtractEmails(email.email);
                            if (res[i].Contains("&"))
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == res[i].Split("&")[1]);
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }
                            else
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == "0");
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }
                            try
                            {
                                
                                if (this.DispatcherQueue.HasThreadAccess)
                                {
                                    double resul1t = (double)_Model.progress.Value + progress;
                                    Debug.WriteLine($"VALUE RING: {(double)_Model.progress.Value}, progress: {progress}");
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));
                                }
                                else
                                {
                                    bool isQueued = this.DispatcherQueue.TryEnqueue(
                                    Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                    async () => {
                                        await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                        _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));

                                    });
                                }
                            }
                            catch
                            {
                                var allProgress = trackerDB.GetAllProgress();

                                // Нахождение самого нового элемента прогресса
                                var latestProgress = allProgress.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                                if (latestProgress != null)
                                {
                                    Debug.WriteLine($"VALUE RING: {(double)latestProgress.Percentage}, progress: {progress}");

                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)latestProgress.Percentage + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)latestProgress.Percentage + (double)progress));
                                }
                                else
                                {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = progress, Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync(progress);
                                }

                            }
                            index_log += 1;
                            if (token.IsCancellationRequested)
                            {
                                Debug.WriteLine("конец");
                                if (!Pause.is_pause)
                                {
                                    trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                }




                                return;
                            }
                        }




                    }
                    int pre_progress = 0;
                    if (this.DispatcherQueue.HasThreadAccess)
                    {
                        pre_progress = (int)this.send_progress_ring.Value;
                    }
                    else
                    {
                        bool isQueued = this.DispatcherQueue.TryEnqueue(
                        Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                        () => pre_progress = (int)this.send_progress_ring.Value);

                    }
                    if (pre_progress + remainder == 100)
                    {
                        Debug.WriteLine("конец");
                        if (!Pause.is_pause)
                        {
                            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                        }

                    }
                }
                else
                {
                    for (var i = 0; i < acc.Count; i++)
                    {
                        if (is_random)
                        {
                            List<email.Create_email> all_email = Email_Sql.Get_email();
                            email.Create_email email = all_email.FirstOrDefault(x => x.id == 1);
                            List<text.CreateNewTextBlock> all_text = new List<text.CreateNewTextBlock>();
                            if (email.texts != null && email.texts.Count != 0)
                            {
                                foreach (var text in email.texts)
                                {
                                    all_text.Add(text);
                                };
                            }
                            if (email.folders != null)
                            {
                                if (email.folders.Count != 0)
                                {
                                    foreach (var folder in email.folders)
                                    {
                                        foreach (var text in folder.texts)
                                        {
                                            all_text.Add(text);
                                        }
                                    }
                                }

                            }
                            Random random = new Random();
                            text.CreateNewTextBlock random_texts = all_text[random.Next(0, all_text.Count)];
                            gms gms = new gms();
                            gms.load_index_log(index_log);
                            DateTime currentDate = DateTime.Now;
                            gms.load_created_datetime(currentDate);
                            gms.smtp_load(smtpSettings.SmtpServer, smtpSettings.SmtpPort);
                            gms.login_account(acc[i].email, acc[i].app_password);
                            
                            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                            
                            gms.Create_message(res[i].Split("&")[0], random_texts.topic, $"<html><body>{random_texts.text}<img src='http://178.253.40.60:5050/tracking_image?device_id={deviceId}&log_id={index_log}&date={currentDate}' alt='' /></body></html>");
                            if (random_texts.files != null)
                            {
                                foreach (var files in random_texts.files)
                                {
                                    gms.Add_Attachment(files.path);
                                }
                            }
                            await gms.send_message(false);
                            List<string> sort_email = ExtractEmails(email.email);
                            if (res[i].Contains("&"))
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == res[i].Split("&")[1]);
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }
                            else
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == "0");
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }

                            try
                            {
                                if (this.DispatcherQueue.HasThreadAccess)
                                {
                                    double resul1t = (double)_Model.progress.Value + progress;
                                    Debug.WriteLine($"VALUE RING: {(double)_Model.progress.Value}, progress: {progress}");
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));
                                }
                                else
                                {
                                    bool isQueued = this.DispatcherQueue.TryEnqueue(
                                    Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                    async () => {
                                        await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                        _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));

                                    });
                                }
                            }
                            catch
                            {
                                var allProgress = trackerDB.GetAllProgress();

                                // Нахождение самого нового элемента прогресса
                                var latestProgress = allProgress.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                                if (latestProgress != null)
                                {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)latestProgress.Percentage + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)latestProgress.Percentage + (double)progress));
                                }
                                else
                                {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = progress, Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync(progress);
                                }

                            }
                            index_log += 1;
                            if (token.IsCancellationRequested)
                            {
                                Debug.WriteLine("конец");
                                if (!Pause.is_pause)
                                {
                                    trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                }




                                return;
                            }
                        }
                        else
                        {
                            List<email.Create_email> all_email = Email_Sql.Get_email();
                            email.Create_email email = all_email.FirstOrDefault(x => x.id == 1);
                            text.CreateNewTextBlock texts = email.texts[0];
                            gms gms = new gms();
                            gms.load_index_log(index_log);
                            DateTime currentDate = DateTime.Now;
                            gms.load_created_datetime(currentDate);
                            gms.smtp_load(smtpSettings.SmtpServer, smtpSettings.SmtpPort);
                            gms.login_account(acc[i].email, acc[i].app_password);
                            
                            string deviceId = new DeviceIdBuilder().AddMachineName().AddUserName().ToString();
                            
                            gms.Create_message(res[i].Split("&")[0], texts.topic, $"<html><body>{texts.text}<img src='http://178.253.40.60:5050/tracking_image?device_id={deviceId}&log_id={index_log}&date={currentDate}' alt='' /></body></html>");
                            if (texts.files != null)
                            {
                                foreach (var files in texts.files)
                                {
                                    gms.Add_Attachment(files.path);
                                }
                            }
                            await gms.send_message(false);
                            List<string> sort_email = ExtractEmails(email.email);
                            if (res[i].Contains("&"))
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == res[i].Split("&")[1]);
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }
                            else
                            {
                                logid.all_email.RemoveAll(x => x.Split("&")[1] == "0");
                                await UpdateEmailById(1, string.Join("\n", ExtractEmails(string.Join("\n", logid.all_email))));
                            }
                            try
                            {
                                if (this.DispatcherQueue.HasThreadAccess)
                                {
                                    double resul1t = (double)_Model.progress.Value + progress;
                                    Debug.WriteLine($"VALUE RING: {(double)_Model.progress.Value}, progress: {progress}");
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));
                                }
                                else
                                {
                                    bool isQueued = this.DispatcherQueue.TryEnqueue(
                                    Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                                    async () => {
                                        await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)_Model.progress.Value + (double)progress), Timestamp = DateTime.Now });
                                        _Model.progress.UpdateValueAsync((double)((double)_Model.progress.Value + (double)progress));

                                    });
                                }
                            }
                            catch
                            {
                                var allProgress = trackerDB.GetAllProgress();

                                // Нахождение самого нового элемента прогресса
                                var latestProgress = allProgress.OrderByDescending(p => p.Timestamp).FirstOrDefault();

                                if (latestProgress != null)
                                {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = (double)((double)latestProgress.Percentage + (double)progress), Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync((double)((double)latestProgress.Percentage + (double)progress));
                                }
                                else
                                {
                                    await trackerDB.AddProgress(new ProgressInfo { Percentage = progress, Timestamp = DateTime.Now });
                                    _Model.progress.UpdateValueAsync(progress);
                                }

                            }
                            index_log += 1;
                            if (token.IsCancellationRequested)
                            {
                                Debug.WriteLine("конец");
                                if (!Pause.is_pause)
                                {
                                    trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                }




                                return;
                            }
                        }

                    }
                    int pre_progress = 0;
                    if (this.DispatcherQueue.HasThreadAccess)
                    {
                        pre_progress = (int)this.send_progress_ring.Value;
                    }
                    else
                    {
                        bool isQueued = this.DispatcherQueue.TryEnqueue(
                        Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                        () => pre_progress = (int)this.send_progress_ring.Value);

                    }
                    if (pre_progress + remainder == 100)
                    {
                        if (!Pause.is_pause)
                        {
                            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                        }
                        Debug.WriteLine("конец");
                    }

                }

            }
        }
        public class united_texts
        {
            public bool NullLabel
            {
                get; set;
            }
            public List<text.CreateNewTextBlock> Text
            {
                get; set;
            }
        }
        public united_texts ProcessNimaiClass(email.Create_email input)
        {
            var combinedTexts = new List<text.CreateNewTextBlock>();

            // Добавляем все объекты MyClass из Text, если список не null и не пуст
            if (input.texts != null && input.texts.Count > 0)
            {
                combinedTexts.AddRange(input.texts);
            }

            // Перебираем все FolderClass в Folders, если список не null и не пуст
            if (input.folders != null && input.folders.Count > 0)
            {
                foreach (var folder in input.folders)
                {
                    // Добавляем содержимое Texts из каждого FolderClass, если оно не null и не пусто
                    if (folder.texts != null && folder.texts.Count > 0)
                    {
                        combinedTexts.AddRange(folder.texts);
                    }
                }
            }

            // Создаем объект Result и проверяем, пуст ли итоговый список
            var result = new united_texts
            {
                NullLabel = combinedTexts.Count == 0, // если список пуст, NullLabel будет true
                Text = combinedTexts
            };

            // Преобразуем результат в JSON строку
            return result;
        }

        public void background_start()
        {
            start_Click(start, new RoutedEventArgs());
        }
        public void SaveToCache(string key, object value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
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
        public async void start_Click(object sender, RoutedEventArgs e)
        {
            MyMethod();
            logid.all_removed = new List<int>();
            logid.id_log = home.log_sql.Get_Logs()!=null&&home.log_sql.Get_Logs().Count!=0 ? home.log_sql.Get_Logs().OrderByDescending(x => x.id).FirstOrDefault().id : 0;
            Action<ProgressInfo> callback = (latestProgress) =>
            {
                if (latestProgress != null)
                {

                    if (this.DispatcherQueue.HasThreadAccess)
                    {
                        this.send_progress_ring.Value = latestProgress.Percentage;
                    }
                    else
                    {
                        bool isQueued = this.DispatcherQueue.TryEnqueue(
                        Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                        () => this.send_progress_ring.Value = latestProgress.Percentage);
                    }

                }
                else
                {
                    if (this.DispatcherQueue.HasThreadAccess)
                    {
                        this.send_progress_ring.Value = 0;
                    }
                    else
                    {
                        bool isQueued = this.DispatcherQueue.TryEnqueue(
                        Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                        () => this.send_progress_ring.Value = 0);
                    }
                }
            };
            Pause.is_pause = false;
            Thread thread = new Thread(new ThreadStart(() => progress_restore(callback)));
            thread.Start();
            var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = true });
            start.Visibility = Visibility.Collapsed;
            send_progress.Visibility = Visibility.Visible;
            List<create_account> acc = new List<create_account>();
            List<create_account> man = new List<create_account>();
            var all_sett = setting_Sql.Get_setting();

            int number_thread = 0;
            try
            {
                number_thread = int.Parse(GetFromCache<string>("threads"));
            }
            catch
            {

            }
            if (Email_Sql.Get_email() != null||Email_Sql.Get_email().Count!=0||Email_Sql.Get_email()==new List<Create_email>())
            {
                string allm = "";
                try
                {
                    allm = Email_Sql.Get_email()[0].email;
                } 
                catch
                {

                }

                var allm1 = ExtractEmails(allm).Count;
                if (number_thread > allm1)
                {
                    await ShowErrorDialog("Количество потоков больше чем количество почт");
                    trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                    try
                    {
                        this.send_progress_ring.Value = 0;
                        send_progress.Visibility = Visibility.Collapsed;
                        start.Visibility = Visibility.Visible;
                    }
                    catch
                    {

                    }
                }
                else
                {
                    var type_setting = "auto";
                    if(GetFromCache<string>("type_setting")!=null&& GetFromCache<string>("type_setting") != "")
                    {
                        type_setting = GetFromCache<string>("type_setting");
                    }
                    if (type_setting == "auto")
                    {
                        var auto = GetFromCache<string>("auto");
                        if (auto != null && auto != "")
                        {
                            all_sett.auto = JsonConvert.DeserializeObject<List<automatic_account>>(auto);
                            var all_account = SqliteFunctions.SelectAllRows();
                            foreach (var account in all_account)
                            {
                                int acc_id = account.id;
                                Debug.WriteLine($"{JsonConvert.SerializeObject(all_sett.auto)}");
                                var selected_auto = all_sett.auto.FirstOrDefault(x => x.id == acc_id);
                                Debug.WriteLine(JsonConvert.SerializeObject(selected_auto));
                                if (selected_auto.is_enabled)
                                {
                                    if (account.is_verified)
                                    {
                                        acc.Add(account);
                                    }


                                }
                            }
                        }
                        else
                        {
                            await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("account_not_verified"));
                            DoSomeWork();
                            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                            try
                            {
                                this.send_progress_ring.Value = 0;
                                send_progress.Visibility = Visibility.Collapsed;
                                start.Visibility = Visibility.Visible;
                            }
                            catch
                            {


                            }


                        }
                        Create_email all_email = new Create_email();
                        try
                        {
                            all_email = Email_Sql.Get_email().FirstOrDefault(x => x.id == 1);
                            if (all_email == null)
                            {
                                all_email = new Create_email();
                            }
                        }
                        catch
                        {

                        }
                        
                        Debug.WriteLine(JsonConvert.SerializeObject(all_email));

                        if (string.IsNullOrEmpty(all_email.email))
                        {
                            Debug.WriteLine("ВВВОТ");
                            await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("email_empty"));
                            DoSomeWork();
                            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                            try
                            {
                                this.send_progress_ring.Value = 0;
                                send_progress.Visibility = Visibility.Collapsed;
                                start.Visibility = Visibility.Visible;
                            }
                            catch
                            {


                            }
                        }
                        else
                        {
                            List<string> sort_email = ExtractEmails(all_email.email);

                            if (sort_email.Count != 0)
                            {
                                for (var i = 0; i < sort_email.Count; i++)
                                {
                                    sort_email[i] = sort_email[i] + "&" + i;
                                }
                            }
                            else
                            {
                                await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("email_empty"));
                                DoSomeWork();
                                trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                try
                                {
                                    this.send_progress_ring.Value = 0;
                                    send_progress.Visibility = Visibility.Collapsed;
                                    start.Visibility = Visibility.Visible;
                                }
                                catch
                                {

                                }

                            }
                            if (sort_email.Count != 0)
                            {
                                logid.all_email = sort_email;
                                if (acc.Count == 1)
                                {
                                    
                                    Algoritm.in_Thread result = new Algoritm.in_Thread()
                                    {
                                        account = new List<List<object>> { acc.Cast<object>().ToList() },
                                        result = new List<List<object>> { sort_email.Cast<object>().ToList() }
                                    };
                                    Debug.WriteLine(JsonConvert.SerializeObject(result));
                                    Algoritm.in_Thread threads = Algoritm.generate_thread(result, number_thread);
                                    Debug.WriteLine($"{JsonConvert.SerializeObject(threads)}  -- - - - --fffffSdadsdsad");
                                    gms gms = new gms();
                                    int max_value = 100;
                                    int num_elements = threads.result.SelectMany(list => list).Count();
                                    double value_increment = (double)max_value / num_elements;
                                    int remainder = max_value % num_elements;
                                    Debug.WriteLine($"num_elements: {num_elements}, value_increment: {value_increment}, remainder: {remainder}");
                                    List<Task> tasks = new List<Task>();
                                    for (var i = 0; i < threads.result.Count; i++)
                                    {
                                        //if (all_email.is_random)
                                        //{
                                        //    List<text.CreateNewTextBlock> all_text = new List<text.CreateNewTextBlock>();
                                        //    if (all_email.texts != null || all_email.texts.Count != 0)
                                        //    {
                                        //        foreach (var text in all_email.texts)
                                        //        {
                                        //            all_text.Add(text);
                                        //        };
                                        //    }
                                        //    if (all_email.folders != null)
                                        //    {
                                        //        if (all_email.folders.Count != 0)
                                        //        {
                                        //            foreach (var folder in all_email.folders)
                                        //            {
                                        //                foreach (var text in folder.texts)
                                        //                {
                                        //                    all_text.Add(text);
                                        //                }
                                        //            }
                                        //        }

                                        //    }
                                        if (!ProcessNimaiClass(all_email).NullLabel)
                                        {

                                            //var parameters = new Tuple<List<object>, List<object>, bool, double, int, bool, System.Threading.CancellationToken>(threads.account[i], threads.result[i], threads.reverse, value_increment, remainder, all_email.is_random, token);
                                            //Thread thread = new Thread(new ParameterizedThreadStart((object state) =>
                                            //{
                                            //    var tuple = (Tuple<List<object>, List<object>, bool, double, int, bool, System.Threading.CancellationToken>)state;
                                            //    var account = tuple.Item1;
                                            //    var result = tuple.Item2;
                                            //    var reverse = tuple.Item3;
                                            //    var valueIncrement = tuple.Item4;
                                            //    var remainder = tuple.Item5;
                                            //    var random = tuple.Item6;
                                            //    var token1 = tuple.Item7;
                                                
                                            //}));
                                            //thread.Start(parameters);
                                            var taskWithState = Task.Factory.StartNew(async (state) =>
                                            {
                                                var tuple = (Tuple<List<object>, List<object>, bool, double, int, bool>)state;
                                                var account = tuple.Item1;
                                                var result = tuple.Item2;
                                                var reverse = tuple.Item3;
                                                var valueIncrement = tuple.Item4;
                                                var remainder = tuple.Item5;
                                                var random = tuple.Item6;

                                                await start_thread(account, result, reverse, valueIncrement, remainder, random, token);


                                            }, new Tuple<List<object>, List<object>, bool, double, int, bool>(threads.account[i], threads.result[i], threads.reverse, value_increment, remainder, all_email.is_random), token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
                                            Debug.WriteLine($"threads - {i}");
                                            tasks.Add(taskWithState);
                                        }
                                        else
                                        {
                                            await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("attach_text_error"));
                                            DoSomeWork();
                                            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                            try
                                            {
                                                this.send_progress_ring.Value = 0;
                                                send_progress.Visibility = Visibility.Collapsed;
                                                start.Visibility = Visibility.Visible;
                                            }
                                            catch
                                            {

                                            }
                                        }

                                    }

                                    await Task.WhenAll(tasks);
                                    source.Cancel();
                                    // Сообщение о завершении
                                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                                    connectionString = $"Filename={dbpath}";
                                    if (Pause.is_pause)
                                    {
                                        Debug.WriteLine("ПАУЗА");
                                    }
                                    else
                                    {
                                        DoSomeWork();
                                        trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                        try
                                        {
                                            this.send_progress_ring.Value = 0;
                                            send_progress.Visibility = Visibility.Collapsed;
                                            start.Visibility = Visibility.Visible;
                                        }
                                        catch
                                        {

                                        }
                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM ProgressInfo";
                                            command.ExecuteNonQuery();


                                        }
                                        var all_email1 = Email_Sql.Get_email();
                                        var selected_email = all_email1.FirstOrDefault(x => x.id == 1);
                                        all_email1.RemoveAll(x => x.id == 1);
                                        for (var i = 0; i < all_email1.Count; i++)
                                        {
                                            all_email1[i].id = i + 1;
                                        }
                                        var all_past_email = Email_Sql.Get_past_email();
                                        if (all_past_email.Count == 0)
                                        {
                                            Email_Sql.Add_past_email(selected_email.name, selected_email.email, selected_email.folders, selected_email.texts, 1, selected_email.is_random);
                                        }
                                        else
                                        {
                                            all_past_email.Insert(0, selected_email);
                                            for (var i = 0; i < all_past_email.Count; i++)
                                            {
                                                all_past_email[i].id = i + 1;
                                            }
                                            using (var connection = new SqliteConnection(connectionString))
                                            {
                                                connection.Open();

                                                var command = connection.CreateCommand();
                                                command.CommandText = "DELETE FROM past_email";
                                                command.ExecuteNonQuery();
                                                foreach (Create_email acc1 in all_past_email)
                                                {

                                                    Email_Sql.Add_past_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                                }

                                            }
                                        }

                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM all_email";
                                            command.ExecuteNonQuery();
                                            foreach (Create_email acc1 in all_email1)
                                            {

                                                Email_Sql.Add_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                            }

                                        }
                                        ShellPage.Current.ChangeView(new home().GetType());

                                    }

                                }
                                else if (acc.Count == 0)
                                {
                                    await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("log_in_acc_error"));
                                }
                                else
                                {
                                    Algoritm.in_Thread result = Algoritm.MainSnippet(acc, sort_email);
                                    Algoritm.in_Thread threads = Algoritm.generate_thread(result, number_thread);
                                    threads.reverse = result.reverse;
                                    int max_value = 100;
                                    int num_elements = threads.result.SelectMany(list => list).Count();
                                    double value_increment = (double)max_value / num_elements;
                                    int remainder = max_value % num_elements;
                                    List<Task> tasks = new List<Task>();
                                    for (var i = 0; i < threads.result.Count; i++)
                                    {
                                        if (!ProcessNimaiClass(all_email).NullLabel)
                                        {

                                            var taskWithState = Task.Factory.StartNew(async (state) =>
                                            {
                                                var tuple = (Tuple<List<object>, List<object>, bool, double, int, bool>)state;
                                                var account = tuple.Item1;
                                                var result = tuple.Item2;
                                                var reverse = tuple.Item3;
                                                var valueIncrement = tuple.Item4;
                                                var remainder = tuple.Item5;
                                                var random = tuple.Item6;

                                                await start_thread(account, result, reverse, valueIncrement, remainder, random, token);


                                            }, new Tuple<List<object>, List<object>, bool, double, int, bool>(threads.account[i], threads.result[i], threads.reverse, value_increment, remainder, all_email.is_random), token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
                                            tasks.Add(taskWithState);
                                        }
                                        else
                                        {
                                            await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("attach_text_error"));
                                        }
                                    }

                                    await Task.WhenAll(tasks);
                                    source.Cancel();
                                    // Сообщение о завершении
                                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                                    connectionString = $"Filename={dbpath}";
                                    if (Pause.is_pause)
                                    {
                                        Debug.WriteLine("ПАУЗА");
                                    }
                                    else
                                    {
                                        DoSomeWork();
                                        trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                        try
                                        {
                                            this.send_progress_ring.Value = 0;
                                            send_progress.Visibility = Visibility.Collapsed;
                                            start.Visibility = Visibility.Visible;
                                        }
                                        catch
                                        {

                                        }
                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM ProgressInfo";
                                            command.ExecuteNonQuery();


                                        }
                                        var all_email1 = Email_Sql.Get_email();
                                        var selected_email = all_email1.FirstOrDefault(x => x.id == 1);
                                        all_email1.RemoveAll(x => x.id == 1);
                                        for (var i = 0; i < all_email1.Count; i++)
                                        {
                                            all_email1[i].id = i + 1;
                                        }
                                        var all_past_email = Email_Sql.Get_past_email();
                                        if (all_past_email.Count == 0)
                                        {
                                            Email_Sql.Add_past_email(selected_email.name, selected_email.email, selected_email.folders, selected_email.texts, 1, selected_email.is_random);
                                        }
                                        else
                                        {
                                            all_past_email.Insert(0, selected_email);
                                            for (var i = 0; i < all_past_email.Count; i++)
                                            {
                                                all_past_email[i].id = i + 1;
                                            }
                                            using (var connection = new SqliteConnection(connectionString))
                                            {
                                                connection.Open();

                                                var command = connection.CreateCommand();
                                                command.CommandText = "DELETE FROM past_email";
                                                command.ExecuteNonQuery();
                                                foreach (Create_email acc1 in all_past_email)
                                                {

                                                    Email_Sql.Add_past_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                                }

                                            }
                                        }

                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM all_email";
                                            command.ExecuteNonQuery();
                                            foreach (Create_email acc1 in all_email1)
                                            {

                                                Email_Sql.Add_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                            }

                                        }
                                        ShellPage.Current.ChangeView(new home().GetType());
                                    }
                                    
                                }


                            }
                        }

                    }
                    else
                    {
                        var manual =  GetFromCache<string>("manual");

                        if (manual != null && manual != "")
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            all_sett.manual = JsonConvert.DeserializeObject<List<manual_account>>(manual);

                            for (var i = 0; i < all_sett.manual.Count; i++)
                            {
                                manual_account selected_element = all_sett.manual[i];
                                create_account selected_acc = all_account.FirstOrDefault(x => x.id == selected_element.id_last);
                                create_account hum = new create_account { id = i + 1, name = selected_acc.name, app_password = selected_acc.app_password, email = selected_acc.email, is_verified= selected_acc.is_verified };
                                if (selected_element.is_enabled)
                                {
                                    if (selected_acc.is_verified)
                                    {
                                        man.Add(hum);
                                    }
                                }
                                
                                        
                            }
                            
                        }
                        else
                        {
                            var all_account = SqliteFunctions.SelectAllRows();
                            foreach (var account in all_account)
                            {
                                int acc_id = account.id;

                                var selected_auto = all_sett.manual.FirstOrDefault(x => x.id == acc_id);

                                if (selected_auto.is_enabled)
                                {
                                    if (account.is_verified)
                                    {
                                        man.Add(account);
                                    }


                                }
                            }
                        }
                        Debug.WriteLine(JsonConvert.SerializeObject(man) + "- mmmnaul");
                        Create_email all_email = new Create_email();
                        try
                        {
                            all_email = Email_Sql.Get_email().FirstOrDefault(x => x.id == 1);
                            if (all_email == null)
                            {
                                all_email = new Create_email();
                            }
                        }
                        catch
                        {

                        }
                        Debug.WriteLine(JsonConvert.SerializeObject(all_email));
                        if (string.IsNullOrEmpty(all_email.email))
                        {
                            Debug.WriteLine("ВВВОТ");
                            await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("email_empty"));
                            DoSomeWork();
                            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                            try
                            {
                                this.send_progress_ring.Value = 0;
                                send_progress.Visibility = Visibility.Collapsed;
                                start.Visibility = Visibility.Visible;
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            var sort_email = ExtractEmails(all_email.email);

                            if (sort_email.Count != 0)
                            {
                                for (var i = 0; i < sort_email.Count; i++)
                                {
                                    sort_email[i] = sort_email[i] + "&" + i;
                                }
                            }
                            else
                            {
                                await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("email_empty"));
                                DoSomeWork();
                                trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                try
                                {
                                    this.send_progress_ring.Value = 0;
                                    send_progress.Visibility = Visibility.Collapsed;
                                    start.Visibility = Visibility.Visible;
                                }
                                catch
                                {

                                }
                            }
                            if (sort_email.Count != 0)
                            {
                                logid.all_email = sort_email;
                                if (man.Count == 1)
                                {
                                    Algoritm.in_Thread result = new Algoritm.in_Thread()
                                    {
                                        account = new List<List<object>> { man.Cast<object>().ToList() },
                                        result = new List<List<object>> { sort_email.Cast<object>().ToList() }
                                    };
                                    Algoritm.in_Thread threads = Algoritm.generate_thread(result, number_thread);
                                    gms gms = new gms();
                                    int max_value = 100;
                                    int num_elements = threads.result.SelectMany(list => list).Count();
                                    double value_increment = (double)max_value / num_elements;
                                    int remainder = max_value % num_elements;
                                    Debug.WriteLine($"value_increment: {value_increment}, remainder: {remainder}");
                                    List<Task> tasks = new List<Task>();
                                    for (var i = 0; i < threads.result.Count; i++)
                                    {
                                        if (!ProcessNimaiClass(all_email).NullLabel)
                                        {

                                            var taskWithState = Task.Factory.StartNew(async (state) =>
                                            {
                                                var tuple = (Tuple<List<object>, List<object>, bool, double, int, bool>)state;
                                                var account = tuple.Item1;
                                                var result = tuple.Item2;
                                                var reverse = tuple.Item3;
                                                var valueIncrement = tuple.Item4;
                                                var remainder = tuple.Item5;
                                                var random = tuple.Item6;

                                                await start_thread(account, result, reverse, valueIncrement, remainder, random, token);

                                            }, new Tuple<List<object>, List<object>, bool, double, int, bool>(threads.account[0], threads.result[i], threads.reverse, value_increment, remainder, all_email.is_random), cancellationToken: token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
                                            tasks.Add(taskWithState);
                                        }
                                        else
                                        {
                                            await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("attach_text_error"));
                                            DoSomeWork();
                                            try
                                            {
                                                this.send_progress_ring.Value = 0;
                                                send_progress.Visibility = Visibility.Collapsed;
                                                start.Visibility = Visibility.Visible;
                                            }
                                            catch
                                            {

                                            }
                                        }

                                    }
                                    await Task.WhenAll(tasks);
                                    source.Cancel();
                                    // Сообщение о завершении
                                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                                    connectionString = $"Filename={dbpath}";
                                    if (Pause.is_pause)
                                    {
                                        Debug.WriteLine("ПАУЗА");
                                    }
                                    else
                                    {
                                        DoSomeWork();
                                        trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                        try
                                        {
                                            this.send_progress_ring.Value = 0;
                                            send_progress.Visibility = Visibility.Collapsed;
                                            start.Visibility = Visibility.Visible;
                                        }
                                        catch
                                        {

                                        }
                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM ProgressInfo";
                                            command.ExecuteNonQuery();


                                        }
                                        var all_email1 = Email_Sql.Get_email();
                                        var selected_email = all_email1.FirstOrDefault(x => x.id == 1);
                                        all_email1.RemoveAll(x => x.id == 1);
                                        for (var i = 0; i < all_email1.Count; i++)
                                        {
                                            all_email1[i].id = i + 1;
                                        }
                                        var all_past_email = Email_Sql.Get_past_email();
                                        if (all_past_email.Count == 0)
                                        {
                                            Email_Sql.Add_past_email(selected_email.name, selected_email.email, selected_email.folders, selected_email.texts, 1, selected_email.is_random);
                                        }
                                        else
                                        {
                                            all_past_email.Insert(0, selected_email);
                                            for (var i = 0; i < all_past_email.Count; i++)
                                            {
                                                all_past_email[i].id = i + 1;
                                            }
                                            using (var connection = new SqliteConnection(connectionString))
                                            {
                                                connection.Open();

                                                var command = connection.CreateCommand();
                                                command.CommandText = "DELETE FROM past_email";
                                                command.ExecuteNonQuery();
                                                foreach (Create_email acc1 in all_past_email)
                                                {

                                                    Email_Sql.Add_past_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                                }

                                            }
                                        }

                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM all_email";
                                            command.ExecuteNonQuery();
                                            foreach (Create_email acc1 in all_email1)
                                            {

                                                Email_Sql.Add_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                            }

                                        }
                                        ShellPage.Current.ChangeView(new home().GetType());
                                    }
                                    
                                }
                                else if (man.Count == 0)
                                {
                                    await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("log_in_acc_error"));
                                }
                                else
                                {
                                    Algoritm.in_Thread result = Algoritm.MainSnippet(man, sort_email);
                                    Algoritm.in_Thread threads = Algoritm.generate_thread(result, number_thread);
                                    int max_value = 100;
                                    int num_elements = threads.result.SelectMany(list => list).Count();
                                    double value_increment = (double)max_value / num_elements;
                                    int remainder = max_value % num_elements;
                                    List<Task> tasks = new List<Task>();
                                    Debug.WriteLine($"value_increment: {value_increment}, remainder: {remainder}");

                                    for (var i = 0; i < threads.result.Count; i++)
                                    {
                                        if (!ProcessNimaiClass(all_email).NullLabel)
                                        {
                                            var taskWithState = Task.Factory.StartNew(async (state) =>
                                            {
                                                var tuple = (Tuple<List<object>, List<object>, bool, double, int, bool>)state;
                                                var account = tuple.Item1;
                                                var result = tuple.Item2;
                                                var reverse = tuple.Item3;
                                                var valueIncrement = tuple.Item4;
                                                var remainder = tuple.Item5;
                                                var random = tuple.Item6;

                                                await start_thread(account, result, reverse, valueIncrement, remainder, random, token);

                                            }, new Tuple<List<object>, List<object>, bool, double, int, bool>(threads.account[i], threads.result[i], threads.reverse, value_increment, remainder, all_email.is_random), token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
                                            tasks.Add(taskWithState);

                                        }
                                        else
                                        {
                                            await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("attach_text_error"));
                                        }
                                    }
                                    await Task.WhenAll(tasks);
                                    source.Cancel();
                                    // Сообщение о завершении
                                    ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                                    string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
                                    connectionString = $"Filename={dbpath}";
                                    if (Pause.is_pause)
                                    {
                                        Debug.WriteLine("ПАУЗА");
                                    }
                                    else
                                    {
                                        DoSomeWork();
                                        trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                                        try
                                        {
                                            this.send_progress_ring.Value = 0;
                                            send_progress.Visibility = Visibility.Collapsed;
                                            start.Visibility = Visibility.Visible;
                                        }
                                        catch
                                        {

                                        }
                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM ProgressInfo";
                                            command.ExecuteNonQuery();


                                        }
                                        var all_email1 = Email_Sql.Get_email();
                                        var selected_email = all_email1.FirstOrDefault(x => x.id == 1);
                                        all_email1.RemoveAll(x => x.id == 1);
                                        for (var i = 0; i < all_email1.Count; i++)
                                        {
                                            all_email1[i].id = i + 1;
                                        }
                                        var all_past_email = Email_Sql.Get_past_email();
                                        if (all_past_email.Count == 0)
                                        {
                                            Email_Sql.Add_past_email(selected_email.name, selected_email.email, selected_email.folders, selected_email.texts, 1, selected_email.is_random);
                                        }
                                        else
                                        {

                                            all_past_email.Insert(0, selected_email);
                                            for (var i = 0; i < all_past_email.Count; i++)
                                            {
                                                all_past_email[i].id = i + 1;
                                            }
                                            using (var connection = new SqliteConnection(connectionString))
                                            {
                                                connection.Open();

                                                var command = connection.CreateCommand();
                                                command.CommandText = "DELETE FROM past_email";
                                                command.ExecuteNonQuery();
                                                foreach (Create_email acc1 in all_past_email)
                                                {

                                                    Email_Sql.Add_past_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                                }

                                            }
                                        }

                                        using (var connection = new SqliteConnection(connectionString))
                                        {
                                            connection.Open();

                                            var command = connection.CreateCommand();
                                            command.CommandText = "DELETE FROM all_email";
                                            command.ExecuteNonQuery();
                                            foreach (Create_email acc1 in all_email1)
                                            {

                                                Email_Sql.Add_email(acc1.name, acc1.email, acc1.folders, acc1.texts, (int)acc1.id, acc1.is_random);
                                            }

                                        }
                                        ShellPage.Current.ChangeView(new home().GetType());
                                    }
                                    
                                }


                            }
                        }

                    }
                }
            }
            else
            {
                Debug.WriteLine(JsonConvert.SerializeObject(Email_Sql.Get_email()));
                await ShowErrorDialog(((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString("email_empty"));
                DoSomeWork();
                trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
                try
                {
                    this.send_progress_ring.Value = 0;
                    send_progress.Visibility = Visibility.Collapsed;
                    start.Visibility = Visibility.Visible;
                }
                catch
                {

                }
            }
            
            


        }

        private void DoSomeWork()
        {
            // Создать уведомление о том, что фоновая задача запущена
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode("Завершенно"));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            DoSomeWork();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var exampleTaskName = "MyBackgroundTask";
            //var builder = new BackgroundTaskBuilder();

            //builder.Name = exampleTaskName;
            //builder.TaskEntryPoint = "Mail_Send.MyBackgroundTask";
            //builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
            //builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
            //BackgroundTaskRegistration task =  builder.Register();
            // Получить текущее окно

            // Установить минимальную и максимальную ширину окна в 200 DIP
            // Запуск фоновой задачитр
            var builder = new BackgroundTaskBuilder
            {
                Name = "Tasks.ExampleBackgroundTask",
                TaskEntryPoint = "Tasks.ExampleBackgroundTask"
            };
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            
            BackgroundTaskRegistration task = builder.Register();

            // Закрытие текущего приложения


            // Получить текущее окно

            // Установить порядок окна поверх всех других окон


            // Установка окна поверх всех окон

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void cancel_progress_Click(object sender, RoutedEventArgs e)
        {
            source.Cancel();
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";
            var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
            trackerDB.UpdateStartInfo(new StartInfo { Id = 1, IsStart = false });
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ProgressInfo";
                command.ExecuteNonQuery();


            }
            start.Visibility = Visibility.Visible;
            send_progress.Visibility = Visibility.Collapsed;
        }

        private async void continue_progress_Click(object sender, RoutedEventArgs e)
        {
            Pause.is_pause = false;
            var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ProgressInfo";
                command.ExecuteNonQuery();


            }


            
            Button str = new Button();
            var allProgress2 = trackerDB.GetAllProgress();
            start_Click(sender, e);
            ShellPage.Current.ChangeView(new home().GetType());
        }
        private async void pausaa()
        {
            source.Cancel();
        }
        private  void pause_progress_Click(object sender, RoutedEventArgs e)
        {
            
            Pause.is_pause = true;
            source.Cancel();
            var trackerDB = new ProgressTrackerDB($"{System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db")}");
            ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            connectionString = $"Filename={dbpath}";

            //using (var connection = new SqliteConnection(connectionString))
            //{
            //    connection.Open();

            //    var command = connection.CreateCommand();
            //    command.CommandText = "DELETE FROM all_email";
            //    command.ExecuteNonQuery();
            //    foreach (Create_email acc in all_email)
            //    {

            //        Email_Sql.Add_email(acc.name, acc.email, acc.folders, acc.texts, (int)acc.id, false);
            //    }

            //}

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ProgressInfo";
                command.ExecuteNonQuery();


            }
            ShellPage.Current.ChangeView(new home().GetType());
        }
    }
}
