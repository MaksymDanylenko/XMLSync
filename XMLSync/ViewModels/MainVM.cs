using Microsoft.Win32;
using Prism.Commands;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using XMLSync.Models;

namespace XMLSync.ViewModels
{
    // ViewModel главного окна программы
    class MainVM : INotifyPropertyChanged
    {
        public string LeftDocumentPath { get; private set; } // Путь к левому файлу
        public string RightDocumentPath { get; private set; } // Путь к правому файлу

        public Document LeftDocument { get; private set; } // Левый документ
        public Document RightDocument { get; private set; } // Правый документ

        public StringBuilder ComparingRepresent { get; private set; } // Строка, сохраняет результат сравнения документов

        public event PropertyChangedEventHandler PropertyChanged; // Событие изменения свойства

        public DelegateCommand<string> LoadLeftCommand { get; private set; } // Комманда загрузки левого файла
        public DelegateCommand<string> LoadRightCommand { get; private set; } // Комманда загрузки правого файла

        public DelegateCommand DialogLeftCommand { get; private set; } // Комманда открытия левого файла в проводнике
        public DelegateCommand DialogRightCommand { get; private set; } // Комманда открытия правого файла в проводнике

        public DelegateCommand AboutCommand { get; private set; } // Комманда вызова справки

        public DelegateCommand LeftToRightCommand { get; set; } // Комманда синхронизации левого файла с правым
        public DelegateCommand RightToLeftCommand { get; set; } // Комманда синхронизации правого файла с левым

        private readonly string _about; // Справка

        // Конструктор, присваивание методов соответствующим коммандам
        public MainVM()
        {
            LoadLeftCommand = new DelegateCommand<string>(LoadLeftAsync);
            LoadRightCommand = new DelegateCommand<string>(LoadRightAsync);

            DialogLeftCommand = new DelegateCommand(DialogLeft);
            DialogRightCommand = new DelegateCommand(DialogRight);

            LeftToRightCommand = new DelegateCommand(LeftToRight);
            RightToLeftCommand = new DelegateCommand(RightToLeft);

            AboutCommand = new DelegateCommand(About);

            _about = "Что-бы открыть файл, введите путь и нажмите \"Открыть\"(->)\n" +
                "Для открытия с помощью проводника нажмите \"Открыть в проводнике\"(...)\n" +
                "----- ----- ----- -----\n" +
                "<> - узлы или аттрибуты не соответствуют\n" +
                "!= - узлы или аттрибуты соответствуют\n" +
                "= - узлы или аттрибуты равны\n" +
                "----- ----- ----- -----\n" +
                "Если файлы имеют разную структуру (разное количество или порядок узлов или аттрибутов), корневые излы будут помечены как несоответствующие (<>)\n" +
                "----- ----- ----- -----\n" +
                "Для синхронизации воспользуйтесь соответствующей кнопкой внизу окна";
        }

        // Асинхронная загрузка левого файла
        // Параметры: string path - путь к файлу
        private async void LoadLeftAsync(string path)
        {
            await Task.Run(() => LoadLeft(path));
            CompareDocumentsAsync();
        }

        // Асинхронная загрузка правого файла
        // Параметры: string path - путь к файлу
        private async void LoadRightAsync(string path)
        {
            await Task.Run(() => LoadRight(path));
            CompareDocumentsAsync();
        }

        // Асинхронное сравнение файлов
        private async void CompareDocumentsAsync()
        {
            await Task.Run(() => CompareDocuments());
        }

        // Загрузка левого файла
        // Параметры: string path - путь к файлу
        private void LoadLeft(string path)
        {
            this.LeftDocumentPath = path;
            try
            {
                this.LeftDocument = new Document(LeftDocumentPath);
            }
            catch (System.Exception)
            {
                MessageBox.Show("Ошибка при загрузке левого файла", "Ошибка");
            }
            OnPropertyChanged("LeftDocument");
        }

        // Загрузка правого файла
        // Параметры: string path - путь к файлу
        private void LoadRight(string path)
        {
            this.RightDocumentPath = path;

            try
            {
                this.RightDocument = new Document(RightDocumentPath);
            }
            catch (System.Exception)
            {
                MessageBox.Show("Ошибка при загрузке правого файла", "Ошибка");
            }
            OnPropertyChanged("RightDocument");
        }

        // Открытие левого файла в проводнике
        private void DialogLeft()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                LeftDocumentPath = dialog.FileName;
                OnPropertyChanged("LeftDocumentPath");
                LoadLeftAsync(LeftDocumentPath);
            }
        }

        // Загрузка правого файла в проводнике
        private void DialogRight()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                RightDocumentPath = dialog.FileName;
                OnPropertyChanged("RightDocumentPath");
                LoadRightAsync(RightDocumentPath);
            }
        }

        // Сравнение документов
        private void CompareDocuments()
        {
            if (LeftDocument != null && RightDocument != null)
            {
                if (LeftDocument.XmlDocument != null && RightDocument.XmlDocument != null)
                {
                    try
                    {
                        this.ComparingRepresent = XmlComparer
                            .CompareXmlDocuments(this.LeftDocument.XmlDocument,
                            this.RightDocument.XmlDocument);
                    }
                    catch (System.Exception)
                    {
                        this.ComparingRepresent = new StringBuilder("<>");
                    }

                    OnPropertyChanged("ComparingRepresent");
                }
            }
        }

        // Синхронизация левого файла с правым
        private void LeftToRight() 
        {
            var documentsExist = DocumentsExist();

            if (documentsExist)
            {
                LeftDocument.XmlDocument = RightDocument.XmlDocument;
                LeftDocument.XmlDocument.Save(LeftDocumentPath);

                LoadLeftAsync(LeftDocumentPath);
            }
        }

        // Синхронизация правого файла с левым
        private void RightToLeft()
        {
            var documentsExist = DocumentsExist();

            if (documentsExist)
            {
                RightDocument.XmlDocument = LeftDocument.XmlDocument;
                RightDocument.XmlDocument.Save(RightDocumentPath);

                LoadRightAsync(RightDocumentPath);
            }
        }

        // Проверка загружености документов
        private bool DocumentsExist()
        {
            if (LeftDocument == null || RightDocument == null)
            {
                MessageBox.Show("Откройте файлы", "Ошибка");
                return false;
            }
            else if (LeftDocument.XmlDocument == null || RightDocument.XmlDocument == null)
            {
                MessageBox.Show("Откройте корректные файлы", "Ошибка");
                return false;
            }
            else
            {
                return true;
            }
        }

        // Вызов справки
        private void About()
        {
            MessageBox.Show(_about, "Справка");
        }

        // Оповещение View о изменении свойства
        // Параметры: string propertyName - название свойства
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
