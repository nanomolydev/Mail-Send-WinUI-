using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static Mail_Send.account;
using static Mail_Send.setting;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class manual_setting : Page
{
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
    private static string connectionString = "Filename==database.db";
    static string ExtractCharacters(string s)
    {
        // ѕроверка на минимальную длину строки
        if (s.Length < 10)
        {
            return s;
        }

        // ѕолучение первых и последних п€ти символов
        string firstFive = s.Substring(0, 5);
        string lastFive = s.Substring(s.Length - 4);

        return firstFive + ".." + lastFive;
    }
    CollectionViewSource setting_source = new CollectionViewSource();
    Frame this_frame = new Frame();
    public manual_setting(CollectionViewSource sett, Frame frame)
    {
        this.InitializeComponent();
        setting_source = sett;
        this_frame = frame; 
        var settings = setting_Sql.Get_setting();
        var manual1 = GetFromCache<string>("manual");
        if (manual1 != null && manual1 != "")
        {
            settings.manual = JsonConvert.DeserializeObject<List<manual_account>>(manual1);
        }
        else
        {

        }
        if (settings == null || settings.manual == null || settings.manual.Count == 0)
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
    static void ѕереместитьЁлемент(int индекс, int индексѕеремещени€, List<manual_account> список)
    {
        manual_account элемент = список[индекс];
        список.RemoveAt(индекс);
        список.Insert(индексѕеремещени€, элемент);
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
        ѕереместитьЁлемент(l - 1, t - 1, all_account);
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
        Debug.WriteLine(JsonConvert.SerializeObject(all_account) + " -ghg  ");

        SaveToCache("manual", JsonConvert.SerializeObject(all_account));
        foreach (var aut in all_account)
        {
            aut.name = ExtractCharacters(aut.name);
        }
        var collectionViewSource = (CollectionViewSource)Resources["manual_list"];
        collectionViewSource.Source = all_account;
        setting_source.Source = all_account;
    }

    private void back_Click(object sender, RoutedEventArgs e)
    {
        this_frame.Content = null;
    }
}
