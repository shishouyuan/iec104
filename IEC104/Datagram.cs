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
        public virtual byte Type { get; set; }
        public virtual DatagramFormat DatagramFormat { get => DatagramFormat.Unknown; }
        public virtual void SendTo(System.Net.Sockets.Socket socket)
        {
            APDU.SendTo(socket);
        }
        protected Datagram(APDU apdu)
        {
            APDU = apdu;
        }
    }

    public class SDatagram: Datagram
    {
        public SDatagram() : base(new APDU () )
        {
            APDU.Format = DatagramFormat.NumberedSupervisory;
        }
    }

    public class UDatagram : Datagram
    {
        public UDatagram(ControlFunctions f):base(new APDU())
        {
            APDU.Format = DatagramFormat.UnnumberedControl;
            APDU.ControlFunction = f;
        }
    }


    public abstract class InfoDatagram : Datagram
    {
        protected InfoDatagram(byte asduAddr, byte cause) : base(new APDU())
        {
            APDU.ASDU = new ASDU();
            APDU.ASDU.Address = asduAddr;
            APDU.ASDU.Cause = cause;
        }
    }

    public class M_ME_NA_1 : InfoDatagram
    {
        public bool SQ { get; }
        public uint FirstMsgAddr { get; }
        public M_ME_NA_1(byte asduAddr, byte cause, uint firstaddr = 0, byte type = 9) : base(asduAddr, cause)
        {
            APDU.ASDU = new ASDU();
            APDU.ASDU.Type = type;
            Type = type;
            FirstMsgAddr = firstaddr;
            SQ = firstaddr != 0;
            APDU.ASDU.SQ = SQ;
        }

        public void PutData(uint addr, float max, float val, bool iv = false, bool nt = false, bool sb = false, bool bl = false, bool ov = false)
        {
            if (SQ)
                throw new Exception("此方法适用于非顺序发送信息体。");

            var m = new Message(t: ElementTypes.NVA, extrl: 1);
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
                m = new Message(t: ElementTypes.NVA, extrl: 1);
                m.Address = FirstMsgAddr;
            }
            else
            {
                m = new Message(t: ElementTypes.NVA, extrl: 1, addrl: 0);
            }
            m.NVA_M = max;
            m.NVA = val;
            APDU.ASDU.Messages.Add(m);
        }

    }
}
