using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServerMultipleConnections
{
    class CustomTextFileTrace : TraceListener
    {
        private FileStream fs;
        private StreamWriter writer;

        public CustomTextFileTrace()
        {
            // Ebbes løsning
            fs = new FileStream(@"c:\logs\EchoChatLog.txt", FileMode.Append, FileAccess.Write);
            writer = new StreamWriter(fs);
            writer.AutoFlush = true;
        }

        public override void Write(string message)
        {
            writer.Write("{0}: {1} ", DateTime.Now, message);
        }

        public override void WriteLine(string message)
        {
            writer.WriteLine("{0}: {1} ", DateTime.Now, message);
        }

        public override void WriteLine(string message, string ip)
        {
            writer.WriteLine("{0} from ip {1}: ", DateTime.Now, ip, message);
        }
    }
}
