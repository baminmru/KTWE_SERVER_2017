using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;


namespace KTWE
{
    /// <summary>
    /// Summary description for TCPSocketListener.
    /// </summary>
    public class TCPSocketListener
    {


        private string GetMyDir()
        {
            String s;
            s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            s = s.Substring(6);
            return s;
        }
        private string LogPath = "\\KTWE_log_";

        private void Console_WriteLine( string s){

            Console.WriteLine( DateTime.Now.ToString() + " " + s + "\r\n");
       }
        private void LogString(string s)
        {
            try
            {

                File.AppendAllText(GetMyDir() + LogPath + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", DateTime.Now.ToString() + " " + s + "\r\n");
                Console_WriteLine(s);
            }
            catch (System.Exception ex)
            {
                Console_WriteLine( s + ex.Message);
            }
        }

        //public static String DEFAULT_FILE_STORE_LOC = "C:\\TCP\\";

        //public enum STATE { FILE_NAME_READ, DATA_READ, FILE_CLOSED };

        /// <summary>
        /// Variables that are accessed by other classes indirectly.
        /// </summary>
        protected Socket m_clientSocket = null;
        private bool m_stopClient = false;
        private Thread m_clientListenerThread = null;
        private bool m_markedForDeletion = false;
        protected Int16 m_AutoCloseTime = 120;

        /// <summary>
        /// Working Variables.
        /// </summary>
        private StringBuilder m_oneLineBuf = new StringBuilder();
        //private STATE m_processState = STATE.FILE_NAME_READ;
        //private long m_totalClientDataSize = 0;
        //private StreamWriter m_cfgFile = null;
        protected DateTime m_lastReceiveDateTime;
        //protected DateTime m_currentReceiveDateTime;

        /// <summary>
        /// Client Socket Listener Constructor.
        /// </summary>
        /// <param name="clientSocket"></param>
        public  TCPSocketListener(Socket clientSocket, Int16 AutoCloseTime)
        {
            m_clientSocket = clientSocket;
        }

        /// <summary>
        /// Client SocketListener Destructor.
        /// </summary>
        ~TCPSocketListener()
        {
            StopSocketListener();
        }

        /// <summary>
        /// Method that starts SocketListener Thread.
        /// </summary>
        public void StartSocketListener()
        {
            if (m_clientSocket != null)
            {
                m_clientListenerThread =
                    new Thread(new ThreadStart(SocketListenerThreadStart));

                m_clientListenerThread.Start();
            }
        }

        /// <summary>
        /// Thread method that does the communication to the client. This 
        /// thread tries to receive from client and if client sends any data
        /// then parses it and again wait for the client data to come in a
        /// loop. The recieve is an indefinite time receive.
        /// </summary>
        private void SocketListenerThreadStart()
        {
            int size = 0;
            Byte[] byteBuffer = new Byte[1024];

            m_lastReceiveDateTime = DateTime.Now;
            //m_currentReceiveDateTime = DateTime.Now;

            Timer t = new Timer(new TimerCallback(CheckClientCommInterval),
                null, 10000, 10000);

            while (!m_stopClient)
            {

                try
                {
                    if (m_clientSocket.Available>0)
                    {
                        size = m_clientSocket.Receive(byteBuffer, m_clientSocket.Available, SocketFlags.None);
                        if (size > 0)
                        {
                            //m_currentReceiveDateTime = DateTime.Now;
                            Console_WriteLine("RCV:" + size.ToString());
                            ParseReceiveBuffer(byteBuffer, size);
                            m_lastReceiveDateTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch  //(System.Exception se)
                {
                    m_stopClient = true;
                    m_markedForDeletion = true;
                }
            }
            t.Change(Timeout.Infinite, Timeout.Infinite);
            t = null;
        }

        /// <summary>
        /// Method that stops Client SocketListening Thread.
        /// </summary>
        public void StopSocketListener()
        {
            
            if (m_clientSocket != null)
            {

                IPEndPoint endpoint=null;
                Console_WriteLine(" Stoping Socket "); 
                try
                {
                    endpoint = (IPEndPoint)m_clientSocket.RemoteEndPoint;
                    Console.Write(" for Address:" + endpoint.Address + "   Port:" + endpoint.Port.ToString()+"\r\n");
                }
                catch (System.Exception ex)
                {
                    Console_WriteLine(ex.Message);
                }
               
                
                m_stopClient = true;
                try
                {
                    m_clientSocket.Close();
                }
                catch(System.Exception ex)
                {
                    LogString(ex.Message);
                }
                if (m_clientListenerThread !=null)
                {
                    // Wait for one second for the the thread to stop.
                    m_clientListenerThread.Join(100);

                    // If still alive; Get rid of the thread.
                    if (m_clientListenerThread.IsAlive)
                    {
                        m_clientListenerThread.Abort();
                    }
                }
                m_clientListenerThread = null;
                m_clientSocket = null;
                m_markedForDeletion = true;
                Console_WriteLine("Socket stopped ");
                if (endpoint !=null)
                    Console.Write(" for Address:" + endpoint.Address + "   Port:" + endpoint.Port.ToString() + "\r\n");
            }
        }

        /// <summary>
        /// Method that returns the state of this object i.e. whether this
        /// object is marked for deletion or not.
        /// </summary>
        /// <returns></returns>
        public bool IsMarkedForDeletion()
        {
            return m_markedForDeletion;
        }

        /// <summary>
        /// This method parses data that is sent by a client using TCP/IP.
        /// As per the "Protocol" between client and this Listener, client 
        /// sends each line of information by appending "CRLF" (Carriage Return
        /// and Line Feed). But since the data is transmitted from client to 
        /// here by TCP/IP protocol, it is not guarenteed that each line that
        /// arrives ends with a "CRLF". So the job of this method is to make a
        /// complete line of information that ends with "CRLF" from the data
        /// that comes from the client and get it processed.
        /// </summary>
        /// <param name="byteBuffer"></param>
        /// <param name="size"></param>
        protected virtual void ParseReceiveBuffer(Byte[] byteBuffer, int size)
        {



            string data = Encoding.ASCII.GetString(byteBuffer, 0, size);
            int lineEndIndex = 0;

            // Check whether data from client has more than one line of 
            // information, where each line of information ends with "CRLF"
            // ("\r\n"). If so break data into different lines and process
            // separately.
            do
            {
                lineEndIndex = data.IndexOf("\r\n");
                if (lineEndIndex != -1)
                {
                    m_oneLineBuf = m_oneLineBuf.Append(data, 0, lineEndIndex + 2);
                    ProcessClientData(m_oneLineBuf.ToString());
                    m_oneLineBuf.Remove(0, m_oneLineBuf.Length);
                    data = data.Substring(lineEndIndex + 2,
                        data.Length - lineEndIndex - 2);
                }
                else
                {
                    // Just append to the existing buffer.
                    m_oneLineBuf = m_oneLineBuf.Append(data);
                }
            } while (lineEndIndex != -1);
        }

        /// <summary>
        /// Method that Process the client data as per the protocol. 
        /// The protocol works like this. 
        /// 1. Client first send a file name that ends with "CRLF".
        /// 
        /// 2. This SocketListener has to return the length of the file name
        /// to the client for validation. If the length matches with the length
        /// what  client had sent earlier, then client starts sending lines of
        /// information. Otherwise socket will be closed by the client.
        /// 
        /// 3. Each line of information that client sends will end with "CRLF".
        /// 
        /// 4. This socketListener has to store each line of information in 
        /// a text file whoose file name has been sent by the client earlier.
        /// 
        /// 5. As a last line of information client sends "[EOF]" line which
        /// also ends with "CRLF" ("\r\n"). This signals this SocketListener
        /// for an end of file and intern this SocketListener sends the total
        /// length of the data (lines of information excludes file name that
        /// was sent earlier) back to client for validation.
        /// </summary>
        /// <param name="oneLine"></param>
        protected virtual void ProcessClientData(String oneLine)
        {
            /*
            switch (m_processState)
            {
                case STATE.FILE_NAME_READ:
                    m_processState = STATE.DATA_READ;
                    int length = oneLine.Length;
                    if (length <= 2)
                    {
                        m_processState = STATE.FILE_CLOSED;
                        length = -1;
                    }
                    else
                    {
                        try
                        {
                            m_cfgFile = new StreamWriter(DEFAULT_FILE_STORE_LOC + oneLine.Substring(0, length - 2));
                        }
                        catch (Exception e)
                        {
                            m_processState = STATE.FILE_CLOSED;
                            length = -1;
                        }
                    }

                    try
                    {
                        m_clientSocket.Send(BitConverter.GetBytes(length));
                    }
                    catch (System.Exception se)
                    {
                        m_processState = STATE.FILE_CLOSED;
                    }
                    break;
                case STATE.DATA_READ:
                    m_totalClientDataSize += oneLine.Length;
                    m_cfgFile.Write(oneLine);
                    m_cfgFile.Flush();
                    if (oneLine.ToUpper().Equals("[EOF]\r\n"))
                    {
                        try
                        {
                            m_cfgFile.Close();
                            m_cfgFile = null;
                            m_clientSocket.Send(BitConverter.GetBytes(m_totalClientDataSize));
                        }
                        catch (System.Exception se)
                        {
                        }
                        m_processState = STATE.FILE_CLOSED;
                        m_markedForDeletion = true;
                    }
                    break;
                case STATE.FILE_CLOSED:
                    break;
                default:
                    break;
            }
             * */
        }

        /// <summary>
        /// Method that checks whether there are any client calls for the
        /// last 15 seconds or not. If not this client SocketListener will
        /// be closed.
        /// </summary>
        /// <param name="o"></param>
        private void CheckClientCommInterval(object o)
        {
            if (m_lastReceiveDateTime.AddMinutes (m_AutoCloseTime) < DateTime.Now)
            {
                try
                {

                    this.StopSocketListener();
                    Console_WriteLine("Close inactive connection"); 
                }
                catch { }
            }
            
        }
    }
}
