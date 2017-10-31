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
            var m0 = new Message(ElementType.NVA, 3, 1, 0);
            m0.NVA = 0.5f;
            APDU.SendingNumber = 0;
            APDU.RecevingNumber = 10;
            APDU.ASDU = ASDU;
            ASDU.Messages.Add(m0);
            ASDU.Address = 1;
            ASDU.Cause = 3;
            m0.Address = 1;
            ASDU.Type = 9;

            timer.Interval = 500;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();


        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => { Title = "S " + node.VS + ",V" + node.VR + ",A" + node.ACK; });

        }

        Socket listenSocket;
        ASDU ASDU = new ASDU();
        APDU APDU = new APDU();
        Node node = new Node();
        System.Timers.Timer timer = new System.Timers.Timer();
        private void Button_Click(object sender, RoutedEventArgs e)
        {

           
                try
            {
                // s.linkSocket.Send(new byte[] { 0x68, 0x0E, 0x00, 0x00, 0x02, 0x00, 0x64, 0x01, 0x07, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x14 });
                if (node.Socket != null)
                {
                    var nva =(M_ME_NA_1) node.Datagram.GetInstance(typeof(M_ME_NA_1));
                    var apdu = nva.CreateAPDU(1,true);

                    nva.PutData(apdu, 1, (DateTime.Now.Millisecond - 500) / 500.0f,1);
                    nva.PutData(apdu, 1, (DateTime.Now.Millisecond - 500 + 5) / 500.0f);
                    nva.PutData(apdu, 1, 0.7f);
                    nva.PutData(apdu, 1, 0.8f);

                    node.SendAPDU(apdu);
                    var list = new List<byte>();
                    apdu.SaveTo(list);
                    var papdu = node.Datagram.ParseAPDU(list.ToArray()).APDU;
                    
                    node.SendAPDU(papdu);
                }

            }
            catch (Exception er) { node.CloseConnection(); }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (node.Socket == null)
            {
                if (listenSocket != null)
                    listenSocket.Dispose();
                listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(0, 2404));
                listenSocket.Listen(1);
                node.BindSocket(listenSocket.Accept());
                node.StartReceive();
                listenSocket.Dispose();
                listenSocket = null;
            }
        }
    }
}
