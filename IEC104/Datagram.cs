using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shouyuan.IEC104
{

    public abstract class Datagram
    {
        public byte ASDUType { get; }

        public ElementType ElementType { get; }
        public byte ExtraLength { get; }
        public byte TimeStampLength { get; }
        public byte AddrLength { get; }

        public abstract byte DefaultASDUType { get; }


        protected Datagram(byte atype, ElementType etype, byte extral, byte tsl = 0, byte addrl = 3)
        {
            ASDUType = atype;
            ElementType = etype;
            ExtraLength = extral;
            TimeStampLength = tsl;
            AddrLength = addrl;
        }


        protected virtual Message CreateNewMessageWithAddr()
        {
            return new Message(ElementType, AddrLength, ExtraLength, TimeStampLength);
        }

        protected virtual Message CreateNewMessageNoAddr()
        {
            return new Message(ElementType, 0, ExtraLength, TimeStampLength);
        }

        protected virtual Message CreateMessageForAPDU(APDU apdu, uint addr)
        {
            Message m;
            if (addr == 0)
            {
                if (!apdu.ASDU.SQ || apdu.ASDU.ActualMsgCount == 0)
                    throw new Exception("非顺序信息体或首个信息体地址不能为0");
                m = CreateNewMessageNoAddr();
            }
            else
            {
                if (apdu.ASDU.SQ && apdu.ASDU.ActualMsgCount > 0)
                {
                    if (addr == apdu.ASDU.Messages.Last().Address + 1)
                        m = CreateNewMessageNoAddr();
                    else
                        throw new Exception("顺序信息体地址不连续");
                }
                else
                {
                    m = CreateNewMessageWithAddr();
                    m.Address = addr;
                }
            }
            return m;
        }

        public virtual APDU CreateAPDU(byte asduAddr, bool sq = false, bool pn = false, bool test = false)
        {
            var v = new APDU();
            v.ASDU = new ASDU();
            v.ASDU.Address = asduAddr;
            v.ASDU.SQ = sq;
            v.ASDU.PN = pn;
            v.ASDU.Test = test;
            return v;
        }


        public static readonly Dictionary<byte, Datagram> DefaultDatagrams;
        static Datagram()
        {
            DefaultDatagrams = new Dictionary<byte, Datagram>();

            var a = typeof(Datagram).Assembly;


            var q = from i in typeof(Datagram).Assembly.GetTypes()
                    where i.IsSubclassOf(typeof(Datagram)) && !i.IsAbstract
                    select i;

            foreach (var i in q)
            {
                try
                {
                    Datagram d = (Datagram)i.CONSTRU.Invoke(null);
                    DefaultDatagrams.Add(d.ASDUType, d);
                }
                catch (Exception er) { }
            }
        }

        public struct ParseResult
        {
            public Datagram Datagram;
            public APDU APDU;
        }

        public static ParseResult ParseAPDU(byte[] buf, Dictionary<byte, Datagram> datagrams = null)
        {
            if (datagrams == null)
                datagrams = DefaultDatagrams;
            ParseResult result;
            result.APDU = null;
            result.Datagram = null;
            try
            {
                Datagram datagram = null;
                var APDU = new APDU(buf);
                var ASDU = new ASDU(buf, APDU.APCILength);
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

    public class M_ME_NA_1 : Datagram
    {

        public static M_ME_NA_1 SharedInstance;

        public const byte defaultASDUType = 9;
        public override byte DefaultASDUType => defaultASDUType;


        public M_ME_NA_1(byte type = defaultASDUType) : base(type, ElementType.NVA, 1, 0)
        {
            if (SharedInstance == null)
                SharedInstance = this;
            //ASDUType = 9;
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
