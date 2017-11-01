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
using System.Globalization;

namespace GuiZhou104
{

    public class Conv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            WindowState s = (WindowState)value;
            if (s == WindowState.Maximized)
                return "2";
            else
                return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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


            node.NewDatagram += Node_NewDatagram;

            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.AllowsTransparency = true;
            this.BorderThickness = new Thickness(0);
            this.OpacityMask = new LinearGradientBrush(Color.FromArgb(200, 0, 0, 0), Colors.White, 45);
            this.MouseDown += ((object b, MouseButtonEventArgs a) =>
            {
                if (a.LeftButton == MouseButtonState.Pressed)
                    //  ResizeMode = ResizeMode.CanMinimize;
                    this.DragMove();
            });
            this.MouseDoubleClick += ((object o, MouseButtonEventArgs e) =>
              {
                  this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
              });
        }

        void addToStateDisplay(Paragraph p)
        {
            Dispatcher.InvokeAsync(() =>
            {
                StateTextblock.Document.Blocks.Add(p);
            });
        }


        private void Node_NewDatagram(APDU d, Node sender)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var sb = new StringBuilder();
                var p = new Paragraph() { Margin = new Thickness(5) };
                p.Inlines.Add(new Run(DateTime.Now.ToString()));
                p.Inlines.Add(" 接收");
                p.Inlines.Add(new LineBreak());


                switch (d.Format)
                {
                    case DatagramFormat.NumberedSupervisory:
                        p.Inlines.Add(new Run("S格式报文：接收序号：") { Foreground = Brushes.DarkBlue });
                        p.Inlines.Add(new Run(d.RecevingNumber.ToString()) { Foreground = Brushes.Blue });
                        break;
                    case DatagramFormat.UnnumberedControl:
                        p.Inlines.Add(new Run("U格式报文：") { Foreground = Brushes.Brown });
                        p.Inlines.Add(new Run(d.ControlFunction.ToString()) { Foreground = Brushes.Blue });
                        break;
                    case DatagramFormat.InformationTransmit:

                        p.Inlines.Add(new Run("I格式报文：发送序号：") { Foreground = Brushes.Green });
                        p.Inlines.Add(new Run(d.SendingNumber.ToString()) { Foreground = Brushes.Blue });
                        p.Inlines.Add(new Run("、接收序号：") { Foreground = Brushes.Green });
                        p.Inlines.Add(new Run(d.RecevingNumber.ToString()) { Foreground = Brushes.Blue });
                        p.Inlines.Add(new LineBreak());
                        p.Inlines.Add(new Run("报文类型：") { Foreground = Brushes.Gray });
                        p.Inlines.Add(new Run(d.Formatter.GetType().Name) { Foreground = Brushes.Blue });
                        p.Inlines.Add(new Run(" | ") { Foreground = Brushes.Gray });
                        p.Inlines.Add(new Run(d.Formatter.Description) { Foreground = Brushes.Blue });
                        break;
                }

                addToStateDisplay(p);




            }
            );
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() => { Title = "S " + node.VS + ",V" + node.VR + ",A" + node.ACK; });

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
                if (node.Socket != null)
                {
                    var nva = (M_ME_TD_1)node.FormatterManager.GetInstance(typeof(M_ME_TD_1));
                    var apdu = nva.CreateAPDU(1);

                    nva.PutData(apdu, 1, 0.8f, DateTime.Now, 1);
                    nva.PutData(apdu, 1, -0.8f, DateTime.Now, 2);
                    nva.PutData(apdu, 1, 0.2f, DateTime.Now, 3);
                    nva.PutData(apdu, 1, 0.3f, DateTime.Now, 4);

                    node.SendAPDU(apdu);

                    var cic = (C_RD_NA_1)node.FormatterManager.GetInstance(typeof(C_RD_NA_1));


                    node.SendAPDU(cic.Create(1, 1));
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

        APDU tosend;
        private void addValueButton_Click(object sender, RoutedEventArgs e)
        {
            var f = (M_ME_TF_1)node.FormatterManager.GetInstance(typeof(M_ME_TF_1));
            if (tosend == null)
                tosend = f.CreateAPDU(1);
            try
            {

                f.PutData(tosend, Convert.ToSingle(valTextbox.Text), DateTime.Now, Convert.ToUInt32(MsgAddrTextbox.Text));
            }
            catch (Exception er) { }

        }

        private void sendValueButton_Click(object sender, RoutedEventArgs e)
        {

            if (tosend != null)
            {
                node.SendAPDU(tosend);
                tosend = null;
            }
        }

        private void minBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void maxBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
