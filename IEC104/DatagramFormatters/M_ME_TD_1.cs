using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 带长时标的归一化测量值报文格式。
    /// </summary>
    public class M_ME_TD_1:DatagramFormatterBase
    {
        public const byte defaultASDUType = 34;
        public override byte DefaultASDUType => defaultASDUType;

        public override string Description => "带长时标的归一化测量值报文格式。";

        public M_ME_TD_1(byte type = defaultASDUType) : base(type, ElementType.NVA, 1, 7)
        {

        }

        public void PutData(APDU apdu, float max ,float val, DateTime time, uint addr = 0, bool iv = false, bool nt = false, bool sb = false, bool bl = false, bool ov = false)
        {
            var m = CreateMessageForAPDU(apdu, addr);

            m.NVA_M = max;
            m.NVA = val;
            m.QDS_IV = iv;
            m.QDS_NT = nt;
            m.QDS_SB = sb;
            m.QDS_BL = bl;
            m.QDS_OV = ov;
            SetTime(m, time);

            apdu.ASDU.Messages.Add(m);
        }
    }
}
