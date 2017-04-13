using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Net;
using System.Xml;



    

public class AntInfo
{
    public String IP;
    public int Ant;
    public String OP;
    public String Place;
    public Int16 CacheTime;
    public String Type;
}


public class TInfo
{
    public List<Actions> Result;
    public DateTime RegTime;
    public string visitId;
}

public class PInfo
{
    public Int16 PassNum;
    public byte ANTNUM;
}


   
public class KTWEServer
{

    UdpClient server=null;
    public IPEndPoint ClientEndPoint=null;
       

    private Dictionary<String, AntInfo> A_Cache;

    private Dictionary<String, TInfo> T_Cache;

    private Dictionary<String, PInfo> P_Cache;

    private string GetMyDir()
    {
        String s;
        s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
        s = s.Substring(6);
        return s;
    }
    private string LogPath = "\\KTWE_log_";

    private void Console_WriteLine(string s)
    {

        Console.WriteLine( s );
    }
    public bool MiniLog = false;


    private void MiniLogString(string s, int Ant)
    {
        if (MiniLog == false)
        {
            LogString(s,Ant);
        }
        else
        {
            Console_WriteLine(s);
        }
    }

    private void LogString(string s,int Ant)
    {
        try
        {

            File.AppendAllText(GetMyDir()+ LogPath +  ClientEndPoint.Address.ToString() + "_" + ClientEndPoint.Port.ToString() + "_"+ DateTime.Now.ToString("yyyy_MM_dd") + ".txt", "'"+DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") + "',"+ Ant.ToString() +",'" + s + "'\r\n");
            Console_WriteLine( s + "; A=" + Ant.ToString() );
        }
        catch (System.Exception ex)
        {
            Console_WriteLine( s + ex.Message);
        }
    }


    private void LogBNK(string s)
    {
        try
        {

            File.AppendAllText(GetMyDir() + "\\" + PAYLOADFILE + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") + " " + s + "\r\n");
            Console_WriteLine(s);
        }
        catch (System.Exception ex)
        {
            Console_WriteLine(s + ex.Message);
        }
    }


    public DateTime lastInventory;
    public bool InventorySent = false;

        public KTWEServer(UdpClient uServer, IPEndPoint MyRemoteEndPoint, Int16 ClientPort)

        {
            MiniLogString ("New KTWEServer ",0);
            try
            {
                IPEndPoint endpoint = (IPEndPoint)MyRemoteEndPoint;
                ClientEndPoint = endpoint;
                ClientEndPoint.Port = ClientPort;
                //LogString("Server for Address:" + endpoint.Address + "   Port:" + endpoint.Port.ToString());
                server = uServer;
            }
            catch (System.Exception ex)
            {
                Console_WriteLine(ex.Message);
            }

            A_Cache = new Dictionary<String, AntInfo>();

            T_Cache=new Dictionary<String, TInfo>();

            P_Cache = new Dictionary<String, PInfo>();
            
        }

        /// <summary>
        /// Client SocketListener Destructor.
        /// </summary>
         ~KTWEServer(){
             A_Cache = null;
             T_Cache = null;
             P_Cache = null;
             MiniLogString("Delete KTWEServer",0);
         }

         public static byte[] S2B(string str)
         {
             System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
             return encoding.GetBytes(str);
         }


         #region "config"


         public String WEBURL;
         public string WEBUSER;
         public string WEBPASSWORD;

        public String WEBLOGURL;
        public string WEBLOGUSER;
        public string WEBLOGPASSWORD;

         public Int16 PAYLOAD;
         public Int16 PAYLOADBANK;
         public Int16 PAYLOADPASS;
         public Int16 PAYLOADOFFSET;
         public Int16 PAYLOADLEN;
         public string PAYLOADFILE = "BANK_";
         public bool ConfigOK = false;

         public void ReadConfig()
         {


             if (ConfigOK == false)
             {
                 XmlDocument xml; //As XmlDocument
                 String s;
                 s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                 s = s.Substring(6);
                 xml = new XmlDocument();
                 xml.Load(s + "\\KTWEConfig.xml");
                 XmlElement node; //As XmlElement
                 node = (XmlElement)xml.LastChild;

                 WEBLOGURL = node.Attributes.GetNamedItem("WEBLOGURL").Value;
                 WEBLOGUSER = node.Attributes.GetNamedItem("WEBLOGUSER").Value;
                 WEBLOGPASSWORD = node.Attributes.GetNamedItem("WEBLOGPASSWORD").Value;

                 WEBURL = node.Attributes.GetNamedItem("WEBURL").Value;
                 WEBUSER = node.Attributes.GetNamedItem("WEBUSER").Value;
                 WEBPASSWORD = node.Attributes.GetNamedItem("WEBPASSWORD").Value;
                 
                 try
                 {
                     PAYLOADFILE = node.Attributes.GetNamedItem("PAYLOADFILE").Value;
                 }
                 catch
                 {
                     PAYLOADFILE = "BANK_";
                 }
                 try
                 {
                     PAYLOAD = Int16.Parse(node.Attributes.GetNamedItem("PAYLOAD").Value);
                 }
                 catch
                 {
                     PAYLOAD = 0;
                 }

                 try
                 {
                     PAYLOADBANK = Int16.Parse(node.Attributes.GetNamedItem("PAYLOADBANK").Value);
                 }
                 catch
                 {
                     PAYLOADBANK = 1;
                 }

                 try
                 {
                     PAYLOADPASS = Int16.Parse(node.Attributes.GetNamedItem("PAYLOADPASS").Value);
                 }
                 catch
                 {
                     PAYLOADPASS = 1;
                 }


                 try
                 {
                     PAYLOADOFFSET = Int16.Parse(node.Attributes.GetNamedItem("PAYLOADOFFSET").Value);
                 }
                 catch
                 {
                     PAYLOADOFFSET = 0;
                 }

                 try
                 {
                     PAYLOADLEN = Int16.Parse(node.Attributes.GetNamedItem("PAYLOADLEN").Value);
                 }
                 catch
                 {
                     PAYLOADLEN = 0;
                 }


                 ConfigOK = true;
             }
         }


         #endregion "config"

      
                        

  


    private void WebLog(String Tag, String ReaderIP,  int Ant)
    {
        ReadConfig();


        if (WEBLOGURL == "") return;

        String addr;
        addr = WEBLOGURL;
        addr = addr + "?tag=" + Tag;
        addr = addr + "&ip=" + ReaderIP;
        addr = addr + "&ant=" + Ant.ToString();
        
        WebRequest wc = WebRequest.Create(addr);
        if (WEBLOGUSER != "")
        {
            wc.Credentials = new NetworkCredential(WEBLOGUSER, WEBLOGPASSWORD);
            wc.PreAuthenticate = true;
        }

        MiniLogString("WEBLOG CALL: " + addr, Ant);

        WebResponse wr;

        try
        {
            wr = wc.GetResponse();
        }
        catch (System.Exception ex)
        {
            wr = null;
        }

        if (wr != null)
        {
            Stream receiveStream = null; //As Stream = null
            try
            {
                receiveStream = wr.GetResponseStream();

            }
            catch (System.Exception ex)
            {
                receiveStream = null;
                MiniLogString("WEB Call Error. Bad stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message, Ant);
            }
            string sout;
            try
            {
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                sout = readStream.ReadToEnd();
               
                readStream.Close();
            }
            catch (System.Exception ex)
            {
                MiniLogString("WEB Call Error. Error while reading stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message, Ant);
            }

            wr.Close();

        }

    }

         private List<Actions> MakeRequest(String Tag, String ReaderIP,  int CacheTime, int Ant)
         {

            
             string tkey;
             TInfo ti;
             tkey = Tag + "_" + ReaderIP + "_" + Ant.ToString();

             if (T_Cache.ContainsKey(tkey)){
                 ti = T_Cache[tkey];
                
                 if (ti.RegTime.AddMinutes(CacheTime) >= DateTime.Now)
                 {
                     return ti.Result;
                 }
                 else
                 {
                    T_Cache.Remove(tkey);
                 }
             }
        

            String sVisitId = "";
             
            String s;
            s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            s = s.Substring(6);
          
            XmlElement node ; //As XmlElement


            ReadConfig(); 

         

            String addr ; 
            addr = WEBURL;
            addr = addr + "?tag=" + Tag;
            addr = addr + "&ip=" + ReaderIP;
            addr = addr + "&ant=" + Ant.ToString() ;
           

            List<Actions> t_results= new List<Actions>() ;
           
           String sout="" ; //As String = ""
           WebRequest wc = WebRequest.Create(addr);
            if (WEBUSER != "")
            {
            
                wc.Credentials = new NetworkCredential(WEBUSER, WEBPASSWORD);
                wc.PreAuthenticate = true;
            }
          
            MiniLogString ("WEB CALL: "+ addr,Ant);
           
            WebResponse wr ; 

            try{
                wr = wc.GetResponse();
            } catch(System.Exception ex){ ; //As Exception
                wr = null;
                MiniLogString("WEB Call error." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
            }

            if (wr != null ){
                Stream receiveStream = null; //As Stream = null
                try{
                    receiveStream = wr.GetResponseStream();
                } catch(System.Exception ex){ 
                    receiveStream = null;
                    MiniLogString("WEB Call Error. Bad stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
                }
                sout = "";
                if(receiveStream != null ){
                    try{
                        StreamReader readStream =new StreamReader(receiveStream, Encoding.UTF8);

                        sout = readStream.ReadToEnd();
                        wr.Close();
                        readStream.Close();
                    } catch(System.Exception ex){ 
                        MiniLogString("WEB Call Error. Error while reading stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
                    }

                    if (sout != "")
                    {
                        MiniLogString("\r\n  RESULT: " + sout,Ant);
                        XmlDocument xmlOut; //As XmlDocument
                        xmlOut = new XmlDocument();
                        XmlNodeList nodesOut = null; //As XmlNodeList

                        try
                        {
                            xmlOut.LoadXml(sout);
                            node = (XmlElement)xmlOut.LastChild;

                            nodesOut = node.GetElementsByTagName("action");
                            foreach(XmlElement node1 in nodesOut){
                                switch (node1.InnerText.ToLower())
                                {
                                    case "pin1":
                                        t_results.Add(Actions.CMD_PIN1);
                                        break;
                                    case "pin2":
                                        t_results.Add(Actions.CMD_PIN2);
                                        break;
                                    case "relay1":
                                        t_results.Add(Actions.CMD_RELAY1 );
                                        break;
                                    case "relay2":
                                        t_results.Add(Actions.CMD_RELAY1);
                                        break;
                                }
                            }

                            nodesOut = node.GetElementsByTagName("id");
                            if (nodesOut.Count > 0){
                                sVisitId = nodesOut.Item(0).InnerText;
                            }
                            else
                            {
                                sVisitId = new Guid().ToString();
                            }
                        
                        }
                        catch (System.Exception ex)
                        {

                            MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message, Ant);
                            nodesOut = null;
                            
                        }

                        ti = new TInfo();
                        ti.visitId = sVisitId;
                        ti.Result = t_results;
                        ti.RegTime = DateTime.Now;
                    try
                    {
                        T_Cache.Add(tkey, ti);
                    }
                    catch (System.Exception ex)
                    {
                        T_Cache.Remove(tkey);
                        T_Cache.Add(tkey, ti);
                    }


                } //sout
                }
            }
            return t_results;
         }



         //private CMD MakeReject(String Tag, String Oper, String Place, int CacheTime,int Ant)
         //{


         //    string tkey;
         //    TInfo ti;
         //    tkey = Tag + "_" + Oper + "_" + Place;

         //    if (T_Cache.ContainsKey(tkey))
         //    {
         //        ti = T_Cache[tkey];
         //    }
         //    else
         //    {
         //        MiniLogString("MakeReject No cahche for " + tkey, Ant);
         //        return CMD.CMD_DENY;
         //    }


            
         //    XmlDocument xml; //As XmlDocument
         //    String s;
         //    s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
         //    s = s.Substring(6);
         //    XmlElement node; //As XmlElement



         //    ReadConfig(); 




         //    String addr; //As String

           

         //   addr = WEBURLCAN;
         //   addr = addr + "?_service=" + SERVICENAMECAN;
         //   addr = addr + "&_version=" + WEBVERSIONCAN;
         //   addr = addr + "&session=" + SESSCAN;
         //   addr = addr + "&visitId=" + ti.visitId;
         //   addr = addr + "&id=" + WEBUSER;
         //   addr = addr + "&password=" + WEBPASSWORD;
         //   addr = addr + "&mode=" + MODE;

           


         
         //    String sout = ""; //As String = ""
         //    WebRequest wc = WebRequest.Create(addr);
         //    MiniLogString("\r\n CALL: " + addr,Ant);

         //    WebResponse wr; //As WebResponse

         //    try
         //    {
         //        wr = wc.GetResponse();
         //    }
         //    catch (System.Exception ex)
         //    {
         //        ; //As Exception
         //        wr = null;
         //        MiniLogString("WEB Call error." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
         //    }

         //    if (wr != null)
         //    {
         //        Stream receiveStream = null; //As Stream = null
         //        try
         //        {
         //            receiveStream = wr.GetResponseStream();
         //        }
         //        catch (System.Exception ex)
         //        {
         //            receiveStream = null;
         //            MiniLogString("WEB Call Error. Bad stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
         //        }
         //        sout = "";
         //        if (receiveStream != null)
         //        {
         //            try
         //            {
         //                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

         //                sout = readStream.ReadToEnd();
         //                wr.Close();
         //                readStream.Close();
         //            }
         //            catch (System.Exception ex)
         //            {
         //                MiniLogString("WEB Call Error. Error while reading stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
         //            }

         //            if (sout != "")
         //            {
         //                T_Cache.Remove(tkey);
         //                MiniLogString("\r\n  RESULT: " + sout,Ant);
         //                {

         //                    XmlDocument xmlOut; //As XmlDocument
         //                    xmlOut = new XmlDocument();
         //                    String sStatus = "";
         //                    try
         //                    {
         //                        xmlOut.LoadXml(sout);
         //                        node = (XmlElement)xmlOut.LastChild;
                              
         //                        try
         //                        {
         //                            sStatus = node.Attributes.GetNamedItem("status").Value;
         //                            if (sStatus != "yes")
         //                            {
                                        
         //                                return CMD.CMD_DENY;
         //                            }
         //                            else
         //                            {
         //                                return CMD.CMD_OK ;
         //                            }
         //                        }
         //                        catch
         //                        {
         //                            MiniLogString("Document has no status attribute, so return DENY\r\n",Ant );
         //                            return CMD.CMD_DENY;
         //                        }

         //                    }
         //                    catch (System.Exception ex)
         //                    {
                                 
         //                        MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message,Ant);
         //                        return CMD.CMD_DENY;
         //                    }

              

         //                }


         //            } //sout
         //        }
         //    }


         //    return CMD.CMD_DENY;
         //}



         private void ClearCache()
         {
             TInfo ti;
             try
             {
                 foreach (string tkey in T_Cache.Keys)
                 {
                     ti = T_Cache[tkey];
                     if (ti.RegTime.AddMinutes(5) < DateTime.Now)
                     {
                         try
                         {
                             T_Cache.Remove(tkey);
                         }
                         catch
                         {

                         }
                     }
                 }

             }
             catch { }
             
         }

         private  byte[] LasRequest;
         public void ProcessRequest(Byte[] request)
         {
             ThreadStart ts = new ThreadStart(ProcessThread);
             System.Threading.Thread th = new System.Threading.Thread(ts);
             LasRequest = request;
             th.Start(); 
         }

        public void ProcessThread(){
            this.ParseReceiveBuffer(LasRequest, LasRequest.Length);
        }

        public void InventoryON()
        {
        
            byte[] bsnd= new byte[3];
            bsnd[0] = 1;
            bsnd[1] = 0;
            bsnd[2] = (byte)CMDTYPE.CMT_INVENTORYMODE;


            //MiniLogString("Send InventoryON to " + ClientEndPoint.ToString() + "\r\n",0);

            try
            {
                lock (server)
                {
                    server.Send(bsnd, bsnd.Length, ClientEndPoint);
                }
                lastInventory = DateTime.Now;
                InventorySent = true;
            }
            catch (System.Exception se)
            {
                MiniLogString(se.Message + "\r\n",0);
            }
            
         
        }


        public void InventoryOFF()
        {
            byte[] bsnd = new byte[3];
            bsnd[0] = 1;
            bsnd[1] = 0;
            bsnd[2] = (byte)CMDTYPE.CMT_MANUALMODE;


            //MiniLogString("Send InventoryOFF to " + ClientEndPoint.ToString() + "\r\n",0);

            try
            {
                lock (server)
                {
                    server.Send(bsnd, bsnd.Length, ClientEndPoint);
                }
                lastInventory = DateTime.Now;
                InventorySent = false;
            }
            catch (System.Exception se)
            {
                MiniLogString(se.Message + "\r\n",0);
            }


        }


        public void Ping()
        {
            byte[] bsnd = new byte[3];
            bsnd[0] = 1;
            bsnd[1] = 0;
            bsnd[2] = (byte)CMDTYPE.CMT_PING;


            MiniLogString("Send Ping to " + ClientEndPoint.ToString() + "\r\n",0);

            try
            {
                lock (server)
                {
                    server.Send(bsnd, bsnd.Length, ClientEndPoint);
                }
            }
            catch (System.Exception se)
            {
                MiniLogString(se.Message + "\r\n",0);
            }


        }

        protected void ParseReceiveBuffer(Byte[] byteBuffer, int size)
        {

            ClearCache();

            MSGTYPE mt;
            if (size >= 3)
            {
                mt = (MSGTYPE)byteBuffer[2];
            }
            else
            {
                return;
            }

        if (mt == MSGTYPE.MST_TAG)
        {

            MSG_TAG_READ tr = RawMessage.TAGS(byteBuffer, size);
            int tIdx;


            String pkey;
            EPCINFO msg;
            String curEPC;
            String rIP= ClientEndPoint.Address.ToString();

            for (tIdx = 0; tIdx < tr.TAGCOUNT; tIdx++)
            {
                msg = tr.TAGS[tIdx];
                curEPC = tr.TAGS[tIdx].GetEPC();
                pkey = curEPC;

                WebLog(curEPC,rIP , msg.ANTNUM);
                MiniLogString(curEPC, msg.ANTNUM);
                List<Actions> results;
                results = MakeRequest(curEPC, rIP, 1, msg.ANTNUM);
                foreach( Actions A in results)
                {

                    if(A==Actions.CMD_PIN1)
                    {

                        MiniLogString(curEPC + "->" + "PIN1", msg.ANTNUM);
                        byte[] buf = new byte[6];
                        buf[0] = 4;
                        buf[1] = 0;
                        buf[2] = (byte)CMDTYPE.CMT_PINON;
                        buf[3] = 1;
                        buf[4] = 2000 % 256;
                        buf[5] = 2000 / 256;
                        lock (server)
                        {
                            server.Send(buf, 6, ClientEndPoint);
                        }
                        
                    }

                    if (A == Actions.CMD_PIN2)
                    {
                        MiniLogString(curEPC + "->" + "PIN2", msg.ANTNUM);

                        byte[] buf = new byte[6];
                        buf[0] = 4;
                        buf[1] = 0;
                        buf[2] = (byte)CMDTYPE.CMT_PINON;
                        buf[3] = 2;
                        buf[4] = 2000 % 256;
                        buf[5] = 2000 / 256;
                        lock (server)
                        {
                            server.Send(buf, 6, ClientEndPoint);
                        }

                    }


                    if (A == Actions.CMD_RELAY1 )
                    {

                        MiniLogString(curEPC + "->" + "RELAY1", msg.ANTNUM);

                        byte[] buf = new byte[6];
                        buf[0] = 4;
                        buf[1] = 0;
                        buf[2] = (byte)CMDTYPE.CMT_RELAYON;
                        buf[3] = 1;
                        buf[4] = 2000 % 256;
                        buf[5] = 2000 / 256;
                        lock (server)
                        {
                            server.Send(buf, 6, ClientEndPoint);
                        }

                    }

                    if (A == Actions.CMD_RELAY2)
                    {

                        MiniLogString(curEPC + "->" + "RELAY2", msg.ANTNUM);
                        byte[] buf = new byte[6];
                        buf[0] = 4;
                        buf[1] = 0;
                        buf[2] = (byte)CMDTYPE.CMT_RELAYON;
                        buf[3] = 2;
                        buf[4] = 2000 % 256;
                        buf[5] = 2000 / 256;
                        lock (server)
                        {
                            server.Send(buf, 6, ClientEndPoint);
                        }

                    }

                    if (A==Actions.CMD_BANK)
                    {
                        MiniLogString(curEPC + "->" + "BANK", msg.ANTNUM);
                        if (PAYLOAD == 1)
                        {
                            PInfo pi;
                            if (P_Cache.ContainsKey(pkey))
                            {
                                pi = P_Cache[pkey];
                                pi.PassNum += 1;
                            }
                            else
                            {
                                pi = new PInfo();
                                pi.PassNum = 0;
                                P_Cache.Add(pkey, pi);
                                LogBNK(String.Format("Tag:{0}", msg.GetEPC()));

                            }

                            if (pi.PassNum < PAYLOADPASS)
                            {
                               
                                byte[] buf = new byte[25];
                                buf[0] = 23;
                                buf[1] = 0;
                                buf[2] = (byte)CMDTYPE.CMT_BLOCK;
                                buf[3] = msg.EPCSIZE;
                                int i;
                                for (i = 0; i <= 15; i++) {
                                    if (i < msg.EPCSIZE)
                                    {
                                        buf[4 + i] = msg.TAGDATA[i]; 
                                    }
                                    else
                                    {
                                        buf[4 + i] = 0;
                                    }
                                }
                                buf[20] = (byte)PAYLOADBANK;
                                i = (Byte)(PAYLOADOFFSET + PAYLOADLEN * (pi.PassNum / 2));
                                buf[21] = (byte)(i % 256);
                                buf[22] = (byte)(i / 256);
                                buf[23] = (Byte)PAYLOADLEN;
                                buf[24] = msg.ANTNUM;

                                lock (server)
                                {
                                    server.Send(buf,25, ClientEndPoint);
                                }
                            }
                            else
                            {
                                P_Cache.Remove(pkey);
                            }
                           
                        }
                      

                    }
                }



      

                   


                   
                }


               
                // MiniLogString("** End parsing buffer **");


            }


        if(mt== MSGTYPE.MST_BLOCK)
        {
           MSG_BLOCK_READ tr = RawMessage.BLOCK(byteBuffer, size);
            String pkey;
            String curEPC;
            String rIP = ClientEndPoint.Address.ToString();
            curEPC = tr.GetEPC();
            pkey = curEPC ;
            if (PAYLOAD == 1)
            {


                PInfo pi;
                if (P_Cache.ContainsKey(pkey))
                {
                    pi = P_Cache[pkey];
                    pi.PassNum += 1;
                    LogBNK(String.Format("Tag:{0},BANK:{1},OFFSET:{2}, LEN:{3},DATA:{4},PASS:{5}", tr.GetEPC(), tr.BANK.ToString(), tr.OFFSET.ToString(), tr.LEN.ToString(), tr.GetBANK(), pi.PassNum.ToString()));


                    if (pi.PassNum < PAYLOADPASS)
                    {

                        byte[] buf = new byte[25];
                        buf[0] = 23;
                        buf[1] = 0;
                        buf[2] = (byte)CMDTYPE.CMT_BLOCK;
                        buf[3] = tr.EPCSIZE;
                        int i;
                        for (i = 0; i <= 15; i++)
                        {
                            if (i < tr.EPCSIZE)
                            {
                                buf[4 + i] = tr.EPC[i];
                            }
                            else
                            {
                                buf[4 + i] = 0;
                            }
                        }
                        buf[20] = (byte)PAYLOADBANK;
                        i = (Byte)(PAYLOADOFFSET + PAYLOADLEN * (pi.PassNum / 2));
                        buf[21] = (byte)(i % 256);
                        buf[22] = (byte)(i / 256);
                        buf[23] = (Byte)PAYLOADLEN;
                        buf[24] = pi.ANTNUM;

                        lock (server)
                        {
                            server.Send(buf, 25, ClientEndPoint);
                        }
                    }
                    else
                    {
                        P_Cache.Remove(pkey);
                    }
                }
                else
                {
                    LogBNK(String.Format("Tag:{0},BANK:{1},OFFSET:{2}, LEN:{3},DATA:{4},PASS:{5}", tr.GetEPC(), tr.BANK.ToString(), tr.OFFSET.ToString(), tr.LEN.ToString(), tr.GetBANK(), 0));
                }

            }

        }

        }

    }

