using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ScriptGraphicHelper.Views;

namespace ScriptGraphicHelper.Models
{
    static class PingState
    {
        public static byte Stop { get; set; } = 0;
        public static byte Ping { get; set; } = 1;
        public static byte ScreenShot { get; set; } = 2;
    }
    class MobileTcpHelper : EmulatorHelper
    {
        public override string Path { get; set; } = "tcp连接";
        public override string Name { get; set; } = "tcp连接";

        private TcpClient MyTcpClient;

        private NetworkStream networkStream;

        private int Width = -1;
        private int Height = -1;
        private bool IsInit = false;
        public override void Dispose()
        {
            if (IsInit)
            {
                try
                {
                    IsInit = false;
                    networkStream.WriteByte(PingState.Stop);
                    networkStream.Close();
                    MyTcpClient.Close();
                }
                catch { };
            }

        }
        public override bool IsStart(int ldIndex)
        {
            return true;
        }
        public override async Task<List<KeyValuePair<int, string>>> ListAll()
        {
            string address = string.Empty;
            int port = -1;
            TcpConfig tcpConfig = new TcpConfig();
            tcpConfig.ShowDialog();
            if ((bool)tcpConfig.DialogResult)
            {
                address = tcpConfig.MyAddress;
                port = tcpConfig.MyPort;
            }
            var task = Task.Run(() =>
            {
                List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();
                
                if (address != string.Empty && port != -1)
                {
                    try
                    {
                        MyTcpClient = new TcpClient(address, port);
                        networkStream = MyTcpClient.GetStream();
                        byte[] buf = new byte[256];
                        for (int i = 0; i < 40; i++)
                        {
                            Task.Delay(100).Wait();
                            if (networkStream.DataAvailable)
                            {
                                int length = networkStream.Read(buf, 0, 256);
                                string[] info = Encoding.UTF8.GetString(buf, 0, length).Split('|');
                                Width = int.Parse(info[1]);
                                Height = int.Parse(info[2]);
                                result.Add(new KeyValuePair<int, string>(key: 0, value: info[0]));
                                IsInit = true;
                                return result;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
                result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                return result;
            });
            return await task;
        }

        private bool GetTcpState()
        {
            networkStream.WriteByte(PingState.Ping);
            for (int i = 0; i < 40; i++)
            {
                Task.Delay(50).Wait();
                byte[] _ = new byte[9];
                if (networkStream.DataAvailable)
                {
                    int length = networkStream.Read(_, 0, 1);
                    if (length == 1)
                    {
                        if (_[0] == PingState.Ping)
                        {
                            return true;
                        }
                        else if (_[0] == PingState.Stop)
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        public override async Task<Bitmap> ScreenShot(int Index)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    if (!GetTcpState())
                    {
                        MessageBox.Show("Tcp已断开连接! 请重新连接");
                        return new Bitmap(-1, -1);
                    }
                    networkStream.WriteByte(PingState.ScreenShot);
                    byte[] data = new byte[Width * Height * 4];
                    int offset = 0;
                    while (offset < data.Length)
                    {
                        int len = data.Length - offset;
                        int length = networkStream.Read(data, offset, len);
                        offset += length;

                    }
                    Bitmap bitmap = new Bitmap(Width, Height);
                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    unsafe
                    {
                        byte* ptr = (byte*)bitmapData.Scan0;
                        //Marshal.Copy(data, 0, ptr, data.Length);
                        int site = 0;
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                ptr[site + 0] = data[site + 2];
                                ptr[site + 1] = data[site + 1];
                                ptr[site + 2] = data[site + 0];
                                ptr[site + 3] = data[site + 3];
                                site += 4;
                            }
                        }
                    }
                    bitmap.UnlockBits(bitmapData);
                    return bitmap;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return new Bitmap(1, 1);
                }
            });
            return await task;
        }
    }
}
