using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Экзамен_ПДД
{
    /// <summary>
    /// Страница, отображающая вопросы в одном билете. Для отображения вопроса используется UserControl - <see cref="QuestionsUserControl"/>
    /// </summary>
    public sealed partial class TicketField : Page
    {
        public List<Question> QuestionsList;
        private Payload payload = new Payload();
        public Examination CurrentExamination = new Examination();

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
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

            if ((e.Parameter != null) && (e.Parameter.ToString() != ""))
            {
                payload = (Payload) e.Parameter;
                PivotQuestion.Title = "Билет №" + payload.NumberOfQuestion;
                QuestionsList = payload.Questions;
            }
        }
        public TicketField()
        {
            this.InitializeComponent();
            CurrentExamination.SecondsChanged += ChangeSeconds;
            ChangeSeconds(CurrentExamination.Seconds);
        }
        private async void ChangeSeconds(int newSeconds)
        {
            var timespan = TimeSpan.FromSeconds(newSeconds);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {TimeExam_TextBlock.Text = "До конца экзамена осталось: " + timespan.ToString(@"mm\:ss"); });
        }
    }
}
