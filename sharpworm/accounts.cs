using System;
using System.Collections.Generic;
using System.Text;

namespace sharpworm
{
    public class accounts
    {
        private byte[] _pattern = null;
        public byte[] Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }

    }
}
