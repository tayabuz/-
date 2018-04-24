using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Экзамен_ПДД
{
    /// <summary>
    /// Отображает содержимое билета
    /// </summary>
    public sealed partial class QuestionsUserControl : UserControl
    {
        public Question Question { get { return this.DataContext as Question; } }
        private static Frame frame = Window.Current.Content as Frame;
        private TicketField page = (TicketField)frame.Content;

        public QuestionsUserControl()
        {
            this.InitializeComponent();
        }

        private void ListBoxQuestionAnswers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool currentAnswerState;
            if (ListBoxQuestionAnswers.SelectedIndex == Question.CorrectAnswerIndex)
            {
                currentAnswerState = true;
                ListBoxQuestionAnswers.Resources["SystemControlHighlightListAccentLowBrush"] = new SolidColorBrush(Colors.Green);
                ListBoxQuestionAnswers.Resources["SystemControlHighlightListAccentMediumBrush"] = new SolidColorBrush(Colors.Green); 
            }
            else
            {
                currentAnswerState = false;
                ListBoxQuestionAnswers.Resources["SystemControlHighlightListAccentLowBrush"] = new SolidColorBrush(Colors.Red);
                ListBoxQuestionAnswers.Resources["SystemControlHighlightListAccentMediumBrush"] = new SolidColorBrush(Colors.Red);
            }
            var index = page.PivotQuestion.SelectedIndex;
            page.CurrentExamination.ResolvedInTicket[index] = currentAnswerState;
            page.CurrentExamination.countOfResolvedQuestions++;
            QuestionHintTextBlock.Visibility = Visibility.Visible;
            ListBoxQuestionAnswers.IsHitTestVisible = false;
            page.CurrentExamination.CheckExamResult();
        }

    }
}
