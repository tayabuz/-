using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static Windows.ApplicationModel.Core.CoreApplication;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Экзамен_ПДД
{
    public sealed partial class MainPage : Page
    {
        private WebParsing webParsing = new WebParsing();
        private ProgressBar ProgressBarQuestionsLoading = new ProgressBar();
        private TextBlock TextBlockQuestionsLoading = new TextBlock();
        private StackPanel StackPanelQuestionsLoading = new StackPanel();
        private ContentDialog downloadContentDialog = new ContentDialog();
        private ContentDialog warningAboutTrainingExam = new ContentDialog();
        public Payload payload = new Payload();

        private const int HEIGTH_FIELD = 8;
        private const int WIDTH_FIELD = 5;

        public MainPage()
        {
            this.InitializeComponent();
            CreateGridRowsAndColumns();
            CreateQuestionsField();
            bool isDownloadQuestionsComplete = IsFilePresent(1 + ".xml").Result;
            if (isDownloadQuestionsComplete == false)
            {
                DownloadQuestionsDialog();
                webParsing.QuestionListCountChanged += ProgressBar_ValueChanged;
            }
            else { WarningAboutTrainingExamDialog(); }
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private async void ProgressBar_ValueChanged(int newCount)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    TextBlockQuestionsLoading.Text = webParsing.DownloadedCount + " из " + WebParsing.MAX_PAGE_NUMBER + " билетов загружено";
                    ProgressBarQuestionsLoading.Value = newCount;
                    if (ProgressBarQuestionsLoading.Value == WebParsing.MAX_PAGE_NUMBER)
                    {
                        downloadContentDialog.Hide();
                    }
                });

        }

        private async void WarningAboutTrainingExamDialog()
        {
            TextBlock textWarningTextBlock = new TextBlock
            {
                Text =
                    "В отличие от экзамена в ГИБДД, тренировка проводится без добавления дополнительных вопросов за ошибки!!!",
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap
            };
            warningAboutTrainingExam.Content = textWarningTextBlock;
            warningAboutTrainingExam.CloseButtonText = "Ок";
            await warningAboutTrainingExam.ShowAsync();
        }

        /// <summary>
        /// Вызывает диалог с прогрессбаром и загрузку билетов
        /// </summary>
        private async void DownloadQuestionsDialog()
        {
            webParsing.GetParsingResults();
            StackPanelQuestionsLoading.Children.Add(TextBlockQuestionsLoading);
            StackPanelQuestionsLoading.Children.Add(ProgressBarQuestionsLoading);

            downloadContentDialog.Title = "Пожалуйста, подождите, билеты загружаются";
            downloadContentDialog.Content = StackPanelQuestionsLoading;
            downloadContentDialog.CloseButtonText = "Закрыть приложение";
            downloadContentDialog.CloseButtonClick += CloseApplication;
            ProgressBarQuestionsLoading.Minimum = 0;
            ProgressBarQuestionsLoading.Maximum = WebParsing.MAX_PAGE_NUMBER;

            await downloadContentDialog.ShowAsync();
        }

        private void CloseApplication(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Exit();
        }

        /// <summary>
        /// Выполняет десериализацию XML
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="filename">Имя файла c с расширением</param>
        /// <returns>Возвращает объект типа T</returns>
        public static async Task<T> ReadObjectFromXmlFileAsync<T>(string filename)
        {
            T objectFromXml = default(T);
            var serializer = new XmlSerializer(typeof(T));
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync(filename);
            Stream stream = await file.OpenStreamForReadAsync();
            objectFromXml = (T)serializer.Deserialize(stream);
            stream.Dispose();
            return objectFromXml;
        }

        /// <summary>
        /// Создает сетку для кнопок,  про кнопки - смотри <see cref="CreateButton"/>
        /// </summary>
        private void CreateGridRowsAndColumns()
        {
            for (int i = 0; i < HEIGTH_FIELD; i++)
            {
                RowDefinition c2 = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                QuestionsField_Grid.RowDefinitions.Add(c2);
            }
            for (int j = 0; j < WIDTH_FIELD; j++)
            {
                ColumnDefinition c1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                QuestionsField_Grid.ColumnDefinitions.Add(c1);
            }

        }

        private async Task<bool> IsFilePresent(string fileName)
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        /// <summary>
        /// Заполняет сетку кнопками, про кнопки - смотри <see cref="CreateButton"/>
        /// </summary>
        private void CreateQuestionsField()
        {
            int count = 1;
            for (int i = 0; i < HEIGTH_FIELD; i++)
            {
                for (int j = 0; j < WIDTH_FIELD; j++)
                {
                    CreateButton(i, j, count);
                    count++;
                }
            }

        }
        /// <summary>
        /// Создает кнопку, при нажатии на которую вызывается <see cref="ReadObjectFromXmlFileAsync{T}"/> и совершается переход на <see cref="TicketField"/> и добавляет ее в сетку
        /// </summary>
        /// <param name="row">Номер столбца</param>
        /// <param name="column">Номер строки</param>
        /// <param name="count">Номер кнопки</param>
        private void CreateButton(int row, int column, int count)
        {
            Button button = new Button
            {
                Height = Double.NaN,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = count,
                BorderBrush = new SolidColorBrush(Colors.Black)
            };
            
            QuestionsField_Grid.Children.Add(button);
            Grid.SetRow(button, row);
            Grid.SetColumn(button, column);
            button.Click += async (s, e) =>
            {
                int numberOfFile = Grid.GetRow(button) * WIDTH_FIELD + Grid.GetColumn(button) + 1;
                payload.NumberOfQuestion = numberOfFile;
                List<Question> questions = await ReadObjectFromXmlFileAsync<List<Question>>(numberOfFile + ".xml");
                payload.Questions = questions;
                Frame.Navigate(typeof(TicketField), payload);
            };
        }

        private void SetSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));
        }
    }
}