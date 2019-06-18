using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KTWE4
{
    class Message
    {
        public CMD cmd;
        public short len;
        public byte[] Data;
        public Message(CMD cmd, short len)
        {
            this.cmd = cmd;
            this.len = len;
            Data = new byte[len];
        }
        public String GetEPC()
        {
            String EPC;
            StringBuilder sb = new StringBuilder();
            for (int x = 1; x < len; x++) sb.Append(Data[x].ToString("X2"));
            EPC = sb.ToString();
            return EPC;
        }
        public int GetAntNum()
        {

            return Data[0];
        }
        public byte[] ToByte()
        {
            byte[] bt = new byte[8 + len];
            bt[0] = 0xDE;
            bt[1] = 0xAD;
            bt[2] = 0xFA;
            bt[3] = 0xCE;

            bt[5] = Convert.ToByte((short)cmd >> 8);
            bt[4] = Convert.ToByte((short)cmd);
            bt[7] = Convert.ToByte(len >> 8);
            bt[6] = Convert.ToByte(len);

            for (int i = 0; i < len; i++)
            {
                bt[i + 8] = Data[i];
            }
            return bt;
        }
        public int GetFoolLen()
        {
            return Data.Length + 8;
        }
        private static void Console_WriteLine(string s)
        {

            Console.WriteLine(DateTime.Now.ToString() + " " + s + "\r\n");
        }

        public static List<EPC_Tag> ParseMsg(byte[] bt, int count)
        {
            List<EPC_Tag> array = new List<EPC_Tag>();
            EPC_Tag msg = null;
            short cmd = 0, len = 0;
            int p = 0;
            if (bt.Length < count) return null;
            while (count > p)
            {
                if (bt[p] == 0xDE && bt[++p] == 0xAD && bt[++p] == 0xFA && bt[++p] == 0xCE)
                {

                    cmd += bt[++p];
                    cmd += Convert.ToInt16(bt[++p] << 8);

                    len = 0;
                    len += bt[++p];
                    len += Convert.ToInt16(bt[++p] << 8);

                    // CMD Message
                    if (cmd == (int)CMD.CMD_PING)
                    {
                        return array;
                    }


                    if (cmd == (int)CMD.CMD_INVENTORY_ON)
                    {
                        return array;
                    }

                    if (cmd == (int)CMD.CMD_INVENTORY_OFF)
                    {
                        return array;
                    }


                    if (cmd == (int)CMD.EPC_MSG)
                    {
                        short tag_cnt = (short)(len / 14);
                        Console_WriteLine("RCV: " + tag_cnt.ToString() + " TAGS");
                        for (int i = 0; i < tag_cnt; i++)
                        {
                            msg = new EPC_Tag();
                            Buffer.BlockCopy(bt, 8 + i * 14, msg.EPC, 0, 12);
                            msg.ant = bt[20 + i * 14];
                            msg.eventtype = bt[21 + i * 14];
                            msg.cmd = cmd;
                            Console_WriteLine("\r\n RCV: " + msg.GetEPC() + " at Ant:" + msg.ant.ToString());
                            array.Add(msg);
                        }


                    }
                    // Log Message
                    if (cmd == (int)CMD.LOG_MSG)
                    {
                        Console_WriteLine("CMD.LOG_MSG");

                    }
                    // etc Message
                    if (cmd == (int)CMD.CANCEL_MSG)
                    {
                        Console_WriteLine("CMD.CANCEL_MSG");

                        short tag_cnt = (short)(len / 14);
                        Console_WriteLine("RCV: " + tag_cnt.ToString() + " TAGS");
                        for (int i = 0; i < tag_cnt; i++)
                        {
                            msg = new EPC_Tag();
                            Buffer.BlockCopy(bt, 8 + i * 14, msg.EPC, 0, 12);
                            msg.ant = bt[20 + i * 14];
                            msg.eventtype = bt[21 + i * 14];
                            msg.cmd = cmd;
                            Console_WriteLine("\r\n RCV: " + msg.GetEPC() + " at Ant:" + msg.ant.ToString());
                            array.Add(msg);
                        }
                    }



                    if (cmd == (int)CMD.CMD_GETBANK)
                    {
                        Console_WriteLine("CMD.CMD_GETBANK");

                        short cPos = 8;

                        while (cPos < len + 8)
                        {
                            msg = new EPC_Tag();
                            msg.cmd = cmd;
                            Buffer.BlockCopy(bt, cPos, msg.EPC, 0, 12); cPos += 12;
                            msg.ant = bt[cPos]; cPos++;
                            msg.eventtype = bt[cPos]; cPos++;
                            msg.bank = bt[cPos]; cPos++;
                            msg.boffset = bt[cPos]; cPos++;
                            msg.blen = bt[cPos]; cPos++;
                            if (msg.blen > 0)
                            {
                                msg.bdata = new byte[msg.blen * 2];
                                Buffer.BlockCopy(bt, cPos, msg.bdata, 0, msg.blen * 2);
                                cPos += msg.blen;
                                cPos += msg.blen;
                            }
                            Console_WriteLine("\r\n RCV BNK : " + msg.GetEPC() + " at Ant:" + msg.ant.ToString());
                            array.Add(msg);
                        }
                    }



                    p = p + 8 + len;
                }
                else return array;

            }

            return array;
        }
    }

    class EPC_Tag
    {
        public byte[] EPC;
        public short ant;
        public short eventtype;
        public short cmd;

        public short bank;
        public short boffset;
        public short blen;
        public byte[] bdata;

        public EPC_Tag()
        {
            ant = 0;
            eventtype = 0;
            EPC = new byte[12];
            bdata = null;
            blen = 0;
            bank = 0;
            boffset = 0;
        }
        public String GetEPC()
        {
            String sEPC;
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < EPC.Length; x++) sb.Append(EPC[x].ToString("X2"));
            sEPC = sb.ToString();
            return sEPC;
        }
        public byte[] GetEPCByte()
        {
            return EPC;
        }

        public byte[] GetBankByte()
        {
            return bdata;
        }

        public String GetBank()
        {
            String sBNK;
            StringBuilder sb = new StringBuilder();
            if (bdata != null)
            {
                for (int x = 0; x < bdata.Length; x++) sb.Append(bdata[x].ToString("X2"));
                sBNK = sb.ToString();
                return sBNK;
            }
            return "";
        }

        public bool HasBANK()
        {
            if (blen > 0 && bdata != null)
                return true;
            else
                return false;
        }
    }

    public enum CMD
    {
        EPC_MSG = 0,
        CMD_OK = 1,
        CMD_ALLOW = 2,
        CMD_DENY = 3,
        LOG_MSG = 4,
        CMD_ALLOW_GETCARD = 4,
        CANCEL_MSG = 5,
        CMD_GETBANK = 6,
        CMD_INVENTORY_ON = 7,
        CMD_INVENTORY_OFF = 8,
        CMD_PING = 9
    }
}
