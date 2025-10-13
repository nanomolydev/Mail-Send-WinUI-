using Mail_Send.Helpers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using System.Diagnostics;
using Mail_Send.Activation;
using Mail_Send.Contracts.Services;
using Mail_Send.Core.Contracts.Services;
using Mail_Send.Core.Services;
using Mail_Send.Helpers;
using Mail_Send.Services;
using Mail_Send.ViewModels;
using Mail_Send.Views;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Popups;
using System.ComponentModel;
using System;
using System.Drawing;

using Microsoft.UI.Xaml;
namespace Mail_Send;

public sealed partial class MainWindow : WindowEx
{
    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private UISettings settings;
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
    public class ViewModel
    {
        public List<Person> Data
        {
            get; set;
        }
        public ViewModel()
        {
            Data = new List<Person>()
        {
            new Person { Name = "David", Height = 170 },
            new Person { Name = "Michael", Height = 96 },
            new Person { Name = "Steve", Height = 65 },
            new Person { Name = "Joel", Height = 182 },
            new Person { Name = "Bob", Height = 134 }
        };
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}
