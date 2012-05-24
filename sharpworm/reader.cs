using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace sharpworm
{
    class reader
    {
        public StringBuilder getString(string file)
        {
            StringBuilder sB = new StringBuilder();

            using (StreamReader sR = new StreamReader(file))
            {
                sB.Append(sR.ReadToEnd());
            }
            return sB;
        }
    }
}
