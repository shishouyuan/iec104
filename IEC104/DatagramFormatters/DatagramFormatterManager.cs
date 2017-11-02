using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    /// <summary>
    /// 报文格式器管理器，利用反射从程序集加载所有格式器，使用相应的格式器解析报文。
    /// </summary>
    public class DatagramFormatterManager
    {
        /// <summary>
        /// 所有格式器的词典，键为格式器对应的类型号。
        /// </summary>
        public readonly Dictionary<byte, DatagramFormatterBase> DefaultDatagrams = new Dictionary<byte, DatagramFormatterBase>();
        /// <summary>
        /// 从程序集加载所有格式器到DefaultDatagrams。
        /// </summary>
        public void LoadFromAssembly()
        {
            DefaultDatagrams.Clear();
            var q = from i in typeof(DatagramFormatterBase).Assembly.GetTypes()
                    where i.IsSubclassOf(typeof(DatagramFormatterBase)) && !i.IsAbstract
                    select i;

            foreach (var i in q)
            {
                try
                {
                    var c = i.GetConstructor(new Type[] { typeof(byte) });
                    DatagramFormatterBase d = (DatagramFormatterBase)c.Invoke(new object[] { c.GetParameters().First().DefaultValue }); //Activator.CreateInstance(i) ;
                    DefaultDatagrams.Add(d.ASDUType, d);
                }
                catch (Exception) { }
            }
        }

        public DatagramFormatterManager()
        {
            LoadFromAssembly();
        }

        /// <summary>
        /// 获取对应格式器的实例。
        /// </summary>
        /// <param name="t">格式器的类型</param>
        /// <returns></returns>
        public DatagramFormatterBase GetInstance(Type t)
        {
            var q = from i in DefaultDatagrams.Values
                    where i.GetType() == t
                    select i;
            var e = q.GetEnumerator();
            if (e.MoveNext())
                return e.Current;
            return null;
        }

        public struct ParseResult
        {
            public DatagramFormatterBase Datagram;
            public APDU APDU;
        }

        /// <summary>
        /// 使用默认或指定的格式器组解析报文。
        /// </summary>
        /// <param name="buf">报文数据</param>
        /// <param name="formatters">指定格式器组</param>
        /// <returns></returns>
        public ParseResult ParseAPDU(byte[] buf, Dictionary<byte, DatagramFormatterBase> formatters = null)
        {
            if (formatters == null)
                formatters = DefaultDatagrams;
            ParseResult result;
            result.APDU = null;
            result.Datagram = null;
            try
            {
                DatagramFormatterBase formatter = null;
                var APDU = new APDU(buf);

                if (APDU.Format == DatagramFormat.InformationTransmit)
                {
                    var ASDU = new ASDU(buf, APDU.APCILength);
                    APDU.ASDU = ASDU;
                    if (formatters.TryGetValue(ASDU.Type, out formatter))
                    {
                        APDU.Formatter = formatter;
                        byte bi = APDU.APCILength + ASDU.HeaderLength;
                        if (ASDU.SQ)
                        {
                            var m = new Message(formatter.ElementType, formatter.AddrLength, formatter.ExtraLength, formatter.TimeStampLength);
                            for (int i = 0; i < formatter.AddrLength; i++)
                                m.Addr[i] = buf[bi++];
                            for (int i = 0; i < formatter.ElementType.Length(); i++)
                                m.Element[i] = buf[bi++];
                            for (int i = 0; i < formatter.ExtraLength; i++)
                                m.Extra[i] = buf[bi++];
                            for (int i = 0; i < formatter.TimeStampLength; i++)
                                m.TimeStamp[i] = buf[bi++];
                            ASDU.Messages.Add(m);
                            var addr = m.Address;
                            for (int k = 1; k < ASDU.MsgCount; k++)
                            {
                                m = new Message(formatter.ElementType, 0, formatter.ExtraLength, formatter.TimeStampLength);
                                m.Address = ++addr;
                                for (int i = 0; i < formatter.ElementType.Length(); i++)
                                    m.Element[i] = buf[bi++];
                                for (int i = 0; i < formatter.ExtraLength; i++)
                                    m.Extra[i] = buf[bi++];
                                for (int i = 0; i < formatter.TimeStampLength; i++)
                                    m.TimeStamp[i] = buf[bi++];
                                ASDU.Messages.Add(m);
                            }
                        }
                        else
                        {
                            for (int k = 0; k < ASDU.MsgCount; k++)
                            {
                                var m = new Message(formatter.ElementType, formatter.AddrLength, formatter.ExtraLength, formatter.TimeStampLength);
                                for (int i = 0; i < formatter.AddrLength; i++)
                                    m.Addr[i] = buf[bi++];
                                for (int i = 0; i < formatter.ElementType.Length(); i++)
                                    m.Element[i] = buf[bi++];
                                for (int i = 0; i < formatter.ExtraLength; i++)
                                    m.Extra[i] = buf[bi++];
                                for (int i = 0; i < formatter.TimeStampLength; i++)
                                    m.TimeStamp[i] = buf[bi++];
                                ASDU.Messages.Add(m);
                            }
                        }
                    }


                }
                result.Datagram = formatter;
                result.APDU = APDU;
            }
            catch (Exception) { }
            return result;
        }
    }
}
