using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Экзамен_ПДД
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = value as Question;
            var index = 0;
            string result = null;
            Frame frame = Window.Current.Content as Frame;
            if (frame != null && frame.Content is TicketField)
            {
                var page = (TicketField)frame.Content;
                index = page.QuestionsList.IndexOf(item) + 1;
                result = "Вопрос №" + index;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
