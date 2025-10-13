using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Mail_Send.Views;

namespace Mail_Send.ViewModels;
internal class start
{
    public ICommand StartCommand
    {
        get; private set;
    }

    public start()
    {
        StartCommand = new RelayCommand(StartAction);
    }

    private void StartAction()
    {
        Button button = new Button();
        home h = new home();

        ShellPage shellpage = ShellPage.Current;
        shellpage.AppFrame.Navigate(h.GetType());
        h.background_start();


    }
}
