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
using System.Net.Sockets;
using System.Net;

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
            ASDU.Type = 9;

             listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(0, 2404));
            listenSocket.Listen(1);

        }
        Socket listenSocket;
        ASDU ASDU = new ASDU();
        APDU APDU = new APDU();
        Node node = new Node();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
          
            try
            {
                // s.linkSocket.Send(new byte[] { 0x68, 0x0E, 0x00, 0x00, 0x02, 0x00, 0x64, 0x01, 0x07, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x14 });
                if (node.Socket == null)
                {
                  
                    node.BindSocket(listenSocket.Accept());
                    node.StartReceive();
                    APDU.Format = DatagramFormat.UnnumberedControl;
                    APDU.ControlFunction = ControlFunctions.STARTDT_A;
                    APDU.SendTo(node.Socket);
                }

                    var dt = new M_ME_NA_1(1, 3,1);


                dt.PutSQData(1,  (DateTime.Now.Millisecond - 500) / 500.0f);
                dt.PutSQData(1, (DateTime.Now.Millisecond - 500 + 5) / 500.0f);
                dt.PutSQData(1, 0.7f);
                dt.PutSQData(1, 0.8f);

                node.SendDatagram(dt);
                Title = dt.APDU.ASDU.Messages.First().NVA.ToString()+" "+node.VS+","+node.VR;
                
            }
            catch (Exception er) { node.CloseSocket(); }

        }
    }
}
