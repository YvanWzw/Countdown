using BundleTransformer.Core.Constants;
using Eco.Persistence.Configuration;
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

namespace Forget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DataTable dt;


        public MainWindow()
        {
            InitializeComponent();
            init();
            initDataTable();
        }

        public void init()
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

        }


        public void initDataTable()
        {
            dt = new DataTable();
            //新增4列，即索引号、事件名、时间、是否无期限
            for (int i = 0; i < 4; i++)
            {
                if(i==0)
                {
                    dt.Columns.Add(new DataColumn("index", typeof(int)));
                }
                else
                {
                    DataColumn dc = new DataColumn();
                    dt.Columns.Add(dc);
                }
            }
        }

        public void addRow(int index,string name, string date, bool if_nonexist)
        {
            dt.Rows.Add(index, name, date, if_nonexist);
        }

        private void Button_Click_Shutdown(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        private void Button_Click_CreateNewItem(object sender, RoutedEventArgs e)
        {
            NewItem newItem = new NewItem();
            newItem.Owner = this;
            if (newItem.ShowDialog() == true)
            {
                //MessageBox.Show(newItem.text_date.Text.Trim());
                int index = dt.Rows.Count + 1;

                if (newItem.if_nonexist.IsChecked == false && newItem.text_date.Text.Length != 0)
                {
                    addRow(index, newItem.text_itemName.Text.Trim(), DateTime.Parse(newItem.text_date.Text.Trim()).Date.ToString().Trim(), false);
                    list1.Items.Add(
                        new Item(
                            index,
                            newItem.text_itemName.Text.Trim(),
                            DateTime.Parse(newItem.text_date.Text.Trim()).Date.ToString().Trim(),
                            calReduceDay(DateTime.Parse(newItem.text_date.Text.Trim())).ToString().Trim()
                            ));
                }
                else
                {
                    addRow(index, newItem.text_itemName.Text.Trim(), "null", false);
                    list1.Items.Add(
                       new Item(
                           index,
                           newItem.text_itemName.Text.Trim(),
                           "无期限",
                           "无期限"
                           ));
                }
            }
        }

        private void Button_Click_DeleteItem(object sender, RoutedEventArgs e)
        {
            //DataTable删除
            Item item = list1.SelectedItem as Item;
            DataRow[] drs = dt.Select("index = " + item.index);
            MessageBox.Show(drs[0][0].ToString());
            dt.Rows.Remove(drs[0]);
            //ListView删除
            list1.Items.Remove(list1.SelectedItems[0]);
        }

        public double calReduceDay(DateTime itemDate)
        {
            DateTime dateTime = new DateTime();
            TimeSpan timeSpan = itemDate.Subtract(dateTime);
            return timeSpan.TotalDays - 738061;
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
                Encoding encoding = Common.GetType(filePath); //Encoding.ASCII;//
                DataTable dt = new DataTable();
                FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                StreamReader sr = new StreamReader(fs, encoding);
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
    }

    public class Item
    {
        public Item(int index,string itemName, string itemDate, string reduceDay)
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
}
