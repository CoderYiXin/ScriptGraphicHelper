using ScriptGraphicHelper.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScriptGraphicHelper.Views
{
    /// <summary>
    /// ImgEditor.xaml 的交互逻辑
    /// </summary>
    public partial class BmpEditor : Window
    {
        public Bitmap InputBmp { get; set; }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        private ImageSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);
            return imageSource;
        }
        public BmpEditor(Bitmap inputImg)
        {
            InputBmp = inputImg;
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (InputBmp is not null)
            {
                Img.Source = Bitmap2BitmapImage(InputBmp);
                Img.Width = InputBmp.Width * 3;
                Img.Height = InputBmp.Height * 3;
                Width = Img.Width + 210;
                Height = Img.Height + 100;
                Rect workArea = SystemParameters.WorkArea;
                Left = (workArea.Width - Width) / 2 + workArea.Left;
                Top = (workArea.Height - Height) / 2 + workArea.Top;
            }
        }

        private void Img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = Img.Width + 210;
            Height = Img.Height + 100;
        }
    }
}
