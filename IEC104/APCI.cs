using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 应用规约控制信息。包含启动字符、APDU长度和控制域。 
    /// </summary>
    public class APCI
    {
        public const int Header = 0x68;

        /// <summary>
        /// 此类所代表的APCI结构的数组
        /// </summary>
        public byte[] Values = new byte[6] { Header, 0, 0, 0, 0, 0 };

        /// <summary>
        /// APDU的长度，从控制域到APDU末尾
        /// </summary>
        public byte APDULength
        {
            get => Values[1];
            set => Values[1] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL1
        {
            get => Values[2];
            set => Values[2] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL2
        {
            get => Values[3];
            set => Values[3] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL3
        {
            get => Values[4];
            set => Values[4] = value;
        }

        /// <summary>
        /// 控制域八位位组1
        /// </summary>
        public byte CTRL4
        {
            get => Values[5];
            set => Values[5] = value;
        }
    }
}
