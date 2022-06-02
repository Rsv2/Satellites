using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Satellites
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Шаг между зондами
        /// </summary>
        private int Step;
        /// <summary>
        /// Число зондов по оси X
        /// </summary>
        private int PosX;
        /// <summary>
        /// Число зондов по оси Y
        /// </summary>
        private int PosY;

        #region Поля
        /// <summary>
        /// Шаг между зондами (string)
        /// </summary>
        private string stepstr;
        /// <summary>
        /// Открыть сохранение
        /// </summary>
        private RelayCommand openxml;
        /// <summary>
        /// Число зондов по оси Y (string)
        /// </summary>
        private string posystr;
        /// <summary>
        /// Число зондов по оси X (string)
        /// </summary>
        private string posxstr;
        /// <summary>
        /// Имя зонда
        /// </summary>
        private string zond;
        #endregion

        #region Команды
        /// <summary>
        /// Открыть сохранение
        /// </summary>
        public RelayCommand OpenXML => openxml ?? (openxml = new RelayCommand(obj => OpenXMLComm()));
        #endregion

        #region Свойства
        /// <summary>
        /// Шаг между зондами
        /// </summary>
        public string StepStr
        {
            get => stepstr;
            set
            {
                while (value.StartsWith("0")) { value = value.Substring(1); }
                char[] text = value.ToCharArray();
                if (text.Length > 0) { for (int i = 0; i < text.Length; i++) { if (!char.IsDigit(text[i])) { value = "32"; } } }
                else { value = "32"; }
                stepstr = value;
                Step = Convert.ToInt32(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StepStr)));
            }
        }
        /// <summary>
        /// Число зондов по оси Y
        /// </summary>
        public string PosYStr
        {
            get => posystr;
            set
            {
                while (value.StartsWith("0")) { value = value.Substring(1); }
                char[] text = value.ToCharArray();
                if (text.Length > 0) { for (int i = 0; i < text.Length; i++) { if (!char.IsDigit(text[i])) { value = "10"; } } }
                else { value = "10"; }
                PosY = Convert.ToInt32(value);
                posystr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PosYStr)));
            }
        }
        /// <summary>
        /// Число зондов по оси X
        /// </summary>
        public string PosXStr
        {
            get => posxstr;
            set
            {
                while (value.StartsWith("0")) { value = value.Substring(1); }
                char[] text = value.ToCharArray();
                if (text.Length > 0) { for (int i = 0; i < text.Length; i++) { if (!char.IsDigit(text[i])) { value = "10"; } } }
                else { value = "10"; }
                PosX = Convert.ToInt32(value);
                posxstr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PosXStr)));
            }
        }
        /// <summary>
        /// Имя зонда
        /// </summary>
        public string Zond
        {
            get => zond;
            set
            {
                zond = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zond)));
            }
        }
        #endregion

        public ViewModel()
        {
            StepStr = "32";
            PosXStr = "10";
            PosYStr = "10";
            Zond = "Тест-зонд";
        }

        /// <summary>
        /// Открыть сохранение
        /// </summary>
        private async void OpenXMLComm()
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Откройте XML или GZ файл сохранения игры",
                Filter = ".gz; .xml|*.gz; *.xml"
            };
            if (fileDialog.ShowDialog() == true)
            {
                Application.Current.MainWindow.Hide();
                Log log = new Log();
                log.Show();
                string InFile = fileDialog.FileName;
                string UnpackedFile = $"{System.Windows.Forms.Application.StartupPath}\\TempZondSource";
                string FinishedFile = $"{System.Windows.Forms.Application.StartupPath}\\TempZondOut";
                AddZonds addZonds = new AddZonds(InFile, UnpackedFile, FinishedFile, Zond, PosX, PosY, Step);
                log.DataContext = addZonds;
                await Task.Run(() => addZonds.Start());
                log.Close();
                Application.Current.MainWindow.Close();
            }
        }
    }
}





