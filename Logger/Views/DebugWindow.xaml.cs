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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace Logger.Views
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public static DebugWindow CurrentDebugWindow { get; set; }
        public DebugWindow()
        {
            InitializeComponent();
            ButtonApply.Click += ButtonApply_Click;
            CurrentDebugWindow = this;
        }
        public DebugWindow(string str)
        {
            InitializeComponent();
            System.Drawing.Rectangle resolution = Screen.PrimaryScreen.Bounds;
            Left = resolution.Width - 550;
            Top = resolution.Height - 450;
            TextBoxDebugInfo.Text = str;
            ButtonApply.Click += ButtonApply_Click;
            CurrentDebugWindow = this;

            ContentRendered += (sender, args) =>
            {
                TextBoxDebugInfo.CaretIndex = TextBoxDebugInfo.Text.Length;
                TextBoxDebugInfo.ScrollToEnd(); // not necessary for single line texts
                TextBoxDebugInfo.Focus();
            };

        }
        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
