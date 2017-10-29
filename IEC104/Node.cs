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
        public ushort t0 = 30000;
        /// <summary>
        /// 发送或测试等待回应超时。
        /// </summary>
        public ushort t1 = 20000;
        /// <summary>
        /// 收到最后数据必须确认超时时间。
        /// </summary>
        public ushort t2 = 15000;
        /// <summary>
        /// 通道长期空闲时发送确认帧的超时时间。
        /// </summary>
        public ushort t3 = 25000;

        private DateTime lastRevTime;
        private ushort lastSendVR;

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
            VR++;
            if (((VR - lastSendVR) & 0x7fff) >= W)
                SendSDatagram();
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
                running = false;
            }
            if (ConnectionLost != null) ConnectionLost(this);
        }

        public void StartReceive()
        {
            if (socket == null)
                throw new Exception("尚未绑定Socket。");
            VS = 0;
            VR = 0;
            ACK = 0;
            running = true;
            ThreadPool.QueueUserWorkItem(ReceiveLoop);
        }

        void SendSDatagram()
        {
            SendDatagram(new SDatagram());

        }
        void SendTestDatagram()
        {


        }

       
        
        System.Timers.Timer timer= new System.Timers.Timer(100);
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

        private void TimerLoop(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Socket == null)
            {
                timer.Stop();
                return;
            }
            ushort tp =(ushort) (DateTime.Now - lastRevTime).TotalMilliseconds;
            if (tp >= t1)
                CloseSocket();
            else if (started)
            {
                if (tp >= t2)
                    SendSDatagram();
            }
            else if (tp > t3)
                SendTestDatagram();
        }

        public void CloseSocket()
        {
            if (socket == null) return;
            running = false;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }

        public void SendDatagram(Datagram d)
        {
            if (socket == null) return;
            lock (this)
            {
                if (d.APDU.Format == DatagramFormat.InformationTransmit || d.APDU.Format == DatagramFormat.NumberedSupervisory)
                {
                    d.APDU.RecevingNumber = VR;
                    lastSendVR = VR;
                }
                if (d.APDU.Format == DatagramFormat.InformationTransmit)
                    d.APDU.SendingNumber = VS++;
                d.SendTo(socket);
                if (((VS - lastSendVR) & 0x7fff) >= K)
                    CloseSocket();
            }

        }



    }
}
