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
using PayPal.Api;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mail_Send;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class shop : Page
{
    public shop()
    {
        this.InitializeComponent();
        
    }
    public async void start_oplata()
    {
        webView.Visibility = Visibility.Visible;
        var apiContext = new APIContext(new OAuthTokenCredential("AQfeE7A-Bkzl8q3uOQVSPofVajYeUT2s94Iuln7xcVjqUvejukQunoBigrYmbYvI1DFSJ8jZiVhKvBJn", "ENxIKDL3lQ_6qjdBP5ZaHeakRV8P-rgQHk5KDzdcaYXujxjwc_cFt29qLr2SKrhGva94d1etXvaIK5ag").GetAccessToken());

        var payment = new Payment
        {
            intent = "sale",
            payer = new Payer
            {
                payment_method = "paypal",
                payer_info = new PayerInfo
                {
                    payer_id = "00001"
                }
            },
            transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        description = "Transaction description.",
                        invoice_number = "001",
                        amount = new Amount
                        {
                            currency = "USD",
                            total = "0.1", // Replace with the actual price.
                        }
                    }
                },
            redirect_urls = new RedirectUrls
            {
                return_url = "http://yourwebsite.com/return", // URL to return to after payment approval.
                cancel_url = "http://yourwebsite.com/cancel" // URL to return to after payment cancellation.
            }
        };
        
        var createdPayment = payment.Create(apiContext);
        payment.redirect_urls = new RedirectUrls { return_url =$"mailsendler://payment_check?id={createdPayment.id}", cancel_url= $"mailsendler://payment_check?id={createdPayment.id}" };
        // Use the createdPayment object to send the user to the PayPal payment page.
        // You can retrieve the approval URL from the `createdPayment` object:
        var approvalUrl = createdPayment.GetApprovalUrl();
        await webView.EnsureCoreWebView2Async();
        webView.CoreWebView2.Navigate(approvalUrl);
        Debug.WriteLine(JsonConvert.SerializeObject(createdPayment.id));

        

    }

    private void buy_Click(object sender, RoutedEventArgs e)
    {
        start_oplata();
    }
}
