using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Экзамен_ПДД
{
    /// <summary>
    /// Содержит логику для проверки ответов и осуществления экзамена
    /// </summary>
    public class Examination
    {
        public const int QUANTITY_OF_BLOCKS = 4;
        private const int QUANTITY_QUESTIONS_IN_BLOCK = 5;
        private const int TIME_OF_EXAM = 1200;
        public int countOfResolvedQuestions = 0;
        public bool[] ResolvedInTicket = new bool[WebParsing.MAX_TICKET_NUMBER];

        private ContentDialog ExamResultDialog = new ContentDialog();
        private readonly DispatcherTimer countdownTimer;
        private int seconds;
        public int Seconds
        {
            get { return seconds; }
            set
            {
                seconds = value;
                SecondsChanged?.Invoke(Seconds);
            }
        }
        public delegate void PropertyChangeHandler(int newValue);
        public event PropertyChangeHandler SecondsChanged;
        public Examination()
        {
            countdownTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            countdownTimer.Tick += CountdownTimer_Tick;
            Seconds = TIME_OF_EXAM;
            countdownTimer.Start();
        }
        private void CountdownTimer_Tick(object sender, object e)
        {
            Seconds--;
            if (Seconds == 0)
            {
                countdownTimer.Stop();
                ShowExamDialog();
            }
        }
        /// <summary>
        /// Проверяет - сдан ли экзамен
        /// Экзамен сдан, если:
        /// 1)Время экзамена не истекло и прорешаны все блоки 
        /// 2)В блоках нет ошибок ИЛИ в двух блоках 1 ошибка, а в оставшихся блоках ошибок нет
        /// Блоки проверяет <see cref="ResultInBlock"/>
        /// </summary>
        /// <returns>Если экзамен успешно сдан - true, иначе - false</returns>
        private bool IsExamSuccess()
        {
            int count = 0;
            for (int i = 1; i <= QUANTITY_OF_BLOCKS; i++)
            {
                if (ResultInBlock(i) == false)
                {
                    count++;
                }
                if (count > 2 || ResolvedInTicket.Where(c => !c).Count() > 2) { return false; }
            }
            return true;
        }

        public void CheckExamResult()
        {
            if (countOfResolvedQuestions == WebParsing.MAX_TICKET_NUMBER)
            {
                ShowExamDialog();
            }
        }
        private async void ShowExamDialog()
        {
            string ExamState(bool ExamResult)
            {
                if (ExamResult) { return "Экзамен сдан"; }
                return "Экзамен не сдан";
            }
            var timespan = TimeSpan.FromSeconds(Seconds);
            TextBlock InformationAboutExam = new TextBlock
            {
                Text =
                    "Количество решенных вопросов: " + countOfResolvedQuestions + " из " + WebParsing.MAX_TICKET_NUMBER + "\n" + ExamState(IsExamSuccess()) + "\n"
                + "Количество ошибок: " + ResolvedInTicket.Where(c => !c).Count() + " из " + WebParsing.MAX_TICKET_NUMBER + "\n" + "Оставшееся время: " + timespan.ToString(@"mm\:ss"),
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap
            };

            ExamResultDialog.Content = InformationAboutExam;
            ExamResultDialog.CloseButtonText = "Потренироваться еще";
            ExamResultDialog.CloseButtonClick += BackToMainPage;
            countdownTimer.Stop();
            await ExamResultDialog.ShowAsync();
        }

        private void BackToMainPage(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame != null && frame.CanGoBack)
            {
                frame.Navigate(typeof(MainPage));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        /// <summary>
        /// Проверяет, верен ли блок заданий  
        /// </summary>
        /// <param name="NumberOfBlock">Номер блока от 1 до <see cref="QUANTITY_OF_BLOCKS"/></param>
        /// <returns>Если в блоке  более 1 ошибки - вернет false, иначе - true</returns>
        private bool ResultInBlock(int NumberOfBlock)
        {
            int errorsInBlock = 0;
            for (int i = QUANTITY_QUESTIONS_IN_BLOCK * (NumberOfBlock - 1); i <= QUANTITY_QUESTIONS_IN_BLOCK * (NumberOfBlock - 1) + QUANTITY_OF_BLOCKS; i++)
            {
                if (ResolvedInTicket[i] == false) { errorsInBlock++; }
                if (errorsInBlock > 1) { return false; }
            }
            return true;
        }

    }
}
