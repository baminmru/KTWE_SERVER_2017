using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.IO;

  

namespace KTWE
{

   
	/// <summary>
	/// TCPServer is the Server class. When "StartServer" method is called
	/// this Server object tries to connect to a IP Address specified on a port
	/// configured. Then the server start listening for client socket requests.
	/// As soon as a requestcomes in from any client then a Client Socket 
	/// Listening thread will be started. That thread is responsible for client
	/// communication.
	/// </summary>
	public class TCPServer
	{

        //private static  Manager dbManager;
        //private static Session dbSession;
        //private static String dbSite;
        //private static String dbUser;
        //private static String dbPassword;
        private static String srvIP;
        Int16 srvPort;
        Int16 m_AutoCloseTime = 120;


		

		/// <summary>
		/// Local Variables Declaration.
		/// </summary>
		private TcpListener m_server = null;
		private bool m_stopServer=false;
		private bool m_stopPurging=false;
		private Thread m_serverThread = null;
		private Thread m_purgingThread = null;
		private ArrayList m_socketListenersList = null;


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

        private void LogString(string s)
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
		public TCPServer(String IP, Int16 Port, Int16 AutoCloseTime)
		{
            srvIP = IP;
            srvPort = Port;
            IPAddress aIP = IPAddress.Parse(srvIP); 
            IPEndPoint svrEndPoint =  new IPEndPoint(aIP, srvPort);
            Init(svrEndPoint);
            m_AutoCloseTime = AutoCloseTime;
		}
    

		/// <summary>
		/// Destructor.
		/// </summary>
		~TCPServer()
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
                if (m_server.Server != null)
                {
                    if (m_server.Server.IsBound)
                    {
                        if (m_serverThread!=null)
                        {
                            if (m_serverThread.IsAlive)
                            {
                                OK = true;
                            }
                            else
                            {
                                OK = false;
                            }
                        }
                        else
                        {
                            OK = false;
                        }
                        
                    }
                    else
                    {
                        OK = false;
                    }
                }
                else
                {
                    OK = false;
                }
                
            }

            //if (dbManager == null)
            //{
            //    OK = false;
            //}
            //if (dbSession == null)
            //{
            //    OK = false;
            //}
            //else
            //{
            //    if (dbSession.SessionID.Equals(Guid.Empty))
            //    {
            //        OK = false;
            //    }
            //    try{
            //        //LATIR.NamedValues nv ;
            //        //LATIR.NamedValue nvi;
            //        //DateTime  dd;
            //        //dd = System.DateTime.Now;
            //        //nv = new LATIR.NamedValues();
            //        //nvi = nv.Add("ServerTime", dd, System.Data.DbType.DateTime, System.Data.ParameterDirection.Output);
            //        //nvi.Size = 20;
            //        //dbSession.Exec("GetServerTime", new LATIR.NamedValues());
            //        dbSession.GetData("select 'OK' OK"); 
            //    }catch(System.Exception ex){
            //        LogString(ex.Message);  
            //        OK = false;
            //    }
            //}
            if (m_stopServer)
            {
                OK = false;
            }
            return OK;
        }
		/// <summary>
		/// Init method that create a server (TCP Listener) Object based on the
		/// IP Address and Port information that is passed in.
		/// </summary>
		/// <param name="ipNport"></param>
		private void Init(IPEndPoint ipNport)
		{
			try
			{
				m_server = new TcpListener(ipNport);
                //LogString("Listener stared"); 
			
			}
			catch(Exception e)
			{
				m_server=null;
                LogString(e.Message); 
			}
		}

		/// <summary>
		/// Method that starts TCP/IP Server.
		/// </summary>
		public void StartServer()
		{
            if (m_server != null)
            {
                // Create a ArrayList for storing SocketListeners before
                // starting the server.
                m_socketListenersList = new ArrayList();

                // Start the Server and start the thread to listen client 
                // requests.
                try
                {
                    m_server.Start();
                    m_serverThread = new Thread(new ThreadStart(ServerThreadStart));
                    m_serverThread.Start();

                    // Create a low priority thread that checks and deletes client
                    // SocktConnection objcts that are marked for deletion.
                    m_purgingThread = new Thread(new ThreadStart(PurgingThreadStart));
                    m_purgingThread.Priority = ThreadPriority.Lowest;
                    m_purgingThread.Start();
                }
                catch(System.Exception ex) {
                    LogString("Error while staring TCP Server :" +ex.Message);
                }
             
            }
            else
            {
                LogString("TCP Server is null");
            }
		}

		/// <summary>
		/// Method that stops the TCP/IP Server.
		/// </summary>
		public void StopServer()
		{
			if (m_server!=null)
			{
				// It is important to Stop the server first before doing
				// any cleanup. If not so, clients might being added as
				// server is running, but supporting data structures
				// (such as m_socketListenersList) are cleared. This might
				// cause exceptions.

				// Stop the TCP/IP Server.
				m_stopServer=true;
				m_server.Stop();

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
				m_stopPurging=true;
                if(m_purgingThread !=null){
				        m_purgingThread.Join(1000);
				        if (m_purgingThread.IsAlive)
				        {
					        m_purgingThread.Abort();
				        }
				        m_purgingThread=null;
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
			foreach (TCPSocketListener socketListener 
						 in m_socketListenersList)
			{
                try
                {

                    socketListener.StopSocketListener();
                }catch{
                }
			}
			// Remove all elements from the list.
			m_socketListenersList.Clear();
			m_socketListenersList=null;
		}

		/// <summary>
		/// TCP/IP Server Thread that is listening for clients.
		/// </summary>
		private void ServerThreadStart()
		{
			// Client Socket variable;
			Socket clientSocket = null;
			TCPSocketListener socketListener = null;
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
                    LogString(e1.Message);
                }
				try
				{
                    //LogString("AcceptSocket");
					// Wait for any client requests and if there is any 
					// request from any client accept it (Wait indefinitely).
					clientSocket = m_server.AcceptSocket();

                    //LogString("Create Listener");
					// Create a SocketListener object for the client.
					socketListener = new KTWEServer(clientSocket,m_AutoCloseTime);

					// Add the socket listener to an array list in a thread 
					// safe fashon.
					//Monitor.Enter(m_socketListenersList);
					lock(m_socketListenersList)
					{
                        //LogString("Register Listener");
						m_socketListenersList.Add(socketListener);
					}
					//Monitor.Exit(m_socketListenersList);

					// Start a communicating with the client in a different
					// thread.
                    //LogString("Start client thread");
					socketListener.StartSocketListener();
				}
				catch (System.Exception se)
				{
					m_stopServer = true;
                    LogString(se.Message);
				}
			}
		}

		/// <summary>
		/// Thread method for purging Client Listeneres that are marked for
		/// deletion (i.e. clients with socket connection closed). This thead
		/// is a low priority thread and sleeps for 10 seconds and then check
		/// for any client SocketConnection obects which are obselete and 
		/// marked for deletion.
		/// </summary>
		private void PurgingThreadStart()
		{
			while (!m_stopPurging)
			{
				ArrayList deleteList = new ArrayList();

				// Check for any clients SocketListeners that are to be
				// deleted and put them in a separate list in a thread sage
				// fashon.
				//Monitor.Enter(m_socketListenersList);
				lock(m_socketListenersList)
				{
					foreach (TCPSocketListener socketListener 
								 in m_socketListenersList)
					{
						if (socketListener.IsMarkedForDeletion())
						{
                            try
                            {
                                deleteList.Add(socketListener);
                                socketListener.StopSocketListener();
                            }
                            catch (System.Exception e2)
                            {
                                LogString(e2.Message);
                            }
						}
					}

					// Delete all the client SocketConnection ojects which are
					// in marked for deletion and are in the delete list.
					for(int i=0; i<deleteList.Count;++i)
					{
                        try
                        {
                            m_socketListenersList.Remove(deleteList[i]);
                        }
                        catch (System.Exception e3)
                        {
                            LogString(e3.Message);
                        }
						
					}
				}
				//Monitor.Exit(m_socketListenersList);

				deleteList=null;
				Thread.Sleep(10000);
			}
		}
       
	}
}
