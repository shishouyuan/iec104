using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    public class C_RD_NA_1:DatagramFormatterBase
    {
        public const byte defaultASDUType = 102;
        public override byte DefaultASDUType => defaultASDUType;

        public override string Description => "召唤单个数据";

        public C_RD_NA_1(byte type = defaultASDUType) : base(type, ElementType.Empty)
        {

        }

        public APDU Create(byte asduaddr, byte msgAddr, byte cot = 5)
        {
            var apdu = CreateAPDU(asduaddr);
            apdu.ASDU.Cause = cot;
            var m = CreateNewMessageWithAddr();
            m.Address = msgAddr;
            apdu.ASDU.Messages.Add(m);
            return apdu;
        }
    }
}
