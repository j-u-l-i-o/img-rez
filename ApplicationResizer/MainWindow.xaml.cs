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
        }

        private void InitializeLabel()
        {
            Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            StoragePath = Config.AppSettings.Settings["DefaultStoragePath"].Value;
            StoragePathLabel.Content = "Configured to save in: " + StoragePath;

            
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
                Config.AppSettings.Settings["DefaultStoragePath"].Value = StoragePath;
                Config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");                
                ResizeImage ri = new ResizeImage(_bitmapImage);
                ri.ProcessByWidth(GetWidth(), dlg.FileName);
            }
        }

        private ImageSource ImageFromClipboardDib()
        {
            MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
            if (ms != null)
            {
                byte[] dibBuffer = new byte[ms.Length];
                ms.Read(dibBuffer, 0, dibBuffer.Length);

                BITMAPINFOHEADER infoHeader =
                    BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

                int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
                int infoHeaderSize = infoHeader.biSize;
                int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

                BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
                fileHeader.bfType = BITMAPFILEHEADER.BM;
                fileHeader.bfSize = fileSize;
                fileHeader.bfReserved1 = 0;
                fileHeader.bfReserved2 = 0;
                fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

                byte[] fileHeaderBytes =
                    BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

                MemoryStream msBitmap = new MemoryStream();
                msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
                msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
                msBitmap.Seek(0, SeekOrigin.Begin);

                _bitmapImage = new System.Drawing.Bitmap(msBitmap);

                return BitmapFrame.Create(msBitmap);
            }
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


        //private static BitmapSource GetImageSourceFromDropData(DragEventArgs e)
        //{
        //    return (BitmapSource)e.Data.GetData(DataFormats.Bitmap, true);

        //    //if (e.Data.GetDataPresent(DataFormats.Bitmap, true))
        //    //{
        //    //    var bm = e.Data.GetData(DataFormats.Bitmap, true);

        //    //    var interopBitmap = bm as InteropBitmap;
        //    //    if (interopBitmap != null)
        //    //    {
        //    //        return interopBitmap;
        //    //    }

        //    //    var bitmap = bm as Bitmap;
        //    //    if (bitmap != null)
        //    //    {
        //    //        return CreateBitmapSource(bitmap);
        //    //    }
        //    //}
        //    //else if (e.Data.GetDataPresent(DataFormats.FileDrop))
        //    //{
        //    //    var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
        //    //    var bmp = new BitmapImage(new Uri("file:///" + fileNames[0].Replace("\\", "/")));
        //    //    return bmp;
        //    //}
        //    //return null;
        //}

        //private static Bitmap CreateBitmap(BitmapSource source)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        var bitmapEncoder = new BmpBitmapEncoder();
        //        bitmapEncoder.Frames.Add(BitmapFrame.Create(source));
        //        bitmapEncoder.Save(memoryStream);
        //        memoryStream.Position = 0;
        //        return new Bitmap(memoryStream);
        //    }
        //}

        //private static BitmapSource CreateBitmapSource(Bitmap bitmap)
        //{
        //    IntPtr hBitmap = bitmap.GetHbitmap();
        //    BitmapSource bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,
        //      Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //    DeleteObject(hBitmap);
        //    return bmpSrc;
        //}

        private Bitmap _bitmapImage;

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            imgTest.Source = ImageFromClipboardDib();
        }
    }

    public static class BinaryStructConverter
    {
        public static T FromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T)obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }

}
