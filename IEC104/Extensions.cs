using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
   public static class  Extensions
    {
        public static bool Bit(this byte v, byte i)
        {
            return (v & (1 << i)) != 0;
        }

        public static byte SetBit(this byte v, byte i)
        {
            return (byte)( v | (1 << i));
        }

        public static byte ClearBit(this byte v ,byte i)
        {
            return (byte)(v & ~(1 << i));
        }

        public static byte Length(this ElementTypes t)
        {
            return Message.GetElementTypeLength(t);
        }
    }

}
