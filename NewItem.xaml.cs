using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forget
{
    /// <summary>
    /// NewItem.xaml 的交互逻辑
    /// </summary>
    public partial class NewItem : Window
    {
        public NewItem()
        {
            InitializeComponent();
        }


        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Button_Click_Sure(object sender, RoutedEventArgs e)
        {
            if(text_itemName.Text.Length!=0)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("事件名不能为空");
            }
        }
    }
}
