using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms_client
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool flagMutex;

            System.Threading.Mutex m_hMutex = new System.Threading.Mutex(true, "ALPR Client", out flagMutex);
            if (flagMutex)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrm());
            }
            else
            {
                Environment.Exit(0);
             
            }
            
        }
    }
}
