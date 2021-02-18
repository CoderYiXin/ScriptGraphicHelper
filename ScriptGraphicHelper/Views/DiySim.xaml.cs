using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScriptGraphicHelper.Views
{
    /// <summary>
    /// DiySim.xaml 的交互逻辑
    /// </summary>
    public partial class DiySim : Window
    {
        public static int Sim { get; set; } = 0;

        public static int Offset { get; set; } = 0;

        public DiySim()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetSim.Text = Sim != 0 ? Sim.ToString() : string.Empty;
            SetOffset.Text = Offset != 0 ? Offset.ToString() : string.Empty;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SetSim.Text.Trim(), out int sim))
            {
                MessageBox.Show("无效输入!", "错误");
                return;
            }

            int.TryParse(SetOffset.Text.Trim(), out int offset);

            Sim = sim;
            Offset = offset;

            this.DialogResult = true;
        }


    }
}
