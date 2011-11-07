using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ApplicationResizer.Logic;
using System.Configuration;
using System.Windows.Interop;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using log4net;
using log4net.Config;


namespace ApplicationResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Configuration Config { get; set; }

        public string StoragePath { get; set; }

        protected static readonly ILog log = LogManager.GetLogger(typeof(App));

        public MainWindow()
        {
            log4net.Config.XmlConfigurator.Configure();            
            log.Info("Initializing app");
            // throw new Exception("bla");
            InitializeComponent();
            InitializeLabel();
            targetQualitySlider.Value = Convert.ToDouble(Config.AppSettings.Settings["TargetQuality"].Value);
            string selectedWidthAsString = Config.AppSettings.Settings["DefaultSelectedWidth"].Value;
            ConfigureDefaultCheckbox(selectedWidthAsString);
            log.Info("Initialization successfull");
        }

        private void ConfigureDefaultCheckbox(string selectedWidthAsString)
        {
            switch (selectedWidthAsString)
            {
                case "110":
                    width110RadioButton.IsChecked = true;
                    break;
                case "300":
                    width300RadioButton.IsChecked = true;
                    break;
                case "400":
                    width400RadioButton.IsChecked = true;
                    break;
                case "520":
                    width520RadioButton.IsChecked = true;
                    break;
                default:
                    width100RadioButton.IsChecked = true;
                    break;
            }
        }
        private void InitializeLabel()
        {
            Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            log.Info("Loaded Exe Configuration");
            StoragePath = Config.AppSettings.Settings["DefaultStoragePath"].Value;
            StoragePathTextbox.Text = "Configured to save in: " + StoragePath;
            log.Info("Loaded DefaultStoragePath: " + StoragePath);
        }

        private int GetWidth()
        {
            int width = -1;
            if (width100RadioButton.IsChecked.HasValue && width100RadioButton.IsChecked.Value == true)
            {
                width = 100;
            }
            else if (width110RadioButton.IsChecked.HasValue && width110RadioButton.IsChecked.Value == true)
            {
                width = 110;
            }
            else if (width300RadioButton.IsChecked.HasValue && width300RadioButton.IsChecked.Value == true)
            {
                width = 300;
            }
            else if (width400RadioButton.IsChecked.HasValue && width400RadioButton.IsChecked.Value == true)
            {
                width = 400;
            }
            else if (width520RadioButton.IsChecked.HasValue && width520RadioButton.IsChecked.Value == true)
            {
                width = 520;
            }

            log.Info("Determined width as: " + width);
            return width;
        }

        private void PickNameButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".jpg";

            dlg.InitialDirectory = StoragePath;
            dlg.Filter = "JPeg Image|*.jpg";
            log.Info("Showing dialog...");
            if (dlg.ShowDialog() == true)
            {
                var fi = new FileInfo(dlg.FileName);
                StoragePath = fi.DirectoryName;
                log.Info("Selected folder: " + StoragePath);
                string name = System.IO.Path.GetFileNameWithoutExtension(fi.FullName);
                log.Info("Original name: " + name);
                string transformed = name.Replace(" ", "-") + "-web.jpg";
                Config.AppSettings.Settings["DefaultStoragePath"].Value = StoragePath;
                ConfigurationManager.RefreshSection("appSettings");
                long quality = Convert.ToInt64(targetQualitySlider.Value);
                Config.AppSettings.Settings["TargetQuality"].Value = quality.ToString();
                Config.Save(ConfigurationSaveMode.Modified);
                log.Info("Saved configuration, attempting to resize now...");
                ResizeImage ri = new ResizeImage(_bitmapImage, quality);
                string path = String.Format("{0}\\{1}", StoragePath, transformed);
                ri.ProcessByWidth(GetWidth(), path);
                log.Info("Resized correctly to " + path + ", with quality " + quality.ToString());
            }
        }

        private ImageSource ImageFromClipboardDib()
        {
            // Ensure clipboard has data..
            IDataObject objClipboard = Clipboard.GetDataObject();
            if (objClipboard == null)
            {
                MessageBox.Show("There is no image detected on the clipboard!");
                return null;
            }

            // Obtain data formats for the clipboard data..
            string[] strFormats = objClipboard.GetFormats();

            if (strFormats.Contains("DeviceIndependentBitmap"))
            {
                BitmapSource bitmapSource = (BitmapSource)Clipboard.GetDataObject().GetData(DataFormats.Bitmap, true);
                using (MemoryStream tempMemoryStream = new MemoryStream())
                {
                    BmpBitmapEncoder e = new BmpBitmapEncoder();
                    e.Frames.Add(BitmapFrame.Create(bitmapSource));
                    e.Save(tempMemoryStream);
                    _bitmapImage = new System.Drawing.Bitmap(tempMemoryStream);
                }
                return bitmapSource;
            }

            MessageBox.Show("There is no image detected on the clipboard!");
            return null;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        private Bitmap _bitmapImage;

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            imgTest.Source = ImageFromClipboardDib();
        }

        private void width100RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetupQualitySlider(100);
        }

        private void width110RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetupQualitySlider(100);
        }

        private void width400RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetupQualitySlider(100);
        }

        private void width520RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetupQualitySlider(100);
        }

        private void width300RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetupQualitySlider(100);
        }
        private void SetupQualitySlider(int quality)
        {
            if (targetQualitySlider != null)
            {
                targetQualitySlider.Value = quality;
            }
        }
    }
}
