using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 信息体对象
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 信息体地址。可选1、2、3个字节。3字节时最高字节一般置0。
        /// </summary>
        public byte[] Addr;

        /// <summary>
        /// 信息体元素。
        /// </summary>
        public byte[] Element;

        /// <summary>
        /// 信息体时标。可选2、3、7个字节。
        /// </summary>
        public byte[] TimeStamp;


        /// <summary>
        /// 信息体总长度。
        /// </summary>
        public byte Length
        {
            get
            {
                byte c = 0;
                if (Addr != null) c +=(byte) Addr.Length;
                if (Element != null) c += (byte)Element.Length;
                if (TimeStamp != null) c += (byte)Element.Length;
                return c;
            }
        }

    }
}
