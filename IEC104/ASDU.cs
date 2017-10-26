using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 应用服务数据单元。
    /// </summary>
    public class ASDU
    {

        public const byte HeaderLength = 6;
        public readonly byte[] Values = new byte[HeaderLength];

        public byte[] ActualValues
        {
            get
            {
                var a =(byte[]) Values.Clone();
                a[1] = (byte)(SQ ? Messages.Count | 0x80 : Messages.Count & ~0x80);
                return a;
            }
        }

        /// <summary>
        /// 类型标识。
        /// </summary>
        public byte Type
        {
            get => Values[0];
            set => Values[0] = value;
        }

        /// <summary>
        /// 可变结构限定词。最高位为SQ，其余7位表示信息体的个数。
        /// </summary>
        public byte VSQ
        {
            get => Values[1];
            set => Values[1] = value;
        }

        /// <summary>
        /// 传送原因低字节。
        /// </summary>
        public byte COT1
        {
            get => Values[2];
            set => Values[2] = value;
        }

        /// <summary>
        /// 传送原因高字节，高字节为源发地址。
        /// </summary>
        public byte COT2
        {
            get => Values[3];
            set => Values[3] = value;
        }

        /// <summary>
        /// 应用服务数据单元公共地址低字节。共2字节，高字节固定为0。
        /// </summary>
        public byte Address
        {
            get => Values[4];
            set => Values[4] = value;
        }

        /// <summary>
        /// 信息体集合
        /// </summary>
        public readonly List<Message> Messages = new List<Message>();


        /// <summary>
        /// 离散或顺序信息排列方式
        /// </summary>
        public bool SQ
        {
            get => (VSQ & 0x80) != 0;
            set => VSQ = (byte)(value ? VSQ | 0x80 : VSQ & ~0x80);
        }

        /// <summary>
        /// 报头标称包含的信息体数量
        /// </summary>
        public byte MsgCount
        {
            get => (byte)(VSQ & ~0x80);
            set => VSQ = (byte)(SQ ? value | 0x80 : value & ~0x80);
        }

        /// <summary>
        /// 实际包含的信息体数量。
        /// </summary>
        public byte ActualMsgCount
        {
            get => (byte)Messages.Count;
        }

        /// <summary>
        /// 传送原因，COT的低6位。
        /// </summary>
        public byte Cause
        {
            get => (byte)(COT1 & 0x3f);
            set => COT1 = (byte)((COT1 & 0xc0) + (value & 0x3f));
        }

        /// <summary>
        /// 试验标志，COT的最高位。
        /// </summary>
        public bool Test
        {
            get => (COT1 & 0x80) != 0;
            set => COT1 = (byte)(value ? COT1 | 0X80 : COT1 & ~0X80);
        }

        /// <summary>
        /// 确认标志P/N，COT的次高位，0为P返回True，1为N返回False。
        /// </summary>
        public bool PN
        {
            get => (COT1 & 0x40) == 0;
            set => COT1 = (byte)(value ? COT1 & ~0X40 : COT1 | 0X40);
        }

        public byte Length
        {
            get
            {
                byte l = HeaderLength;
                if (Messages != null)
                    foreach (var m in Messages)
                        l += m.Length;
                return l;
            }
        }

        public ASDU()
        {

        }

        public ASDU(byte[] buf, int starti = APDU.APCILength)
        {

            for (int i = 0; i < HeaderLength; i++)
            {
                Values[i] = buf[starti + i];
            }

        }

        public void SendTo(System.Net.Sockets.Socket socket)
        {
            socket.Send(ActualValues);
            foreach (var msg in Messages)
                msg.SendTo(socket);
        }
    }
}
