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
using Shouyuan.IEC104;
namespace GuiZhou104
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
        ASDU ASDU=new ASDU();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ASDU.COT[0] = 0;
            ASDU.SQ = true;
            ASDU.Cause = 0x39;
            ASDU.Test = false;
            ASDU.PN = true;
            tb.Text = ASDU.COT[0].ToString("x");

           
        }
    }
}
