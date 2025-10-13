using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
namespace Mail_Send.ViewModels;
public class ProgressViewModel : INotifyPropertyChanged
{
    private double _value;
    private readonly SemaphoreSlim _valueLock = new SemaphoreSlim(1, 1);

    public double Value
    {
        get
        {
            return _value;
        }
        private set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    public ProgressViewModel()
    {
        _value = 0;
    }

    public async Task UpdateValueAsync(double newValue)
    {
        await _valueLock.WaitAsync();
        try
        {
            Value = newValue;
        }
        finally
        {
            _valueLock.Release();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
