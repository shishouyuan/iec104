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
    public enum ControlFunctions : byte
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
    /// 应用规约数据单元。包含控制域和应用服务数据单元。
    /// </summary>
    public class APDU
    {

        public const int Header = 0x68;
        public ASDU ASDU;


        #region APCI

        public const int APCILength = 6;


        public readonly byte[] APCIValues = new byte[6];

        public byte[] ActualAPCIValues
        {
            get
            {

                var a = (byte[])APCIValues.Clone();
                a[1] = ActualLength;
                return a;
            }
        }

        /// <summary>
        /// 报头内标称的APDU长度。
        /// </summary>
        public byte APDULength
        {
            get => APCIValues[1];
            set => APCIValues[1] = value;
        }


        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL1
        {
            get => APCIValues[2];
            set => APCIValues[2] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL2
        {
            get => APCIValues[3];
            set => APCIValues[3] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL3
        {
            get => APCIValues[4];
            set => APCIValues[4] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL4
        {
            get => APCIValues[5];
            set => APCIValues[5] = value;
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
                if (Format != DatagramFormat.InformationTransmit && Format != DatagramFormat.NumberedSupervisory)
                    throw new Exception("非I格式或S格式报文不支持RecevingNumber属性。");
                CTRL3 = (byte)((value << 1) + (CTRL3 & 0x1));
                CTRL4 = (byte)(value >> 7);
            }
        }

        public ControlFunctions ControlFunction
        {
            get
            {
                if (Format != DatagramFormat.UnnumberedControl)
                    return ControlFunctions.Unknown;
                if ((CTRL1 & 0x4) != 0)
                    return ControlFunctions.STARTDT_C;
                else if ((CTRL1 & 0x8) != 0)
                    return ControlFunctions.STARTDT_A;
                else if ((CTRL1 & 0x10) != 0)
                    return ControlFunctions.STOPDT_C;
                else if ((CTRL1 & 0x20) != 0)
                    return ControlFunctions.STOPDT_A;
                else if ((CTRL1 & 0x40) != 0)
                    return ControlFunctions.TESTFR_C;
                else if ((CTRL1 & 0x80) != 0)
                    return ControlFunctions.TESTFR_A;
                else
                    return ControlFunctions.Unknown;
            }
            set
            {
                if (Format != DatagramFormat.UnnumberedControl)
                    throw new Exception("仅U格式支持ControlFunctions属性。");
                switch (value)
                {
                    case ControlFunctions.STARTDT_C:
                        CTRL1 = 0x4 + 0x3;
                        break;
                    case ControlFunctions.STARTDT_A:
                        CTRL1 = 0x8 + 0x3;
                        break;
                    case ControlFunctions.STOPDT_C:
                        CTRL1 = 0x10 + 0x3;
                        break;
                    case ControlFunctions.STOPDT_A:
                        CTRL1 = 0x20 + 0x3;
                        break;
                    case ControlFunctions.TESTFR_C:
                        CTRL1 = 0x40 + 0x3;
                        break;
                    case ControlFunctions.TESTFR_A:
                        CTRL1 = 0x80 + 0x3;
                        break;

                }
            }
        }

        #endregion

        public APDU()
        {
            APCIValues[0] = Header;

        }

        public byte[] InputedBuffer;

        public APDU(byte[] buf, InfoDatagram infoDatagram = null)
        {
            if (buf[0] == Header && buf.Length >= APCILength)
            {
                InputedBuffer = buf;
                for (int i = 0; i < APCIValues.Length; i++)
                    APCIValues[i] = buf[i];
                if (infoDatagram == null)
                    return;
                if (Format == DatagramFormat.InformationTransmit)
                {
                    ASDU = new ASDU(buf, APCILength);
                    if (infoDatagram.ASDUType == ASDU.Type)
                    {
                        byte bi = APCILength + ASDU.HeaderLength;
                        if (ASDU.SQ)
                        {
                            var m = new Message(infoDatagram.ElementType, infoDatagram.AddrLength, infoDatagram.ExtraLength, infoDatagram.TimeStampLength);
                            for (int i = 0; i < infoDatagram.AddrLength; i++)
                                m.Addr[i] = buf[bi++];
                            for (int i = 0; i < infoDatagram.ElementType.Length(); i++)
                                m.Element[i] = buf[bi++];
                            for (int i = 0; i < infoDatagram.ExtraLength; i++)
                                m.Extra[i] = buf[bi++];
                            for (int i = 0; i < infoDatagram.TimeStampLength; i++)
                                m.TimeStamp[i] = buf[bi++];
                            ASDU.Messages.Add(m);
                            var addr = m.Address;
                            for (int k = 1; k < ASDU.MsgCount; k++)
                            {
                                m = new Message(infoDatagram.ElementType, 0, infoDatagram.ExtraLength, infoDatagram.TimeStampLength);
                                m.Address = ++addr;
                                for (int i = 0; i < infoDatagram.ElementType.Length(); i++)
                                    m.Element[i] = buf[bi++];
                                for (int i = 0; i < infoDatagram.ExtraLength; i++)
                                    m.Extra[i] = buf[bi++];
                                for (int i = 0; i < infoDatagram.TimeStampLength; i++)
                                    m.TimeStamp[i] = buf[bi++];
                                ASDU.Messages.Add(m);
                            }

                        }
                        else
                        {
                            for (int k = 0; k < ASDU.MsgCount; k++)
                            {
                                var m = new Message(infoDatagram.ElementType, infoDatagram.AddrLength, infoDatagram.ExtraLength, infoDatagram.TimeStampLength);
                                for (int i = 0; i < infoDatagram.AddrLength; i++)
                                    m.Addr[i] = buf[bi++];
                                for (int i = 0; i < infoDatagram.ElementType.Length(); i++)
                                    m.Element[i] = buf[bi++];
                                for (int i = 0; i < infoDatagram.ExtraLength; i++)
                                    m.Extra[i] = buf[bi++];
                                for (int i = 0; i < infoDatagram.TimeStampLength; i++)
                                    m.TimeStamp[i] = buf[bi++];
                                ASDU.Messages.Add(m);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("不是一个合法的APDU数据。");
            }
        }

        public byte ActualLength
        {
            get
            {
                return (byte)(4 + (Format == DatagramFormat.InformationTransmit ? ASDU.Length : 0));
            }
        }

        public void SaveTo(List<byte> buf)
        {
            buf.AddRange(ActualAPCIValues);
            if (Format == DatagramFormat.InformationTransmit)
                ASDU.SaveTo(buf);
        }

    }

    public interface sk
    {
        void Send(byte[] d);
    }
    public class d : sk
    {
        public List<byte> list = new List<byte>();
        void sk.Send(byte[] d)
        {
            list.AddRange(d);
        }
    }
}
