using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 应用服务数据单元。
    /// </summary>
    public class ASDU
    {
        /// <summary>
        /// 类型标识。
        /// </summary>
        public byte Type;

        /// <summary>
        /// 可变结构限定词。最高位为SQ，其余7位表示信息体的个数。
        /// </summary>
        public byte VarConstrain;

        /// <summary>
        /// 传送原因。2字节，高字节为源发地址。
        /// </summary>
        public byte[] COT = new byte[2];

        /// <summary>
        /// 应用服务数据单元公共地址。2字节，高字节固定为0。
        /// </summary>
        public byte addr;

        /// <summary>
        /// 信息体集合
        /// </summary>
        public Message[] Messages;
    }
}
