using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Globalization;

using System.Windows;

using System.Web;
using System.Configuration;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

using Windows.UI.Xaml.Data;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Net.Http;
using OpenAI_API.Moderation;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using Mail_Send;
using Syncfusion.UI.Xaml.Charts;
namespace gm
{

    

    public class manual
    {
        public static List<string> CountDigits(List<int> lst)
        {
            Dictionary<int, Dictionary<string, int>> digitCounts = new Dictionary<int, Dictionary<string, int>>();
            var enumeratedList = lst.Select((index, number) => new Tuple<int, int>(index, number));
            foreach (Tuple<int, int> k in enumeratedList)
            {
                int number = k.Item1;
                int index = k.Item2;
                if (digitCounts.ContainsKey(number))
                {
                    digitCounts[number]["count"] += 1;
                }
                else
                {
                    Dictionary<string, int> innerDict = new Dictionary<string, int>();
                    innerDict.Add("count", 1);
                    innerDict.Add("index", index);

                    // Добавление вложенного словаря в основной словарь
                    digitCounts.Add(number, innerDict);
                }
            }
            List<string> result = new List<string>();
            foreach (var i in digitCounts)
            {
                var digit = i.Key;
                var count = i.Value["count"];
                var index = i.Value["index"];
                if (count > 1)
                {
                    result.Add($"{digit}::{count}::{index}");
                }

            }



            return result;
        }
        public List<int> FindMatchingSublist(List<int> mainList, List<List<int>> subLists, int digit)
        {
            for (int index = 0; index < subLists.Count; index++)
            {
                List<int> sublist = subLists[index];
                if (sublist.Contains(digit))
                {
                    return sublist;
                }
            }
            return new List<int>();
        }

        static bool check_last_element(List<List<int>> nestedList, int number)
        {
            try
            {
                foreach (List<int> sublist in nestedList)
                {
                    if (sublist[sublist.Count - 1] == number)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                Console.WriteLine();
            }

            return false;
        }
        public static List<T> RemoveDuplicates<T>(List<T> list)
        {
            HashSet<T> uniqueItems = new HashSet<T>(list);
            return new List<T>(uniqueItems);
        }
        public static void PrintList<T>(List<T> list)
        {
            Console.Write("[");
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is IList && list[i].GetType().IsGenericType)
                {
                    Console.Write("[");
                    var innerList = (IList)list[i];
                    for (int j = 0; j < innerList.Count; j++)
                    {
                        Console.Write(innerList[j]);
                        if (j != innerList.Count - 1)
                            Console.Write(", ");
                    }
                    Console.Write("]");
                }
                else
                {
                    Console.Write(list[i]);
                }
                if (i != list.Count - 1)
                    Console.Write(", ");
            }
            Console.Write("]");
        }
        public List<List<string>> manual_convert(List<int> list_number, List<string> list_string)
        {
            List<int> list_number1 = list_number;
            List<string> list_string1 = list_string;
            List<int> list_sort = list_number1.OrderBy(x => x).ToList();
            List<int> list_sort_self = list_number1.OrderBy(x => x).ToList();
            List<string> list_dubl = CountDigits(list_sort);
            list_sort = RemoveDuplicates(list_sort);
            list_sort_self = RemoveDuplicates(list_sort);
            int min_list_number_sort = 0;



            List<List<int>> index_in_list_string = new List<List<int>>();
            if (list_string.Count > list_number.Count)
            {
                List<int> list_generative_number = new List<int>();
                for (var i = 0; i < list_sort.Count; i++)
                {

                    double special_number = (double)list_sort[i] / list_sort.Last();
                    list_generative_number.Add((int)Math.Ceiling(list_string.Count * special_number));

                }
                foreach (int innerList in list_generative_number)
                {
                    Console.WriteLine("[" + string.Join(", ", innerList) + "]");
                }
                list_sort = list_generative_number;
                for (var k = 0; k < list_sort.Count; k++)
                {

                    min_list_number_sort = list_sort[0];

                    if (k == 0)
                    {
                        Console.WriteLine();
                    }
                    else
                    {
                        min_list_number_sort = list_sort[k - 1];

                    }

                    int number_in_list_sort = list_sort[k];
                    List<int> index = new List<int>();
                    if (index_in_list_string.Count == 0)
                    {
                        for (var i = 0; i <= min_list_number_sort; i++)
                        {
                            index.Add(i);
                            Console.WriteLine(i);
                            if (i == min_list_number_sort)
                            {

                                Console.WriteLine("sadasdasd");

                                Console.WriteLine($"{index.GetRange(1, index.Last())}");
                                index_in_list_string.Add(index.GetRange(1, index.Last()));
                                index.Clear();
                            }
                        }
                    }
                    else
                    {

                        for (var i = min_list_number_sort; i <= number_in_list_sort; i++)
                        {

                            if (list_sort[k] == list_sort[k - 1])
                            {
                                Console.WriteLine("min");
                                int x1 = k - 2;
                                int x2 = k - 1;

                                for (var h = list_sort[x1]; h < list_sort[x2] + 1; h++)
                                {
                                    index.Add(h);
                                }
                            }
                            else
                            {
                                index.Add(i);
                            }
                            if (i == number_in_list_sort)
                            {
                                index_in_list_string.Add(index.GetRange(1, index.Count - 1));
                                index.Clear();
                            }
                        }
                    }


                }

                List<int> indices = new List<int>();
                foreach (int x in list_number)
                {
                    indices.Add(list_sort_self.IndexOf(x));
                }
                foreach (int innerList in indices)
                {
                    Console.WriteLine("index in list [" + string.Join(", ", innerList) + "]");
                }
                Console.WriteLine($" d - {min_list_number_sort}");
                if (check_last_element(index_in_list_string, min_list_number_sort))
                {
                    Console.WriteLine("s");
                }
                else
                {
                    List<int> index = new List<int>();
                    for (var i = 1; i < list_sort_self[0] + 1; i++)
                    {
                        index.Add(i);
                    }
                }
                foreach (var i in list_sort)
                {
                    foreach (var k in list_dubl)
                    {
                        if (i == int.Parse(k.Split("::")[0]))
                        {
                            for (var m = 1; m < int.Parse(k.Split("::")[1]); m++)
                            {

                                Console.WriteLine($"{list_sort_self}, {list_generative_number}, {i}");
                                index_in_list_string.Insert(int.Parse(k.Split("::")[2]), FindMatchingSublist(list_sort_self, index_in_list_string, i));
                            }
                        }
                    }
                }
                index_in_list_string = index_in_list_string.Where(subList => subList != null).ToList();


                List<int> number_dubl = new List<int>();
                foreach (var i in list_sort_self)
                {
                    foreach (var k in list_dubl)
                    {
                        if (i == int.Parse(k.Split("::")[0]))
                        {
                            number_dubl.Add(i);
                        }
                    }
                }
                for (var i = 0; i < list_dubl.Count; i++)
                {
                    for (var m = 1; m < int.Parse(list_dubl[i].Split("::")[1]); m++)
                    {
                        list_sort_self.Insert(int.Parse(list_dubl[i].Split("::")[2]), number_dubl[i]);
                    }
                }

                List<int> result = new List<int>();

                indices = new List<int>();
                foreach (int x in list_number1)
                {
                    indices.Add(list_sort_self.IndexOf(x));
                }

                List<List<int>> result_number = new List<List<int>>();
                List<List<string>> result_string = new List<List<string>>();





                foreach (int i in indices)
                {
                    try
                    {
                        result_number.Add(index_in_list_string[i]);
                    }
                    catch
                    {
                        List<int> localStrings = new List<int>();
                        result_number.Add(localStrings);
                    }


                }
                List<int>[] array = new List<int>[result_number.Count];
                for (int i = 0; i < result_number.Count; i++)
                {
                    array[i] = result_number[i];
                }
                Console.WriteLine("d");
                foreach (List<int> innerList in array)
                {
                    Console.Write("[");
                    foreach (int element in innerList)
                    {
                        Console.Write($"{element}, ");
                    }
                    Console.WriteLine("]");
                }
                for (int k = 0; k < result_number.Count; k++)
                {
                    List<string> index = new List<string>();
                    foreach (int i in result_number[k])
                    {
                        try
                        {
                            index.Add(list_string1[i - 1]);
                        }
                        catch
                        {
                            Console.WriteLine();
                        }
                    }
                    //Console.WriteLine(index);
                    result_string.Add(index);
                }
                //Console.WriteLine(result_string);
                return result_string;
            }
            else
            {
                for (var k = 0; k < list_sort.Count; k++)
                {
                    min_list_number_sort = list_sort[0];

                    if (k == 0)
                    {
                        Console.WriteLine();
                    }
                    else
                    {
                        min_list_number_sort = list_sort[k - 1];

                    }

                    int number_in_list_sort = list_sort[k];
                    List<int> index = new List<int>();
                    Console.WriteLine(number_in_list_sort + 1 + " - min");
                    for (var i = min_list_number_sort; i < number_in_list_sort + 1; i++)
                    {

                        int indx = k - 1;
                        int positiveIndex = indx < 0 ? list_sort.Count + indx : indx;

                        if (list_sort[k] == list_sort[positiveIndex])
                        {
                            Console.WriteLine("min");
                            int x1 = k - 2;
                            int x2 = k - 1;
                            if (list_sort.Count == 1)
                            {
                                index.Add(i);
                            }
                            else
                            {

                                for (var h = list_sort[x1]; h < list_sort[x2] + 1; h++)
                                {
                                    index.Add(h);
                                }
                            }
                        }
                        else
                        {

                            index.Add(i);
                        }
                        if (i == number_in_list_sort)
                        {

                            index_in_list_string.Add(index.GetRange(1, index.Count - 1));
                            index.Clear();
                        }
                    }


                }


                if (check_last_element(index_in_list_string, list_sort[0]))
                {
                    Console.WriteLine();
                }
                else
                {
                    List<int> index = new List<int>();
                    for (var i = 1; i < list_sort[0] + 1; i++)
                    {
                        index.Add(i);
                    }
                    index_in_list_string.Insert(0, index);
                }
                foreach (var i in list_sort)
                {
                    foreach (var k in list_dubl)
                    {
                        if (i == int.Parse(k.Split("::")[0]))
                        {
                            for (var m = 1; m < int.Parse(k.Split("::")[1]); m++)
                            {
                                index_in_list_string.Insert(int.Parse(k.Split("::")[2]), FindMatchingSublist(list_sort, index_in_list_string, i));
                            }
                        }
                    }
                }

                index_in_list_string.RemoveAll(item => item.Count == 0);
                try
                {
                    int indexZero = list_sort.IndexOf(0);

                    index_in_list_string.Insert(indexZero, new List<int>() { 0 });
                }
                catch
                {
                    Console.WriteLine("pass");

                }

                List<int> number_dubl = new List<int>();
                foreach (var i in list_sort)
                {
                    foreach (var k in list_dubl)
                    {
                        if (i == int.Parse(k.Split("::")[0]))
                        {
                            number_dubl.Add(i);
                        }
                    }
                }
                for (var i = 0; i < list_dubl.Count; i++)
                {
                    for (var m = 1; m < int.Parse(list_dubl[i].Split("::")[1]); m++)
                    {
                        list_sort.Insert(int.Parse(list_dubl[i].Split("::")[2]), number_dubl[i]);
                    }
                }

                List<int> indices = new List<int>();
                foreach (int x in list_number1)
                {
                    indices.Add(list_sort.IndexOf(x));
                }

                List<int> result = new List<int>();



                List<List<int>> result_number = new List<List<int>>();
                List<List<string>> result_string = new List<List<string>>();
                foreach (var item in index_in_list_string)
                {
                    foreach (var number in item)
                    {
                        Console.WriteLine();
                    }
                }

                foreach (int i in indices)
                {
                    result_number.Add(index_in_list_string[i]);

                }
                
                for (int k = 0; k < result_number.Count; k++)
                {
                    List<string> index = new List<string>();
                    foreach (int i in result_number[k])
                    {
                        try
                        {
                            index.Add(list_string1[i - 1]);
                        }
                        catch
                        {
                            Console.WriteLine();
                        }
                    }
                    //Console.WriteLine(index);
                    result_string.Add(index);
                }
                //Console.WriteLine(result_string);
                return result_string;
            }





        }
    }

}




public class link_list
{
    public static List<List<string>> splitList(List<string> originalList, int numUsers)
    {
        int itemsPerUser = originalList.Count / numUsers;
        int itemsLeftOver = originalList.Count % numUsers;

        List<List<string>> userList = new List<List<string>>();

        int index = 0;
        for (int i = 0; i < numUsers; i++)
        {
            int itemCount = itemsPerUser + (i < itemsLeftOver ? 1 : 0);

            List<string> userItems = new List<string>();
            for (int j = 0; j < itemCount; j++)
            {
                userItems.Add(originalList[index]);
                index++;
            }

            userList.Add(userItems);
        }

        return userList;
    }
}

public class logs
{

    public int id
    {
        get; set;
    }
    public bool error
    {
        get; set;
    }
    public string from_send
    {
        get; set;
    }
    public string to_send
    {
        get; set;
    }
    public string message
    {
        get; set;
    }
    public DateTime date
    {
        get; set;
    }
    public DateTime date_created
    {
        get; set;
    }
    public bool is_read
    {
        get;set;
    }
}

public class gms
{
    

    
    public string ReadSetting(string key)
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        object value = localSettings.Values[key];
        string result = (value != null) ? value.ToString() : "Not Found";
        return result;
    }
    public DateTime GetCurrentDate()
    {
        DateTime currentDate = DateTime.Now;
        return currentDate;
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
    string smtpServer;
    int smtpPort;
    string smtpUsername;
    bool custom = false;
    string smtpPassword;
    string to_email_send;
    int id_log;
    DateTime created_time;
    MailMessage message;
    private static string connectionString = "Filename==database.db";
    SmtpClient smtpClient;
    public void smtp_load(string smtp_server, int smtp_port)
    {
        smtpServer = smtp_server;
        smtpPort = smtp_port;
    }
    public void load_created_datetime(DateTime date_created)
    {
        
        created_time = date_created;

    }
    public void load_index_log(int index_log)
    {
        id_log = index_log;
    }
    public bool login_account(string your_email, string your_password)
    {
        try
        {
            smtpUsername = your_email;
            smtpPassword = your_password;
            smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            return true;
        }
        catch (Exception ex)
        {
            logs pre_log = new logs { error = true, from_send = smtpUsername, to_send = to_email_send, date = GetCurrentDate(), date_created = created_time, message = ex.Message, is_read = false };
            home.log_sql.Add_Logs(id: id_log,error: pre_log.error, to_send: pre_log.to_send, date: pre_log.date, date_created:created_time, message: pre_log.message, from_send: pre_log.from_send, is_read: pre_log.is_read);


            return false;
        }
    }
    public bool Create_message(string to_email, string subject, string body)
    {
        try
        {

            to_email_send = to_email;
            smtpUsername = smtpUsername.Trim();
            Debug.WriteLine(body);
            body = body.Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
            message = new MailMessage(smtpUsername, to_email_send, subject, body) { IsBodyHtml = true };
            return true;
            
        }
        catch (Exception ex)
        {

            logs pre_log = new logs { error = true, from_send = smtpUsername, to_send = to_email_send, date = GetCurrentDate(), message = ex.Message, is_read = false };
            home.log_sql.Add_Logs(id: id_log, error: pre_log.error, to_send: pre_log.to_send, date: pre_log.date, date_created: created_time,  message: pre_log.message, from_send: pre_log.from_send, is_read: pre_log.is_read);
            return false;
        }
    }
    public void Add_Attachment(string attachmentPath)
    {
        String filename_s = attachmentPath.Split('\\').Last();
        try
        {
            
            System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType();
            contentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Octet;
            contentType.Name = filename_s;
            Attachment attachment = new Attachment(attachmentPath.Trim(), contentType);
            message.Attachments.Add(attachment);
        }
        catch (Exception ex)
        {
            logs pre_log = new logs { error = true, from_send = smtpUsername, to_send = to_email_send, date = GetCurrentDate(), message = ex.Message, is_read = false };
            home.log_sql.Add_Logs(id: id_log, error: pre_log.error, to_send: pre_log.to_send, date: pre_log.date, date_created: created_time, message: pre_log.message, from_send: pre_log.from_send, is_read: pre_log.is_read);
        }
        
    }

    public  async Task send_message(bool is_error)
    {
        try
        {
            await smtpClient.SendMailAsync(message);

            logs pre_log = new logs { error = false, from_send = smtpUsername, to_send = to_email_send, date = GetCurrentDate(), is_read = false };

            home.log_sql.Add_Logs(id: id_log, error: pre_log.error, to_send: pre_log.to_send, date: pre_log.date, date_created: created_time, message: pre_log.message, from_send: pre_log.from_send, is_read: pre_log.is_read);

            message.Dispose();

        }
        catch (Exception ex) 
        {
            Debug.WriteLine(ex.Message);
            logs pre_log = new logs { error = true, from_send = smtpUsername, to_send = to_email_send, date = GetCurrentDate(), message = ex.Message, is_read = false };
            home.log_sql.Add_Logs(id: id_log, error: pre_log.error, to_send: pre_log.to_send, date: pre_log.date, date_created: created_time, message: pre_log.message, from_send: pre_log.from_send, is_read: pre_log.is_read);
            if (is_error)
            {
                throw new Exception("Error"); // Это

            }
        }
        

    }
}

