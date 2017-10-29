using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    public  abstract class  Datagram
    {
        public readonly APDU APDU=new APDU();
        public virtual byte Type { get; set; }   
        public virtual DatagramFormat DatagramFormat { get => DatagramFormat.Unknown; }
        public virtual void SendTo(System.Net.Sockets.Socket socket)
        {
            APDU.SendTo(socket);
        }
    }

    public class M_ME_NA_1 : Datagram
    {
        
    }
}
