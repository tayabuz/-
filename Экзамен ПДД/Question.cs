using System.Collections.Generic;

namespace Экзамен_ПДД
{
    /// <summary>
    /// Класс, реализующий вопрос в билете, содержит в себе: 1)текст вопроса - QuestionText; 2)подсказку - Hint; 3)список вопроса - Answers; 4)ссылку на картинку (может быть null) - ImageURL; 5)CorrectAnswerIndex - индекс правильного ответа в Answers
    /// </summary>
    public class Question
    {
        public int CorrectAnswerIndex { get; set; }

        public string Hint { get; set; }

        public string QuestionText { get; set; }

        public List<string> Answers { get; set; }

        public string ImageURL { get; set; }

        public Question()
        {
        }
    }
}
