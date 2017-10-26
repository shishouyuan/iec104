using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Shouyuan.IEC104
{
    public enum ElementTypes : byte
    {
        /// <summary>
        /// 带品质描述词的单点信息。
        /// </summary>
        SIQ = 1,

        /// <summary>
        /// 带品质描述词的双点信息。
        /// </summary>
        DIQ = 2,

        /// <summary>
        /// 带变位检索的单点遥信变位信息。
        /// </summary>
        SCD = 3,

        /// <summary>
        /// 品质描述词。
        /// </summary>        
        QDS = 4,

        /// <summary>
        /// 规一化遥测值。
        /// </summary>
        NVA = 5,

        /// <summary>
        /// 标度化值。
        /// </summary>
        SVA = 6,

        /// <summary>
        /// 短浮点数。
        /// </summary>
        R = 7,

        /// <summary>
        /// 二进制计数器读数。
        /// </summary>
        BCR = 8,

        /// <summary>
        /// 初始化原因。
        /// </summary>        
        COI = 9,

        /// <summary>
        /// 单命令。
        /// </summary>
        SCO = 10,

        /// <summary>
        /// 双命令。
        /// </summary>
        DCO = 11,

        /// <summary>
        /// 召唤限定词。
        /// </summary>
        QOI = 12,

        /// <summary>
        /// 命令限定词。
        /// </summary>
        QOC = 13,

        /// <summary>
        /// 设定命令限定词。
        /// </summary>
        QOS = 14,

        /// <summary>
        /// 复位进程命令限定词。
        /// </summary>
        QRP = 15
    }

    /// <summary>
    /// 信息体对象
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 信息体地址。可选1、2、3个字节。3字节时最高字节一般置0。
        /// </summary>
        public readonly byte[] Addr;

        /// <summary>
        /// 信息体元素。
        /// </summary>
        public readonly byte[] Element;

        /// <summary>
        /// 信息体时标。可选2、3、7个字节。
        /// </summary>
        public readonly byte[] TimeStamp;

        /// <summary>
        /// 信息体总长度。
        /// </summary>
        public byte Length
        {
            get
            {
                byte c = 0;
                if (Addr != null) c += (byte)Addr.Length;
                if (Element != null) c += (byte)Element.Length;
                if (TimeStamp != null) c += (byte)Element.Length;
                return c;
            }
        }

        private static readonly byte[] elementTypeLengths =
        {
            1,//SIQ
            1,//DIQ
            5,//SCD
            1,//QDS
            2,//NVA
            2,//SVA
            4,//R
            5,//BCR
            1,//COI
            1,//SCO
            1,//DCO
            1,//QOI
            1,//QOC
            1,//QOS
            1//QRP            
        };
        public static byte GetElementTypeLength(ElementTypes t)
        {
            return elementTypeLengths[(byte)t - 1];
        }

        public ElementTypes Type { get; }

        /// <summary>
        /// 实例化信息体。
        /// </summary>
        /// <param name="t">信息元素类型。</param>
        /// <param name="addrl">地址长度，可选1、2、3个字节。</param>
        /// <param name="tml">时标长度，可选2、3、7个字节。</param>
        public Message(ElementTypes t, byte addrl, byte tml)
        {
            Type = t;
            Element = new byte[t.Length()];
            Addr = new byte[addrl];
            TimeStamp = new byte[tml];
        }

        public uint Address
        {
            get
            {
                uint v = Addr[0];
                for (var i = 1; i < Addr.Length; i++)
                    v |= (uint)Addr[i] << (i * 8);
                return v;
            }
            set
            {
                for (var i = 0; i < Addr.Length; i++)
                    Addr[i] = (byte)(value >> (i * 8));
            }
        }

        public ushort MiliSeconds
        {
            get
            {
                if (TimeStamp.Length < 2)
                    return 0;
                ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                switch (TimeStamp.Length)
                {
                    case 2:
                        return ms;
                    case 3:
                    case 7:
                        return (ushort)(ms % 1000);
                }
                return 0;
            }
            set
            {
                if (TimeStamp.Length < 2)
                    return;
                if (TimeStamp.Length == 3 || TimeStamp.Length == 7)
                {
                    ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                    ms -= (ushort)(ms % 1000);
                    ms += (ushort)(value % 1000);
                    value = ms;
                }
                TimeStamp[0] = (byte)value;
                TimeStamp[1] = (byte)(value >> 8);
            }
        }

        public ushort Seconds
        {
            get
            {
                if (TimeStamp.Length <= 2)
                    return 0;
                ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                if (TimeStamp.Length == 3 || TimeStamp.Length == 7)
                    return (ushort)(ms / 1000);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp.Length < 2)
                    return;
                if (TimeStamp.Length == 3 || TimeStamp.Length == 7)
                {
                    ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                    ms = (ushort)(ms % 1000);
                    value = (ushort)(value * 1000 + ms);
                }
                TimeStamp[0] = (byte)value;
                TimeStamp[1] = (byte)(value >> 8);
            }
        }

        public byte Minutes
        {
            get
            {
                if (TimeStamp.Length == 3 || TimeStamp.Length == 7)
                    return (byte)(TimeStamp[2] & 0x3f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp.Length == 3 || TimeStamp.Length == 7)
                    TimeStamp[2] = (byte)(TimeStamp[2] & 0x80 | value & 0x3f);
            }
        }

        public byte Hours
        {
            get
            {
                if (TimeStamp.Length == 7)
                    return (byte)(TimeStamp[3] & 0x1f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp.Length == 7)
                {
                    TimeStamp[3] = (byte)(value & 0x1f);
                }
            }
        }


        public byte Days
        {
            get
            {
                if (TimeStamp.Length == 7)
                    return (byte)(TimeStamp[4] & 0x1f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp.Length == 7)
                {
                    TimeStamp[4] = (byte)(TimeStamp[4] & 0xe0 | value & 0x1f);
                }
            }
        }
        public byte DayInWeek
        {
            get
            {
                if (TimeStamp.Length == 7)
                    return (byte)((TimeStamp[4] & 0xe0) >> 5);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp.Length == 7)
                {
                    TimeStamp[4] = (byte)(TimeStamp[4] & 0x1f | (value << 5));
                }
            }
        }

        public byte Months
        {
            get
            {
                if (TimeStamp.Length == 7)
                    return (byte)(TimeStamp[5] & 0x0f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp.Length == 7)
                {
                    TimeStamp[5] = (byte)(value & 0x0f);
                }
            }
        }
        public byte Years
        {
            get
            {
                if (TimeStamp.Length == 7)
                    return (byte)(TimeStamp[6] & 0x8f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp.Length == 7)
                {
                    TimeStamp[6] = (byte)(value & 0x8f);
                }
            }
        }



    }
}
