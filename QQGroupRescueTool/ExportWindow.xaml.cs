using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// ExportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExportWindow : Window
    {
        List<MemberInfo> memberInfosNeedConvert = new List<MemberInfo>();

        public ExportWindow()
        {
            InitializeComponent();
        }

        private ExportWindow(List<MemberInfo> inputDatas)
        {
            InitializeComponent();

            memberInfosNeedConvert = inputDatas;
        }

        public static void ExportData(List<MemberInfo> inputDatas)
        {
            ExportWindow exportWindow = new ExportWindow(inputDatas);
            exportWindow.Show();
        }

        public string ConvertInfosToString()
        {
            int count = memberInfosNeedConvert.Count;
            if (count == 0)
            {
                return string.Empty;
            }

            //一个邮箱大概20个字符，预分配空间
            StringBuilder stringBuilder = new StringBuilder(count * 20);

            foreach (MemberInfo item in memberInfosNeedConvert)
            {
                switch (item.IDType)
                {

                    case IDType.QQ:
                        stringBuilder.Append(item.ID + "@qq.com;");
                        break;
                    case IDType.Email:
                        stringBuilder.Append(item.ID + ";");
                        break;
                    case IDType.Unknown:
                    default:
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Output_TextBox.Text = ConvertInfosToString();
        }

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Output_TextBox.Text);
                MessageBox.Show("复制成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败！原因为：{Environment.NewLine + ex.Message}", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
