using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace Countdown
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DataTable dt;
        DataTable dt_daily;
        int next_dt_num = 0;
        int next_dt_daily_num = 0;
        string savePath_main = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Countdown\\countdown.csv";
        string savePath_daily = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Countdown\\daily.csv";

        public MainWindow()
        {
            InitializeComponent();
            initWindow();
            initDataTable();
            initCSVdata();
            //  MessageBox.Show(DateTime.Now.AddDays(-1).ToShortDateString());
        }

        public void initWindow()
        {
            //窗口拖动
            this.Loaded += (r, s) =>
            {
                this.MouseDown += (x, y) =>
                {
                    if (y.LeftButton == MouseButtonState.Pressed)
                    {
                        this.DragMove();
                    }
                };
            };
            //禁止拉伸
            this.ResizeMode = ResizeMode.NoResize;
            //在屏幕中间
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }


        public void initDataTable()
        {
            dt = new DataTable();
            //新增4列，即索引号、事件名、时间、是否无期限
            dt.Columns.Add(new DataColumn("index", typeof(int)));
            dt.Columns.Add(new DataColumn("name", typeof(string)));
            dt.Columns.Add(new DataColumn("date", typeof(string)));
            dt.Columns.Add(new DataColumn("if_nonexist", typeof(bool)));

            dt_daily = new DataTable();
            dt_daily.Columns.Add(new DataColumn("indexDaily", typeof(int)));
            dt_daily.Columns.Add(new DataColumn("nameDaily", typeof(string)));
            dt_daily.Columns.Add(new DataColumn("dateDaily", typeof(string)));
        }

        public void initCSVdata()
        {
            FileInfo fi = new FileInfo(savePath_main);
            if (fi.Directory.Exists)
            {
                dt = CSVFileHelper.OpenCSV(savePath_main);
                foreach (DataRow row in dt.Rows)
                {
                    if (row["if_nonexist"].ToString() == "False")
                    {
                        list1.Items.Add(
                           new Item(
                               int.Parse(row["index"].ToString()),
                               row["name"].ToString(),
                               DateTime.Parse(row["date"].ToString()).Date.ToShortDateString().Trim(),
                               calReduceDay(DateTime.Parse(row["date"].ToString())).ToString().Trim()
                               ));
                    }
                    else
                    {
                        list1.Items.Add(
                           new Item(
                               int.Parse(row["index"].ToString()),
                               row["name"].ToString(),
                               "无期限",
                               "无期限"
                               ));
                    }
                    next_dt_num = int.Parse(row["index"].ToString());
                }
            }

            FileInfo fi_daily = new FileInfo(savePath_daily);
            if (fi_daily.Directory.Exists)
            {
                dt_daily = CSVFileHelper.OpenCSV(savePath_daily);
                foreach (DataRow row in dt_daily.Rows)
                {
                    if (row["dateDaily"].ToString() != DateTime.Now.ToShortDateString())
                    {
                        list2.Items.Add(
                            new DailyItem(
                                int.Parse(row["indexDaily"].ToString()),
                                row["nameDaily"].ToString(),
                                DateTime.Parse(row["dateDaily"].ToString()).Date.ToShortDateString().Trim()
                                ));
                    }
                    next_dt_daily_num = int.Parse(row["indexDaily"].ToString());
                }
            }
        }

        public void addRow(int index, string name, string date, bool if_nonexist)
        {
            dt.Rows.Add(index, name, date, if_nonexist);
        }

        public void addDailyRow(int index, string name, string date)
        {
            dt_daily.Rows.Add(index, name, date);
        }

        private void Button_Click_Shutdown(object sender, RoutedEventArgs e)
        {
            CSVFileHelper.SaveCSV(dt, savePath_main);
            CSVFileHelper.SaveCSV(dt_daily, savePath_daily);
            System.Windows.Application.Current.Shutdown();
        }


        private void Button_Click_CreateNewItem(object sender, RoutedEventArgs e)
        {
            NewItem newItem = new NewItem();
            newItem.Owner = this;
            if (newItem.ShowDialog() == true)
            {
                //MessageBox.Show(newItem.text_date.Text.Trim());
                ++next_dt_num;
                if (newItem.if_nonexist.IsChecked == false && newItem.text_date.Text.Length != 0)
                {
                    addRow(next_dt_num, newItem.text_itemName.Text.Trim(), DateTime.Parse(newItem.text_date.Text.Trim()).Date.ToString().Trim(), false);
                    list1.Items.Add(
                        new Item(
                            next_dt_num,
                            newItem.text_itemName.Text.Trim(),
                            DateTime.Parse(newItem.text_date.Text.Trim()).Date.ToString().Trim(),
                            calReduceDay(DateTime.Parse(newItem.text_date.Text.Trim())).ToString().Trim()
                            ));
                }
                else
                {
                    addRow(next_dt_num, newItem.text_itemName.Text.Trim(), "null", true);
                    list1.Items.Add(
                       new Item(
                           next_dt_num,
                           newItem.text_itemName.Text.Trim(),
                           "无期限",
                           "无期限"
                           ));
                }
            }
        }

        private void Button_Click_DeleteItem(object sender, RoutedEventArgs e)
        {
            if (list1.SelectedIndex != -1)
            {
                //DataTable删除
                Item item = list1.SelectedItem as Item;
                DataRow[] drs = dt.Select("index = " + item.index);
                dt.Rows.Remove(drs[0]);
                //ListView删除
                list1.Items.Remove(list1.SelectedItems[0]);
            }
        }

        public double calReduceDay(DateTime itemDate)
        {
            TimeSpan timeSpan = itemDate.Subtract(DateTime.Now.Date);
            return timeSpan.TotalDays;
        }

        public class CSVFileHelper
        {
            /// <summary>
            /// 将DataTable中数据写入到CSV文件中
            /// </summary>
            /// <param name="dt">提供保存数据的DataTable</param>
            /// <param name="fileName">CSV的文件路径</param>
            public static void SaveCSV(DataTable dt, string fullPath)
            {

                //datatable因不明原因导致序号未按序时导致的不明BUG，故而必须每次存储时将序号有序
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    dt.Rows[i][0] = i;
                }

                FileInfo fi = new FileInfo(fullPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                //StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                string data = "";
                //写出列名称
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    data += dt.Columns[i].ColumnName.ToString();
                    if (i < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
                //写出各行数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        string str = dt.Rows[i][j].ToString();
                        str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                        if (str.Contains(',') || str.Contains('"')
                            || str.Contains('\r') || str.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
                        {
                            str = string.Format("\"{0}\"", str);
                        }

                        data += str;
                        if (j < dt.Columns.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    sw.WriteLine(data);
                }
                sw.Close();
                fs.Close();
            }

            /// <summary>
            /// 将CSV文件的数据读取到DataTable中
            /// </summary>
            /// <param name="fileName">CSV文件路径</param>
            /// <returns>返回读取了CSV数据的DataTable</returns>
            public static DataTable OpenCSV(string filePath)
            {
                DataTable dt = new DataTable();
                FileStream fs = new FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);

                //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                StreamReader sr = new StreamReader(fs);
                //string fileContent = sr.ReadToEnd();
                //encoding = sr.CurrentEncoding;
                //记录每次读取的一行记录
                string strLine = "";
                //记录每行记录中的各字段内容
                string[] aryLine = null;
                string[] tableHead = null;
                //标示列数
                int columnCount = 0;
                //标示是否是读取的第一行
                bool IsFirst = true;
                //逐行读取CSV中的数据
                while ((strLine = sr.ReadLine()) != null)
                {
                    //strLine = Common.ConvertStringUTF8(strLine, encoding);
                    //strLine = Common.ConvertStringUTF8(strLine);

                    if (IsFirst == true)
                    {
                        tableHead = strLine.Split(',');
                        IsFirst = false;
                        columnCount = tableHead.Length;
                        //创建列
                        for (int i = 0; i < columnCount; i++)
                        {
                            DataColumn dc = new DataColumn(tableHead[i]);
                            dt.Columns.Add(dc);
                        }
                    }
                    else
                    {
                        aryLine = strLine.Split(',');
                        DataRow dr = dt.NewRow();
                        for (int j = 0; j < columnCount; j++)
                        {
                            dr[j] = aryLine[j];
                        }
                        dt.Rows.Add(dr);
                    }
                }
                if (aryLine != null && aryLine.Length > 0)
                {
                    dt.DefaultView.Sort = tableHead[0] + " " + "asc";
                }

                sr.Close();
                fs.Close();
                return dt;
            }
        }


        private void Button_Click_AddDailyItem(object sender, RoutedEventArgs e)
        {
            NewItem newItem = new NewItem();
            newItem.Owner = this;
            if (newItem.ShowDialog() == true)
            {
                ++next_dt_daily_num;
                addDailyRow(next_dt_daily_num, newItem.text_itemName.Text.Trim(), DateTime.Now.AddDays(-1).ToShortDateString());
                list2.Items.Add(
                    new DailyItem(
                        next_dt_daily_num,
                        newItem.text_itemName.Text.Trim(),
                        DateTime.Now.AddDays(-1).ToShortDateString()
                        ));

            }
        }

        private void Button_Click_DeleteDailyItem(object sender, RoutedEventArgs e)
        {
            if (list2.SelectedIndex != -1)
            {
                //DataTable删除
                DailyItem dailyItem = list2.SelectedItem as DailyItem;
                DataRow[] drs = dt_daily.Select("indexDaily = " + dailyItem.indexDaily);
                dt_daily.Rows.Remove(drs[0]);
                //ListView删除
                list2.Items.Remove(list2.SelectedItems[0]);
            }
        }

        public class Item
        {
            public Item(int index, string itemName, string itemDate, string reduceDay)
            {
                this.index = index;
                this.name = itemName;
                this.date = itemDate;
                this.reduceTime = reduceDay;
            }
            public int index { get; set; }
            public string name { get; set; }
            public string date { get; set; }
            public string reduceTime { get; set; }
        }

        public class DailyItem
        {
            public DailyItem(int indexDaily, string nameDaily, string dateDaily)
            {
                this.indexDaily = indexDaily;
                this.nameDaily = nameDaily;
                this.dateDaily = dateDaily;
            }
            public int indexDaily { get; set; }
            public string nameDaily { get; set; }
            public string dateDaily { get; set; }
        }

        private void list2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (list2.SelectedIndex != -1)
            {
                //DataTable删除
                DailyItem dailyItem = list2.SelectedItem as DailyItem;
                DataRow[] drs = dt_daily.Select("indexDaily = " + dailyItem.indexDaily);
                if (drs.Length!=0)
                {
                    drs[0][2] = DateTime.Now.ToShortDateString();
                }
                //ListView删除
                list2.Items.Remove(list2.SelectedItems[0]);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            normal.Visibility = Visibility.Visible;
            daily.Visibility = Visibility.Hidden;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            normal.Visibility = Visibility.Hidden;
            daily.Visibility = Visibility.Visible;
        }

        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            list2.Items.Clear();
            dt_daily.Clear();
            FileInfo fi_daily = new FileInfo(savePath_daily);
            if (fi_daily.Directory.Exists)
            {
                dt_daily = CSVFileHelper.OpenCSV(savePath_daily);
                foreach (DataRow row in dt_daily.Rows)
                {
                    if (row["dateDaily"].ToString() != DateTime.Now.ToShortDateString())
                    {
                        list2.Items.Add(
                            new DailyItem(
                                int.Parse(row["indexDaily"].ToString()),
                                row["nameDaily"].ToString(),
                                DateTime.Parse(row["dateDaily"].ToString()).Date.ToShortDateString().Trim()
                                ));
                    }
                }
            }
        }
    }
}
