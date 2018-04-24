using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using static System.String;

namespace Экзамен_ПДД
{
    /// <summary>
    /// Класс для парсинга билетов с сайта
    /// </summary>
    public class WebParsing
    {
        public const int MAX_TICKET_NUMBER = 20;
        public const int MAX_PAGE_NUMBER = 40;
        const string TYPE_QUESTION_TEXT = "question-text";
        const string TYPE_QUESTION_HINT = "question-comment";
        private int QuestionsDownloaded = 0;

        [XmlArray("QuestionsList"), XmlArrayItem(typeof(Question), ElementName = "Question")]
        public List<Question> QuestionsList = new List<Question>();
        public delegate void QuestionCountChangeHandler(int newCount);
        public event QuestionCountChangeHandler QuestionListCountChanged;

        public int DownloadedCount
        {
            get { return QuestionsDownloaded; }
            set
            {
                QuestionListCountChanged?.Invoke(QuestionsDownloaded);
            }
        }

        /// <summary>
        /// Берет со страницы данные и создает экземпляр класса <see cref="Question"/> и добавляет его в QuestionsList
        /// </summary>
        /// <param name="html">Код веб-страницы</param>
        private void GetData(HtmlDocument html)
        {
            Question question = new Question
            {
                QuestionText = QuestionOrHintParsing(html, TYPE_QUESTION_TEXT),
                Hint = QuestionOrHintParsing(html, TYPE_QUESTION_HINT),
                Answers = ListOfAnswers(html)
            };
            question.CorrectAnswerIndex = question.Answers.IndexOf(CorrectAnswer(QuestionOrHintParsing(html, TYPE_QUESTION_HINT)));
            if (!IsNullOrEmpty(GetImageURL(html))) {question.ImageURL = GetImageURL(html);}
            QuestionsList.Add(question);
            
        }

        /// <summary>
        /// Обращается к серверу по ссылке, вызывая <see cref="GetServerSourceAsString"/> , если обращение неудачно - перевызыается по таймауту (задан в константой TIMEOUT_MAX в мс внутри метода)
        /// </summary>
        /// <param name="url">Ссылка на сайт</param>
        /// <returns>Если обращение к серверу - удачно - true, иначе - false </returns>
        private async Task<bool> AccessToServer(string url)
        {
            var client = new HttpClient();
            const int TIMEOUT_MAX = 12000;
            var stopWatch = Stopwatch.StartNew();
            var ts = stopWatch.ElapsedMilliseconds;
            while (ts < TIMEOUT_MAX)
            {
                var t = await GetServerSourceAsString(url, client);
                if (t == true)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Получает содержимое веб-страницы в виде HtmlDocument и передает его в <see cref="GetData"/>
        /// </summary>
        /// <param name="url">Ссылка на сайт</param>
        /// <param name="client">Экземпляр HttpClient для вызова client.GetStringAsync(url)</param>
        /// <returns>Если содержимое страницы получено успешно - true, иначе - false </returns>
        private async Task<bool> GetServerSourceAsString(string url, HttpClient client)
        {
            bool IsSuccesful = true;
            try
            {
                var html = new HtmlDocument();
                string data;
                data = await client.GetStringAsync(url);
                html.LoadHtml(data);
                GetData(html);
                IsSuccesful = true;
            }
            catch (HttpRequestException)
            {
                client.DefaultRequestHeaders.Add("user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
                IsSuccesful = false;
            }

            return IsSuccesful;
        }

        /// <summary>
        /// Этот метод парсит либо вопрос, либо подсказку - в зависимости от значения TypeOfContent
        /// </summary>
        /// <param name="html">Код веб-страницы</param>
        /// <param name="TypeOfContent">Парсит текст вопроса - TYPE_QUESTION_TEXT, парсит текст подсказки - TYPE_QUESTION_HINT</param>
        /// <returns>Возвращает строку</returns>
        private string QuestionOrHintParsing(HtmlDocument html, string TypeOfContent)
        {
            string result = "";
            var root = html.DocumentNode;
            var p = root.Descendants()
                .Where(n => n.GetAttributeValue("class", "").Equals(TypeOfContent))
                .Single();
            var content = p.InnerText;
            result = content.Trim();
            result = WebUtility.HtmlDecode(result);
            if (TypeOfContent == TYPE_QUESTION_HINT)
            {
                int textIndex = result.IndexOf(" Темы вопроса");
                if (textIndex >= 0)
                {
                    result = result.Substring(0, textIndex + 1);
                }
            }

            result = Regex.Replace(result, @"\s+", " ");
            return result;
        }

        /// <summary>
        /// В цикле вызывает <see cref="AccessToServer"/> и сериализует QuestionsList в XML
        /// </summary>
        public async void GetParsingResults()
        {
            int ticketNumber = 1;
            int count = 1;
            for (int pageNumber = 1; pageNumber < MAX_PAGE_NUMBER + 1; pageNumber++)
            {
                while (ticketNumber < MAX_TICKET_NUMBER + 1)
                {
                    string url = "https://pdde.ru/bilet" + pageNumber + "/vopros" + ticketNumber;
                    await Task.Run(() => { var t = AccessToServer(url).Result; });
                    count++;
                    ticketNumber++;
                    
                }
                ticketNumber = 1;
                QuestionsDownloaded = pageNumber;
                DownloadedCount++;
                XmlSerializer Serializer = new XmlSerializer(typeof(List<Question>));
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile questionFile =
                    await storageFolder.CreateFileAsync(pageNumber + ".xml",
                        CreationCollisionOption.ReplaceExisting);
                using (Stream stream = await questionFile.OpenStreamForWriteAsync())
                {
                    Serializer.Serialize(stream, QuestionsList);
                }
                QuestionsList.Clear();
            }
        }

        /// <summary>
        /// Парсит варианты ответов
        /// </summary>
        /// <param name="html">Код веб-страницы</param>
        /// <returns>Возвращает варианты ответов</returns>
        private static List<string> ListOfAnswers(HtmlDocument html)
        {
            var result = html.DocumentNode
                .SelectNodes("//span[@class='question-option']")
                .Select(p => p.InnerText)
                .ToList();
            return result;
        }

        /// <summary>
        /// Парсит правильный ответ из подсказки
        /// </summary>
        /// <param name="Hint">Строка с подсказкой</param>
        /// <returns>Возвращает правильный ответ</returns>
        private string CorrectAnswer(string Hint)
        {
            string result = Hint.Substring(Hint.IndexOf("Правильный ответ"));
            result = Regex.Match(result, @"«(.*)»").Groups[0].Value;
            result = result.Replace(@"«", "").Replace(@"»", "");
            return result;
        }

        /// <summary>
        /// Берет ссылку на картинку с сайта
        /// </summary>
        /// <param name="html">Код веб-страницы</param>
        /// <returns>Ссылку на картинку</returns>
        private string GetImageURL(HtmlDocument html)
        {
            string result = "";
            var elements = html.DocumentNode.Descendants("p").Where(o => o.GetAttributeValue("class", "") == "question-image");
            foreach (var nodeItem in elements)
            {
                try
                {
                    var imgTag = nodeItem.Descendants("img").First();
                    var imgTagSrcValue = imgTag.Attributes["src"];
                    result = "https://pdde.ru" + imgTagSrcValue.Value;
                }
                catch (InvalidOperationException)
                {
                    result = "";
                    throw;
                }
            }
            return result;
        }
    }
}