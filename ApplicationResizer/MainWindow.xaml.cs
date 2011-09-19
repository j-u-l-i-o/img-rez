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

namespace ApplicationResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Configuration Config { get; set; }

        public string StoragePath { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeLabel();
            targetQualitySlider.Value = Convert.ToDouble(Config.AppSettings.Settings["TargetQuality"].Value);
        }

        private void InitializeLabel()
        {
            Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            StoragePath = Config.AppSettings.Settings["DefaultStoragePath"].Value;
            StoragePathTextbox.Text = "Configured to save in: " + StoragePath;

            
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

            return width;
        }
        
        private void PickNameButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".jpg";

            dlg.InitialDirectory = StoragePath;
            dlg.Filter = "JPeg Image|*.jpg";
            if (dlg.ShowDialog() == true)
            {
                var fi = new System.IO.FileInfo(dlg.FileName);
                StoragePath = fi.DirectoryName;
                string name = System.IO.Path.GetFileNameWithoutExtension(fi.FullName);
                string transformed = name.Replace(" ", "-") + "-web.jpg";
                Config.AppSettings.Settings["DefaultStoragePath"].Value = StoragePath;
                ConfigurationManager.RefreshSection("appSettings");                
                long quality = Convert.ToInt64(targetQualitySlider.Value);
                Config.AppSettings.Settings["TargetQuality"].Value = quality.ToString();
                Config.Save(ConfigurationSaveMode.Modified);
                ResizeImage ri = new ResizeImage(_bitmapImage, quality);
                ri.ProcessByWidth(GetWidth(), transformed);
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
                MemoryStream tempMemoryStream = new MemoryStream();
                System.Windows.Media.Imaging.BmpBitmapEncoder e = new BmpBitmapEncoder();
                e.Frames.Add(BitmapFrame.Create(bitmapSource));
                e.Save(tempMemoryStream);
                _bitmapImage = new System.Drawing.Bitmap(tempMemoryStream);
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
    }
}
