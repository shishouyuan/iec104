using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 报文格式
    /// </summary>
    public enum DatagramFormat : byte
    {
        /// <summary>
        /// 未知格式
        /// </summary>
        Unknown,

        /// <summary>
        /// I格式
        /// </summary>
        InformationTransmit,

        /// <summary>
        /// S格式
        /// </summary>
        NumberedSupervisory,

        /// <summary>
        /// U格式
        /// </summary>
        UnnumberedControl
    }

    /// <summary>
    /// U格式报文功能
    /// </summary>
    public enum ControlFunction : byte
    {
        /// <summary>
        /// 测试命令
        /// </summary>
        TESTFR_C,

        /// <summary>
        /// 测试确认
        /// </summary>
        TESTFR_A,

        /// <summary>
        /// 停止命令
        /// </summary>
        STOPDT_C,

        /// <summary>
        /// 停止确认
        /// </summary>
        STOPDT_A,

        /// <summary>
        /// 开启命令
        /// </summary>
        STARTDT_C,

        /// <summary>
        /// 开启确认
        /// </summary>
        STARTDT_A,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown
    }

    /// <summary>
    /// 应用规约控制信息。包含启动字符、APDU长度和控制域。 
    /// </summary>
    public class APCI
    {
        public const int Header = 0x68;
        public const int Length = 6;
        /// <summary>
        /// 此类所代表的APCI结构的数组
        /// </summary>
        public byte[] Values;

        public APCI()
        {
            Values = new byte[6] { Header, 0, 0, 0, 0, 0 };
        }

        public APCI(byte[] buf)
        {
            if (buf.Length < 6)
                throw new Exception("APCI传入的数组过小，不足6个元素。");
            Values = buf;
            buf[0] = Header;
        }


        /// <summary>
        /// APDU的长度，从控制域到APDU末尾
        /// </summary>
        public byte APDULength
        {
            get => Values[1];
            set => Values[1] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL1
        {
            get => Values[2];
            set => Values[2] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL2
        {
            get => Values[3];
            set => Values[3] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL3
        {
            get => Values[4];
            set => Values[4] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL4
        {
            get => Values[5];
            set => Values[5] = value;
        }

        public DatagramFormat Format
        {
            get
            {
                if ((CTRL1 & 0x1) == 0 && (CTRL3 & 0x1) == 0)
                    return DatagramFormat.InformationTransmit;
                else if ((CTRL1 & 0x1) == 1 && (CTRL1 & 0x2) == 0 && (CTRL3 & 0x1) == 0)
                    return DatagramFormat.NumberedSupervisory;
                else if ((CTRL1 & 0x1) == 1 && (CTRL1 & 0x2) != 0 && (CTRL3 & 0x1) == 0)
                    return DatagramFormat.UnnumberedControl;
                else
                    return DatagramFormat.Unknown;
            }
            set
            {
                switch (value)
                {
                    case DatagramFormat.InformationTransmit:
                        unchecked
                        {
                            CTRL1 &= (byte)~0x1;
                            CTRL3 &= (byte)~0x1;
                        }
                        break;
                    case DatagramFormat.NumberedSupervisory:
                        unchecked
                        {
                            CTRL1 |= 0x1;
                            CTRL1 &= (byte)~0x2;
                            CTRL3 &= (byte)~0x1;
                        }
                        break;
                    case DatagramFormat.UnnumberedControl:
                        unchecked
                        {
                            CTRL1 |= 0x3;
                            CTRL3 &= (byte)~0x1;
                        }
                        break;
                        //case DatagramFormat.Unknown:
                        //    CTRL1 |= 0x1;
                        //    CTRL1 |= 0x2;
                        //    CTRL3 |= 0x1;
                        //    break;
                }
            }
        }

        public ushort SendingNumber
        {
            get => (ushort)((CTRL1 >> 1) + (CTRL2 << 7));
            set
            {
                if (Format != DatagramFormat.InformationTransmit)
                    throw new Exception("非I格式报文不支持SendingNumber属性。");
                CTRL1 = (byte)((value << 1) + (CTRL1 & 0x1));
                CTRL2 = (byte)(value >> 7);
            }
        }
        public ushort RecevingNumber
        {
            get => (ushort)((CTRL3 >> 1) + (CTRL4 << 7));
            set
            {
                if (Format != DatagramFormat.InformationTransmit|| Format != DatagramFormat.NumberedSupervisory)
                    throw new Exception("非I格式或S格式报文不支持RecevingNumber属性。");
                CTRL3 = (byte)((value << 1) + (CTRL3 & 0x1));
                CTRL4 = (byte)(value >> 7);
            }
        }

        public  ControlFunction ControlFunction
        {
            get
            {
                if (Format != DatagramFormat.UnnumberedControl)
                    return ControlFunction.Unknown;
                if ((CTRL1 & 0x4) != 0)
                    return ControlFunction.STARTDT_C;
                else if ((CTRL1 & 0x8) != 0)
                    return ControlFunction.STARTDT_A;
                else if ((CTRL1 & 0x10) != 0)
                    return ControlFunction.STOPDT_C;
                else if ((CTRL1 & 0x20) != 0)
                    return ControlFunction.STOPDT_A;
                else if ((CTRL1 & 0x40) != 0)
                    return ControlFunction.TESTFR_C;
                else if ((CTRL1 & 0x80) != 0)
                    return ControlFunction.TESTFR_A;
                else
                    return ControlFunction.Unknown;
            }
            set
            {
                if (Format != DatagramFormat.UnnumberedControl)
                    throw new Exception("仅U格式支持ControlFunction属性。");
                switch (value)
                {
                    case ControlFunction.STARTDT_C:
                        CTRL1 = 0x4 + 0x3;
                        break;
                    case ControlFunction.STARTDT_A:
                        CTRL1 = 0x8 + 0x3;
                        break;
                    case ControlFunction.STOPDT_C:
                        CTRL1 = 0x10 + 0x3;
                        break;
                    case ControlFunction.STOPDT_A:
                        CTRL1 = 0x20 + 0x3;
                        break;
                    case ControlFunction.TESTFR_C:
                        CTRL1 = 0x40 + 0x3;
                        break;
                    case ControlFunction.TESTFR_A:
                        CTRL1 = 0x80 + 0x3;
                        break;

                }
            }
        }




    }
}
