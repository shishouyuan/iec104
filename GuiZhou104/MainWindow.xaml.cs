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
            APDU.Format = DatagramFormat.InformationTransmit;
            var m0 = new Message(ElementTypes.NVA, 3, 1, 0);
            m0.NVA = 0.5f;
            APDU.SendingNumber = 0;
            APDU.RecevingNumber = 10;
            APDU.ASDU = ASDU;
            ASDU.Messages.Add(m0);
            ASDU.Address = 1;
            ASDU.Cause = 3;
            m0.Address = 1;
            ASDU.Type =9;

        }
        ASDU ASDU = new ASDU();
        APDU APDU = new APDU();
        Slave s = new Slave(2404, 1);
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            byte i = 0;
            i = i.SetBit(8).SetBit(8).SetBit(8);
            s.startService();
            
            try
            {
                // s.linkSocket.Send(new byte[] { 0x68, 0x0E, 0x00, 0x00, 0x02, 0x00, 0x64, 0x01, 0x07, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x14 });
                ASDU.Messages.First().NVA = (DateTime.Now.Millisecond-500) / 500.0f;
                Title = ASDU.Messages.First().NVA.ToString();
                APDU.SendTo(s.linkSocket);
                
            }
            catch (Exception er) { }

        }
    }
}
