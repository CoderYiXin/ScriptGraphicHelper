using ScriptGraphicHelper.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace ScriptGraphicHelper.Models
{
    namespace ScriptGraphicHelper.Models
    {
        class HwndHelper : EmulatorHelper
        {
            public override string Path { get; set; } = "句柄";
            public override string Name { get; set; } = "句柄";


            private Dmsoft DM;
            private int Hwd = -1;
            private bool IsInit = false;
            public override void Dispose()
            {
                try
                {
                    DM.UnBindWindow();
                }
                catch { }
            }
            public override async Task<Bitmap> ScreenShot(int Index)
            {
                if (Index == -1 || !IsInit)
                {
                    MessageBox.Show("请先选择窗口句柄!");
                    return new Bitmap(1, 1);
                }
                var task = Task.Run(() =>
                {
                    try
                    {
                        if (DM.GetClientSize(Hwd, out int width, out int height) == 1)
                        {
                            IntPtr intPtr = (IntPtr)DM.GetScreenData(0, 0, width, height);
                            byte[] data = new byte[width * height * 4];
                            Marshal.Copy(intPtr, data, 0, width * height * 4);
                            Bitmap bitmap = new Bitmap(width, height);
                            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                            IntPtr ptr = bitmapData.Scan0;
                            Marshal.Copy(data, 0, ptr, data.Length);
                            bitmap.UnlockBits(bitmapData);
                            return bitmap;
                        }
                        throw new Exception("获取窗口图像失败!");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return new Bitmap(1, 1);
                    }
                });
                return await task;
            }
            public override List<KeyValuePair<int, string>> ListAll()
            {
                DM = new Dmsoft();
                List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();
                if (!DM.IsInit)
                {
                    result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                    return result;
                }
                string[] graphicModes = new string[] { "normal", "gdi", "gdi2", "dx2", "dx3", "dx.graphic.2d", "dx.graphic.2d.2", "dx.graphic.3d", "dx.graphic.3d.8", "dx.graphic.opengl", "dx.graphic.opengl.esv2", "dx.graphic.3d.10plus" };
                string[] attributes = new string[] { "", " dx.public.active.api", "dx.public.active.message", "dx.public.hide.dll", "dx.public.graphic.protect", "dx.public.anti.api", "dx.public.prevent.block", "dx.public.inject.super" };
                int[] modes = new int[] { 0, 2, 101, 103, 11, 13 };

                GetHwnd getHwnd = new GetHwnd();

                if ((bool)getHwnd.ShowDialog())
                {
                    Hwd = getHwnd.ResultHwnd;
                    int graphicMode = getHwnd.ResultGraphicMode;
                    int attribute = getHwnd.ResultAttribute;
                    int mode = getHwnd.ResultMode;
                    if (DM.BindWindowEx(Hwd, graphicModes[graphicMode], "normal", "normal", attributes[attribute], modes[mode]) == 1)
                    {
                        IsInit = true;
                        result.Add(new KeyValuePair<int, string>(key: 0, value: Hwd.ToString() + "-" + graphicModes[graphicMode]));
                        return result;
                    }
                }

                result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                return result;
            }

            public override bool IsStart(int Index)
            {
                return IsInit;
            }
        }
    }

}
