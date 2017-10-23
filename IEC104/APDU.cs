using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 应用规约数据单元。包含控制域和应用服务数据单元。
    /// </summary>
    public class APDU
    {
        public APCI APCI { get; set; }
        public ASDU ASDU { get; set; }
        
    }

    public class APDU_I : APDU
    {

    }
    public class APDU_S : APDU
    {

    }
    public class APDU_U : APDU
    {

    }
}
