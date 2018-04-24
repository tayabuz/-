using System;
using Windows.Storage;

namespace Экзамен_ПДД
{
    /// <summary>
    /// Нужен для удаления файлов
    /// </summary>
    public class DeleteFiles
    {
        private int deletedFiles = 1;

        public delegate void DeletedFilesCountChangeHandler(int newCount);
        public event DeletedFilesCountChangeHandler DeletedCountChanged;
        public int DeletedFilesCount
        {
            get { return deletedFiles; }
            set
            {
                deletedFiles = value;
                DeletedCountChanged?.Invoke(deletedFiles);
            }
        }
        /// <summary>
        /// Удаляет файлы из локальной директории приложения
        /// </summary>
        public async void DeleteFilesInLocalFolder()
        {
            for (int i = 1; i <= WebParsing.MAX_PAGE_NUMBER; i++)
            {
                StorageFile filed = await ApplicationData.Current.LocalFolder.GetFileAsync(i + ".xml");
                if (filed != null)
                {
                    await filed.DeleteAsync();
                    DeletedFilesCount = i;
                }
            }
        }
    }
}
