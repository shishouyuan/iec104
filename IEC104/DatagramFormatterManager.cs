using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{
    public class DatagramFormatterManager
    {
        public readonly Dictionary<byte, DatagramFormatterBase> DefaultDatagrams = new Dictionary<byte, DatagramFormatterBase>();
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

        public ParseResult ParseAPDU(byte[] buf, Dictionary<byte, DatagramFormatterBase> datagrams = null)
        {
            if (datagrams == null)
                datagrams = DefaultDatagrams;
            ParseResult result;
            result.APDU = null;
            result.Datagram = null;
            try
            {
                DatagramFormatterBase datagram = null;
                var APDU = new APDU(buf);
                var ASDU = new ASDU(buf, APDU.APCILength);
                APDU.ASDU = ASDU;

                if (APDU.Format == DatagramFormat.InformationTransmit)
                {
                    if (datagrams.TryGetValue(ASDU.Type, out datagram))
                    {
                        byte bi = APDU.APCILength + ASDU.HeaderLength;
                        if (ASDU.SQ)
                        {
                            var m = new Message(datagram.ElementType, datagram.AddrLength, datagram.ExtraLength, datagram.TimeStampLength);
                            for (int i = 0; i < datagram.AddrLength; i++)
                                m.Addr[i] = buf[bi++];
                            for (int i = 0; i < datagram.ElementType.Length(); i++)
                                m.Element[i] = buf[bi++];
                            for (int i = 0; i < datagram.ExtraLength; i++)
                                m.Extra[i] = buf[bi++];
                            for (int i = 0; i < datagram.TimeStampLength; i++)
                                m.TimeStamp[i] = buf[bi++];
                            ASDU.Messages.Add(m);
                            var addr = m.Address;
                            for (int k = 1; k < ASDU.MsgCount; k++)
                            {
                                m = new Message(datagram.ElementType, 0, datagram.ExtraLength, datagram.TimeStampLength);
                                m.Address = ++addr;
                                for (int i = 0; i < datagram.ElementType.Length(); i++)
                                    m.Element[i] = buf[bi++];
                                for (int i = 0; i < datagram.ExtraLength; i++)
                                    m.Extra[i] = buf[bi++];
                                for (int i = 0; i < datagram.TimeStampLength; i++)
                                    m.TimeStamp[i] = buf[bi++];
                                ASDU.Messages.Add(m);
                            }
                        }
                        else
                        {
                            for (int k = 0; k < ASDU.MsgCount; k++)
                            {
                                var m = new Message(datagram.ElementType, datagram.AddrLength, datagram.ExtraLength, datagram.TimeStampLength);
                                for (int i = 0; i < datagram.AddrLength; i++)
                                    m.Addr[i] = buf[bi++];
                                for (int i = 0; i < datagram.ElementType.Length(); i++)
                                    m.Element[i] = buf[bi++];
                                for (int i = 0; i < datagram.ExtraLength; i++)
                                    m.Extra[i] = buf[bi++];
                                for (int i = 0; i < datagram.TimeStampLength; i++)
                                    m.TimeStamp[i] = buf[bi++];
                                ASDU.Messages.Add(m);
                            }
                        }
                    }


                }
                result.Datagram = datagram;
                result.APDU = APDU;
            }
            catch (Exception) { }
            return result;
        }
    }
}
