using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Threading;

namespace Image2ASCii
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private Bitmap bitmap;
        private BackgroundWorker worker;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".png|.jpg";
            dialog.Title = "选择图片";
            dialog.Filter = "jpg|*.jpg|png|*.png";
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() == true)
            {
                if (!String.IsNullOrWhiteSpace(output.Text))
                {
                    output.Text = "";
                }
                String name = dialog.FileName;
                BitmapImage image;
                image = new BitmapImage(new Uri(name));
                pictureBox.Source = image;
                using (MemoryStream output = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(image));
                    enc.Save(output);
                    bitmap = new Bitmap(output);
                }
                if (Properties.Settings.Default.AllowMultithreading)
                {
                    DoMultithreadWork();
                }
                else
                {
                    InitWorker();
                    worker.RunWorkerAsync();
                }
            }
            else
            {
                return;
            }
        }

        private void setting_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
            output.FontSize = Properties.Settings.Default.FontSize;
        }

        private void InitWorker()
        {
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker _worker = sender as BackgroundWorker;
            Dispatcher.Invoke(new Action(() =>
            {
                output.IsEnabled = true;
            }));
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Byte[] mess1 = Encoding.UTF8.GetBytes(output.Text);
                    String UTF8 = Encoding.ASCII.GetString(mess1);
                    output.Text = UTF8;
                }));
                MessageBox.Show("转换完成");
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker _worker = sender as BackgroundWorker;
            Dispatcher.Invoke(new Action(() =>
            {
                output.IsEnabled = false;
            }));
            OneThreadWork(_worker);
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        protected static readonly string charset = "MNHQ&OC?7>!:-;.";

        private int[,] RGBGray(Bitmap bmp)
        {
            int[,] Gray = new int[bmp.Width, bmp.Height];
            Bitmap map = new Bitmap(bmp.Width, bmp.Height);
            System.Drawing.Color color;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    color = bmp.GetPixel(i, j);
                    int numGray = (int)(color.R * 0.299+ color.G * 0.587+ color.B * 0.114);
                    Gray[i, j] = numGray;
                    map.SetPixel(i, j, System.Drawing.Color.FromArgb(numGray,numGray,numGray));
                }
            }
            Dispatcher.Invoke(new Action(() =>
            {
                GrayBox.Source = BitmapToBitmapImage(map);
            }));
            return Gray;
        }

        //private void SetText()
        //{
        //    String outText = Simage;
        //    FlowDocument d = new FlowDocument();
        //    Paragraph p = new Paragraph();
        //    Run r = new Run(outText);
        //    p.Inlines.Add(r);
        //    d.Blocks.Add(p);
        //    output.Document = d;
        //}

        //private string GetText()
        //{
        //    TextRange textRange = new TextRange(output.Document.ContentStart, output.Document.ContentEnd);
        //    return textRange.Text;
        //}

        private void OneThreadWork(BackgroundWorker _worker)
        {
            int[,] GrayImg = RGBGray(bitmap);
            int RowSize = Properties.Settings.Default.RowSize;
            int ColSize = Properties.Settings.Default.ColSize;
            Dispatcher.Invoke(new Action(() =>
            {
                progress.Maximum = bitmap.Height / RowSize;
                progress.Value = 0;
            }));
            for (int h = 0; h < bitmap.Height / RowSize; h++)
            {
                int Hoffset = h * RowSize;
                for (int w = 0; w < bitmap.Width / ColSize; w++)
                {
                    int Woffset = w * ColSize;
                    int AvgGray = 0;
                    for (int x = 0; x < RowSize; x++)
                    {
                        for (int y = 0; y < ColSize; y++)
                        {
                            AvgGray += GrayImg[Woffset + y, Hoffset + x];
                        }
                    }
                    AvgGray /= RowSize * ColSize;
                    if (AvgGray / 17 < charset.Length)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            output.Text+= charset[AvgGray / 17];
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            output.Text += " ";
                        }));
                    }
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    progress.Value += 1;
                    output.Text += "\r\n";
                }));
            }
        }
        private int[,] GrayImg;
        private void DoMultithreadWork()
        {
            GrayImg = new int[bitmap.Width, bitmap.Height];
            Task task = new Task(() =>
            {
                 GrayImg = RGBGray(bitmap);
            });
            task.Start();
            task.ContinueWith((tasks) =>
            {
                CreateThread();
            });
        }
        private Dictionary<int, bool> workList = new Dictionary<int, bool>();
        private int RowSize;
        private int ColSize;
        private int Height;
        private int Width;
        private void CreateThread()
        {
            RowSize = Properties.Settings.Default.RowSize;
            ColSize = Properties.Settings.Default.ColSize;
            Height = bitmap.Height;
            Width = bitmap.Width;
            cache = new String[Height / RowSize];
            Dispatcher.Invoke(() =>
            {
                progress.Maximum = Height / RowSize;
                progress.Value = 0;
            });
            workList.Clear();
            for (int i = 0; i < cache.Length; i++)
            {
                workList.Add(i, false);
            }
            Task parent = new Task(() =>
              {
                  for (int i = 0; i < Properties.Settings.Default.ThreadSize; i++)
                  {
                      new Task(() => { AutoDoWork(); }, TaskCreationOptions.AttachedToParent).Start();
                      workList[0] = true;
                  }
              });
            parent.ContinueWith((t) => 
            {
                Dispatcher.Invoke(() =>
                {
                    output.Text = "";
                    foreach (var item in cache)
                    {
                        output.Text += item;
                    }
                    MessageBox.Show("转换完成");
                });
            });
            parent.Start();
        }
        private void AutoDoWork()
        {
            int i = 0;
            while (workList.ContainsValue(false))
            {
                if (workList[i]==false)
                {
                    workList[i] = true;
                    MackOneLineASCii(RowSize,ColSize, i);
                    Dispatcher.Invoke(() =>
                    {
                        progress.Value++;
                    });
                }
                i++;
            }
        }
        private void MackOneLineASCii(int RowSize,int ColSize, int line)
        {
            int Hoffset = line * RowSize;
            for (int w = 0; w < Width / ColSize; w++)
            {
                int Woffset = w * ColSize;
                int AvgGray = 0;
                for (int x = 0; x < RowSize; x++)
                {
                    for (int y = 0; y < ColSize; y++)
                    {
                        AvgGray += GrayImg[Woffset + y, Hoffset + x];
                    }
                }
                AvgGray /= RowSize * ColSize;
                if (AvgGray / 17 < charset.Length)
                {
                    cache[line] += charset[AvgGray / 17];
                }
                else
                {
                    cache[line] += " ";
                }
            }
            cache[line] += "\r\n";
        }
        private String[] cache;
    }
}
