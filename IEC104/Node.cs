using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Shouyuan.IEC104
{
    public class Node
    {
        protected Socket socket;
        public Socket Socket { get => socket; }

        /// <summary>
        /// 己方最大未确认I格式数目。
        /// </summary>
        public byte K = 12;
        /// <summary>
        /// 对方最大未确认I格式数目。
        /// </summary>
        public byte W = 8;

        /// <summary>
        /// 连接超时。
        /// </summary>
        public ushort t0 = 30 * 1000;
        /// <summary>
        /// 发送或测试等待回应超时。
        /// </summary>
        public ushort t1 = 15 * 1000;
        /// <summary>
        /// 收到最后数据必须确认超时时间。
        /// </summary>
        public ushort t2 = 10 * 1000;
        /// <summary>
        /// 通道长期空闲时发送确认帧的超时时间。
        /// </summary>
        public ushort t3 = 20 * 1000;

        private DateTime lastRevTime;
        private DateTime lastIRevTime;
        private ushort lastSendVR;
        private DateTime lastSentTime;
        private DateTime lastUISentTime;

        public delegate void NewDatagramEventHandler(APDU d, Node sender);
        public event NewDatagramEventHandler NewDatagram;

        public delegate void DatagramSendingEventHandler(APDU d, Node sender);
        public event DatagramSendingEventHandler DatagramSending;

        public delegate void ConnectionLostEventHandler(Node sender);
        public event ConnectionLostEventHandler ConnectionLost;

        private bool running = false;

        private bool started = true;

        const ushort MAX = 1 << 15;
        private ushort vs;
        public ushort VS { get => vs; private set => vs = (ushort)(value % MAX); }

        private ushort vr;
        public ushort VR { get => vr; private set => vr = (ushort)(value % MAX); }

        private ushort ack;
        public ushort ACK { get => ack; private set => ack = (ushort)(value % MAX); }


        private void HandleData(byte[] data)
        {
            lastRevTime = DateTime.Now;
            testDatagramSentInCycle = false;
            var a = FormatterManager.ParseAPDU(data).APDU;
            if (a == null) return;
            a.TransferTime =lastRevTime;  
            ThreadPool.QueueUserWorkItem((object o) => { if (NewDatagram != null) NewDatagram(a, this); });  
            switch (a.Format)
            {
                case DatagramFormat.InformationTransmit:
                    VR = (ushort)(a.SendingNumber + 1);
                    lastIRevTime = lastRevTime;
                    ACK = a.RecevingNumber;
                    UIResponsed = true;
                    if (((VR - lastSendVR) & 0x7fff) >= W)
                        SendSDatagram();
                    break;
                case DatagramFormat.NumberedSupervisory:
                    ACK = a.RecevingNumber;
                    UIResponsed = true;
                    break;
                case DatagramFormat.UnnumberedControl:
                    if ((byte)a.ControlFunction % 2 == 0)
                        SendUDatagram(a.ControlFunction + 1);
                    else
                        UIResponsed = true;
                    break;
            }         
        }

        private void ReceiveLoop(object obj)
        {
            byte rc = 0;
            byte[] revBuf = new byte[260];
            byte len = 255;
            byte[] cBuf = null;
            try
            {
                while (running)
                {
                    int c = socket.Receive(revBuf);
                    if (!running)
                        break;
                    byte bi;
                    for (int i = 0; i < c; i++)
                    {
                        bi = revBuf[i];
                        if (rc == 0)
                        {
                            if (bi != APDU.Header)
                                continue;
                            else
                            {
                                rc++;
                            }
                        }
                        else if (rc == 1)
                        {
                            if (bi >= 4 && bi <= 253)
                            {
                                len = (byte)(bi + 2);
                                cBuf = new byte[len];
                                cBuf[0] = APDU.Header;
                                cBuf[1] = bi;
                                rc++;
                            }
                            else
                            {
                                rc = 0;
                                continue;
                            }
                        }
                        else
                        {
                            cBuf[rc] = bi;
                            rc++;
                            if (rc >= len)
                            {
                                HandleData(cBuf);
                                rc = 0;
                            }
                        }
                    }
                }


            }
            catch (Exception e)
            {
                CloseConnection();
            }
            if (ConnectionLost != null)
            {
                ConnectionLost(this);
            }
        }

        public void init()
        {
            VS = 0;
            VR = 0;
            ACK = 0;
            lastSendVR = 0;
            testDatagramSentInCycle = false;
            UIResponsed = true;
        }
        public void StartReceive()
        {
            if (socket == null)
                throw new Exception("尚未绑定Socket。");
            if (running) return;
            init();
            running = true;
            ThreadPool.QueueUserWorkItem(ReceiveLoop);
        }

        void SendSDatagram()
        {
            var apdu = new APDU();
            apdu.Format = DatagramFormat.NumberedSupervisory;
            SendAPDU(apdu);

        }
        void SendUDatagram(ControlFunction cf)
        {
            var apdu = new APDU();
            apdu.Format = DatagramFormat.UnnumberedControl;
            apdu.ControlFunction = cf;
            SendAPDU(apdu);
        }

        System.Timers.Timer timer = new System.Timers.Timer(100);
        public Node()
        {
            timer.Elapsed += TimerLoop;
        }
        public void BindSocket(Socket s)
        {
            socket = s;
            lastRevTime = DateTime.Now;
            timer.Start();
        }


        bool testDatagramSentInCycle = false;
        bool UIResponsed = true;
        private void TimerLoop(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Socket == null)
            {
                CloseConnection();
                return;
            }
            var now = DateTime.Now;

            if (!UIResponsed && (now - lastUISentTime).TotalMilliseconds >= t1)
            {
                CloseConnection();
                return;
            }
            else if (lastSendVR != VR && (now - lastIRevTime).TotalMilliseconds >= t2)
            {
                SendSDatagram();
            }
            else if (!testDatagramSentInCycle && (now - lastRevTime).TotalMilliseconds >= t3 && (now - lastSentTime).TotalMilliseconds >= t3)
            {
                SendUDatagram(ControlFunction.TESTFR_C);
                testDatagramSentInCycle = true;
            }

        }



        public void CloseConnection()
        {
            running = false;
            timer.Stop();
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
            init();
        }

        List<byte> sendBuf = new List<byte>(260);
        public void SendAPDU(APDU apdu)
        {
            if (socket == null) return;
            lock (this)
            {
                apdu.TransferTime = DateTime.Now;
                if (DatagramSending != null)
                    DatagramSending(apdu, this);
                lastSentTime = DateTime.Now;
                switch (apdu.Format)
                {
                    case DatagramFormat.InformationTransmit:
                        apdu.RecevingNumber = VR;
                        lastSendVR = VR;
                        apdu.SendingNumber = VS++;
                        UIResponsed = false;
                        lastUISentTime = lastSentTime;
                        break;
                    case DatagramFormat.NumberedSupervisory:
                        apdu.RecevingNumber = VR;
                        lastSendVR = VR;
                        break;
                    case DatagramFormat.UnnumberedControl:
                        if ((byte)apdu.ControlFunction % 2 == 0)
                        {
                            UIResponsed = false;
                            lastUISentTime = lastSentTime;
                        }
                        break;
                }
                sendBuf.Clear();
                apdu.SaveTo(sendBuf);
                socket.Send(sendBuf.ToArray());
                if (((VS - ACK) & 0x7fff) >= K)
                    CloseConnection();
            }

        }

        public DatagramFormatterManager FormatterManager = new DatagramFormatterManager();


    }
}
