using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gamebot.Helpers
{
    public static class Logger
    {


        public static void WriteLog(string testo)
        {
            string file_name = "Logs_" + DateTime.Now.ToString("yyyy_MM_dd_HH") + ".txt";
            string file_path = Path.Combine(Application.StartupPath, "LogRequests");
            string full_log = Path.Combine(file_path, file_name);

            if (!Directory.Exists(file_path))
            {
                Directory.CreateDirectory(file_path);
            }

            File.AppendAllText(full_log, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + testo + Environment.NewLine);
        }




    }
}
