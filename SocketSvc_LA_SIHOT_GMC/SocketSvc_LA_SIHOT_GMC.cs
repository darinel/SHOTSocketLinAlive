using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Net.Sockets;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace SocketSvc_LA_SIHOT_GMC
{
    public partial class SocketSvc_LA_SIHOT_GMC : ServiceBase
    {
       string m_hn="6113";
        bool varError = false;
        Timer myTimer = new Timer();
        string IPAddress = "10.0.19.242";
        int PortAddress = 14779;
        private static Object m_lock;

        string xmlStringLinkAlive = "<?xml version='1.0' encoding='ISO-8859-1'?><SIHOT-Document><TN>6666</TN><OC>LA</OC><STATUS>0</STATUS></SIHOT-Document>";
        System.Data.DataSet dsAux = new System.Data.DataSet();

        public SocketSvc_LA_SIHOT_GMC()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            m_lock = new Object();
            System.Timers.Timer timer = new System.Timers.Timer();
            //ad 1: handle Elapsed event
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);

            //ad 2: set interval to 1 minute (= 60,000 milliseconds)
            timer.Interval = 60000; //every 1 minute

            //ad 3: enabling the timer
            timer.Enabled = true;
            ProcessReStart();
        }

        protected override void OnStop()
        {
        }

        void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            lock (m_lock)
            {
                ProcessReStart();
            }
        }


        public void ProcessData()
        {
            try
            {
                System.Net.Sockets.TcpClient tcpClientObj = new System.Net.Sockets.TcpClient();
                tcpClientObj.Connect(IPAddress, PortAddress);
                NetworkStream networkStream = tcpClientObj.GetStream();

                if (networkStream.CanWrite & networkStream.CanRead)
                {
                    StringBuilder myCompleteMessage = new StringBuilder();
                    //Step 1
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(xmlStringLinkAlive);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    // Read the NetworkStream into a byte buffer.
                    tcpClientObj.ReceiveBufferSize = 1024;

                    //Do I need to clean the buffer?
                    byte[] bytes = new byte[tcpClientObj.ReceiveBufferSize];
                    do
                    {
                        int bytesRead = networkStream.Read(bytes, 0, tcpClientObj.ReceiveBufferSize);

                        // Output the data received from the host to the console.
                        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(bytes, 0, bytesRead));
                    } while (networkStream.DataAvailable);

                    //Analize this value and get positive confirmation, if negative save the value in the database
                    //Parse the XML here and create a SQL connection
                    myCompleteMessage.Replace(myCompleteMessage.ToString(), CleanInput(myCompleteMessage.ToString()));
                    WriteLogFile(myCompleteMessage.ToString() + "\\r\\n");
                    ProcessReadXMLValue(myCompleteMessage.ToString());
                }
                else
                {
                    if (!networkStream.CanRead)
                    {
                        //Console.WriteLine("cannot not write data to this stream")
                        WriteLogFile("cannot not write data to this stream");
                        tcpClientObj.Close();
                    }
                    else
                    {
                        if (!networkStream.CanWrite)
                        {
                            //Console.WriteLine("cannot read data from this stream")
                            WriteLogFile("cannot not write data to this stream");
                            tcpClientObj.Close();
                        }
                    }
                }
                networkStream.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }

        }

        private void ProcessRequestXML(string strFileName)
        {

            System.Xml.XmlDocument Document = new XmlDocument();
            StringBuilder sb = new StringBuilder(strFileName);
            sb = sb.Replace(sb.ToString(), CleanInput(sb.ToString()));
            Document.LoadXml(sb.ToString());
            XmlNodeList dispatchs = Document.GetElementsByTagName("SIHOT-Document");

            // Parse the Dispatch nodes and place data into a class for later processing
            SIHOT dd = new SIHOT();
            foreach (XmlNode n in dispatchs)
            {
                dd.OC = n.SelectSingleNode("descendant::OC").InnerText;
                dd.TN = n.SelectSingleNode("descendant::TN").InnerText;
                dd.RC = n.SelectSingleNode("descendant::STATUS").InnerText;
                dd.HN = m_hn;
            }
            //Save the value

            SIHOT_dB.InsertSIHOT_ResStart(dd);
        }

        private string CleanInput(string inputXML)
        {
            //Return Regex.Replace(inputXML, "[^><\w\.@-]", "")
            return Regex.Replace(inputXML, "[\\4\\0]", "");
        }

        private void ProcessReadXMLValue(string strFileName)
        {

            System.Xml.XmlDocument Document = new XmlDocument();

            Document.LoadXml(strFileName);
            //For Each n As XmlNode In Document.SelectSingleNode("SIHOT-Document").ChildNodes
            //    lblError.Text += n.InnerText + "/r/n"
            //Next
            XmlNodeList dispatchs = Document.GetElementsByTagName("SIHOT-Document");

            // Parse the Dispatch nodes and place data into a class for later processing
            SIHOT dd = new SIHOT();

            foreach (XmlNode n in dispatchs)
            {
                dd.OC = n.SelectSingleNode("descendant::OC").InnerText;
                dd.TN = n.SelectSingleNode("descendant::TN").InnerText;
                dd.RC = n.SelectSingleNode("descendant::RC").InnerText;
                dd.HN = m_hn;
            }
            //Save the value
            SIHOT_dB.InsertSIHOT_ResStart(dd);
        }


        void WriteLogFile(string parString)
        {

            FileStream fs = new FileStream(@"c:\tmp\GMC\log\LA.log", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter m_streamWriter = new StreamWriter(fs);
            m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            m_streamWriter.WriteLine(parString + " " + DateTime.Now + "(EST-CH)\n");
            m_streamWriter.Flush();
            m_streamWriter.Close();
        }

        public void ProcessReStart()
        {

            ProcessRequestXML(xmlStringLinkAlive);
            ProcessData();

            if (varError == true)
            {
                return;
            }
        }

        public void SendEmailGoogle()
        {

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("pmsalertbysol@gmail.com", "smvcpassword"),
                EnableSsl = true
            };
            client.Send("pmsalertbysol@gmail.com", "4072563998@mymetropcs.com", "SIHOT_LinkAlive_Alert_from_GMC", "GMC SIHOT_LinkAlive_Problem");

        }
    }
}
