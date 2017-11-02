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
            node.DatagramSending += Node_DatagramSending;
            node.ConnectionLost += Node_ConnectionLost;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.AllowsTransparency = true;
            this.BorderThickness = new Thickness(0);
            this.OpacityMask = new LinearGradientBrush(Colors.White, Color.FromArgb(150, 0, 0, 0), -45);
            this.MouseDown += ((object b, MouseButtonEventArgs a) =>
            {
                if (a.LeftButton == MouseButtonState.Pressed)
                    //  ResizeMode = ResizeMode.CanMinimize;
                    this.DragMove();
            });
            this.MouseDoubleClick += ((object o, MouseButtonEventArgs e) =>
              {
                  if (e.GetPosition(TitleGrid).Y <= TitleGrid.ActualHeight)
                      this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
              });
        }

        List<Paragraph> paragraphToAdd = new List<Paragraph>();


        void DisplayMsg(string s)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var a = new Paragraph() { TextAlignment = TextAlignment.Center, Margin = new Thickness(5), Foreground = Brushes.DarkBlue };
                a.Inlines.Add(s);
                a.Inlines.Add(new LineBreak());
                addToStateDisplay(a);
            });
        }

        private void Node_ConnectionLost(Node sender)
        {
            DisplayMsg("连接已断开");
        }

        private void Node_DatagramSending(APDU d, Node sender)
        {

            DispalyDatagram(d, false);
        }

        void addToStateDisplay(Paragraph p)
        {
            Dispatcher.InvokeAsync(() =>
            {
                StateTextblock.Document.Blocks.Add(p);
                StateTextblock.ScrollToEnd();
            });
        }

        void DispalyDatagram(APDU d, bool rev = true)
        {
            Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var sb = new StringBuilder();
                    var p = new Paragraph() { Margin = new Thickness(5) };
                    var t = d.TransferTime;
                    p.Inlines.Add(new Run(string.Format("{0}-{1}-{2} {3:d2}:{4:d2}:{5:d2}.{6:d3} {7}",
                        t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, t.Millisecond, rev ? "接收" : "发送"))
                    { Foreground = rev ? Brushes.DarkBlue : Brushes.Brown });

                    p.Inlines.Add(new LineBreak());


                    switch (d.Format)
                    {
                        case DatagramFormat.NumberedSupervisory:

                            p.Inlines.Add(new Run("S格式报文：") { Foreground = Brushes.DarkBlue, Background = Brushes.SkyBlue });
                            p.Inlines.Add(new Run("接收序号：") { Foreground = Brushes.Gray });
                            p.Inlines.Add(new Run(d.RecevingNumber.ToString()) { Foreground = Brushes.Blue });
                            break;
                        case DatagramFormat.UnnumberedControl:
                            //p.Background = Brushes.Orange;
                            p.Inlines.Add(new Run("U格式报文：") { Foreground = Brushes.Brown, Background = Brushes.LightPink });
                            p.Inlines.Add(new Run(d.ControlFunction.ToString()) { Foreground = Brushes.Blue });
                            break;
                        case DatagramFormat.InformationTransmit:
                            // p.Background = Brushes.Wheat;
                            p.Inlines.Add(new Run("I格式报文：") { Foreground = Brushes.Green, Background = Brushes.Wheat });
                            p.Inlines.Add(new Run("发送序号：") { Foreground = Brushes.Green });
                            p.Inlines.Add(new Run(d.SendingNumber.ToString()) { Foreground = Brushes.Blue });
                            p.Inlines.Add(new Run("，接收序号：") { Foreground = Brushes.Green });
                            p.Inlines.Add(new Run(d.RecevingNumber.ToString()) { Foreground = Brushes.Blue });
                            p.Inlines.Add(new LineBreak());
                            p.Inlines.Add(new Run("报文类型：") { Foreground = Brushes.Gray });
                            p.Inlines.Add(new Run(d.Formatter.GetType().Name) { Foreground = Brushes.Blue });
                            p.Inlines.Add(new Run(" | ") { Foreground = Brushes.Gray });
                            p.Inlines.Add(new Run(d.Formatter.Description) { Foreground = Brushes.Blue });
                            p.Inlines.Add(new Run("，传送原因：") { Foreground = Brushes.Gray });
                            p.Inlines.Add(new Run(d.ASDU.Cause.ToString()) { Foreground = Brushes.Blue });
                            p.Inlines.Add(new Run("，P/N：") { Foreground = Brushes.Gray });
                            p.Inlines.Add(new Run(d.ASDU.PN ? "1" : "0") { Foreground = Brushes.Blue });
                            p.Inlines.Add(new Run("，TEST：") { Foreground = Brushes.Gray });
                            p.Inlines.Add(new Run(d.ASDU.Test ? "1" : "0") { Foreground = Brushes.Blue });
                            p.Inlines.Add(new LineBreak());

                            foreach (var i in d.ASDU.Messages)
                            {
                                p.Inlines.Add(new Run("信息体") { Foreground = Brushes.Gray });
                                p.Inlines.Add(new Run(i.Type.ToString()) { Foreground = Brushes.Blue });
                                p.Inlines.Add(new Run("，地址") { Foreground = Brushes.Gray });
                                p.Inlines.Add(new Run(i.Address.ToString()) { Foreground = Brushes.Blue });
                                if (i.Type != ElementType.Empty)
                                {
                                    p.Inlines.Add(new Run("，值：") { Foreground = Brushes.Gray });
                                    switch (i.Type)
                                    {
                                        case ElementType.NVA:
                                            p.Inlines.Add(new Run(i.NVA.ToString()) { Foreground = Brushes.Blue });
                                            break;
                                        case ElementType.R:
                                            p.Inlines.Add(new Run(i.R.ToString()) { Foreground = Brushes.Blue });
                                            break;
                                        case ElementType.SVA:
                                            p.Inlines.Add(new Run(i.SVA.ToString()) { Foreground = Brushes.Blue });
                                            break;
                                    }
                                }
                                if (i.TimeStamp != null && i.TimeStamp.Length == 7)
                                {
                                    p.Inlines.Add(new Run("，时标：") { Foreground = Brushes.Gray });
                                    p.Inlines.Add(new Run((2000 + i.Year).ToString()) { Foreground = Brushes.Blue });
                                    p.Inlines.Add(new Run("年") { Foreground = Brushes.Gray });
                                    p.Inlines.Add(new Run(i.Month.ToString()) { Foreground = Brushes.Blue });
                                    p.Inlines.Add(new Run("月") { Foreground = Brushes.Gray });
                                    p.Inlines.Add(new Run(i.Day.ToString()) { Foreground = Brushes.Blue });
                                    p.Inlines.Add(new Run("日，星期") { Foreground = Brushes.Gray });
                                    p.Inlines.Add(new Run(i.DayInWeek.ToString()) { Foreground = Brushes.Blue });
                                    p.Inlines.Add(new Run(string.Format("，{0:d2}:{1:d2}:{2:d2}.{3:d3}\n", i.Hour, i.Minute, i.Second, i.Milisecond)) { Foreground = Brushes.Gray });
                                }
                            }
                            break;
                    }

                    addToStateDisplay(p);
                }
                catch (Exception) { }
            });
        }

        private void Node_NewDatagram(APDU d, Node sender)
        {
             DispalyDatagram(d, true);
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
                DisplayMsg("正在等待连接");
                listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(0, 2404));
                listenSocket.Listen(1);
                node.BindSocket(listenSocket.Accept());
                DisplayMsg("新连接已建立");
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
