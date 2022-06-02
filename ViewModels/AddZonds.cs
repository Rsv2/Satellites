using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.ComponentModel;

namespace Satellites
{
    public class AddZonds : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Исходный файл
        /// </summary>
        private readonly string InFile;
        /// <summary>
        /// Распакованный исходный файл
        /// </summary>
        private readonly string UnpackedFile;
        /// <summary>
        /// Завершённый распакованный файл
        /// </summary>
        private readonly string FinishedFile;
        /// <summary>
        /// Имя зонда
        /// </summary>
        private readonly string ZondName;
        /// <summary>
        /// Крыло зондов по оси X
        /// </summary>
        private readonly int Sx;
        /// <summary>
        /// Крыло зондов по оси Y
        /// </summary>
        private readonly int Sy;
        /// <summary>
        /// Шаг между зондами
        /// </summary>
        private readonly int Step;
        /// <summary>
        /// Генератор случайных чисел.
        /// </summary>
        private readonly Random Rand = new Random();
        /// <summary>
        /// Накопитель строк.
        /// </summary>
        private readonly List<string> Buffer = new List<string>();
        /// <summary>
        /// Массив координат зонда.
        /// </summary>
        private readonly int[] pos = new int[3];
        /// <summary>
        /// Позиция вставки кода.
        /// </summary>
        private int fitpos;
        /// <summary>
        /// Окно лога.
        /// </summary>
        private readonly Log Log;
        /// <summary>
        /// Текст лога.
        /// </summary>
        private string txt;
        /// <summary>
        /// Текст лога.
        /// </summary>
        public string Txt
        {
            get => txt;
            set
            {
                txt = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Txt)));
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="InFile">Исходный файл</param>
        /// <param name="UnpackedFile">Распакованный исходный файл</param>
        /// <param name="FinishedFile">Завершённый распакованный файл</param>
        /// <param name="PackedFile">Завершённый запакованный файл</param>
        /// <param name="ZondName">Имя зонда</param>
        /// <param name="Sx">Крыло зондов по оси X</param>
        /// <param name="Sy">Крыло зондов по оси Y</param>
        /// <param name="Step">Шаг между зондами</param>
        /// <param name="log">Окно лога</param>
        public AddZonds(string InFile, string UnpackedFile, string FinishedFile, string ZondName, int Sx, int Sy, int Step)
        {
            //Log = log;
            //Log.DataContext = new LogVM(this);
            this.InFile = InFile;
            this.UnpackedFile = UnpackedFile;
            this.FinishedFile = FinishedFile;
            this.ZondName = ZondName;
            this.Sx = Sx;
            this.Sy = Sy;
            this.Step = Step;
            if (File.Exists(FinishedFile))
            {
                File.Delete(FinishedFile);
            }
            if (File.Exists(UnpackedFile))
            {
                File.Delete(UnpackedFile);
            }
        }
        /// <summary>
        /// Запуск
        /// </summary>
        public void Start()
        { 
            Buffer.Clear();
            if (InFile.Contains(".gz"))
            {
                Txt = "Распаковка";
                UnZip(InFile, UnpackedFile);
            }
            else
            {
                Txt = "Копирование";
                File.Copy(InFile, UnpackedFile, true);
            }
            Txt = "Поиск имени зонда";
            if (LookingForZond())
            {
                Txt = "Создание XML файла";
                int count = 0;
                foreach (string line in File.ReadLines(this.UnpackedFile))
                {
                    if (count == fitpos)
                    {
                        CreateZond();
                    }
                    if (Buffer.Count >= 10_000_000)
                    {
                        File.AppendAllLines(FinishedFile, Buffer.ToArray());
                        Buffer.Clear();
                    }
                    Buffer.Add(line);
                    count++;
                }
                File.AppendAllLines(FinishedFile, Buffer.ToArray());
                Txt = "Сохранение .gz файла";
                SaveFileDialog fileDialog = new SaveFileDialog()
                {
                    Title = "Укажите путь и имя для модифицированного GZ файла",
                    Filter = ".gz;|*.gz;"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    Zip(FinishedFile, fileDialog.FileName);
                }
                File.Delete(FinishedFile);
                File.Delete(UnpackedFile);
                MessageBox.Show("Готово");
            }
        }
        /// <summary>
        /// Раcпаковка Zip файла
        /// </summary>
        /// <param name="compressed">Сжатый файл</param>
        /// <param name="decompressed">Распакованный файл</param>
        private void UnZip(string compressed, string decompressed)
        {
            using (FileStream ss = new FileStream(compressed, FileMode.OpenOrCreate))
            {

                using (FileStream ts = File.Create(decompressed))
                {
                    using (GZipStream ds = new GZipStream(ss, CompressionMode.Decompress))
                    {
                        ds.CopyTo(ts);
                    }
                }
            }
        }
        /// <summary>
        /// Запаковка в Zip.
        /// </summary>
        /// <param name="decompressed">Распакованный файл</param>
        /// <param name="compressed">Сжатый файл</param>
        private void Zip(string decompressed, string compressed)
        {
            using (FileStream ss = new FileStream(decompressed, FileMode.OpenOrCreate))
            {
                using (FileStream ts = File.Create(compressed))
                {
                    using (GZipStream cs = new GZipStream(ts, CompressionMode.Compress))
                    {
                        ss.CopyTo(cs);
                    }
                }
            }
        }
        /// <summary>
        /// Создание коврика зондов.
        /// </summary>
        /// <returns></returns>
        private void CreateZond()
        {
            int code = 0;
            
            for (int i = -1 * Sx; i <= Sx; i++)
            {
                for (int j = -1 * Sy; j <= Sy; j++)
                {
                    code++;
                    byte[] leters = { Convert.ToByte(Rand.Next(65, 90)), Convert.ToByte(Rand.Next(65, 90)), Convert.ToByte(Rand.Next(65, 90)) };
                    string Leters = Encoding.UTF8.GetString(leters);
                    Buffer.Add("<connection connection=\"resourceprobe\">");
                    Buffer.Add($"<component class=\"resourceprobe\" macro=\"eq_arg_resourceprobe_01_macro\" connection=\"space\" code=\"" +
                        $"{Leters}-{Rand.Next(100, 999)}\" owner=\"player\" knownto=\"player\" id=\"[0x{Rand.Next(0, 268_435_455):X}]\">");
                    Buffer.Add("<movement class=\"lineardeceleration\">");
                    Buffer.Add("<deceleration time=\"10\"/>");
                    Buffer.Add("<velocity>");
                    Buffer.Add("<linear y=\"-1\"/>");
                    Buffer.Add("</velocity>");
                    Buffer.Add("<time start=\"620566.88\"/>");
                    Buffer.Add("<offset>");
                    Buffer.Add($"<position x=\"{pos[0] + i * Step * 1000}\" y=\"{pos[1]}\" z=\"{pos[2] + j * Step * 1000}\"/>");
                    Buffer.Add("<rotation yaw = \"-70.09721\"/>");
                    Buffer.Add("</offset>");
                    Buffer.Add("</movement>");
                    Buffer.Add("<offset>");
                    Buffer.Add($"<position x=\"{pos[0] + i * Step * 1000}\" y=\"{pos[1]}\" z=\"{pos[2] + j * Step * 1000}\"/>");
                    Buffer.Add("<rotation yaw = \"-70.09721\"/>");
                    Buffer.Add("</offset>");
                    Buffer.Add("</component>");
                    Buffer.Add("</connection>");
                    Txt = $"Добавлено {code} зонд(ов)";
                }
            }
        }
        /// <summary>
        /// Поиск имени зонда в тексте.
        /// </summary>
        private bool LookingForZond()
        {
            bool found = false;
            int count = 0;
            
            foreach (string line in File.ReadLines(UnpackedFile))
            {
                if (!found && line.Contains(ZondName))
                {
                    found = true;
                    fitpos = count - 1;
                }
                if (found && line.Contains("<position x="))
                {
                    string getstr = line.Substring(line.IndexOf("<position x=\"") + "<position x=\"".Length);
                    string num = getstr.Substring(0, getstr.IndexOf("\""));
                    if (num.Contains("."))
                    {
                        pos[0] = Int32.Parse(num.Substring(0, num.IndexOf(".")));
                    }
                    else
                    {
                        pos[0] = Int32.Parse(num);
                    }
                    getstr = getstr.Substring(getstr.IndexOf("y=\"") + "y=\"".Length);
                    num = getstr.Substring(0, getstr.IndexOf("\""));
                    if (num.Contains("."))
                    {
                        pos[1] = Int32.Parse(num.Substring(0, num.IndexOf(".")));
                    }
                    else
                    {
                        pos[1] = Int32.Parse(num);
                    }
                    getstr = getstr.Substring(getstr.IndexOf("z=\"") + "z=\"".Length);
                    num = getstr.Substring(0, getstr.IndexOf("\""));
                    if (num.Contains("."))
                    {
                        pos[2] = Int32.Parse(num.Substring(0, num.IndexOf(".")));
                    }
                    else
                    {
                        pos[2] = Int32.Parse(num);
                    }
                    return true;
                }
                count++;
            }
            MessageBox.Show($"Зонд с именем {ZondName} не найден.");
            File.Delete(UnpackedFile);
            return false;
        }
    }
}
