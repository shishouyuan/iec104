using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    public abstract class DatagramFormatterBase
    {
        public byte ASDUType { get; }

        public ElementType ElementType { get; }
        public byte ExtraLength { get; }
        public byte TimeStampLength { get; }
        public byte AddrLength { get; }

        public abstract byte DefaultASDUType { get; }

        public virtual string Description { get => "报文格式器。"; }

        /// <summary>
        /// 创建含地址的信息体。
        /// </summary>
        /// <returns></returns>
        protected virtual Message CreateNewMessageWithAddr()
        {
            return new Message(ElementType, AddrLength, ExtraLength, TimeStampLength);
        }

        /// <summary>
        /// 创建不含地址的信息体。
        /// </summary>
        /// <returns></returns>
        protected virtual Message CreateNewMessageNoAddr()
        {
            return new Message(ElementType, 0, ExtraLength, TimeStampLength);
        }
        protected virtual void SetTime(Message m, DateTime t)
        {
            m.Years = (byte)(t.Year % 2000);
            m.Months = (byte)t.Month;
            m.Days = (byte)t.Day;
            m.Hours = (byte)t.Hour;
            m.Minutes = (byte)t.Minute;
            m.Seconds = (ushort)t.Second;
            m.Miliseconds = (ushort)t.Millisecond;

            m.DayInWeek =(byte) t.DayOfWeek;
        }
        /// <summary>
        /// 为指定的APDU创建信息体并返回，但不自动添加进ASDU。
        /// </summary>
        /// <param name="apdu">目标APDU</param>
        /// <param name="addr">信息体地址，为零时创建不含地址的顺序信息体。</param>
        /// <returns></returns>
        protected virtual Message CreateMessageForAPDU(APDU apdu, uint addr)
        {
            Message m;
            if (addr == 0)
            {
                if (!apdu.ASDU.SQ || apdu.ASDU.ActualMsgCount == 0)
                    throw new Exception("非顺序信息体或首个信息体地址不能为0");
                m = CreateNewMessageNoAddr();
            }
            else
            {
                if (apdu.ASDU.SQ && apdu.ASDU.ActualMsgCount > 0)
                {
                    if (addr == apdu.ASDU.Messages.Last().Address + 1)
                        m = CreateNewMessageNoAddr();
                    else
                        throw new Exception("顺序信息体地址不连续");
                }
                else
                {
                    m = CreateNewMessageWithAddr();
                    m.Address = addr;
                }
            }
            return m;
        }

        public virtual APDU CreateAPDU(byte asduAddr, bool sq = false, bool pn = false, bool test = false)
        {
            var v = new APDU(this);
            v.ASDU = new ASDU();
            v.ASDU.Type = ASDUType;
            v.ASDU.Address = asduAddr;
            v.ASDU.SQ = sq;
            v.ASDU.PN = pn;
            v.ASDU.Test = test;
            return v;
        }

        public virtual String GetBriefing (APDU apdu)
        {
            var sb = new StringBuilder();
            switch (apdu.Format)
            {
                case DatagramFormat.NumberedSupervisory:
                    sb.AppendFormat("S格式报文：序号{0}。\n",apdu.RecevingNumber);
                    break;
                case DatagramFormat.UnnumberedControl:
                    sb.AppendFormat("U格式报文：{0}\n", apdu.ControlFunction);
                    break;
                case DatagramFormat.InformationTransmit:
                    sb.AppendFormat("I格式报文：\n");
                    break;
            }
            return sb.ToString();
        }


        protected DatagramFormatterBase(byte atype, ElementType etype, byte extral=0, byte tsl = 0, byte addrl = 3)
        {
            ASDUType = atype;
            ElementType = etype;
            ExtraLength = extral;
            TimeStampLength = tsl;
            AddrLength = addrl;

        }

    }
}
