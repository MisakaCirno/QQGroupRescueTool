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

namespace QQGroupRescueTool
{
    /// <summary>
    /// HelpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HelpWindow : Window
    {
        //单例模式窗口
        static bool isShowing = false;
        static HelpWindow instance = null;

        public HelpWindow()
        {
            InitializeComponent();
        }

        public static void ShowWindow()
        {
            if (isShowing == false)
            {
                instance = new HelpWindow();
                instance.Show();
                isShowing = true;
            }
            else
            {
                instance.Activate();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isShowing = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Version_TextBlock.Text = Constant.Version;
            UpdateLog_TextBox.Text = Constant.UpdateLog;
            GithubPage_TextBlock.Text = Constant.GithubPage;
            BilibiliPage_TextBlock.Text = Constant.BilibiliPage;
        }
    }
}
