using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using ScriptGraphicHelper.Models;
using System.Windows;
using System.Windows.Media;

namespace ScriptGraphicHelper.Views
{
    /// <summary>
    /// Config.xaml 的交互逻辑
    /// </summary>
    public partial class Config : Window
    {
        public Setting _Setting { get; set; }
        public Config()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }
        public Config(Setting setting)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            _Setting = new Setting
            {
                LastSize = setting.LastSize,
                LastOffsetColorShow = setting.LastOffsetColorShow,
                LastHintColorShow = setting.LastHintColorShow,
                LastAllOffset = setting.LastAllOffset,
                LastIsAddRange = setting.LastIsAddRange
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(Theme.Dark);
            theme.SecondaryMid = new ColorPair(Color.FromRgb(0x66, 0x66, 0x66), Colors.White);
            theme.PrimaryMid = new ColorPair(Color.FromRgb(0x66, 0x66, 0x66), Colors.White);
            paletteHelper.SetTheme(theme);

            LastSize.IsChecked = _Setting.LastSize;
            AllOffsetShow.IsChecked = _Setting.LastOffsetColorShow;
            HintColorShow.IsChecked = _Setting.LastHintColorShow == 1;
            IsAddRange.IsChecked = _Setting.LastIsAddRange;
            AllOffset.Text = _Setting.LastAllOffset != "000000" ? _Setting.LastAllOffset : string.Empty;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            _Setting.LastSize = (bool)LastSize.IsChecked;
            _Setting.LastOffsetColorShow = (bool)AllOffsetShow.IsChecked;
            _Setting.LastHintColorShow = (bool)HintColorShow.IsChecked ? 1 : 0;
            _Setting.LastIsAddRange = (bool)IsAddRange.IsChecked;
            if (AllOffset.Text.Trim().Length == 6)
            {
                _Setting.LastAllOffset = AllOffset.Text;
            }
            else
            {
                _Setting.LastAllOffset = "000000";
            }
            this.DialogResult = true;
        }
    }
}
