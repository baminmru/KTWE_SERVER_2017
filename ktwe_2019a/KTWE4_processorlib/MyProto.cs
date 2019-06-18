using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




namespace KTWE4
{



    //public class MyConstpublic UInt16
    //{
    //    public const int MSG_MAX_DATA_LENGTH = 255;
    //    public const int MAX_EPC_SEND = 16;
    //}



    //public class ETH_MSG	 {
    // 		//u32 sequence;
    // 		public UInt16 len;
    // 		public byte cmd;
    // 		public byte  [] data = new  byte[255];
    // } ;


    public enum MSGTYPE
    {
        MST_PNG,
        MST_OK,
        MST_ERROR,
        MST_TAG,
        MST_BLOCK,
        MST_INFO,
        MST_POWERINFO
    };


    public enum CMDTYPE
    {
        CMT_PING,
        CMT_TAG,
        CMT_WRITETAG,
        CMT_BLOCK,
        CMT_WRITEBLOCK,
        CMT_GPO,
        CMT_LED,
        CMT_VERSION,
        CMT_ID,
        CMT_GETPOWER,
        CMT_SETPOWER,
        CMT_INVENTORYMODE,
        CMT_MANUALMODE,
        CMT_WRITETAG2,
        CMT_SETSINGLEREADPOWER,
        CMT_SETSINGLEWRITEPOWER,
        CMT_RELAYON,
        CMT_PINON,
        CMT_SELECT,
        CMT_SETSERVER,
        CMT_ANT,
        CMT_LOCK,
        CMT_UNLOCK
    }

    public class EPCINFO{
    	public byte EPCSIZE;
        public byte ANTNUM;
        public UInt32  RSSI;
        public byte [] TAGDATA = new  byte [16];

        public String GetEPC()
        {
            String sEPC;
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < EPCSIZE; x++) sb.Append(TAGDATA[x].ToString("X2"));
            sEPC = sb.ToString();
            return sEPC;
        }
    } ;

    public class MSG_TAG_READ{
    	public MSGTYPE MSGTYPE;
    	public byte TAGCOUNT;
    	public EPCINFO [] TAGS  ;
    } ;

    public class MSG_ERRROR
    {
    	public MSGTYPE MSGTYPE;
    	public byte []ERR = new  byte [64];
    } ;

    public class MSG_OK{
    	public MSGTYPE MSGTYPE;
    } ;

    public class MSG_BLOCK_READ
    {
    	public MSGTYPE MSGTYPE;
    	public byte EPCSIZE;
    	public byte[] EPC =  new  byte [16];
    	public byte BANK;
    	public UInt16 OFFSET;
    	public byte LEN;
    	public byte[] DATA = new  byte[64];
    };


  

    public class CMD_TAG_READ
    {
    	public CMDTYPE CTYPE;
    	public byte FILTERSIZE;
    	 public byte[] FILTERDATA = new  byte [16];
    } ;

    public class CMD_TAG_WRITE{
    	public CMDTYPE CTYPE;
    public byte OLDEPCSIZE;
    	public byte []OLDEPC =new  byte[16];
    	public byte NEWEPCSIZE;
    	public byte []NEWEPC= new  byte[16];
    } ;


    public class CMD_BLOCK_WRITE{
    	public CMDTYPE CTYPE;
    	public byte EPCSIZE;
    	public byte []EPC = new  byte[16];
    	public byte BANK;
    	public UInt16 OFFSET;
    	public byte LEN;
    	public byte []DATA= new  byte[64];
    } ;

    public class CMD_BLOCK_READ{
    	public CMDTYPE CTYPE;
    	public byte EPCSIZE;
    	public byte []EPC = new  byte[16];
    	public byte BANK;
    	public UInt16 OFFSET;
    	public byte LEN;
    };

    public class CMD_GPO{
    	public CMDTYPE CTYPE;
    	public byte R1;
    	public byte R2;
    } ;

    public class CMD_LED{
    	public CMDTYPE CTYPE;
    	public byte L1;
    	public byte L2;
    	public byte L3;
    };

    public class CMD_TAG_WRITE2{
    	public CMDTYPE CTYPE;
    	public byte OLDEPCSIZE;
    	public byte []OLDEPC = new  byte[16];
    	public byte NEWEPCSIZE;
    	public byte []NEWEPC= new  byte[16];
    	public byte ANTEN;
    };


    public class CMD_SINGLEPOWER{
    	public CMDTYPE CTYPE;
    	public byte ANT;
    	public UInt16 NEWPOWER;
    };

    public class CMD_ON_INFO{
    	public CMDTYPE CTYPE;
    	public byte PIN;
    	public UInt16 DELAY;
    } ;

    public class CMD_SELECT{
    	public CMDTYPE CTYPE;
    	public byte ANT;
    	public byte BANK;
    	public UInt16 BITPOINTER;
    	public UInt16 MASKSIZE;
    	public byte []MASK = new  byte[16];
    };


    public class RawMessage
    {
        public static  MSG_TAG_READ TAGS(  byte[] Data,  int Size)
        {
            if (Size < 3) return null;

            if((MSGTYPE)Data[2] == MSGTYPE.MST_TAG)
            {
                MSG_TAG_READ tt = new MSG_TAG_READ();
                tt.MSGTYPE = MSGTYPE.MST_TAG;
                tt.TAGCOUNT = Data[3];
                int i;
                int j;
                tt.TAGS = new EPCINFO[tt.TAGCOUNT];
                for (i=0;i<tt.TAGCOUNT  && i < 16; i++)
                {
                    tt.TAGS[i] = new EPCINFO();
                    tt.TAGS[i].EPCSIZE = Data[4 + 22 * i];
                    tt.TAGS[i].ANTNUM  = Data[5 + 22 * i];
                    tt.TAGS[i].RSSI = BitConverter.ToUInt32(  Data, 6 + 22 * i);

                    for (j=0;j< tt.TAGS[i].EPCSIZE && j < 16; j++)
                    {
                        tt.TAGS[i].TAGDATA[j]  = Data[10 + 22 * i+ j];
                    }
                }
                return tt;
            }
            else
            {
                return null;
            }
        }


        public static MSG_BLOCK_READ  BLOCK(byte[] Data, int Size)
        {
            if (Size < 3) return null;

            if ((MSGTYPE)Data[2] == MSGTYPE.MST_BLOCK )
            {
                MSG_BLOCK_READ tt = new MSG_BLOCK_READ();
                tt.MSGTYPE = MSGTYPE.MST_BLOCK;
                tt.EPCSIZE  = Data[3];
                int i;
                int j;
                for (i = 0; i < tt.EPCSIZE && i < 16; i++)
                {
                    tt.EPC[i] = Data[4 + i];
                }
                tt.BANK = Data[20];
                tt.OFFSET = BitConverter.ToUInt16(Data, 21);
                tt.LEN = Data[23];

                
                for (j = 0; j < tt.LEN && j < 64; j++)
                {
                    tt.DATA[j] = Data[24 + j];
                }
                return tt;
            }
            else
            {
                return null;
            }
        }
    }



}
