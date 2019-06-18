using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Net;
using System.Xml;

namespace KTWE2
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

         private CMD  MakeDecision(string str, int Ant, bool Reject)
         {
             ReadConfig();
             try {
                 IPEndPoint endpoint = (IPEndPoint)ClientEndPoint;
                 LogString("MakeDesision for IP:" + endpoint.Address + " Port:" + endpoint.Port.ToString() + " Reject:" + Reject.ToString (),Ant);

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
                             return MakeReject(str, AI.OP, AI.Place, AI.CacheTime,Ant);
                         else
                             return MakeRequest(str, AI.OP, AI.Place, AI.CacheTime,Ant);
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
                        LogString("Не найден файл конфигурации считывателей:" + s + "\\Readers.xml",Ant);
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
                                    AI.Type = enode.Attributes.GetNamedItem("Type").Value.ToUpper() ;
                                    
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
                                                return MakeReject(str, AI.OP, AI.Place, AI.CacheTime,Ant);
                                            else
                                                return MakeRequest(str, AI.OP, AI.Place, AI.CacheTime,Ant);
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



         private CMD MakeRequest(String Tag, String Oper, String Place, int CacheTime, int Ant)
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
             XmlDocument xml ; //As XmlDocument
             String s;
             s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
             s = s.Substring(6);
          
            XmlElement node ; //As XmlElement


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
                addr = addr + "?_service=" + SERVICENAME;
                addr = addr + "&_version=" + WEBVERSION;
                addr = addr + "&session=1";
                addr = addr + "&cod=" + Tag;
                addr = addr + "&operation=" + Oper;
                addr = addr + "&place=" + Place;
                addr = addr + "&id=" + WEBUSER;
                addr = addr + "&password=" + WEBPASSWORD;
            }


            CMD t_resut=CMD.CMD_DENY ;
           
           String sout="" ; //As String = ""
           WebRequest wc = WebRequest.Create(addr);
           MiniLogString ("\r\n CALL: "+ addr,Ant);
           
            WebResponse wr ; //As WebResponse

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
                        
                            MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message,Ant);
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
                                            
                                            MiniLogString("WEB Call Error. Error while parsing entry XML." + "\r\n" + " XML:" + nodesOut.Item(en).InnerText + "\r\n" + " Err:" + ex.Message,Ant);


                                        }
                                    }

                                }
                            }
                    }else if (Oper.ToLower() == "допуск"  )
                        {
                            XmlDocument xmlOut; //As XmlDocument
                            xmlOut = new XmlDocument();
                            XmlNodeList nodesOut = null; //As XmlNodeList
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
                                             MiniLogString("penalty=" + sStatus + "\r\n",Ant);
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
                                    MiniLogString("Error while access atribute, so return DENY\r\n"  +ex0.Message,Ant );
                                    t_resut = CMD.CMD_DENY;
                                    nodesOut = null;
                                }

                            }
                            catch (System.Exception ex)
                            {
                                t_resut = CMD.CMD_DENY; //As Exception
                                MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message, Ant);
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
                                        MiniLogString("status=" + sStatus + "\r\n", Ant);
                                        t_resut = CMD.CMD_DENY;
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
                                    MiniLogString("Document has no status attribute, so return DENY\r\n", Ant);
                                    t_resut = CMD.CMD_DENY;
                                    nodesOut = null;
                                    sVisitId = "";
                                }

                            }
                            catch (System.Exception ex)
                            {
                              
                                MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message, Ant);
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
                                            //Console_WriteLine(enStr);
                                            String[] splits;
                                            splits = enStr.Split(':');
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
                                                        if (Oper == "ВЫХОД")
                                                            t_resut = CMD.CMD_ALLOW_GETCARD;
                                                        else
                                                            t_resut = CMD.CMD_ALLOW;
                                                    }
                                                }
                                            }

                                        }
                                        catch (System.Exception ex)
                                        {
                                            
                                            MiniLogString("WEB Call Error. Error while parsing entry XML." + "\r\n" + " XML:" + nodesOut.Item(en).InnerText + "\r\n" + " Err:" + ex.Message, Ant);


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



         private CMD MakeReject(String Tag, String Oper, String Place, int CacheTime,int Ant)
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
                 MiniLogString("MakeReject No cahche for " + tkey, Ant);
                 return CMD.CMD_DENY;
             }


            
             XmlDocument xml; //As XmlDocument
             String s;
             s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
             s = s.Substring(6);
             XmlElement node; //As XmlElement



             ReadConfig(); 




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
             MiniLogString("\r\n CALL: " + addr,Ant);

             WebResponse wr; //As WebResponse

             try
             {
                 wr = wc.GetResponse();
             }
             catch (System.Exception ex)
             {
                 ; //As Exception
                 wr = null;
                 MiniLogString("WEB Call error." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
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
                     MiniLogString("WEB Call Error. Bad stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
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
                         MiniLogString("WEB Call Error. Error while reading stream." + "\r\n" + " Query:" + addr + "\r\n" + " Err:" + ex.Message,Ant);
                     }

                     if (sout != "")
                     {
                         T_Cache.Remove(tkey);
                         MiniLogString("\r\n  RESULT: " + sout,Ant);
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
                                     MiniLogString("Document has no status attribute, so return DENY\r\n",Ant );
                                     return CMD.CMD_DENY;
                                 }

                             }
                             catch (System.Exception ex)
                             {
                                 
                                 MiniLogString("WEB Call Error. Error while parsing XML." + "\r\n" + " XML:" + sout + "\r\n" + " Err:" + ex.Message,Ant);
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

                for (tIdx = 0; tIdx < tr.TAGCOUNT; tIdx++)
                {
                    msg = tr.TAGS[tIdx];
                    pkey = tr.TAGS[tIdx].GetEPC() + "_" + tr.TAGS[tIdx].ANTNUM.ToString();

                    MiniLogString(msg.GetEPC(), msg.ANTNUM);

                    /*
                    if (msg.cmd == (int)CMD.EPC_MSG)
                    {

                        

                        CMD cmd = CMD.CMD_ALLOW;
                        try
                        {
                            cmd = MakeDecision(msg.GetEPC(),msg.ANTNUM, false);
                            Message sndMsg;
                            if (cmd == CMD.CMD_GETBANK)
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
                                        Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                                        sndMsg.Data[12] = (Byte)PAYLOADBANK;  // BANK EPC
                                        sndMsg.Data[13] = (Byte)(PAYLOADOFFSET + PAYLOADLEN * (pi.PassNum / 2));  // offset
                                        sndMsg.Data[14] = (Byte)PAYLOADLEN; // len 
                                    }
                                    else
                                    {
                                        cmd = CMD.CMD_OK;
                                        sndMsg = new Message(cmd, 12);
                                        Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                                    }
                                }
                                else
                                {
                                    cmd = CMD.CMD_OK;
                                    sndMsg = new Message(cmd, 12);
                                    Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                                }

                            }
                            else
                            {
                                sndMsg = new Message(cmd, 12);
                                Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                            }

                            byte[] bsnd = sndMsg.ToByte();


                            MiniLogString("Send " + bsnd.Length.ToString() + " bytes to " + ClientEndPoint.ToString() + "\r\n");

                            try
                            {
                                lock (server)
                                {
                                    server.Send(bsnd, bsnd.Length, ClientEndPoint);
                                }
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



                        LogString(String.Format("Process Tag: {0}, Ant:{1}, Oper:{2}, Access:{3}", msg.GetEPC(),msg.ANTNUM.ToString(), msg.cmd.ToString(), cmd.ToString()));
                        // answer

                    }

    */

                    /*
                    if (msg.cmd == (int)CMD.CANCEL_MSG)
                    {
                        MiniLogString("*** Rejecting Tag ***");
                        CMD cmd = CMD.CMD_OK;
                        try
                        {
                            cmd = MakeDecision(msg.GetEPC(),msg.ANTNUM, true);

                            Message sndMsg = new Message(cmd, 12);
                            Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                            byte[] bsnd = sndMsg.ToByte();



                            try
                            {
                                lock (server)
                                {
                                    server.Send(bsnd, bsnd.Length, ClientEndPoint);
                                }
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


                        LogString(String.Format("Reject Tag: {0}, Ant:{1}, Oper:{2}, Access:{3}", msg.GetEPC(),msg.ANTNUM.ToString(), msg.cmd.ToString(), cmd.ToString()));

                        // answer
                    }
                    */


                   /*
                    //if (msg.cmd == (int)CMD.CMD_GETBANK)
                    {
                        LogString(String.Format("Tag:{0},BANK:{1},OFFSET:{2}, LEN:{3}, DATA:{4}", msg.GetEPC(), msg.bank.ToString(), msg.boffset.ToString(), msg.blen.ToString(), msg.GetBank()));

                        CMD cmd = CMD.CMD_OK;
                        Message sndMsg;
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
                                pi.PassNum = 1;
                                P_Cache.Add(pkey, pi);
                            }

                            LogBNK(String.Format("Tag:{0},BANK:{1},OFFSET:{2}, LEN:{3},DATA:{4},PASS:{5}", msg.GetEPC(), msg.bank.ToString(), msg.boffset.ToString(), msg.blen.ToString(), msg.GetBank(), pi.PassNum.ToString()));

                            if (pi.PassNum < PAYLOADPASS)
                            {
                                cmd = CMD.CMD_GETBANK;
                                sndMsg = new Message(cmd, 15);
                                Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                                sndMsg.Data[12] = (Byte)PAYLOADBANK;  // BANK EPC
                                sndMsg.Data[13] = (Byte)(PAYLOADOFFSET + PAYLOADLEN * (pi.PassNum / 2));  // offset
                                sndMsg.Data[14] = (Byte)PAYLOADLEN; // len 
                            }
                            else
                            {
                                P_Cache.Remove(pkey);
                                sndMsg = new Message(cmd, 12);
                                Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                            }

                        }
                        else
                        {
                            sndMsg = new Message(cmd, 12);
                            Buffer.BlockCopy(msg.TAGDATA, 0, sndMsg.Data, 0, 12);
                        }

                        byte[] bsnd = sndMsg.ToByte();


                        try
                        {

                            try
                            {
                                lock (server)
                                {
                                    server.Send(bsnd, bsnd.Length, ClientEndPoint);
                                }
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
                       
                     } // end of GetBank
                     */
                }


                //if (tr.TAGCOUNT == 0 && size > 3)
                //{
                //    MiniLogString("*** No Tags in message ***");
                //    Message sndMsg = new Message(CMD.CMD_OK, 12);
                //    MiniLogString("Send CMD_OK");
                //    byte[] bsnd = sndMsg.ToByte();
                //    try
                //    {
                //        lock (server)
                //        {
                //            server.Send(bsnd, bsnd.Length, ClientEndPoint);
                //        }
                //    }
                //    catch (System.Exception se)
                //    {
                //        MiniLogString("Send error:" + se.Message + "\r\n Send \r\n");
                //    }
                //}

                // MiniLogString("** End parsing buffer **");


            }

        }

    }
}
