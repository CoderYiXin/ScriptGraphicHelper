using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Windows.Media.Color;
namespace ScriptGraphicHelper.Models
{
    class BmpEditorHelper
    {
        public static int Width { get; set; } = 0;
        public static int Height { get; set; } = 0;
        public static int FormatSize { get; set; }
        public static int Stride { get; set; }
        public static byte[] ScreenData { get; set; }

        public static void KeepScreen(Bitmap bitmap)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            IntPtr IntPtr = data.Scan0;
            FormatSize = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            Stride = data.Stride;
            ScreenData = new byte[Stride * Height];
            Marshal.Copy(IntPtr, ScreenData, 0, Stride * Height);
            bitmap.UnlockBits(data);
        }

        public static async Task<Bitmap> GetBmp(Range range)
        {
            var task = Task.Run(() =>
            {
                int left = (int)range.Left;
                int top = (int)range.Top;
                int right = (int)range.Right;
                int bottom = (int)range.Bottom;
                int width = right - left + 1;
                int height = bottom - top + 1;
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;
                    int stride = data.Stride;
                    for (int i = top; i < height; i++)
                    {
                        int ptr_location = (i - top) * stride;
                        int parent_location = i * Stride + left * FormatSize;
                        for (int j = left; j < width; j++)
                        {
                            ptr[ptr_location] = ScreenData[parent_location];
                            ptr[ptr_location + 1] = ScreenData[parent_location + 1];
                            ptr[ptr_location + 2] = ScreenData[parent_location + 2];
                            ptr[ptr_location + 3] = 255;
                            ptr_location += 4;
                            parent_location += FormatSize;
                        }
                    }
                }
                bitmap.UnlockBits(data);
                return bitmap;
            });
            return await task;
        }

        public static byte[] GetPixel(int x, int y)
        {
            byte[] retRGB = new byte[] { 0, 0, 0 };
            try
            {
                if (x < Width && y < Height)
                {
                    int location = x * FormatSize + y * Stride;
                    retRGB[0] = ScreenData[location + 2];
                    retRGB[1] = ScreenData[location + 1];
                    retRGB[2] = ScreenData[location];
                }
            }
            catch
            {
                retRGB = new byte[] { 0, 0, 0 };
            }
            return retRGB;
        }

        internal static async Task<Bitmap> SetPixels(Color sc, Color fc, int offset)
        {
            byte[] data = (byte[])ScreenData.Clone();
            byte sr = sc.R; byte sg = sc.G; byte sb = sc.B;
            byte fr = fc.R; byte fg = fc.G; byte fb = fc.B;

            int step = 0;
            int similarity = (int)(255 - 255 * ((100 - offset) / 100.0));
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (Math.Abs(data[step] - sb) <= similarity && Math.Abs(data[step + 1] - sg) <= similarity && Math.Abs(data[step + 2] - sr) <= similarity)
                    {
                        data[step] = fb;
                        data[step + 1] = fg;
                        data[step + 2] = fr;
                    }
                    step += FormatSize;
                }
            }
            return await GetBmp(data);
        }

        public static async Task<Bitmap> GetBmp(byte[] screenData)
        {
            var task = Task.Run(() =>
            {
                int left = 0;
                int top = 0;
                int right = Width - 1;
                int bottom = Height - 1;
                int width = Width;
                int height = Height;
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;
                    int stride = data.Stride;
                    for (int i = top; i < height; i++)
                    {
                        int ptr_location = (i - top) * stride;
                        int parent_location = i * Stride + left * FormatSize;
                        for (int j = left; j < width; j++)
                        {
                            ptr[ptr_location] = screenData[parent_location];
                            ptr[ptr_location + 1] = screenData[parent_location + 1];
                            ptr[ptr_location + 2] = screenData[parent_location + 2];
                            ptr[ptr_location + 3] = 255;
                            ptr_location += 4;
                            parent_location += FormatSize;
                        }
                    }
                }
                bitmap.UnlockBits(data);
                return bitmap;
            });
            return await task;
        }


        public static async Task<Bitmap> CutBmp(Bitmap bitmap)
        {
            var task = Task.Run(() =>
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                int right = width - 1;
                int bottom = height - 1;
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                IntPtr IntPtr = data.Scan0;
                int formatSize = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int stride = data.Stride;
                byte[] screenData = new byte[stride * height];
                Marshal.Copy(IntPtr, screenData, 0, stride * height);
                bitmap.UnlockBits(data);
                byte[] rgb = new byte[] { screenData[0], screenData[1], screenData[2] };
                if (rgb[0] == screenData[right * formatSize] &&
                    rgb[0] == screenData[bottom * stride] &&
                    rgb[0] == screenData[right * formatSize + bottom * stride])
                {
                    if (rgb[1] == screenData[right * formatSize + 1] &&
                        rgb[1] == screenData[bottom * stride + 1] &&
                        rgb[1] == screenData[right * formatSize + bottom * stride + 1])
                    {
                        if (rgb[2] == screenData[right * formatSize + 2] &&
                            rgb[2] == screenData[bottom * stride + 2] &&
                            rgb[2] == screenData[right * formatSize + bottom * stride + 2])
                        {
                            Range range = new Range(0, 0, right, bottom);
                            for (int i = 0; i < height; i++)
                            {
                                int num = 0;
                                int location = i * stride;
                                for (int j = 0; j < width; j++)
                                {
                                    byte[] arr = new byte[3];
                                    arr[0] = screenData[location];
                                    arr[1] = screenData[location + 1];
                                    arr[2] = screenData[location + 2];
                                    if (arr[0] == rgb[0] && arr[1] == rgb[1] && arr[2] == rgb[2])
                                    {
                                        num++;
                                    }
                                    location += formatSize;
                                }
                                if (num != width)
                                {
                                    range.Top = i > 0 ? i - 1 : 0;
                                    break;
                                }
                            }
                            for (int i = bottom; i >= 0; i--)
                            {
                                int num = 0;
                                int location = i * stride;
                                for (int j = 0; j < width; j++)
                                {
                                    byte[] arr = new byte[3];
                                    arr[0] = screenData[location];
                                    arr[1] = screenData[location + 1];
                                    arr[2] = screenData[location + 2];

                                    if (arr[0] == rgb[0] && arr[1] == rgb[1] && arr[2] == rgb[2])
                                    {
                                        num++;
                                    }
                                    location += formatSize;
                                }
                                if (num != width)
                                {
                                    range.Bottom = i < bottom ? i + 1 : bottom;
                                    break;
                                }

                            }
                            for (int i = 0; i < width; i++)
                            {
                                int num = 0;
                                for (int j = 0; j < height; j++)
                                {
                                    int location = j * stride + i * formatSize;
                                    byte[] arr = new byte[3];
                                    arr[0] = screenData[location];
                                    arr[1] = screenData[location + 1];
                                    arr[2] = screenData[location + 2];
                                    if (arr[0] == rgb[0] && arr[1] == rgb[1] && arr[2] == rgb[2])
                                    {
                                        num++;
                                    }
                                }
                                if (num != height)
                                {
                                    range.Left = i > 0 ? i - 1 : 0;
                                    break;
                                }
                            }
                            for (int i = right; i >= 0; i--)
                            {
                                int num = 0;
                                for (int j = 0; j < height; j++)
                                {
                                    int location = j * stride + i * formatSize;
                                    byte[] arr = new byte[3];
                                    arr[0] = screenData[location];
                                    arr[1] = screenData[location + 1];
                                    arr[2] = screenData[location + 2];

                                    if (arr[0] == rgb[0] && arr[1] == rgb[1] && arr[2] == rgb[2])
                                    {
                                        num++;
                                    }
                                }
                                if (num != height)
                                {
                                    range.Right = i < right ? i + 1 : right;
                                    break;
                                }
                            }

                            return GetBmp(screenData, stride, formatSize, range);

                        }
                    }
                }
                return GetBmp(new Range(0, 0, width - 1, height - 1));

            });
            return await task;
        }

        public static async Task<Bitmap> GetBmp(byte[] screenData, int stride, int formatSize, Range range)
        {
            var task = Task.Run(() =>
            {
                int left = (int)range.Left;
                int top = (int)range.Top;
                int right = (int)range.Right;
                int bottom = (int)range.Bottom;
                int width = right - left + 1;
                int height = bottom - top + 1;
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;
                    int _stride = data.Stride;
                    for (int i = top; i <= bottom; i++)
                    {
                        int ptr_location = (i - top) * _stride;
                        int parent_location = i * stride + left * formatSize;
                        for (int j = left; j <= right; j++)
                        {
                            ptr[ptr_location] = screenData[parent_location];
                            ptr[ptr_location + 1] = screenData[parent_location + 1];
                            ptr[ptr_location + 2] = screenData[parent_location + 2];
                            ptr[ptr_location + 3] = 255;
                            ptr_location += 4;
                            parent_location += formatSize;
                        }
                    }
                }
                bitmap.UnlockBits(data);
                return bitmap;
            });
            return await task;
        }
    }
}
