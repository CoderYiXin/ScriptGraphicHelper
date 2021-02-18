using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

enum Anchors
{
    L = 0,
    C = 1,
    R = 2
}

namespace ScriptGraphicHelper.Models
{
    public static class GraphicHelper
    {
        public static int Width { get; set; } = 0;
        public static int Height { get; set; } = 0;
        public static int FormatSize { get; set; }
        public static int Stride { get; set; }
        public static byte[] ScreenData { get; set; }
        public static int DiySim { get; set; } = 95;
        public static int DiyOffset { get; set; } = 0;
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
                int width = right - left;
                int height = bottom - top;
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;
                    int stride = data.Stride;
                    for (int i = top; i < bottom; i++)
                    {
                        int ptr_location = (i - top) * stride;
                        int parent_location = i * Stride + left * FormatSize;
                        for (int j = left; j < right; j++)
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

        public static bool AnchorsCompareColor(double width, double height, string colorString, int sim = 95)
        {
            string[] compareColorArr = colorString.Trim('"').Split(',');

            double multiple = Height / height;
            string result = string.Empty;
            for (int i = 0; i < compareColorArr.Length; i++)
            {
                string[] compareColor = compareColorArr[i].Split('|');
                double findX = int.Parse(compareColor[1]);
                double findY = int.Parse(compareColor[2]);
                if (compareColor[0] == "L")
                {
                    findX = Math.Floor(findX * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                else if (compareColor[0] == "C")
                {
                    findX = Math.Floor(Width / 2 - 1 - (width / 2 - findX - 1) * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                else if (compareColor[0] == "R")
                {
                    findX = Math.Floor(Width - 1 - (width - findX - 1) * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                result += findX.ToString() + "|" + findY.ToString() + "|" + compareColor[3] + ",";
            }
            result = result.Trim(',');
            return CompareColorEx(result, sim);
        }
        public static Point AnchorsFindColor(Range rect, double width, double height, string colorString, int sim = 95)
        {
            string compareColorStr = colorString.Trim('"');
            string[] compareColorArr = compareColorStr.Split(',');
            if (compareColorArr.Length < 2)
            {
                System.Windows.MessageBox.Show("多点找色至少需要勾选两个颜色才可进行测试!", "错误");
                return new Point(-1, -1);
            }
            double multiple = Height / height;
            string[] startColorArr = compareColorArr[0].Split('|');
            double x = int.Parse(startColorArr[1]);
            double y = int.Parse(startColorArr[2]);
            double startX = -1;
            double startY = -1;
            if (startColorArr[0] == "L")
            {
                startX = Math.Floor(x * multiple);
                startY = Math.Floor(y * multiple);
            }
            else if (startColorArr[0] == "C")
            {
                startX = Math.Floor(Width / 2 - 1 - (width / 2 - x - 1) * multiple);
                startY = Math.Floor(y * multiple);
            }
            else if (startColorArr[0] == "R")
            {
                startX = Math.Floor(Width - 1 - (width - x - 1) * multiple);
                startY = Math.Floor(y * multiple);
            }

            string result = string.Empty;
            for (int i = 1; i < compareColorArr.Length; i++)
            {
                string[] compareColor = compareColorArr[i].Split('|');
                double findX = int.Parse(compareColor[1]);
                double findY = int.Parse(compareColor[2]);
                if (compareColor[0] == "L")
                {
                    findX = Math.Floor(findX * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                else if (compareColor[0] == "C")
                {
                    findX = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - findX) * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                else if (compareColor[0] == "R")
                {
                    findX = Math.Floor(Width - 1 - (width - findX - 1) * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                result += findX.ToString() + "|" + findY.ToString() + "|" + compareColor[3] + ",";
            }
            result = result.Trim(',');

            if (rect.Mode_1 == 0 || rect.Mode_1 == -1)
            {
                rect.Left = Math.Floor(rect.Left * multiple);
            }
            else if (rect.Mode_1 == 1)
            {
                rect.Left = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - rect.Left) * multiple);
            }
            else if (rect.Mode_1 == 2)
            {
                rect.Left = Math.Floor(Width - 1 - (width - rect.Left - 1) * multiple);
            }
            if (rect.Mode_2 == 0 || rect.Mode_2 == -1)
            {
                rect.Right = Math.Floor(rect.Right * multiple);
            }
            else if (rect.Mode_2 == 1)
            {
                rect.Right = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - rect.Right) * multiple);
            }
            else if (rect.Mode_2 == 2)
            {
                rect.Right = Math.Floor(Width - 1 - (width - rect.Right - 1) * multiple);
            }
            rect.Top = Math.Floor(rect.Top * multiple);
            rect.Bottom = Math.Floor(rect.Bottom * multiple);
            return FindMultiColor((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, startColorArr[3], result, sim);
        }

        public static bool CompareColor(byte[] rgb, double offsetR, double offsetG, double offsetB, int x, int y, int offset)
        {
            int offsetSize = offset == 0 ? 1 : 9;
            Point[] offsetPoint = new Point[]{
                new Point(x, y),
                new Point(x - 1, y - 1),
                new Point(x - 1, y),
                new Point(x - 1, y + 1),
                new Point(x, y - 1),
                new Point(x, y + 1),
                new Point(x + 1, y - 1),
                new Point(x + 1, y),
                new Point(x + 1, y + 1),
            };

            for (int j = 0; j < offsetSize; j++)
            {
                int _x = offsetPoint[j].X;
                int _y = offsetPoint[j].Y;
                if (_x >= 0 && _x < Width && _y >= 0 && _y < Height)
                {
                    byte[] GetRGB = GetPixel(_x, _y);
                    if (Math.Abs(GetRGB[0] - rgb[0]) <= offsetR && Math.Abs(GetRGB[1] - rgb[1]) <= offsetG && Math.Abs(GetRGB[2] - rgb[2]) <= offsetB)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CompareColorEx(string colorString, int sim = 95, int x = 0, int y = 0, int offset = 0)
        {
            int findX;
            int findY;
            colorString = colorString.Trim("\"".ToCharArray());
            double similarity = 255 - 255 * (sim / 100.0);
            string[] findColors = colorString.Split(',');
            if (findColors.Length != 0)
            {
                int offsetSize = offset == 0 ? 1 : 9;

                for (byte i = 0; i < findColors.Length; i++)
                {
                    string[] findColor = findColors[i].Split('|');
                    string[] offsetColor = findColor[2].Split('-');
                    byte[] findRGB = { 0, 0, 0 };
                    findRGB[0] = Convert.ToByte(offsetColor[0].Substring(0, 2), 16);
                    findRGB[1] = Convert.ToByte(offsetColor[0].Substring(2, 2), 16);
                    findRGB[2] = Convert.ToByte(offsetColor[0].Substring(4, 2), 16);
                    findX = x + int.Parse(findColor[0]);
                    findY = y + int.Parse(findColor[1]);
                    if (findX < 0 || findY < 0 || findX > Width || findY > Height)
                    {
                        return false;
                    }
                    byte[] OffsetRGB = { 0, 0, 0 };
                    if (offsetColor.Length > 1)
                    {
                        OffsetRGB[0] = Convert.ToByte(offsetColor[1].Substring(0, 2), 16);
                        OffsetRGB[1] = Convert.ToByte(offsetColor[1].Substring(2, 2), 16);
                        OffsetRGB[2] = Convert.ToByte(offsetColor[1].Substring(4, 2), 16);
                    }
                    double offsetR = (OffsetRGB[0] + similarity) < 255 ? (OffsetRGB[0] + similarity) : 255;
                    double offsetG = (OffsetRGB[1] + similarity) < 255 ? (OffsetRGB[0] + similarity) : 255;
                    double offsetB = (OffsetRGB[2] + similarity) < 255 ? (OffsetRGB[0] + similarity) : 255;

                    if (!CompareColor(findRGB, offsetR, offsetG, offsetB, findX, findY, offset))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static Point FindMultiColor(int startX, int startY, int endX, int endY, string findcolorString, string compareColorString, int sim = 95)
        {
            startX = Math.Max(startX, 0);
            startY = Math.Max(startY, 0);
            endX = Math.Min(endX, Width - 1);
            endY = Math.Min(endY, Height - 1);

            int offset = 0;
            if (sim == 0)
            {
                sim = DiySim;
                offset = DiyOffset;
            }

            string[] findColor = findcolorString.Split('-');
            byte findR = Convert.ToByte(findColor[0].Substring(0, 2), 16);
            byte findG = Convert.ToByte(findColor[0].Substring(2, 2), 16);
            byte findB = Convert.ToByte(findColor[0].Substring(4, 2), 16);
            byte offsetR = 0;
            byte offsetG = 0;
            byte offsetB = 0;
            if (findColor.Length > 1)
            {
                offsetR = Convert.ToByte(findColor[1].Substring(0, 2), 16);
                offsetG = Convert.ToByte(findColor[1].Substring(2, 2), 16);
                offsetB = Convert.ToByte(findColor[1].Substring(4, 2), 16);
            }
            double similarity = 255 - 255 * (sim / 100.0);

            for (int i = startY; i <= endY; i++)
            {
                int location = startX * FormatSize + Stride * i;
                for (int j = startX; j <= endX; j++)
                {
                    if (Math.Abs(ScreenData[location + 2] - findR) <= offsetR + similarity)
                    {
                        if (Math.Abs(ScreenData[location + 1] - findG) <= offsetG + similarity)
                        {
                            if (Math.Abs(ScreenData[location] - findB) <= offsetB + similarity)
                            {
                                if (CompareColorEx(compareColorString, sim, j, i, offset))
                                {
                                    return new Point(j, i);
                                }
                            }
                        }
                    }
                    location += FormatSize;
                }
            }
            return new Point(-1, -1);
        }
    }
}
