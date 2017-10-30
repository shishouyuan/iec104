using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{

    public abstract class Datagram
    {
        public readonly APDU APDU;
        //public virtual DatagramFormat DatagramFormat { get => DatagramFormat.Unknown; }
        public virtual void SaveTo(List<byte> buf)
        {
            APDU.SaveTo(buf);
        }
        protected Datagram(APDU apdu)
        {
            APDU = apdu;
        }
    }

    public class SDatagram : Datagram
    {
        public SDatagram() : base(new APDU())
        {
            APDU.Format = DatagramFormat.NumberedSupervisory;
        }
    }

    public class UDatagram : Datagram
    {
        public UDatagram(ControlFunctions f) : base(new APDU())
        {
            APDU.Format = DatagramFormat.UnnumberedControl;
            APDU.ControlFunction = f;
        }
    }


    public abstract class InfoDatagram : Datagram
    {

        public abstract byte ASDUType { get; set; }
        protected InfoDatagram(byte asduAddr, byte cause) : base(new APDU())
        {
            APDU.ASDU = new ASDU();
            APDU.ASDU.Address = asduAddr;
            APDU.ASDU.Cause = cause;
        }

        public abstract ElementTypes ElementType { get; }
        public abstract byte ExtraLength { get; }
        public abstract byte TimeStampLength { get; }
        public virtual byte AddrLength { get => 3; }

        protected InfoDatagram(APDU apdu) : base(apdu)
        {

        }
    }

    public class M_ME_NA_1 : InfoDatagram
    {
        public bool SQ { get; }
        public uint FirstMsgAddr { get; }

        public override  ElementTypes ElementType  =>ElementTypes.NVA;

        public override byte ExtraLength => 1;

        public override byte TimeStampLength => 0;

        public override byte ASDUType { get; set; } = 9;

        public M_ME_NA_1() : base(null)
        {
            //ASDUType = 9;
        }

        public M_ME_NA_1(byte asduAddr, byte cause, uint firstaddr = 0, byte type = 9) : base(asduAddr, cause)
        {
            APDU.ASDU = new ASDU();
            APDU.ASDU.Type = type;
            ASDUType = type;
            FirstMsgAddr = firstaddr;
            SQ = firstaddr != 0;
            APDU.ASDU.SQ = SQ;
        }

        public void PutData(uint addr, float max, float val, bool iv = false, bool nt = false, bool sb = false, bool bl = false, bool ov = false)
        {
            if (SQ)
                throw new Exception("此方法适用于非顺序发送信息体。");

            var m = new Message( ElementType,AddrLength,ExtraLength,TimeStampLength);
            m.NVA_M = max;
            m.NVA = val;
            m.Address = addr;
            APDU.ASDU.Messages.Add(m);
        }

        public void PutSQData(float max, float val, bool iv = false, bool nt = false, bool sb = false, bool bl = false, bool ov = false)
        {
            if (!SQ)
                throw new Exception("此方法适用于顺序发送信息体。");
            Message m;
            if (APDU.ASDU.Messages.Count == 0)
            {
                m = new Message(ElementType, AddrLength, ExtraLength, TimeStampLength);
                m.Address = FirstMsgAddr;
            }
            else
            {
                m = new Message(ElementType, 0, ExtraLength, TimeStampLength);
            }
            m.NVA_M = max;
            m.NVA = val;
            APDU.ASDU.Messages.Add(m);
        }

        public M_ME_NA_1(APDU apdu) : base(apdu)
        {

        }
    }
}
