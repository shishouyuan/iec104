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


        byte[,] bufs=new byte[10,260];
        byte pbufi = 0, rbufi = 0;
        byte rc = 0;
      


        byte[] revBuf = new byte[260];
        byte revBufi = 0;
        private void receiveMsg(Socket socket)
        {
            try
            {
                while (socket != null)
                {
                    int c = socket.Receive(revBuf);
                    for (var i = 0; i < c; i++) {
                        if (rc == 0 &&)
                        {
                            continue;
                        }
                        bufs[rbufi,rc] = revBuf[i];
                        rc++;
                        if (rc >= LEN)
                        {
                            bufs[rbufi][BUF_SIZE] = rc;
                            rc = 0;
                            if (bufs[rbufi][1] == addr1 && bufs[rbufi][2] == addr2)
                            {
                                rbufi = (rbufi + 1) % BUF_COUNT;
                                newMsg = 1;
                            }

                        }
                    }
                }
            }
        }

        public void startService()
        {
            listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(0, PortNumber));
            listenSocket.Listen(1);
            System.Threading.ThreadPool.QueueUserWorkItem(
                (object o) =>
                {
                    linkSocket = listenSocket.Accept();
                    receiveMsg(linkSocket);
                });
        }

        public void openPort()
        {

            var s = listenSocket.Accept();
            var a = new byte[10];
            while (s.Receive(a) > 0)
            {
                string sf = "";
                foreach (var i in a)
                    sf = sf + i.ToString("x");

            }
        }

    }
}
