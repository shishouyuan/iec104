using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    public class C_IC_NA_1:DatagramFormatterBase
    {
        public const byte defaultASDUType = 100;
        public override byte DefaultASDUType => defaultASDUType;

        public override string Description => "总招唤。";

        public C_IC_NA_1(byte type = defaultASDUType) : base(type, ElementType.Empty, 1)
        {

        }
       
        public APDU Create(byte asduaddr,byte group,byte cot=6)
        {
            var apdu = CreateAPDU(asduaddr);
            apdu.ASDU.Cause = cot;
            var m = CreateNewMessageWithAddr();
            m.Address = 0;
            m.QOI=(byte)(group+Message.QOI_WholeStation);            
            apdu.ASDU.Messages.Add(m);
            return apdu;
        }
    }
}
