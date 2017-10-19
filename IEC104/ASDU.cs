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
        public byte[] Addr = new byte[2] { 0, 0 };

        /// <summary>
        /// 信息体集合
        /// </summary>
        public Message[] Messages;


        /// <summary>
        /// 离散或顺序信息排列方式
        /// </summary>
        public bool SQ
        {
            get => (VarConstrain & 0x80) != 0;
            set => VarConstrain = (byte)(value ? VarConstrain | 0x80 : VarConstrain & ~0x80);
        }

        /// <summary>
        /// 包含的信息体数量
        /// </summary>
        public byte MsgCount
        {
            get => (byte)(VarConstrain & ~0x80);
            set => VarConstrain = (byte)(SQ ? value | 0x80 : value & ~0x80);
        }

        /// <summary>
        /// 传送原因，COT的低6位。
        /// </summary>
        public byte Cause
        {
            get => (byte)(COT[0] & 0x3f);
            set => COT[0] = (byte)(COT[0] & 0xc0 + value & 0x3f);
        }

        /// <summary>
        /// 试验标志，COT的最高位。
        /// </summary>
        public bool Test
        {
            get => (COT[0] | 0x80) != 0;
            set => COT[0] = (byte)(value ? COT[0] | 0X80 : COT[0] & ~0X80);
        }

        /// <summary>
        /// 确认标志P/N，COT的次高位。
        /// </summary>
        public bool PN
        {
            get => (COT[0] | 0x40) != 0;
            set => COT[0] = (byte)(value ? COT[0] | 0X40 : COT[0] & ~0X40);
        }
    }
}
