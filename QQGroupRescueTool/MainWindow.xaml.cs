using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

namespace QQGroupRescueTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //txt导出后的格式
        //QQ号由圆括号包围，QQ邮箱由尖括号包围
        //2023-01-01 0:00:01 名称(QQ号)
        //2023-01-01 23:59:59 名称<邮箱>
        /// <summary>
        /// 第一组：日期；第二组：时间；第三组：名称；第四组：QQ号或QQ邮箱
        /// </summary>
        readonly Regex infoLineRegex = new Regex(@"([\d]{4}-[\d]{2}-[\d]{2})\s([\d]{1,2}:[\d]{2}:[\d]{2})\s([.\s\S]+)[<(](.+?)[)>]");

        bool isWindowLoaded = false;

        /// <summary>
        /// 存储所有原始数据的列表
        /// </summary>
        ObservableCollection<DisplayMemberInfo> sourceMemberInfos = new ObservableCollection<DisplayMemberInfo>();
        ObservableCollection<DisplayMemberInfo> displayMemberInfos = new ObservableCollection<DisplayMemberInfo>();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region 自定义方法

        ObservableCollection<DisplayMemberInfo> GetMemberInfosFromFile(string filePath)
        {
            Dictionary<string, MemberInfo> memberDic = new Dictionary<string, MemberInfo>();

            //每一个聊天信息的上一行应该是空行
            bool prevLineIsNull = false;

            //逐行读取文本文件
            using (StreamReader sr = File.OpenText(filePath))
            {
                string lineContent = string.Empty;
                while ((lineContent = sr.ReadLine()) != null)
                {
                    if (prevLineIsNull)
                    {
                        Match matchResult = infoLineRegex.Match(lineContent);
                        if (matchResult.Success)
                        {
                            //QQ的聊天记录中，小时数可能只有一位，需要补0
                            string dateString = matchResult.Groups[1].Value;
                            string timeString = matchResult.Groups[2].Value;
                            if (timeString.Length == 7)
                            {
                                timeString = "0" + timeString;
                            }
                            string datetimeString = dateString + " " + timeString;

                            DateTime messageTime = DateTime.ParseExact(datetimeString, "yyyy-MM-dd HH:mm:ss", null);

                            string name = matchResult.Groups[3].Value;

                            string id = matchResult.Groups[4].Value;

                            if (memberDic.ContainsKey(id))
                            {
                                memberDic[id].RecentName = name;
                                memberDic[id].Names.Add(name);

                                memberDic[id].LastMessage = messageTime;

                                memberDic[id].MessageCount++;
                            }
                            else
                            {
                                MemberInfo memberInfo = new MemberInfo
                                {
                                    ID = id,
                                    IDType = id.Contains("@") ? IDType.Email : IDType.QQ,
                                    RecentName = name,
                                    FirstMessage = messageTime,
                                    LastMessage = messageTime,
                                    MessageCount = 1
                                };

                                memberInfo.Names.Add(name);

                                memberDic.Add(id, memberInfo);
                            }
                        }
                    }

                    //如果当前行是空行，则下一行的信息应该是聊天信息
                    if (string.IsNullOrEmpty(lineContent))
                    {
                        prevLineIsNull = true;
                    }
                    else
                    {
                        prevLineIsNull = false;
                    }
                }
            }

            //将Dictionary转换为ObservableCollection
            if (memberDic.Count == 0)
            {
                throw new Exception("没有找到任何聊天信息");
            }

            ObservableCollection<DisplayMemberInfo> loadedMemberInfos = new ObservableCollection<DisplayMemberInfo>();
            foreach (KeyValuePair<string, MemberInfo> item in memberDic)
            {
                DisplayMemberInfo loadedMemberInfo = new DisplayMemberInfo
                {
                    Info = item.Value,
                    IsExport = false
                };

                loadedMemberInfos.Add(loadedMemberInfo);
            }

            return loadedMemberInfos;
        }

        ObservableCollection<DisplayMemberInfo> SortMemberInfos(ObservableCollection<DisplayMemberInfo> inputList)
        {
            ObservableCollection<DisplayMemberInfo> resultList = new ObservableCollection<DisplayMemberInfo>();

            string type = (OrderBy_ComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            string mode = (OrderMode_ComboBox.SelectedItem as ComboBoxItem).Content.ToString();


            switch (mode)
            {
                case "正序":
                    switch (type)
                    {
                        case "QQ号/邮箱":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderBy(x => x.Info.ID));
                            break;
                        case "ID类型":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderBy(x => x.Info.IDType));
                            break;
                        case "最近所用群名片":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderBy(x => x.Info.RecentName));
                            break;
                        case "最早发言时间":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderBy(x => x.Info.FirstMessage));
                            break;
                        case "最晚发言时间":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderBy(x => x.Info.LastMessage));
                            break;
                        case "发言总数量":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderBy(x => x.Info.MessageCount));
                            break;
                        default:
                            break;
                    }
                    break;

                case "倒序":
                    switch (type)
                    {
                        case "QQ号/邮箱":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderByDescending(x => x.Info.ID));
                            break;
                        case "ID类型":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderByDescending(x => x.Info.IDType));
                            break;
                        case "最近所用群名片":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderByDescending(x => x.Info.RecentName));
                            break;
                        case "最早发言时间":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderByDescending(x => x.Info.FirstMessage));
                            break;
                        case "最晚发言时间":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderByDescending(x => x.Info.LastMessage));
                            break;
                        case "发言总数量":
                            resultList = new ObservableCollection<DisplayMemberInfo>(inputList.OrderByDescending(x => x.Info.MessageCount));
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }


            return resultList;
        }

        void UpdateListView(ObservableCollection<DisplayMemberInfo> newList)
        {
            displayMemberInfos = SortMemberInfos(newList);
            Member_ListView.ItemsSource = displayMemberInfos;
        }

        void ExportData(bool isExportAll)
        {
            if (displayMemberInfos.Count == 0)
            {
                MessageBox.Show($"导出失败！没有任何数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<MemberInfo> memberInfos = new List<MemberInfo>();
            foreach (DisplayMemberInfo item in displayMemberInfos)
            {
                if (isExportAll)
                {
                    memberInfos.Add(item.Info);
                }
                else
                {
                    if (item.IsExport)
                    {
                        memberInfos.Add(item.Info);
                    }
                }

            }

            if (memberInfos.Count == 0)
            {
                MessageBox.Show($"导出失败！没有选中任何数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ExportWindow.ExportData(memberInfos);
        }

        #endregion

        #region 事件处理
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isWindowLoaded = true;
        }

        private void OpenFile_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "文本文件|*.txt",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    sourceMemberInfos = GetMemberInfosFromFile(openFileDialog.FileName);
                    UpdateListView(sourceMemberInfos);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void Help_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow.ShowWindow();
        }

        private void OrderList_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isWindowLoaded)
            {
                return;
            }

            UpdateListView(sourceMemberInfos);
        }

        #endregion

        #region 右键菜单事件
        private void ExportSelected_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportData(false);
        }

        private void ExportAll_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportData(true);
        }

        private void CheckSelected_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DisplayMemberInfo item in Member_ListView.SelectedItems)
            {
                item.IsExport = true;
            }
        }

        private void UncheckSelected_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DisplayMemberInfo item in Member_ListView.SelectedItems)
            {
                item.IsExport = false;
            }
        }

        private void InverseSelected_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DisplayMemberInfo item in Member_ListView.SelectedItems)
            {
                item.IsExport = !item.IsExport;
            }
        }

        private void SelectAll_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DisplayMemberInfo item in displayMemberInfos)
            {
                item.IsExport = true;
            }
        }

        private void SelectNone_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DisplayMemberInfo item in displayMemberInfos)
            {
                item.IsExport = false;
            }
        }

        private void SelectInverse_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DisplayMemberInfo item in displayMemberInfos)
            {
                item.IsExport = !item.IsExport;
            }
        }
        #endregion
    }
    public class DisplayMemberInfo : INotifyPropertyChanged
    {
        public MemberInfo Info { get; set; }
        private bool isExport;

        public bool IsExport
        {
            get { return isExport; }
            set
            {
                isExport = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsExport"));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class MemberInfo
    {
        public MemberInfo()
        {
            ID = string.Empty;
            IDType = IDType.Unknown;

            RecentName = string.Empty;
            Names = new HashSet<string>();

            FirstMessage = DateTime.MaxValue;
            LastMessage = DateTime.MinValue;

            MessageCount = 0;
        }
        public string ID { get; set; }
        public IDType IDType { get; set; }
        public string RecentName { get; set; }
        public HashSet<string> Names { get; set; }
        public DateTime FirstMessage { get; set; }
        public DateTime LastMessage { get; set; }
        public long MessageCount { get; set; }


        public string FirstMessageText
        {
            get
            {
                return FirstMessage.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public string LastMessageText
        {
            get
            {
                return LastMessage.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public string IDTypeText
        {
            get
            {
                switch (IDType)
                {
                    case IDType.Unknown:
                        return "未知";
                    case IDType.QQ:
                        return "QQ号";
                    case IDType.Email:
                        return "邮箱";
                    default:
                        return "异常";
                }
            }
        }
    }

    public enum IDType
    {
        Unknown,
        QQ,
        Email
    }
}
