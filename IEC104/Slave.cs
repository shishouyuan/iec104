using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// IEC104规约从站
    /// </summary>
    public class Slave
    {

        Socket listenSocket, linkSocket;

        private int portNumber = 2404;

        /// <summary>
        ///规约使用的TCP/IP端口号。
        /// </summary>
        public int PortNumber
        {
            get => portNumber;
            set
            {
                portNumber = value;
            }
        }

        private byte addr = 0;
        /// <summary>
        /// 站地址
        /// </summary>
        public byte Addr
        {
            get => addr;
            set
            {
                addr = value;
            }
        }

        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="port">本地端口号</param>
        /// <param name="ad">站地址</param>
        public Slave(int port, byte ad)
        {
            portNumber = port;
            addr = ad;
        }


        byte rc = 0;

        private void HandleData(byte[] data)
        {
            var a = new APCI(data);
            try
            {
                var s = new ASDU(data, APCI.Length);
            }
            catch (Exception e) { }
        }

        byte[] revBuf = new byte[260];


        private void ReceiveMsg(Socket socket)
        {
            byte len = 255;
            byte[] cBuf = null;
            try
            {
                while (socket != null)
                {
                    int c = socket.Receive(revBuf);
                    byte bi;
                    for (int i = 0; i < c; i++)
                    {
                        bi = revBuf[i];
                        if (rc == 0)
                        {
                            if (bi != APCI.Header)
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
                                cBuf[0] = APCI.Header;
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
                socket = null;
            }
        }

        public void startService()
        {
            if (listenSocket != null) return;
            listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(0, PortNumber));
            listenSocket.Listen(1);
            System.Threading.ThreadPool.QueueUserWorkItem(
                (object o) =>
                {
                    while (true)
                    {
                        linkSocket = listenSocket.Accept();
                        ReceiveMsg(linkSocket);
                    }
                });
        }


    }
}
