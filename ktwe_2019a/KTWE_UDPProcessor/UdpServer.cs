using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace KTWE2
{
 

	public class UDPServer
	{


        private static String srvIP;
        Int16 srvPort;
        Int16 m_clientPort;
        Int16 m_AutoCloseTime = 120;

       
		

		/// <summary>
		/// Local Variables Declaration.
		/// </summary>
		private UdpClient  m_server = null;
		private bool m_stopServer=false;
	    private Thread m_serverThread = null;
	    private Dictionary<String,KTWEServer>  m_Processor = null;


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

            Console.WriteLine(DateTime.Now.ToString() + " " + s + "\r\n");
        }

        private void ULogString(string s)
        {
            try
            {

                File.AppendAllText(GetMyDir() + LogPath + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") + " " + s + "\r\n");
                Console_WriteLine(s);
            }
            catch (System.Exception ex)
            {
                Console_WriteLine( s + ex.Message);
            }
        }
        
        /// <summary>
		/// Constructors.
		/// </summary>
		public UDPServer(String IP, Int16 Port, Int16 ClientPort)
		{
            srvIP = IP;
            srvPort = Port;
            IPAddress aIP = IPAddress.Parse(srvIP); 
            IPEndPoint svrEndPoint =  new IPEndPoint(aIP, srvPort);
            Init(svrEndPoint);
            m_clientPort = ClientPort;
		}
    

		/// <summary>
		/// Destructor.
		/// </summary>
		~UDPServer()
		{
            StopServer();
            //if(dbSession!=null)
            //    dbSession.Logout();
            //dbManager = null;
			
		}

        public bool IsLive()
        {
            bool OK = true;

            if (m_server == null)
            {
                OK = false;
            }
            else
            {
          
                                OK = true;
               
                
            }

            if (m_stopServer)
            {
                OK = false;
            }
            return OK;
        }
		
		private void Init(IPEndPoint ipNport)
		{
			try
			{
                m_server = new UdpClient(ipNport );
			   
			}
			catch(Exception e)
			{
				m_server=null;
                ULogString(e.Message); 
			}
		}

		/// <summary>
		/// Method that starts UDP Server.
		/// </summary>
		public void StartServer()
		{
            if (m_server != null)
            {
              
                m_Processor = new Dictionary<String,KTWEServer>();

                // Start the Server and start the thread to listen client 
                // requests.
                try
                {
                  
                    m_serverThread = new Thread(new ThreadStart(ServerThreadStart));
                    m_serverThread.Start();

                 
                }
                catch(System.Exception ex) {
                    ULogString("Error while staring UDP Server :" +ex.Message);
                }
             
            }
            else
            {
                ULogString("UDP Server is null");
            }
		}

		/// <summary>
		/// Method that stops the UDP/IP Server.
		/// </summary>
		public void StopServer()
		{
			if (m_server!=null)
			{
			

				// Stop the UDP/IP Server.
				m_stopServer=true;
                m_server.Close(); 

                if (m_serverThread != null) { 
				    // Wait for one second for the the thread to stop.
				    m_serverThread.Join(1000);
				
				    // If still alive; Get rid of the thread.
				    if (m_serverThread.IsAlive)
				    {
					    m_serverThread.Abort();
				    }
				    m_serverThread=null;
                }
			

				// Free Server Object.
				m_server = null;

				// Stop All clients.
				StopAllSocketListers();
			}
		}


		/// <summary>
		/// Method that stops all clients and clears the list.
		/// </summary>
		private void StopAllSocketListers()
		{
            //foreach (UDPSocketListener myProcessor 
            //             in m_Processor)
            //{
            //    try
            //    {

            //        myProcessor.StopSocketListener();
            //    }catch{
            //    }
            //}
            //// Remove all elements from the list.
			m_Processor.Clear();
			m_Processor=null;
		}

		/// <summary>
		/// UDP/IP Server Thread that is listening for clients.
		/// </summary>
		private void ServerThreadStart()
		{
		
            while(!m_stopServer)
			{
                try
                {
                    if (!IsLive())
                    {
                        m_stopServer = true;
                    }
                }
                catch(System.Exception e1){
                    m_stopServer = true;
                    ULogString(e1.Message);
                }

				try
				{
                   
                   IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                   //ULogString("Call from :" + RemoteIpEndPoint.ToString());
                   Byte []request ;
					request = m_server.Receive(ref RemoteIpEndPoint);
                    String addr;
                    addr=RemoteIpEndPoint.Address.ToString().ToLower();

                    KTWEServer myProcessor = null;
                    if (m_Processor.ContainsKey(addr))
                    {
                        myProcessor = m_Processor[addr];
                    }
                    else
                    {
                        myProcessor = new KTWEServer(m_server, RemoteIpEndPoint,m_clientPort);
                       
                        lock (m_Processor)
                        {
                            m_Processor.Add(addr, myProcessor);
                        }
                       
                    }

                    if (!myProcessor.InventorySent && myProcessor.lastInventory.AddSeconds(10) < DateTime.Now )
                    {
                        myProcessor.InventoryON();
                
                    }


                

					myProcessor.ProcessRequest(request);

				}
				catch (System.Exception se)
				{
					m_stopServer = true;
                    ULogString(se.Message);
				}
			}
		}
 	}
}
