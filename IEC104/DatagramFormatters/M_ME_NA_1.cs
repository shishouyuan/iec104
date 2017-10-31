using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Shouyuan.IEC104
{

    public class M_ME_NA_1 : DatagramFormatterBase
    {

        public static M_ME_NA_1 SharedInstance;

        public const byte defaultASDUType = 9;
        public override byte DefaultASDUType => defaultASDUType;

        public M_ME_NA_1(byte type = defaultASDUType) : base(type, ElementType.NVA, 1, 0)
        {
            if (SharedInstance == null)
                SharedInstance = this;
        }

        public void PutData(APDU apdu, float max, float val, uint addr = 0, bool iv = false, bool nt = false, bool sb = false, bool bl = false, bool ov = false)
        {
            var m = CreateMessageForAPDU(apdu, addr);

            m.NVA_M = max;
            m.NVA = val;
            m.QDS_IV = iv;
            m.QDS_NT = nt;
            m.QDS_SB = sb;
            m.QDS_BL = bl;
            m.QDS_OV = ov;
            apdu.ASDU.Messages.Add(m);
        }
    }
}
