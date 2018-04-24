using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static Windows.ApplicationModel.Core.CoreApplication;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Экзамен_ПДД
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private ContentDialog DeleteContentDialog = new ContentDialog();
        private ProgressBar DeleteProgressBar = new ProgressBar();
        private TextBlock DeleteTextBlock = new TextBlock();
        private StackPanel DeleteStackPanel = new StackPanel();
       
        private DeleteFiles Delete = new DeleteFiles();

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame != null && frame.CanGoBack)
            {
                frame.Navigate(typeof(MainPage));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                e.Handled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame frame = Window.Current.Content as Frame;
            if (frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }
        }

        public Settings()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private async void ProgressBar_ValueChanged(int newCount)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    DeleteTextBlock.Text = Delete.DeletedFilesCount + " из " + WebParsing.MAX_PAGE_NUMBER + " билетов удалено";
                    DeleteProgressBar.Value = newCount;
                    if (DeleteProgressBar.Value == WebParsing.MAX_PAGE_NUMBER)
                    {
                        Exit();
                    }
                });

        }

        private void DeleteTickets_Button_OnClick(object sender, RoutedEventArgs e)
        {
            ShowDeleteProgress();
        }

        private async void ShowDeleteProgress()
        {
            Delete.DeletedCountChanged += ProgressBar_ValueChanged;
            Delete.DeleteFilesInLocalFolder();
            DeleteStackPanel.Children.Add(DeleteTextBlock);
            DeleteStackPanel.Children.Add(DeleteProgressBar);
            DeleteContentDialog.Content = DeleteStackPanel;
            DeleteProgressBar.Minimum = 0;
            DeleteProgressBar.Maximum = WebParsing.MAX_PAGE_NUMBER;
            await DeleteContentDialog.ShowAsync();
        }
    }
}
