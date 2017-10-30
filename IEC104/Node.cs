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
        public ushort t3 = 10 * 1000;

        private DateTime lastRevTime;
        private DateTime lastIRevTime;
        private ushort lastSendVR;
        private DateTime LastSentTime;

        public delegate void NewDatagramEventHandler(Datagram d, Node sender);
        public event NewDatagramEventHandler NewDatagram;

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
            var a = new APDU(data, 0);

            switch (a.Format)
            {
                case DatagramFormat.InformationTransmit:
                    VR = (ushort)(a.SendingNumber + 1);
                    lastIRevTime = lastRevTime;
                    ACK = a.RecevingNumber;
                    if (((VR - lastSendVR) & 0x7fff) >= W)
                        SendSDatagram();
                    break;
                case DatagramFormat.NumberedSupervisory:
                    ACK = a.RecevingNumber;
                    break;
                case DatagramFormat.UnnumberedControl:
                    if ((byte)a.ControlFunction % 2 == 0)
                        SendDatagram(new UDatagram(a.ControlFunction + 1));
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
            needResponse = false;
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
            SendDatagram(new SDatagram());

        }
        void SendTestDatagram()
        {
            SendDatagram(new UDatagram(ControlFunctions.TESTFR_C));

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
        bool needResponse = false;
        private void TimerLoop(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Socket == null)
            {
                timer.Stop();
                return;
            }
            var now = DateTime.Now;

            if (needResponse && (now - lastRevTime).TotalMilliseconds >= t1)
            {
                CloseConnection();
                return;
            }
            else if (lastSendVR != VR && (lastIRevTime - now).TotalMilliseconds >= t2)
            {
                SendSDatagram();
            }
            else if (!testDatagramSentInCycle && (now - lastRevTime).TotalMilliseconds >= t3 && (now - LastSentTime).TotalMilliseconds >= t3)
            {
                SendTestDatagram();
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

        public void SendDatagram(Datagram d)
        {
            if (socket == null) return;
            lock (this)
            {

                switch (d.APDU.Format)
                {
                    case DatagramFormat.InformationTransmit:
                        d.APDU.RecevingNumber = VR;
                        lastSendVR = VR;
                        d.APDU.SendingNumber = VS++;
                        needResponse = true;
                        break;
                    case DatagramFormat.NumberedSupervisory:
                        d.APDU.RecevingNumber = VR;
                        lastSendVR = VR;
                        needResponse = false;
                        break;
                    case DatagramFormat.UnnumberedControl:
                        needResponse = (byte)d.APDU.ControlFunction % 2 == 0;
                        break;
                }
                d.SendTo(socket);
                LastSentTime = DateTime.Now;
                if (((VS - ACK) & 0x7fff) >= K)
                    CloseConnection();
            }

        }



    }
}
