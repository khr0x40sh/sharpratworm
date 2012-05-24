using System;
using System.Collections.Generic;
using System.Text;

namespace sharpworm
{
    class Program
    {
        static dumper dump = new dumper();
        static List<accounts> acc = new List<accounts>();
        static connections conn = new connections();

        static void Main(string[] args)
        {
            conn.connection();
            
            //dump.reg_dump();
            //acc = dump.WMI_REG("localhost", null, null, "HKLM", "System\\CurrentControlSet\\Control\\Lsa");
            //acc = dump.WMI_REG("localhost", null, null, "HKLM", "System\\CurrentControlSet\\Control\\Lsa\\Data");
            Console.WriteLine("El Fin!");
            Console.ReadLine();
        }
    }
}
