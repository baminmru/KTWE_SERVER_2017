using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Net;
using System.Xml;

namespace KTWE
{
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
        public CMD Result;
        public DateTime RegTime;
        public string visitId;
       
    }

   public class PInfo
   {
       public Int16 PassNum;
   }


    public class KTWEServer : TCPSocketListener
    {

     

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
        public bool MiniLog = true;


        private void MiniLogString(string s)
        {
            if (MiniLog == false)
            {
                LogString(s);
            }
            else
            {
                Console_WriteLine(s);
            }
        }

        private void LogString(string s)
        {
            try
            {

                File.AppendAllText(GetMyDir()+ LogPath + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") + " " + s + "\r\n");
                Console_WriteLine( s);
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

                File.AppendAllText(GetMyDir() +"\\"+  PAYLOADFILE  + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") + " " + s + "\r\n");
                Console_WriteLine(s);
            }
            catch (System.Exception ex)
            {
                Console_WriteLine(s + ex.Message);
            }
        }
        public KTWEServer(Socket clientSocket, Int16 CloseTime):base(clientSocket,CloseTime)
        {
            //rnd = new Random();
            LogString ("New KTWEServer ");
            try
            {
                IPEndPoint endpoint = (IPEndPoint)clientSocket.RemoteEndPoint;
                LogString(" for Address:" + endpoint.Address + "   Port:" + endpoint.Port.ToString());
            }
            catch (System.Exception ex)
            {
                Console_WriteLine(ex.Message);
            }

            A_Cache = new Dictionary<String, AntInfo>();

            T_Cache=new Dictionary<String, TInfo>();

            P_Cache = new Dictionary<String, PInfo>();
            InventoryON(); 
            
        }

        /// <summary>
        /// Client SocketListener Destructor.
        /// </summary>
         ~KTWEServer(){
             A_Cache = null;
             T_Cache = null;
             P_Cache = null;
             MiniLogString("Delete KTWEServer");
         }

         public static byte[] S2B(string str)
         {
             System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
             return encoding.GetBytes(str);
         }


         public DateTime lastInventory;
         public bool InventorySent = false;

         public void InventoryON()
         {
             Message sndMsg;
             sndMsg = new Message(CMD.CMD_INVENTORY_ON, 0);
             byte[] bsnd = sndMsg.ToByte();


             MiniLogString("Send InventoryON  \r\n");

             try
             {
                 lock (m_clientSocket)
                 {
                     m_clientSocket.Send(bsnd);
                 }
                 lastInventory = DateTime.Now;
                 InventorySent = true;
             }
             catch (System.Exception se)
             {
                 MiniLogString(se.Message + "\r\n");
             }


         }


         public void InventoryOFF()
         {
             Message sndMsg;
             sndMsg = new Message(CMD.CMD_INVENTORY_OFF, 0);
             byte[] bsnd = sndMsg.ToByte();


             MiniLogString("Send InventoryOFF  \r\n");

             try
             {
                 lock (m_clientSocket)
                 {
                     m_clientSocket.Send(bsnd);
                 }
                 lastInventory = DateTime.Now;
                 InventorySent = false;
             }
             catch (System.Exception se)
             {
                 MiniLogString(se.Message + "\r\n");
             }


         }


         public void Ping()
         {
             Message sndMsg;
             sndMsg = new Message(CMD.CMD_PING, 0);
             byte[] bsnd = sndMsg.ToByte();


             MiniLogString("Send Ping \r\n");

             try
             {
                 lock (m_clientSocket)
                 {
                     m_clientSocket.Send(bsnd);
                 }
             }
             catch (System.Exception se)
             {
                 MiniLogString(se.Message + "\r\n");
             }


         }


         private CMD  MakeDecision(string str, int Ant, bool Reject)
         {

             ReadConfig();
             try {
                 IPEndPoint endpoint = (IPEndPoint)m_clientSocket.RemoteEndPoint;
                 LogString("MakeDesision for IP:" + endpoint.Address + " Port:" + endpoint.Port.ToString() + " Reject:" + Reject.ToString ());

                 AntInfo AI;
                 String Key;
                 Key=endpoint.Address + "_" + Ant.ToString();
                 if (A_Cache.ContainsKey(Key))
                 {
                     AI = A_Cache[Key];
                     if (AI.Type == "R")
                     {
                         return CMD.CMD_GETBANK;
                     }
                     else
                     {
                         if (Reject)
                             return MakeReject(str, AI.OP, AI.Place, AI.CacheTime);
                         else
                             return MakeRequest(str, AI.OP, AI.Place, AI.CacheTime);
                     }
                     
                 }
                 else {
                     
                    String s;
                    s = GetMyDir();
        
                    XmlDocument Xml = new XmlDocument();
                    try{
                         Xml.Load(s + "\\Readers.xml");
                    }
           
                    catch{
                        LogString("Не найден файл конфигурации считывателей:" + s + "\\Readers.xml");
                        return CMD.CMD_DENY;
                    }

                  
                        XmlNodeList nodesOut  = null; //As XmlNodeList
                        try{
                            nodesOut = Xml.GetElementsByTagName("Ant");
                        } catch{ 
                            nodesOut = null;
                        }

                        if (nodesOut != null)
                        {
                            if (nodesOut.Count > 0)
                            {
                                XmlNode enode;
                                int en;

                                AI = new AntInfo(); 
                                for (en = 0; en < nodesOut.Count; en++)
                                {
                                    enode = nodesOut.Item(en);
                                    AI.IP = enode.Attributes.GetNamedItem("IP").Value;
                                    AI.Ant = Int32.Parse(enode.Attributes.GetNamedItem("Number").Value);
                                    AI.Type = enode.Attributes.GetNamedItem("Type").Value.ToUpper();
                                   
                                    
                                    if (AI.IP == endpoint.Address.ToString()  && AI.Ant == Ant)
                                    {
                                        AI.OP = enode.Attributes.GetNamedItem("Mode").Value;
                                        AI.Place = enode.Attributes.GetNamedItem("Place").Value;
                                        try
                                        {
                                            AI.CacheTime = Int16.Parse(enode.Attributes.GetNamedItem("Cache").Value);
                                        }
                                        catch
                                        {
                                            AI.CacheTime = 2;
                                        }
                                        A_Cache.Add(Key,AI);

                                        if (AI.Type == "R")
                                        {
                                            return CMD.CMD_GETBANK;
                                        }
                                        else
                                        {
                                            if (Reject)
                                                return MakeReject(str, AI.OP, AI.Place, AI.CacheTime);
                                            else
                                                return MakeRequest(str, AI.OP, AI.Place, AI.CacheTime);
                                        }

                                    }
                                }
                            }
                        }
                 }
                        

                
             }
             catch(System.Exception ex) {
                 Console_WriteLine(ex.Message);
             }
            
       
             return CMD.CMD_DENY;
         }


         #region "config"


         public String WEBURL;
         public string WEBUSER;
         public string WEBPASSWORD;
         public string WEBVERSION;
         public string SERVICENAME;
         public String WEBURLACC;
         public string SERVICENAMEACC;
         public string FROMBIBL;
         public string WEBVERSIONACC;

         public String WEBURLCAN;
         public string SERVICENAMECAN;
         public string MODE;
         public string SESSCAN;
         public string WEBVERSIONCAN;


         public Int16  PAYLOAD;
         public Int16 PAYLOADBANK;
         public Int16 PAYLOADPASS;
         public Int16 PAYLOADOFFSET;
         public Int16 PAYLOADLEN;
        public string PAYLOADFILE="BANK_";
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

                 WEBURLACC = node.Attributes.GetNamedItem("WEBURL_DOSTUP").Value;
                 SERVICENAMEACC = node.Attributes.GetNamedItem("SERVICENAME_DOSTUP").Value;
                 FROMBIBL = node.Attributes.GetNamedItem("FROMBIBL").Value;
                 WEBVERSIONACC = node.Attributes.GetNamedItem("WEBVERSION_DOSTUP").Value;

                 WEBURL = node.Attributes.GetNamedItem("WEBURL").Value;
                 SERVICENAME = node.Attributes.GetNamedItem("SERVICENAME").Value;
                 WEBUSER = node.Attributes.GetNamedItem("WEBUSER").Value;
                 WEBPASSWORD = node.Attributes.GetNamedItem("WEBPASSWORD").Value;
                 WEBVERSION = node.Attributes.GetNamedItem("WEBVERSION").Value;

                 WEBURLCAN = node.Attributes.GetNamedItem("WEBURL_CANCEL").Value;
                 SERVICENAMECAN = node.Attributes.GetNamedItem("SERVICENAME_CANCEL").Value;
                 MODE = node.Attributes.GetNamedItem("MODE").Value;
                 SESSCAN = node.Attributes.GetNamedItem("SESSION_CANCEL").Value;
                 WEBVERSIONCAN = node.Attributes.GetNamedItem("WEBVERSION_CANCEL").Value;
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


         private CMD MakeRequest(String Tag, String Oper, String Place, int CacheTime)
         {


             string tkey;
             TInfo ti;
             tkey = Tag + "_" + Oper + "_" + Place;

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
             XmlElement node; 
             String s;
             s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
             s = s.Substring(6);
             
             
             ReadConfig(); 

         

            String addr ; //As String

            if (Oper.ToLower() == "допуск" || Oper.ToLower()=="вернибилет" )
            {

                addr = WEBURLACC;
                addr = addr + "?_service=" + SERVICENAMEACC;
                addr = addr + "&_version=" + WEBVERSIONACC;
                addr = addr + "&session=1";
                addr = addr + "&code=" + Tag;
                addr = addr + "&id=" + WEBUSER;
                addr = addr + "&password=" + WEBPASSWORD;
                addr = addr + "&formBibl=" + FROMBIBL;


            }
            else
            {

            addr = WEBURL;
            addr = addr + "?_service="+SERVICENAME;
            addr = addr + "&_version=" + WEBVERSION;
            addr = addr + "&session=1";
            addr = addr + "&cod=" + Tag;
            addr = addr + "&operation=" + Oper;
            addr = addr + "&place=" + Place;
            addr = addr + "&id="+WEBUSER;
            addr = addr + "&password="+WEBPASSWORD ;
            }


            CMD t_resut=CMD.CMD_DENY ;
           
           String sout="" ; //As String = ""
           WebRequest wc = WebRequest.Create(addr);
           MiniLogString ("\r\n CALL: "+ addr);
           
            WebResponse wr ; //As WebResponse

            try{
                wr = wc.GetResponse();
            } catch(System.Exception ex){ ; //As Exception
                wr = null;
                MiniLogString("WEB Call error." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message);
            }

            if (wr != null ){
                Stream receiveStream = null; //As Stream = null
                try{
                    receiveStream = wr.GetResponseStream();
                } catch(System.Exception ex){ 
                    receiveStream = null;
                    MiniLogString("WEB Call Error. Bad stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message);
                }
                sout = "";
                if(receiveStream != null ){
                    try{
                        StreamReader readStream =new StreamReader(receiveStream, Encoding.UTF8);

                        sout = readStream.ReadToEnd();
                        wr.Close();
                        readStream.Close();
                    } catch(System.Exception ex){ 
                        MiniLogString("WEB Call Error. Error while reading stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message);
                    }

                    if (sout != "")
                    {
                        MiniLogString("\r\n  RESULT: " + sout);

                    
                    if (Oper.ToLower() == "вернибилет")
                    {
                    
                        XmlDocument xmlOut; //As XmlDocument
                        xmlOut = new XmlDocument();
                        XmlNodeList nodesOut = null; //As XmlNodeList
                     
                        try
                        {
                            xmlOut.LoadXml(sout);
                            node = (XmlElement)xmlOut.LastChild;


                            if (node.Attributes.GetNamedItem("whatThis").Value.ToString() == "READER")
                            {
                                    nodesOut = xmlOut.GetElementsByTagName("reader");
                                    node = (XmlElement)nodesOut.Item(0);
                            }
                            else
                            {
                                t_resut = CMD.CMD_DENY;
                            }

                            nodesOut = node.GetElementsByTagName("entry");
                          

                        }
                        catch (System.Exception ex)
                        {
                        
                            MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message);
                            nodesOut = null;
                            sVisitId = "";
                        }

                            if (nodesOut != null)
                            {
                                if (nodesOut.Count > 0)
                                {
                                    XmlNode enode;
                                    int en;
                                    String enStr = "";
                                    for (en = 0; en < nodesOut.Count; en++)
                                    {

                                        try
                                        {
                                            enode = nodesOut.Item(en);
                                            enStr = enode.InnerText;
                                            String[] splits;
                                            splits = enStr.Split(':');
                                            if (splits.GetUpperBound(0) == 1)
                                            {
                                                if (splits[0] == "FD")
                                                {
                                                    if (splits[1] != "01")
                                                    {
                                                        t_resut = CMD.CMD_OK;
                                                    }
                                                    else
                                                    {
                                                        t_resut = CMD.CMD_DENY;
                                                    }
                                                }
                                            }

                                        }
                                        catch (System.Exception ex)
                                        {
                                            
                                            MiniLogString("WEB Call Error. Error while parsing entry XML." + "\r\n" + " XML:" + nodesOut.Item(en).InnerText + "\r\n" + " Err:" + ex.Message);


                                        }
                                    }

                                }
                            }
                    }else if (Oper.ToLower() == "допуск"  )
                        {
                        XmlDocument xmlOut ; //As XmlDocument
                        xmlOut = new XmlDocument();
                        XmlNodeList nodesOut  = null; //As XmlNodeList
                        String sStatus = "";
                            try
                            {
                                xmlOut.LoadXml(sout);
                                node = (XmlElement)xmlOut.LastChild;

                                try
                                {
                                    if (node.Attributes.GetNamedItem("whatThis").Value.ToString() == "READER")
                                    {

                                         nodesOut = xmlOut.GetElementsByTagName("reader");
                                         node = (XmlElement)nodesOut.Item(0);
                                         sStatus = node.Attributes.GetNamedItem("penalty").Value;
                                         if (sStatus != "ШТРАФ")
                                         {
                                             MiniLogString("penalty=" + sStatus + "\r\n");
                                             t_resut = CMD.CMD_ALLOW ;
                                             nodesOut = null;
                                         }

                                    }
                                    else
                                    {
                                        t_resut = CMD.CMD_DENY;
                                    }
                              
                                }
                                catch(System.Exception ex0)
                                {
                                    MiniLogString("Error while access atribute, so return DENY\r\n"  +ex0.Message );
                                    t_resut = CMD.CMD_DENY;
                                    nodesOut = null;
                                }

                            }
                            catch (System.Exception ex)
                            {
                                t_resut = CMD.CMD_DENY; //As Exception
                                MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message);
                                nodesOut = null;
                            }
                        }
                        else
                        {

                            XmlDocument xmlOut; //As XmlDocument
                            xmlOut = new XmlDocument();
                            XmlNodeList nodesOut = null; //As XmlNodeList
                            String sStatus = "";
                            try
                            {
                            xmlOut.LoadXml(sout);
                            node = (XmlElement)xmlOut.LastChild;
                            nodesOut = xmlOut.GetElementsByTagName("entry");
                            try
                            {
                                sStatus = node.Attributes.GetNamedItem("status").Value;
                                if (sStatus != "yes")
                                {
                                        MiniLogString("status=" + sStatus + "\r\n");
                                    t_resut= CMD.CMD_DENY;
                                    nodesOut = null;
                                        sVisitId = "";
                                    }
                                    else
                                    {
                                        sVisitId = node.Attributes.GetNamedItem("visitId").Value;
                                }
                            }
                            catch
                            {
                                    MiniLogString("Document has no status attribute, so return DENY\r\n" );
                                t_resut = CMD.CMD_DENY;
                                nodesOut = null;
                                    sVisitId = "";
                            }
                           
                            }
                            catch (System.Exception ex)
                            {
                              
                                MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message);
                            nodesOut = null;
                                sVisitId = "";
                        }

                            if (nodesOut != null)
                            {
                                if (nodesOut.Count > 0)
                                {
                                XmlNode enode;
                                int en;
                                String enStr="";
                                    for (en = 0; en < nodesOut.Count; en++)
                                    {

                                        try
                                        {
                                        enode=nodesOut.Item(en);
                                        enStr=enode.InnerText ;
                                        //Console_WriteLine(enStr);
                                        String [] splits;
                                        splits=enStr.Split(':');
                                        if (splits.GetUpperBound(0) == 1)
                                        {
                                            if (splits[0] == "FD")
                                            {
                                                if (splits[1] != "01")
                                                {
                                                    t_resut = CMD.CMD_ALLOW;
                                                }
                                                else
                                                {
                                                    if (Oper=="ВЫХОД")
                                                        t_resut = CMD.CMD_ALLOW_GETCARD;
                                                    else
                                                        t_resut = CMD.CMD_ALLOW;
                                                }
                                            }
                                        }
                                    
                                        }
                                        catch (System.Exception ex)
                                        {
                                            
                                            MiniLogString("WEB Call Error. Error while parsing entry XML." + "\r\n" + " XML:" + nodesOut.Item(en).InnerText + "\r\n" + " Err:" + ex.Message);

                                   
                                    }
                                }
                               
                            }
                        }

                        }  // вход и т.п.


                    } //sout
                }
            }


            ti = new TInfo();
            ti.visitId = sVisitId;
            ti.Result = t_resut;
            ti.RegTime = DateTime.Now;
            T_Cache.Add(tkey, ti); 
           // MiniLogString("Desision=" +t_resut.ToString());
            return t_resut;
         }



         private CMD MakeReject(String Tag, String Oper, String Place, int CacheTime)
         {


             string tkey;
             TInfo ti;
             tkey = Tag + "_" + Oper + "_" + Place;

             if (T_Cache.ContainsKey(tkey))
             {
                 ti = T_Cache[tkey];
             }
             else
             {
                 MiniLogString("MakeReject No cahche for " + tkey);
                 return CMD.CMD_DENY;
             }



             ReadConfig(); 

            
             String s;
             XmlElement node; //As XmlElement
             s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
             s = s.Substring(6);
             //xml = new XmlDocument();
             //xml.Load(s + "\\KTWEConfig.xml");
             
             //node = (XmlElement)xml.LastChild;

           
             //string WEBUSER;
             //string WEBPASSWORD;
             //string WEBVERSION;
          
             //String WEBURLCAN;
             //string SERVICENAMECAN;
             //string MODE;
             //string SESSCAN;
             //string WEBVERSIONCAN;
             //WEBURLCAN = node.Attributes.GetNamedItem("WEBURL_CANCEL").Value;
             //SERVICENAMECAN = node.Attributes.GetNamedItem("SERVICENAME_CANCEL").Value;
             //MODE = node.Attributes.GetNamedItem("MODE").Value;
             //SESSCAN = node.Attributes.GetNamedItem("SESSION_CANCEL").Value;
             //WEBVERSIONCAN = node.Attributes.GetNamedItem("WEBVERSION_CANCEL").Value;

  
             //WEBUSER = node.Attributes.GetNamedItem("WEBUSER").Value;
             //WEBPASSWORD = node.Attributes.GetNamedItem("WEBPASSWORD").Value;
             //WEBVERSION = node.Attributes.GetNamedItem("WEBVERSION").Value;




             String addr; //As String

           

            addr = WEBURLCAN;
            addr = addr + "?_service=" + SERVICENAMECAN;
            addr = addr + "&_version=" + WEBVERSIONCAN;
            addr = addr + "&session=" + SESSCAN;
            addr = addr + "&visitId=" + ti.visitId;
            addr = addr + "&id=" + WEBUSER;
            addr = addr + "&password=" + WEBPASSWORD;
            addr = addr + "&mode=" + MODE;

           


         
             String sout = ""; //As String = ""
             WebRequest wc = WebRequest.Create(addr);
             MiniLogString("\r\n CALL: " + addr);

             WebResponse wr; //As WebResponse

             try
             {
                 wr = wc.GetResponse();
             }
             catch (System.Exception ex)
             {
                 ; //As Exception
                 wr = null;
                 MiniLogString("WEB Call error." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message);
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
                     MiniLogString("WEB Call Error. Bad stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message);
                 }
                 sout = "";
                 if (receiveStream != null)
                 {
                     try
                     {
                         StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                         sout = readStream.ReadToEnd();
                         wr.Close();
                         readStream.Close();
                     }
                     catch (System.Exception ex)
                     {
                         MiniLogString("WEB Call Error. Error while reading stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message);
                     }

                     if (sout != "")
                     {
                         T_Cache.Remove(tkey);

                         MiniLogString("\r\n  RESULT: " + sout);
                         {

                             XmlDocument xmlOut; //As XmlDocument
                             xmlOut = new XmlDocument();
                             String sStatus = "";
                             try
                             {
                                 xmlOut.LoadXml(sout);
                                 node = (XmlElement)xmlOut.LastChild;
                              
                                 try
                                 {
                                     sStatus = node.Attributes.GetNamedItem("status").Value;
                                     if (sStatus != "yes")
                                     {
                                        
                                         return CMD.CMD_DENY;
                                     }
                                     else
                                     {
                                         return CMD.CMD_OK ;
                                     }
                                 }
                                 catch
                                 {
                                     MiniLogString("Document has no status attribute, so return DENY\r\n" );
                                     return CMD.CMD_DENY;
                                 }

                             }
                             catch (System.Exception ex)
                             {
                                 
                                 MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message);
                                 return CMD.CMD_DENY;
                             }

              

                         }


                     } //sout
                 }
             }


             return CMD.CMD_DENY;
         }





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

         protected override void ParseReceiveBuffer(Byte[] byteBuffer, int size)
         {

             ClearCache();
             MiniLogString("** Start parsing buffer **");
             List<EPC_Tag> tags = Message.ParseMsg(byteBuffer, size);
             String pkey;



           
             foreach (EPC_Tag msg in tags)
             {
                 pkey = msg.GetEPC() + "_" + msg.ant.ToString();  

                if (msg.cmd == (int)CMD.EPC_MSG)
                {

                    MiniLogString("*** New Tag ***");
                 CMD cmd = CMD.CMD_ALLOW;
                
                 try
                 {
                        cmd = MakeDecision(msg.GetEPC(), msg.ant,false);
                        Message sndMsg;
                        if (cmd == CMD.CMD_GETBANK )
                        {

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
                                    sndMsg = new Message(cmd, 15);
                                    Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                                    sndMsg.Data[12] = (Byte)PAYLOADBANK;  // BANK EPC
                                    sndMsg.Data[13] = (Byte)(PAYLOADOFFSET + PAYLOADLEN * (pi.PassNum / 2));  // offset
                                    //sndMsg.Data[13] = (Byte)(PAYLOADOFFSET + (pi.PassNum ));  // offset

                                    sndMsg.Data[14] = (Byte)(PAYLOADLEN ); // len 
                                }
                                else
                                {
                                    cmd = CMD.CMD_OK;
                                    sndMsg = new Message(cmd, 12);
                                    Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                                }
                            }
                            else
                            {
                                cmd = CMD.CMD_OK;
                                sndMsg = new Message(cmd, 12);
                                Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                            }

                        }
                        else
                        {
                            sndMsg = new Message(cmd, 12);
                            Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                        }

                        byte[] bsnd = sndMsg.ToByte();

                     try
                     {
                         m_clientSocket.Send(bsnd);
                     }
                     catch (System.Exception se)
                     {
                            MiniLogString(se.Message + "\r\n");
                     }
                 }
                 catch (System.Exception se)
                 {
                        MiniLogString(se.Message + "\r\n");
                 }
                
                      

                    LogString(String.Format("Process Tag: {0}, Ant:{1}, Oper:{2}, Access:{3}", msg.GetEPC(), msg.ant.ToString() ,msg.cmd.ToString() , cmd.ToString()));
                 // answer

             }


            if (msg.cmd == (int)CMD.CANCEL_MSG)
                {
                        MiniLogString("*** Rejecting Tag ***");
                    CMD cmd = CMD.CMD_OK;
                    try
                    {
                        cmd = MakeDecision(msg.GetEPC(), msg.ant, true);

                        Message sndMsg = new Message(cmd, 12);
                        Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                        byte[] bsnd = sndMsg.ToByte();



                        try
                        {
                              m_clientSocket.Send(bsnd);
                        }
                        catch (System.Exception se)
                        {
                            MiniLogString(se.Message + "\r\n");
                        }
                    }
                    catch (System.Exception se)
                    {
                        MiniLogString(se.Message + "\r\n");
                    }

                   
                    LogString(String.Format("Reject Tag: {0}, Ant:{1}, Oper:{2}, Access:{3}", msg.GetEPC(), msg.ant.ToString(), msg.cmd.ToString(), cmd.ToString()));
                 
                    // answer
                }


                if (msg.cmd == (int)CMD.CMD_GETBANK)
                {
                    LogString(String.Format("Tag:{0},BANK:{1},OFFSET:{2}, LEN:{3}, DATA:{4}", msg.GetEPC(), msg.bank.ToString(), msg.boffset.ToString(), msg.blen.ToString(), msg.GetBank()));


                    CMD cmd = CMD.CMD_OK;

                    Message sndMsg;
                    if ( PAYLOAD == 1)
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
                            pi.PassNum = 1;
                            P_Cache.Add(pkey, pi);
                        }
                        
                        LogBNK(String.Format("Tag:{0},BANK:{1},OFFSET:{2}, LEN:{3},DATA:{4},PASS:{5}", msg.GetEPC(), msg.bank.ToString(), msg.boffset.ToString(), msg.blen.ToString(), msg.GetBank(),pi.PassNum.ToString()  ));
                        
                        if (pi.PassNum < PAYLOADPASS)
                        {
                            cmd = CMD.CMD_GETBANK;
                            sndMsg = new Message(cmd, 15);
                            Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                            sndMsg.Data[12] = (Byte)PAYLOADBANK;  // BANK EPC
                            sndMsg.Data[13] = (Byte)(PAYLOADOFFSET + PAYLOADLEN * (pi.PassNum / 2));  // offset
                            //sndMsg.Data[13] = (Byte)(PAYLOADOFFSET + (pi.PassNum));  // offset
                            sndMsg.Data[14] = (Byte)(PAYLOADLEN); // len 
                        }
                        else
                        {
                            P_Cache.Remove(pkey);  
                            sndMsg = new Message(cmd, 12);
                            Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                        }

                    }
                    else
                    {
                        sndMsg = new Message(cmd, 12);
                        Buffer.BlockCopy(msg.EPC, 0, sndMsg.Data, 0, 12);
                    }

                    byte[] bsnd = sndMsg.ToByte();


                    try
                    {

                        try
                        {
                            m_clientSocket.Send(bsnd);
                        }
                        catch (System.Exception se)
                        {
                            MiniLogString(se.Message + "\r\n");
                        }
                    }
                    catch (System.Exception se)
                    {
                        MiniLogString(se.Message + "\r\n");
                    }




                    // answer
                }
            }

             if (tags.Count == 0 && size > 12)
             {
                 MiniLogString("*** No Tags in message ***");
                 Message sndMsg = new Message(CMD.CMD_OK, 12);
                MiniLogString("Send CMD_OK");
                 byte[] bsnd = sndMsg.ToByte();
                 try
                 {
                     m_clientSocket.Ttl = 3;
                     m_clientSocket.SendTimeout = 1500;
                     m_clientSocket.Send(bsnd, SocketFlags.DontRoute);
                 }
                 catch (System.Exception se)
                 {

                     LogString("Send error:" + se.Message + "\r\n Try to send with default flsgs & ttl \r\n");
                     try
                     {
                         m_clientSocket.Ttl = 32;
                         m_clientSocket.Send(bsnd);
                         m_clientSocket.SendTimeout = 3000;
                     }
                     catch (System.Exception se2)
                     {
                    MiniLogString("Send error:" + se.Message + "\r\n Send \r\n");
                     }
                 }

             }
             MiniLogString("** End parsing buffer **");
         }
         protected override void ProcessClientData(String oneLine)
         {

                 MiniLogString(oneLine);
         }


       

    }
}
