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
using static Mail_Send.email;
using static Mail_Send.text;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class past_emails_content : Page
{
    Int64 ind = 0;
    TextBlock email_block;
    private static string connectionString = "Filename==database.db";
    public past_emails_content(Int64 index, TextBlock sharedem)
    {
        email_block = sharedem;
        ind = (int)index;
        this.InitializeComponent();
        var all_email = Email_Sql.Get_past_email();
        var selected_all_email = all_email.FirstOrDefault(x => x.id == index);
        email_name.Text = selected_all_email.name;
        email_text.Text = selected_all_email.email;
        random_texts.IsChecked = selected_all_email.is_random;
        var collectionViewSource = (CollectionViewSource)Resources["text_list"];
        collectionViewSource.Source = selected_all_email.texts;
        var collectionViewSource1 = (CollectionViewSource)Resources["folder_list"];
        collectionViewSource1.Source = selected_all_email.folders;
        this.email_block = email_block;
    }
}
