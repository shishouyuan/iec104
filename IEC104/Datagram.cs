using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{

    public interface IParameters
    {
        byte i { get; }
    }

    public class u : IParameters{

        public byte IParameters.i => 1;
    }
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
        protected Datagram(int i)
        {

        }
    }

    public class M_ME_NA_1 : Datagram
    {

        public M_ME_NA_1(byte type = 9) : base(new APDU())
        {
            Type = type;
        }

        public void PutData(float max, float val, bool iv = false, bool nt = false, bool sb = false, bool bl = false, bool ov = false)
        {
            var m = new Message(ElementTypes.NVA,);
            APDU.ASDU.Messages.Add(new Message())
        }

    }
}
