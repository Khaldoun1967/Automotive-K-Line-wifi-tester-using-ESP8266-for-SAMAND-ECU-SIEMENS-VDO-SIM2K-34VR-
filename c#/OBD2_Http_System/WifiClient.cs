using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Collections;
using SimpleWifi;
using SimpleWifi.Win32;
using System.Diagnostics;




//Router setting
//ADVANCED WIRELESS SETTINGS/SSID/User Isolation :OFF
namespace Easy_Queue_System
{
    //------------------------------------------------------------------------------------------------------------------
    public delegate void TaskCompletedCallBack(int serialNo, string taskResult, string Exception);
    public delegate void Connect_EventHandler(object sender, Connect_EventArgs e);
    public delegate void Queue_EventHandler(object sender, Queue_EventArgs e);
    public delegate void ExceptionEventHandler(object sender, MSG_EventArgs e);
    public delegate void Connected_EventHandler(object sender, MSG_EventArgs e);
    public delegate void K_Line_EventHandler(object sender, MSG_EventArgs e);
    //------------------------------------------------------------------------------------------------------------------
    public class Connect_EventArgs : EventArgs
    {
        public string STR;
        public int Progress;
        public bool STATUS;
        public Connect_EventArgs(string str, int progress, bool status)
        {
            this.STR = str;
            this.Progress = progress;
            this.STATUS = status;
        }
    }
    //------------------------------------------------------------------------------------------------------------------
    public class MSG_EventArgs : EventArgs
    {
        public string MSG;
        public MSG_EventArgs(string msg)
        {
            this.MSG = msg;
        }
    }
    //------------------------------------------------------------------------------------------------------------------
    public class Queue_EventArgs : EventArgs
    {
        public String TicketNo;
        public String WindowNo;
        public String PersonNo;
        public String MSG;
        public Queue_EventArgs(String ticketNo, String windowNo, String personNo, String msg)
        {
            this.TicketNo = ticketNo;
            this.WindowNo = windowNo;
            this.PersonNo = personNo;
            this.MSG = msg;
        }
    }
    //------------------------------------------------------------------------------------------------------------------

    public class WifiClient
    {        
        private static Wifi wifi;
        private Thread Client_Thread;
        private Thread Station_Thread;
        public event ExceptionEventHandler ExceptionEvent;
        public event Connect_EventHandler Connect_Event;
        public event Queue_EventHandler Queue_Event;
        public event Connected_EventHandler Connected_Event;

        public event K_Line_EventHandler K_Line;

        private Hashtable httpHeaders = new Hashtable();
        private bool keep_connected;
        private bool Ever_Flage = false;
        private String Query_String = String.Empty;
        private String Requsted_ssid = String.Empty;
        private string StationIP = String.Empty;
        private String Station_ssid, Station_psw;
        private String Connected_IP = String.Empty;
        private String Connected_ssid = String.Empty;
        private String Connected_psw = String.Empty;
        private bool Connect_success = false;
        private bool Connecting_to_Station = false;

        private int Serial_No;
        private bool Respond_Received = false;
        private string Received_Content = String.Empty;
        private string Exception_Sent = String.Empty;

        private String AP_ssid = "OBD2_SCAN";       //Access Point SSID
        private String AP_pswd = "K_LineCANbus";      //Access Point Password
        private String AP_Domain = String.Empty;    //Access Point Domain
        private String AP_ip = "192.168.44.150";
        private int port = 80;
        private int update_time = 800;//msec

        private bool oldConnectedStatus;
        public WifiClient()
        {
            wifi = new Wifi();
            wifi.ConnectionStatusChanged += wifi_ConnectionStatusChanged;

            if(wifi.ConnectionStatus == WifiStatus.Connected)
            {
                oldConnectedStatus = true;
            }
            else
            {
                oldConnectedStatus = false;
            }
        }

        public void DisConnect()
        {
            oldConnectedStatus = false;
            keep_connected = false;
            wifi.Disconnect();
            //if (Connected_Event != null) Connected_Event(this, new MSG_EventArgs(""));//disconnected msg
        }
        void wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
        {
            if ((e.NewStatus == WifiStatus.Connected) && (oldConnectedStatus == true)) return;//Ignoe
            if (e.NewStatus == WifiStatus.Connected) oldConnectedStatus = true; else oldConnectedStatus = false;
            //---------------------------------------------------------------------------------------------------
            //              Keep Connecting to [Connected_ssid,Connected_psw]
            if ((keep_connected == true) && (e.NewStatus == WifiStatus.Disconnected))
            {
                if (Connect_AP(Connected_ssid, Connected_psw) == false)
                {
                    string msg = "Connection to the AP : " + Connected_ssid + " disconnected !!!";
                    if (ExceptionEvent != null) ExceptionEvent(this, new MSG_EventArgs(msg));
                }
            }
            //---------------------------------------------------------------------------------------------------
            if (e.NewStatus == WifiStatus.Connected)
            {
                if (Connecting_to_Station == false) Start_Logging();
            }
            else
            {
                Stop_Logging();
            }
            //---------------------------------------------------------------------------------------------------
            if (Connected_Event != null)
            {
                if (e.NewStatus == WifiStatus.Connected)
                {
                    Connected_Event(this, new MSG_EventArgs(Connected_ssid));
                }
                else
                {
                    Connected_Event(this, new MSG_EventArgs(""));
                }
            }           
        }
        //====================================================================
       
        public void Open_Wifi_Networks_Flyout()        //update Avilable Wifi networks
        {
            Start_Process("ms-availablenetworks:");
        }
 
        private void Start_Process(string str)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = @"c:\windows\explorer.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = str; 
            proc.Start();
            //Console.WriteLine("Process: {0} ID: {1}", proc.ProcessName, proc.Id);   
        }
      
        private static IEnumerable<AccessPoint> List()
        {
            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);
            return accessPoints;
        }
 

        public List<string> get_station_list()
        {
            List<string> StationList = new List<string>();
            SimpleWifi.AccessPoint[] AccessPoints = List().ToArray();
            foreach (SimpleWifi.AccessPoint ap in AccessPoints)
            {
                if (ap.Name.Length > 0)
                {
                    if (ap.Name != AP_ssid) StationList.Add(ap.Name);
                }
            }
            return StationList;
        }

            
        private AccessPoint Get_connected_AP()
        {
            AccessPoint AP = null;

            AccessPoint[] AccessPoints = List().ToArray();
            foreach (AccessPoint ap in AccessPoints)
            {
                if (ap.IsConnected == true)
                {
                    AP = ap;
                    break;
                }
            }
            return AP;
        }

        private bool Check_AP_Connected(string SSID)
        {
            Open_Wifi_Networks_Flyout();        //update Avilable Wifi networks
            Thread.Sleep(1000);

            SimpleWifi.AccessPoint[] AccessPoints = List().ToArray();
            foreach (SimpleWifi.AccessPoint ap in AccessPoints)
            {
                if (ap.Name == SSID)return ap.IsConnected; 
            }
            return false;
        }
        private AccessPoint Get_AccessPoint(String SSID)
        {
            AccessPoint AP = null;
            AccessPoint[] AccessPoints = List().ToArray();
            foreach (AccessPoint ap in AccessPoints)
            {
                if (ap.Name == SSID)
                {
                    AP = ap;
                    break;
                }
            }
            return AP;
        }
        private bool Check_PSWD_Validation(String SSID, String PSWD)
        {
            AccessPoint AP = Get_AccessPoint(SSID);
            bool validPassFormat = AP.IsValidPassword(PSWD);

            return validPassFormat;
        }

        
       
        private bool Connect_AP(string SSID, string Password)
        {
            AccessPoint AP = Get_connected_AP();
            if ((AP != null) && (AP.Name == SSID))
            {
                Connect_success = true;
                Connected_ssid = SSID;
                Start_Logging();
                if(Connected_Event != null) Connected_Event(this, new MSG_EventArgs(Connected_ssid));
            
                return true;     //already connected to SSID                  
            }
            else
            {
                keep_connected = false;
                if (wifi.ConnectionStatus == WifiStatus.Connected) wifi.Disconnect();
            }

            AP = Get_AccessPoint(SSID);
            if (AP == null) return false;       // SSID not found

            AuthRequest authRequest = new AuthRequest(AP);
            authRequest.Username = SSID;
            authRequest.Password = Password;
            authRequest.Domain = "";
            bool overwrite = true;

            //if (authRequest.IsPasswordRequired)
            //{
            //    if (AP.HasProfile)
            //    // If there already is a stored profile for the network, we can either use it or overwrite it with a new password.
            //    {
            //        //overwrite = false;
            //    }
            //    if (overwrite)
            //    {
            //        if (authRequest.IsUsernameRequired)
            //        {
            //            authRequest.Username = SSID;
            //        }

            //        authRequest.Password = Password;

            //        if (authRequest.IsDomainSupported)
            //        {
            //            authRequest.Domain = "";
            //        }
            //    }
            //}
            Requsted_ssid = SSID;
            Connect_success = false;
            Connected_ssid = SSID; Connected_psw = Password;

            AP.ConnectAsync(authRequest, overwrite, OnConnectedComplete);
            return true;
        }



        
        private void OnConnectedComplete(bool success)
        {
            Connect_success = success;
            if (success == true)
            {
                keep_connected = true;
            }
            
            Console.WriteLine("\nOnConnectedComplete, success: {0}", success);
        }


        //====================================================================

        private void Start_Logging()
        {
            Client_Thread = new Thread(RunServer);
            Ever_Flage = true;
            Client_Thread.Start();
        }
        private void Stop_Logging()
        {
            if (Client_Thread != null)
            {
                //Thread_Runing = false;
                Ever_Flage = false;
                //Client_Thread.Abort();
                //// Waiting for a thread to terminate.
                //Client_Thread.Join();
            }

        }

        private void Abort_Logging()
        {
            if (Client_Thread != null)
            {
                //Thread_Runing = false;
                Ever_Flage = false;
                Client_Thread.Abort();
                // Waiting for a thread to terminate.
                Client_Thread.Join();
            }

        }
        //public void Fast_KWP_CMD()
        //{
        //    string CMD = "/KWP";    
        //    Send_Command(23, CMD);
        //}

        public void Send_CMD(String cmd)
        {
            string CMD = "/CMD/";
            CMD += cmd;
            CMD += "/CCMD";
            Send_Command(11, CMD);
        }
        private bool Send_Command(int id, String Request)
        {
            if ((Connected_IP.Length == 0) || (Connecting_to_Station == true)) return false;

            Send_HTTP(id, Request, Connected_IP, port);
            return true;
        }

        public bool Is_Connected()
        {
            if (Connected_IP.Length == 0) return false;

            return true;
        }


        private void Send_HTTP(int serialNo, string Req, string ip, int port)
        {
            Respond_Received = false;
            Send_HTTP_Server sending = new Send_HTTP_Server(serialNo, Req, ip, port, new TaskCompletedCallBack(ResultCallback));

            Thread thread = new Thread(new ThreadStart(sending.ThreadProc));
            thread.Start();
        }




        private void Process_HTTP_Content(String Content)
        {
            String content = Content;
            String TicketNo = "";
            String WindowNo = "";
            String Person = "";
            int start, end, length;
            if (K_Line != null) K_Line(this, new MSG_EventArgs(content));
            //------------------------------------------------------------------------------------------------------------
            if (content.Contains("="))
            {
                if (content.Contains("goto"))
                {
                    //Ticket No= B18 goto Window No= 4
                    start = content.IndexOf("Ticket No= ");
                    end = content.IndexOf(" goto");
                    length = end - start - 11;
                    if (length > 0) TicketNo = content.Substring(start + 11, length);

                    start = content.IndexOf("Window No= ");
                    WindowNo = content.Substring(start + 11);
                }
                else
                {
                    if (content.Contains("persons"))
                    {
                        //Register Ticket No= B40 ,persons ahead= 21
                        start = content.IndexOf("Ticket No= ");
                        end = content.IndexOf(" ,persons");
                        length = end - start - 11;
                        if (length > 0) TicketNo = content.Substring(start + 11, length);

                        start = content.IndexOf("ahead= ");
                        Person = content.Substring(start + 7);
                    }
                    else
                    {
                        //Window No= 1 is Closed
                        //Window No= 1 is Ready
                        if (content.Contains("is"))
                        {
                            start = content.IndexOf("Window No= ");
                            end = content.IndexOf(" is");
                            length = end - start - 11;
                            if (length > 0) WindowNo = content.Substring(start + 11, length);
                        }
                    }
                }

                if (Queue_Event != null) Queue_Event(this, new Queue_EventArgs(TicketNo, WindowNo, Person, content));
                return;
            }
            else
            {
                //if (K_Line != null) K_Line(this, new MSG_EventArgs(content));
            }
            //------------------------------------------------------------------------------------------------------------           
        }



        private void RunServer()
        {
            int id=0;
            int timeout;
            int Fail_Counter = 0;
            int TimeOut_Counter = 0;
            
            try
            {
                while (Ever_Flage)
                {
                    //Send_HTTP(id, "/msg", Connected_IP, port);
                    Send_Command(id, "/msg");
                    timeout = 0;
                    Lp:
                    while (Respond_Received == false)                            
                    {
                        if (Connecting_to_Station == true) return;
                        timeout++;                                
                        if (timeout > 500)break;                          
                        Thread.Sleep(10);                            
                    };
                    if ((Respond_Received == true) && (id != Serial_No)) { Respond_Received = false; goto Lp; }//respond not recieved yet
                    if (Respond_Received == false)                    
                    {
                        TimeOut_Counter++;
                        if (Connecting_to_Station == true) return;
                        if (TimeOut_Counter > 3)
                        {
                            if (ExceptionEvent != null) ExceptionEvent(this, new MSG_EventArgs("HTTP Respond Time Out!"));
                            return;
                        }
                    } 
                    
                    id++;
                    if(Exception_Sent.Length == 0)
                    {                     
                        Fail_Counter = 0;
                    }
                    else
                    {
                        Fail_Counter++;
                        if (Fail_Counter > 3)
                        {
                            if (Connecting_to_Station == true) return;
                            if (ExceptionEvent != null) ExceptionEvent(this, new MSG_EventArgs(Exception_Sent));
                            return;
                        }
                    }

                    Thread.Sleep(update_time);
                }
            }
            catch (Exception Ex)
            {
                if (ExceptionEvent != null) ExceptionEvent(this, new MSG_EventArgs(Ex.Message));
            }
        }
        
        public void Connect_to_Station_LAN(String ssid, String psw)
        {
            Connecting_to_Station = true;
            Abort_Logging();
            Station_ssid = ssid;
            Station_psw = psw;
            Station_Thread = new Thread(ConnectionThread);
            Station_Thread.Start();
        }

        public void Abort_Connection()
        {
            if (Station_Thread != null)
            {
                if (Station_Thread.IsAlive == true)
                {
                    Station_Thread.Abort();
                    // Waiting for a thread to terminate.
                    Station_Thread.Join();
                    Connecting_to_Station = false;
                }

            }            
        }
        

        private bool Search_AP(String ssid, int waiting)
        {
            AccessPoint AP;
            Open_Wifi_Networks_Flyout();        //update Avilable Wifi networks
            Thread.Sleep(1000);

            int timeout = 0;
            while (true)
            {
                AP = Get_AccessPoint(ssid);
                if (AP != null) return true;
                timeout++;
                Thread.Sleep(100);
                if (timeout > waiting) return false;// SSID not found                               
            }
        }

        private bool Connect_to_AP(String ssid, String pswd, int waiting, bool FB)
        {
            Open_Wifi_Networks_Flyout();        //update Avilable Wifi networks
            Thread.Sleep(3000);
            Connect_success = false;
            for (int i = 0; i < 3; i++)
            {
                if (Connect_AP(ssid, pswd) == true)
                {
                    int timeout = 0;
                    while (Connect_success == false)
                    {
                        timeout++;
                        Thread.Sleep(10);
                        if (timeout > waiting) goto QT;
                    }
                    if (ssid == AP_ssid) Connected_IP = AP_ip; else Connected_IP = StationIP;
                    //Connected_ssid = ssid; Connected_psw = pswd;
                    if ((Connect_Event != null) && (FB == true)) Connect_Event(this, new Connect_EventArgs(ssid, 200, Connect_success));
                    return true;
                }
                else
                {
                    Thread.Sleep(waiting * 10);
                }
            }
        QT:
            if ((Connect_Event != null) && (FB == true)) Connect_Event(this, new Connect_EventArgs(ssid, 200, Connect_success));
            return false;
        }

        public void Connect_EasyQueue_AP()
        {
            Connect_to_AP(AP_ssid, AP_pswd, 200, true);  //Connect PC to the EasyQueue AP.            
        }
        private void ConnectionThread()
        {
            int Stage = 0;
            int No_IP_Count = 0;
            int timeout;
            Connecting_to_Station = true;
            //Stop_Logging();
            
            try
            {
                while (true)
                {
                    switch (Stage)
                    {
                        case 0:
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Searching for the EasyQueue AP...", 5, false));
                            if (Search_AP(AP_ssid, 200) == false)        //Search for EasyQueue AP on PC
                            {
                                if (Connect_Event != null) Connect_Event(this, 
                                    new Connect_EventArgs("Fail, Can not find the EasyQueue AP.\n Make sure its powered on !", 5, true));
                                Stage = 10;//fail,quit
                                break;
                             }
                            Stage++;
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Found EasyQueue AP ", 10, false));
                            break;

                        case 1:
                            if (Connect_to_AP(AP_ssid, AP_pswd, 200, false) == false)  //Connect PC to the EasyQueue AP.
                            {
                                if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Fail, Can not Connect to the EasyQueue AP !", 15, true));
                                Stage = 10;//fail,quit
                                break;                          
                            }
                            Thread.Sleep(1000);
                            Stage++;
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Connected to the EasyQueue AP ", 15, false));
                            break;

                        case 2:
                            // check if Esay Queue System is connected Station AP.
                            if (Check_AP_Connected(AP_ssid) == false) { Stage = 0; break; }
                            
                            //Send Station credentials to the Easy Queue System.
                            String Request = "/psw/" + Station_ssid + "/" + Station_psw + "/ppsw";                        
                            Send_HTTP(111, Request, AP_ip, port);

                            timeout = 0;
                            Lp1:
                            while (Respond_Received == false)                            
                            {                                
                                timeout++;                                
                                if (timeout > 500)break;                          
                                Thread.Sleep(10);                            
                            };
                            if ((Respond_Received == true) && (Serial_No != 111)) { Respond_Received = false; goto Lp1; }//respond not recieved yet
                            if ((Respond_Received == false) ||(Exception_Sent.Length > 0))  { Stage = 0; break; }//time out ,reconnect
                            
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Sent :" + Received_Content, 30, false));
                            Thread.Sleep(10000);//delay until EasyQueue System is connected to the Station.
                            Stage++;
                            break;

                        case 3:
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Searching for the EasyQueue AP...", 35, false));
                            if (Search_AP(AP_ssid, 500) == false)        //Search for EasyQueue AP on PC
                            {
                                if (Connect_Event != null) Connect_Event(this, 
                                    new Connect_EventArgs("Fail, Can not find the EasyQueue AP.\n Make sure its powered on !", 35, true));
                                Stage = 10;//fail,quit
                                break;                       
                            }
                            Stage++;
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Found EasyQueue AP ", 40, false));
                            break;

                        case 4:
                            if (Connect_to_AP(AP_ssid, AP_pswd, 700, false) == false)//Connect PC to the EasyQueue AP.
                            {
                                if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Fail, Can not Connect to the EasyQueue AP !", 40, true));
                                Stage = 10;//fail,quit
                                break;                     
                            }
                            Thread.Sleep(1000);
                            Stage++;
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Connected to the EasyQueue AP... ", 50, false));
                            break;

                        case 5:
                            // check if Esay Queue System is connected Station AP.

                            if (Check_AP_Connected(AP_ssid) == false) { Stage = 3; break; }
                            No_IP_Count++;
                            Send_HTTP(222, "/sta", AP_ip, port);
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Request Station IP, Count= " + No_IP_Count.ToString(), 55, false));
                            Thread.Sleep(1000);
                            if (No_IP_Count > 25)
                            {
                                String msg1 = "The Easy Queue System can not connect to the ";
                                String msg2 = " Station LAN, Check Station credentials";
                                if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs(msg1 + Station_ssid + msg2, 60, true));
                                Stage = 10;//fail,quit
                                break;
                            }    

                            timeout = 0;
                            Lp2:
                            while (Respond_Received == false)                            
                            {                                
                                timeout++;                                
                                if (timeout > 500)break;                          
                                Thread.Sleep(10);                            
                            };
                            if ((Respond_Received == true) && (Serial_No != 222)) { Respond_Received = false; goto Lp2; }//respond not recieved yet
                            if ((Respond_Received == false) ||(Exception_Sent.Length > 0))  { Stage = 3; break; }//time out ,reconnect
                           
                            if (Received_Content.Contains(":"))
                            {
                                int indx = Received_Content.IndexOf(':');
                                Station_ssid = Received_Content.Substring(0, indx);
                                StationIP = Received_Content.Substring(indx + 1);
                                if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("IP=" + StationIP, 75, false));
                                Stage++;
                            }
                                        
                            break;

                        case 6:
                            timeout = 0;
                            while (Connect_to_AP(Station_ssid, Station_psw, 1000, false) == false)
                            {
                                timeout++;
                                if (timeout > 10)
                                {
                                    if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("Fail, Can not Connect to Station AP!", 70, true));
                                    Stage = 10;//fail,quit
                                    break;
                                }
                                Thread.Sleep(5000);
                            }
                            if (Connect_Event != null) Connect_Event(this, new Connect_EventArgs("\nConnected to " + Station_ssid, 100, false));
                            Thread.Sleep(500);
                            Stage++;
                            break;

                        case 7:
                            if (Connect_Event != null) Connect_Event(this, 
                                new Connect_EventArgs("Connection to the Station LAN :" + Station_ssid + " Established Successfully!", 100, true));
                            Stage = 10;//fail,quit
                            break;

                        case 10:
                            Connecting_to_Station = false;
                            Start_Logging();
                            return; //Quit thread execution  
                    }
                }
            }
            catch (Exception Ex)
            {
                if (ExceptionEvent != null) ExceptionEvent(this, new MSG_EventArgs(Ex.Message));
            }
        }

        private void ResultCallback(int serialNo, string content, string Exception)
        {           
            Serial_No = serialNo;
            Respond_Received = true;
            Received_Content = content;
            Exception_Sent = String.Empty;

            if (Exception.Length > 0)
            {
                Exception_Sent = Exception;
                //if (ExceptionEvent != null) ExceptionEvent(this, new MSG_EventArgs(Exception));
                return;
            }
            if (content.Length > 0)
            {
                Process_HTTP_Content(content);
            }
        }

    }


    public class Send_HTTP_Server
    {
        private TaskCompletedCallBack callback;
        private int SerialNo;
        private string Request;
        private string IP;
        private int PORT;
        private BinaryWriter writer;
        private BinaryReader reader;
        private Hashtable httpHeaders = new Hashtable();
        public Send_HTTP_Server(int serialNo, string request, string ip, int port,
            TaskCompletedCallBack callbackDelegate)
        {
            SerialNo = serialNo;
            Request = request;
            IP = ip;
            PORT = port;
            callback = callbackDelegate;
        }

        private string streamReadLine(BinaryReader reader)
        {
            int next_char;
            string data = "";
         
            while (true)
            {
                next_char = reader.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                //if (next_char == -1) { Thread.Sleep(1); continue; };
                if (next_char == -1) { break; }
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        private void readHeaders(BinaryReader reader)
        {
            String line;
            while ((line = streamReadLine(reader)) != null)
            {
                if (line.Equals(""))
                {
                    //got all headers.
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    //throw new Exception("invalid http header line: " + line);
                    return;
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                string value = line.Substring(pos, line.Length - pos);
                httpHeaders[name] = value;
            }
        }
        public void ThreadProc()
        {
            TcpClient mTcpClient;
            String content_String = "";
            String HTTP_Version, Status_Code, Reason_Phrase;
            try
            {
                mTcpClient = new TcpClient();

                mTcpClient.Connect(IPAddress.Parse(IP), PORT);
                Stream socketStream = mTcpClient.GetStream();
                writer = new BinaryWriter(socketStream);
                reader = new BinaryReader(socketStream);

                String str = "GET " + Request + " HTTP/1.1";
                str += "User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)";
                str += "Host: www.tutorialspoint.com";
                str += "Accept-Language: en-us";
                str += "Accept-Encoding: gzip, deflate";
                str += "Connection: Keep-Alive";
                str += "\n";
                str += "\n";

                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] array = ascii.GetBytes(str);
                writer.Write(array, 0, array.Length);

                Thread.Sleep(100);//delay until get respond
                if (mTcpClient.Available > 0)
                {
                    String Status_Line = streamReadLine(reader);
                    //Status-Line = HTTP-Version SP Status-Code SP Reason-Phrase CRLF
                    string[] tokens = Status_Line.Split(' ');
                    if (tokens.Length == 3)
                    {
                        HTTP_Version = tokens[0].ToUpper();
                        Status_Code = tokens[1];
                        Reason_Phrase = tokens[2].ToUpper();
                        if ((Status_Code == "200") && (Reason_Phrase == "OK"))
                        {
                            readHeaders(reader);
                            int content_len = 0;
                            if (this.httpHeaders.ContainsKey("Content-Length"))
                            {
                                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                                if (content_len > 0)
                                {
                                    if (this.httpHeaders.ContainsKey("Content-Type"))
                                    {
                                        if (this.httpHeaders["Content-Type"].Equals("text/plain"))
                                        {
                                            byte[] buf = new byte[content_len];
                                            reader.Read(buf, 0, content_len);
                                            content_String = System.Text.Encoding.UTF8.GetString(buf, 0, content_len);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (callback != null) callback(SerialNo, content_String, "");
                if (reader != null) reader.Close();
                if (writer != null) writer.Close();
                if (socketStream != null) socketStream.Close();
                if (mTcpClient != null) mTcpClient.Close();
            }
            catch (Exception Ex)
            {
                //if (ExceptionEvent != null) ExceptionEvent(this, new MSG_EventArgs(Ex.Message));
                callback(SerialNo, content_String, Ex.Message);
            }
        }
    }
   
     
}