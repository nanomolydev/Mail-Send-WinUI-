using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace Main_Algorithm
{
    class Algoritm
    {

        public class Result
        {
            public List<List<int>> account { get; set; }
            public List<List<int>> result { get; set; }
        }
       public class in_Thread
        {
            public List<List<object>> account { get; set; }
            public List<List<object>> result { get; set; }
            public bool reverse { get; set; }
        }


        static int NearestEven(int num, bool is_reverse)
        {
            if (is_reverse)
            {
                if (num > 0 && num < 2)
                {
                    return (num % 2 == 0) ? num : num;
                }
                else
                {
                    return (num % 2 == 0) ? num : num - 1;
                }
            }
            else
            {
                if (num > 0 && num < 2)
                {
                    return (num % 2 == 0) ? num : num+1;
                }
                else
                {
                    return (num % 2 == 0) ? num : num - 1;
                }
            }
            
            
        }

        public static in_Thread MainSnippet(List<Mail_Send.account.create_account> L1, List<string> L2)
        {
            List<Mail_Send.account.create_account> pre_L1 = new List<Mail_Send.account.create_account>(L1);
            List<string> pre_L2 = new List<string>(L2);
            List<List<object>> account = new List<List<object>>();
            List<List<object>> result = new List<List<object>>();
            bool reverse = false;

            if (L1.Count < L2.Count)
            {

                int k = 0;
                while (L2.Count > 0)
                {
                    List<string> preResult = new List<string>();
                    int delit = (k == 0) ? NearestEven(L1.Count, false) : L1.Count;
                    int delim = L2.Count;
                    int resultDel = (int)Math.Ceiling((double)delim / delit);

                    for (int i = 0; i < resultDel && i < L2.Count; i++)
                    {
                        preResult.Add(L2[i]);
                    }

                    foreach (string val in preResult)
                    {
                        L2.Remove(val);
                    }
                    result.Add(preResult.Cast<object>().ToList());
                    L1.RemoveAt(0);
                    k++;

                }

                for (int i = 0; i < result.Count; i++)
                {
                    List<Mail_Send.account.create_account> preAccount = new List<Mail_Send.account.create_account>();
                    for (int g = 0; g < result[i].Count; g++)
                    {
                        preAccount.Add(pre_L1[i]);
                    }
                    account.Add(preAccount.Cast<object>().ToList());
                }
            }
            else
            {

                reverse = true;
                int k = 0;
                while (L1.Count > 0)
                {
                    List<Mail_Send.account.create_account> preResult = new List<Mail_Send.account.create_account>();
                    int delit = (k == 0) ? NearestEven(L2.Count, true) : L2.Count;
                    int delim = L1.Count;
                    int resultDel = (int)Math.Ceiling((double)delim / delit);

                    for (int i = 0; i < resultDel && i < L1.Count; i++)
                    {
                        preResult.Add(L1[i]);
                    }

                    foreach (Mail_Send.account.create_account val in preResult)
                    {
                        L1.Remove(val);
                    }

                    result.Add(preResult.Cast<object>().ToList());
                    L2.RemoveAt(0);
                    k++;
                }
                for (int i = 0; i < result.Count; i++)
                {
                    List<string> preAccount = new List<string>();
                    for (int g = 0; g < result[i].Count; g++)
                    {
                        preAccount.Add(pre_L2[i]);
                    }
                    account.Add(preAccount.Cast<object>().ToList());
                }
            }

            in_Thread data = new in_Thread
            {
                account = account,
                result = result,
                reverse = reverse
            };

            return data;
        }


        public static in_Thread generate_thread(in_Thread data, int thread)
        {

            int colvo_email = data.result.Sum(x => x.Count);
            int quotient = colvo_email / thread;
            int remainder = colvo_email % thread;
            int i = 0;
            int iter = 0;
            in_Thread pre_thread = new in_Thread();
            while (data.result.All(sublist => sublist.Count == 0) != true)
            {

                var pre_result = data.result[i];
                var pre_account = data.account[i];
                Console.WriteLine($"{JsonConvert.SerializeObject(pre_result)} - rrrrr");

                int pre_quotient = 0;
                if (pre_result.Count < quotient)
                {
                    pre_quotient = pre_result.Count;
                }
                else
                {
                    pre_quotient = quotient;
                }
                int remaind_element = 0;
                Console.WriteLine(JsonConvert.SerializeObject(pre_thread));
                if (pre_thread.account == null)
                {
                    pre_thread.account = new List<List<object>>();
                }
                if (pre_thread.result == null)
                {
                    pre_thread.result = new List<List<object>>();
                }
                pre_thread.account.Add(pre_account.Take(pre_quotient).ToList());
                pre_thread.result.Add(pre_result.Take(pre_quotient).ToList());


                pre_result.RemoveRange(0, pre_quotient);
                Console.WriteLine(JsonConvert.SerializeObject(pre_result));
                remaind_element = quotient - pre_quotient;
                int m = 1;
                if (data.result[i].Count != 0)
                {
                    m = 0;
                }

                int mr = 0;
                Console.WriteLine($"{remaind_element} - ДО remaind_element");
                Console.WriteLine(JsonConvert.SerializeObject(pre_thread));
                while (remaind_element != 0)
                {
                    if (data.result.All(sublist => sublist.Count == 0) || data.result[i + m].Count == 0)
                    {
                        if (data.result.All(sublist => sublist.Count == 0))
                        {
                            m++;
                            mr = 0;
                            remaind_element = 0;
                            break;
                        }
                        else
                        {
                            m++;
                            mr = 0;
                            continue;
                        }

                    }

                    Console.WriteLine($"{i} - l");

                    bool areAllSublistsEmpty1 = data.result.All(sublist => sublist.Count == 0);
                    if (areAllSublistsEmpty1)
                    {

                        remaind_element = 0;
                        break;
                    };
                    Console.WriteLine(iter);
                    Console.WriteLine($"{remaind_element} - ПОСЛЕ remaind_element");

                    pre_thread.account[iter].Add(data.account[i + m][0]);
                    pre_thread.result[iter].Add(data.result[i + m][0]);

                    data.result[i + m].Remove(data.result[i + m][0]);

                    Console.WriteLine($"{JsonConvert.SerializeObject(pre_thread)} - thread");
                    Console.WriteLine($"{JsonConvert.SerializeObject(data)} - data");
                    if (remaind_element > 0) remaind_element--;
                    if (data.result[i + m].Count == 0)
                    {

                    }
                    else
                    {
                        mr++;
                    }

                    Console.WriteLine($"Выполнилось: {mr} раз");


                }

                Console.WriteLine(i);
                if (data.result.All(sublist => sublist.Count == 0))
                {
                    if (data.result.All(sublist => sublist.Count == 0))
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                iter++;

            }
            if (data.account.Count == 1)
            {
                remainder = 0;
            }
            if (remainder != 0)
            {
                int l = 0;

                while (remainder != 0)
                {
                    var pre_result = pre_thread.result.ElementAt(pre_thread.result.Count - 1);
                    var pre_account = pre_thread.account.ElementAt(pre_thread.account.Count - 1);
                    if (pre_account.Count == 0)
                    {
                        remainder = 0;
                        break;
                    }
                    else
                    {
                        pre_thread.account[l].Add(pre_account[pre_account.Count - 1]);
                        pre_thread.result[l].Add(pre_result[pre_result.Count - 1]);
                        pre_result.Remove(pre_result[pre_result.Count - 1]);
                        pre_account.Remove(pre_account[pre_account.Count - 1]);
                        l++;
                        remainder--;
                    }
                    
                    /*Console.WriteLine(JsonConvert.SerializeObject(pre_thread));*/


                }
            }
            return pre_thread;
        }

    }
}

