using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using NSS_3330S;

namespace NSS_3330S
{
    public class StripTTLogThread
    {
        public struct TackTimeItem
        {
            public string path;
            public string contents;

            public TackTimeItem(string path, string contents)
            {
                this.path = path;
                this.contents = contents;
            }
        }

        Thread m_thread;
        ConcurrentQueue<TackTimeItem> m_queue = new ConcurrentQueue<TackTimeItem>();

        DateTime dtPrevStrip = new DateTime();
        DateTime dtNowStrip = new DateTime();
        TimeSpan tsT_T = new TimeSpan();

        bool m_bFlagThreadAlive = false;
        bool m_bOpened = false;
        bool m_bFirst = false;
        int m_nLineNo = 1;

        string[] sLines;
        string content = "";
        string path = "";
        TackTimeItem item;

        const string m_sTitle = "NO,INTIME,STRIP_ID,T/T";
        const char cSplit = '*';
        public static string LOTDATE = DateTime.Now.ToString("yyyy-MM-dd");
        public static string LOTID = "UNKNOWN_LOTID";
        public static string  FiledDir = String.Format("{0}{1}", PathMgr.Inst.PATH_LOG_TT_LOG, LOTDATE);
        public static string FilePath = string.Format("{0}\\{1}.csv", FiledDir, LOTID);

        public void StartStopManualThread(bool bStart)
        {
            if (m_thread != null)
            {
                // Join의 시간은 Thread에서 처리되는 최대 시간으로 지정
                m_bFlagThreadAlive = false;
                m_thread.Join(1000);
                m_thread.Abort();
                m_thread = null;
            }

            if (bStart)
            {
                m_bFlagThreadAlive = true;
                m_thread = new Thread(new ParameterizedThreadStart(Run));
                m_thread.Name = "STRIP LOG THREAD";
                if (m_thread.IsAlive == false)
                    m_thread.Start(this);
            }
        }

        void Run(object obj)
        {
            while (m_bFlagThreadAlive)
            {
                Thread.Sleep(500);

                if (m_queue.Count > 0)
                {
                    content = "";
                    path = "";

                    if (!m_queue.TryPeek(out item))
                        continue;

                    //sLines = content.Split(cSplit);
                    //if (sLines.Length < 2) continue;
                    path = item.path;

                    try
                    {
                        if (!Directory.Exists(FiledDir))
                            Directory.CreateDirectory(FiledDir);

                        if (!File.Exists(path))
                        {
                            File.Create(path).Close();
                            m_bFirst = true;
                            m_nLineNo = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        GbFunc.WriteExeptionLog(ex.ToString());

                        m_queue.TryDequeue(out item);
                        continue;
                    }

                    m_bOpened = CheckFileLocked(path);
                    if (m_bOpened == false)
                    {
                        string line = "";
                        if (!m_queue.TryDequeue(out item))
                            continue;

                        //sLines = content.Split(cSplit);
                        //if (sLines.Length < 2) continue;
                        path = item.path;
                        line = item.contents;

                        try
                        {
                            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

                            if (m_bFirst)
                            {
                                m_bFirst = false;
                                sw.WriteLine(m_sTitle); //Title   
                            }

                            sw.WriteLine(string.Format("{0},{1}", m_nLineNo, line));

                            sw.Close();
                            fs.Close();
                        }
                        catch (Exception ex)
                        {
                            GbFunc.WriteExeptionLog(ex.ToString());
                        }
                        m_nLineNo++;
                    }
                }
            }

        }

        public bool CheckFileLocked(string filePath)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);

                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 파일 저장 경로와 저장한 내용은 cSplit로 구분하여 한 큐로 추가한다
        /// </summary>
        /// <param name="sStripId"></param>
        public void AddStripLog(string sStripId)
        {
            try
            {
                dtNowStrip = DateTime.Now;
                tsT_T = dtNowStrip - dtPrevStrip;
                string strLine = string.Format("{0},{1},{2}", dtNowStrip.ToString("yyyy-MM-dd HH:mm:ss"), sStripId, tsT_T.TotalSeconds.ToString("0.0"));
                m_queue.Enqueue(new TackTimeItem(FilePath, strLine));

                dtPrevStrip = dtNowStrip;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 파일 경로를 설정한다.
        /// (프로그램 로딩시, 새로운 랏 투입 시 설정)
        /// </summary>
        /// <param name="dtNow"></param>
        /// <param name="sLotId"></param>
        public void SetFileName(DateTime dtNow, string sLotId)
        {
            try
            {
                //쓰레드에 있는 Static 변수
                LOTDATE = dtNow.ToString("yyyy-MM-dd");
                LOTID = sLotId == "" ? "UNKNOWN_LOTID" : sLotId;
                FiledDir = String.Format("{0}\\{1}", PathMgr.Inst.PATH_LOG_TT_LOG, LOTDATE);
                if (!Directory.Exists(FiledDir)) Directory.CreateDirectory(FiledDir);
                dtPrevStrip = DateTime.Now;
                dtNowStrip = DateTime.Now;

                FilePath = string.Format("{0}\\{1}.csv", FiledDir, LOTID);

                if (File.Exists(FilePath))
                {
                    if (m_nLineNo == 1)
                    {
                        string strLastLine = "";
                        using (StreamReader sr = new StreamReader(FilePath))
                        {
                            string line = "";
                            while ((line = sr.ReadLine()) != null)
                            {
                                strLastLine = line;   
                            }
                        }

                        string[] arr = strLastLine.Split(',');

                        if (arr != null && arr.Length == 4)
                        {
                            int nLineNo = 0;
                            if (int.TryParse(arr[0], out nLineNo))
                            {
                                m_nLineNo = nLineNo + 1;
                            }
                        }
                    }   
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
