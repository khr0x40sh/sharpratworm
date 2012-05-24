using System;
using System.Collections.Generic;
using System.Text;

namespace sharpworm
{
    class targets
    {
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private System.Net.IPAddress _ipaddr = System.Net.IPAddress.Parse("0.0.0.0");
        public System.Net.IPAddress IPAddr
        {
            get { return _ipaddr; }
            set { _ipaddr = value; }
        }

    }
}
