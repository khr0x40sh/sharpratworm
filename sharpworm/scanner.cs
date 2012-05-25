using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace sharpworm
{
    class scanner
    {
            public bool Connect(string IPAddress, int Port, int WaitSeconds)
            {
                TcpClient TcpScan = new TcpClient();
                try
                {
                    // Try to connect 
                    bool ConnectSuccess = false;
                    TcpScan.BeginConnect(IPAddress, Port, null, null);
                    for (int i = 0; i <= WaitSeconds; i++) //wait specified # seconds for connection
                    {
                        System.Threading.Thread.Sleep(1000); //wait 1 sec
                        if (TcpScan.Connected)
                        {
                            ConnectSuccess = true;
                            break;
                        }
                    }
                    return ConnectSuccess;
                    // If there\'s no exception, we can say the port is open 

                }
                catch
                {
                    return false; // An exception occured, thus the port is probably closed
                }
                finally
                {
                    TcpScan.Close(); //close the connection and socket
                }
            }
        }


    }
