using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace sharpworm
{
    class download
    {
        WebClient webC = new WebClient();
        
        public string downloader(string URL, string file)
        {
            string complete = "Download Complete!";
            try
            {
                webC.DownloadFile(URL, file);
            }
            catch (Exception err)
            {
                complete = err.Message;
                return complete;
            }

            return complete;
        }

    }
}
