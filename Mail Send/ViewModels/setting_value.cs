using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Mail_Send.setting;
namespace Mail_Send.ViewModels;
public class setting_value : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int SliderValue
    {
        get
        {
            return setting_Sql.Get_setting().threads; // Предполагается, что GetSetting возвращает актуальные данные
        }
        set
        {
            // Здесь код для обновления значения в источнике данных, например в SettingSql
            OnPropertyChanged(nameof(SliderValue));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

