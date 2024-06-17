using NSS_3330S.FORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S
{
    static class Program
    {
        private static Mutex mutexObject;
        private static string strAppName = "NSS_3330S";//phj 220728 수정

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            try
            {
                mutexObject = new Mutex(false, strAppName);
            }
            catch (ApplicationException ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                MessageBox.Show(new Form() { TopMost = true }, ex.Message);
                return;
            }

            if (mutexObject.WaitOne(100, false))
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmMain());
                }
                catch (ApplicationException e)
                {
                    MessageBox.Show(new Form() { TopMost = true }, e.Message);
                    return;
                }
            }
            else
            {
                MessageBox.Show(new Form() { TopMost = true },  FormTextLangMgr.FindKey("Already running application"));
            }

            mutexObject.Close();
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //DateTime dtNow = DateTime.Now;
            //DionesTool.UTIL.MinidumpWriter.MakeDump(string.Format("c:\\th{0}.dmp", dtNow.ToString("yyyyMMdd_HHmmss")), System.Diagnostics.Process.GetCurrentProcess().Id);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //DateTime dtNow = DateTime.Now;
            //DionesTool.UTIL.MinidumpWriter.MakeDump(string.Format("c:\\{0}.dmp", dtNow.ToString("yyyyMMdd_HHmmss")), System.Diagnostics.Process.GetCurrentProcess().Id);
        }
    }
}
