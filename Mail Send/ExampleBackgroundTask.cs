using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class ExampleBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // Выполнить некоторую работу в фоне
            DoSomeWork();

            // Освободить деферрал, чтобы сообщить о завершении фоновой задачи
            deferral.Complete();
        }
        private void DoSomeWork()
        {
            // Создать уведомление о том, что фоновая задача запущена
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode("Фоновая задача запущена"));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}