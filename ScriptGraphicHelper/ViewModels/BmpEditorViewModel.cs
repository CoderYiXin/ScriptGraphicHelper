using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;
using Range = ScriptGraphicHelper.Models.Range;

namespace ScriptGraphicHelper.ViewModels
{
    class BmpEditorViewModel : BindableBase
    {
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

        private Cursor editor_Cursor = Cursors.Arrow;
        public Cursor Editor_Cursor
        {
            get { return editor_Cursor; }
            set { SetProperty(ref editor_Cursor, value); }
        }

        private bool penState = false;
        public bool PenState
        {
            get { return penState; }
            set { SetProperty(ref penState, value); }
        }

        private bool reverseState = false;
        public bool ReverseState
        {
            get { return reverseState; }
            set { SetProperty(ref reverseState, value); }
        }

        bool isDown = false;
        public ICommand Img_MouseDown => new DelegateCommand<System.Windows.Controls.Image>(async(e) =>
        {
            if (PenState)
            {
                isDown = true;
                Point point = Mouse.GetPosition(e);
                int x = (int)point.X / 3 - 1;
                int y = (int)point.Y / 3;
                Color fc = ((SolidColorBrush)FillColor).Color;
                for (int i = 0; i < 3; i++)
                {
                    BmpEditorHelper.SetPixel(x + i, y, fc);
                }
                Bitmap bitmap = await BmpEditorHelper.GetBmp(new Range(0,0, BmpEditorHelper.Width-1, BmpEditorHelper.Height-1));
                ImgSource = Bitmap2BitmapImage(bitmap);
                bitmap.Dispose();
            }

        });
        public ICommand Img_MouseMove => new DelegateCommand<System.Windows.Controls.Image>(async(e) =>
        {
            if (PenState && isDown)
            {
                Point point = Mouse.GetPosition(e);
                int x = (int)point.X / 3 - 1;
                int y = (int)point.Y / 3;
                Color fc = ((SolidColorBrush)FillColor).Color;
                for (int i = 0; i < 3; i++)
                {
                    BmpEditorHelper.SetPixel(x + i, y, fc);
                }
                Bitmap bitmap = await BmpEditorHelper.GetBmp(new Range(0, 0, BmpEditorHelper.Width - 1, BmpEditorHelper.Height - 1));
                ImgSource = Bitmap2BitmapImage(bitmap);
                bitmap.Dispose();
            }
        });

        public ICommand Img_MouseLeftButtonUp => new DelegateCommand<System.Windows.Controls.Image>((e) =>
        {
            isDown = false;
            Point point = Mouse.GetPosition(e);
            byte[] c = BmpEditorHelper.GetPixel((int)point.X / 3, (int)point.Y / 3);
            SelectColor = new SolidColorBrush(Color.FromRgb(c[0], c[1], c[2]));

        });

        public ICommand Img_MouseLeave => new DelegateCommand<System.Windows.Controls.Image>((e) =>
        {
            isDown = false;
          
        });

        public ICommand TB_TextChanged => new DelegateCommand(async () =>
        {
            if (PenState)
            {
                return;
            }

            Color sc = ((SolidColorBrush)SelectColor).Color;
            Color fc = ((SolidColorBrush)FillColor).Color;
            Bitmap bitmap;
            if (!ReverseState)
            {
                bitmap = await BmpEditorHelper.SetPixels(sc, fc, Tolerance);
            }
            else
            {
                bitmap = await BmpEditorHelper.SetPixels_Reverse(sc, fc, Tolerance);
            }

            ImgSource = Bitmap2BitmapImage(bitmap);
            bitmap.Dispose();
        });

        public ICommand Reset_Click => new DelegateCommand(() =>
        {
            ImgSource = Bitmap2BitmapImage(Bmp);
            BmpEditorHelper.KeepScreen(Bmp);
        });

        public ICommand Pen_Click => new DelegateCommand(() =>
        {
            Editor_Cursor = Editor_Cursor == Cursors.Arrow ? Cursors.Cross : Cursors.Arrow;
        });

        public ICommand Cut_Click => new DelegateCommand(async () =>
        {
            BitmapSource bs = (BitmapSource)ImgSource;
            Bitmap bmp = new Bitmap(bs.PixelWidth, bs.PixelHeight, PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
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
                Bitmap bmp_32 = new Bitmap(bs.PixelWidth, bs.PixelHeight, PixelFormat.Format32bppPArgb);
                BitmapData data_32 = bmp_32.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp_32.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
                bs.CopyPixels(Int32Rect.Empty, data_32.Scan0, data_32.Height * data_32.Stride, data_32.Stride);

                Bitmap bmp_24 = new Bitmap(bmp_32.Width, bmp_32.Height, PixelFormat.Format24bppRgb);
                BitmapData data_24 = bmp_24.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp_24.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                unsafe
                {
                    byte* ptr_32 = (byte*)data_32.Scan0;
                    byte* ptr_24 = (byte*)data_24.Scan0;
                    for (int i = 0; i < bmp_24.Height; i++)
                    {
                        int location_32 = i * data_32.Stride;
                        int location_24 = i * data_24.Stride;
                        for (int j = 0; j < bmp_24.Width; j++)
                        {
                            ptr_24[location_24] = ptr_32[location_32];
                            ptr_24[location_24 + 1] = ptr_32[location_32 + 1];
                            ptr_24[location_24 + 2] = ptr_32[location_32 + 2];
                            location_32 += 4;
                            location_24 += 3;
                        }
                    }
                }
                bmp_32.UnlockBits(data_32);
                bmp_24.UnlockBits(data_24);
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
                    if (savefire.IndexOf("png") != -1)
                    {
                        bmp_24.Save(savefire, ImageFormat.Png);
                    }
                    else
                    {
                        bmp_24.Save(savefire, ImageFormat.Bmp);
                    }
                }
                bmp_32.Dispose();
                bmp_24.Dispose();
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
