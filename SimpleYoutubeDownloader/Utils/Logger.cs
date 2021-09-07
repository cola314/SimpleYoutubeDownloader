using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleYoutubeDownloader
{
    public class Logger
    {
        private Logger() { }

        public static Logger Instance = new Logger();

        public delegate void WriteLog(string line);

        public event WriteLog OnWrite;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void WriteLine(string msg)
        {
            string line = msg?.Replace("\n", "\r\n");
            OnWrite($"{DateTime.Now} - {line}\r\n");
        }
    }
}
