using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace ScriptGraphicHelper.ViewModels
{
    class BmpEditorViewModel : BindableBase
    {
        private static int STEPNUM = 0;

        private Bitmap bmp;
        public Bitmap Bmp
        {
            get { return bmp; }
            set { SetProperty(ref bmp, value); }
        }


        private Brush selectColor = new SolidColorBrush(Colors.Blue);
        public Brush SelectColor
        {
            get { return selectColor; }
            set { SetProperty(ref selectColor, value); }
        }

        private Brush fillColor = new SolidColorBrush(Colors.Red);
        public Brush FillColor
        {
            get { return fillColor; }
            set { SetProperty(ref fillColor, value); }
        }

        private int tolerance = 5;
        public int Tolerance
        {
            get { return tolerance; }
            set { SetProperty(ref tolerance, value); }
        }

        private int imgWidth;
        public int ImgWidth
        {
            get { return imgWidth; }
            set { SetProperty(ref imgWidth, value); }
        }

        private int imgHeight;
        public int ImgHeight
        {
            get { return imgHeight; }
            set { SetProperty(ref imgHeight, value); }
        }

        private ImageSource imgSource;
        public ImageSource ImgSource
        {
            get { return imgSource; }
            set
            {
                SetProperty(ref imgSource, value);
                if (Bmp is null)
                {
                    BitmapSource bs = (BitmapSource)ImgSource;
                    Bmp = new Bitmap(bs.PixelWidth, bs.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    System.Drawing.Imaging.BitmapData data = Bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, Bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    bs.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                    Bmp.UnlockBits(data);
                    BmpEditorHelper.KeepScreen(Bmp);
                }
            }
        }

        public ICommand Img_MouseLeftButtonUp => new DelegateCommand<System.Windows.Controls.Image>((e) =>
        {
            Point point = Mouse.GetPosition(e);
            byte[] c = BmpEditorHelper.GetPixel((int)point.X / 3, (int)point.Y / 3);
            SelectColor = new SolidColorBrush(Color.FromRgb(c[0], c[1], c[2]));

        });
        public ICommand TB_TextChanged => new DelegateCommand(async () =>
        {
            Color sc = ((SolidColorBrush)SelectColor).Color;
            Color fc = ((SolidColorBrush)FillColor).Color;
            Bitmap bitmap = await BmpEditorHelper.SetPixels(sc, fc, Tolerance);
            ImgSource = Bitmap2BitmapImage(bitmap);
        });

        public ICommand Reset_Click => new DelegateCommand(() =>
        {
            ImgSource = Bitmap2BitmapImage(Bmp);
        });

        public ICommand Cut_Click => new DelegateCommand(async () =>
        {
            BitmapSource bs = (BitmapSource)ImgSource;
            Bitmap bmp = new Bitmap(bs.PixelWidth, bs.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            bs.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            Bmp = await BmpEditorHelper.CutBmp(bmp);
            ImgSource = Bitmap2BitmapImage(Bmp);
            ImgWidth = Bmp.Width * 3;
            ImgHeight = Bmp.Height * 3;
            BmpEditorHelper.KeepScreen(Bmp);
        });

        public ICommand Save_Click => new DelegateCommand(() =>
        {
            if (ImgSource is not null)
            {
                BitmapSource bs = (BitmapSource)ImgSource;
                Bitmap bmp = new Bitmap(bs.PixelWidth, bs.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                bs.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                bmp.UnlockBits(data);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = "",
                    Filter = "bmp files   (*.bmp)|*.bmp|png files   (*.png)|*.png",
                    FilterIndex = 0,
                    RestoreDirectory = false
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    string savefire = saveFileDialog.FileName.ToString();
                    bmp.Save(savefire);
                    STEPNUM++;
                }
            }
        });

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        private ImageSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);
            return imageSource;
        }
    }
}
