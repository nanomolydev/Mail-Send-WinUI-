using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using static Mail_Send.account;
using static Mail_Send.setting;
using static Mail_Send.App;
using static Mail_Send.setting;
namespace Mail_Send.ViewModels;
public class localizated : INotifyPropertyChanged
{

    private int _sliderValue;
    public event PropertyChangedEventHandler PropertyChanged; 
    public string GetString(string key)
    {
        return ((App)Microsoft.UI.Xaml.Application.Current).localizedStrings.GetString(key);
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
    public localizated()
    {
        // Инициализация при создании ViewModel
        var value = GetFromCache<string>("threads");
        if (value != null)
        {
            Debug.WriteLine($"{value}  -- = == ");
            _sliderValue = int.Parse(value);
        }
        else
        {
            _sliderValue = 1;
        }
        
    }
    private static string connectionString = "Filename==database.db";
    public int SliderValue
    {
        get => _sliderValue;
        set
        {
            if (_sliderValue != value)
            {
                _sliderValue = value;
                SaveToCache("threads", value.ToString());
                OnPropertyChanged(nameof(SliderValue));
            }
        }
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Дополнительная логика ViewModel...
}