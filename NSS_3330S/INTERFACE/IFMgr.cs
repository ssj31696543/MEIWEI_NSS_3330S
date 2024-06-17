using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Concurrent;

using LinkMessage;
using DionesTool.SOCKET;
using ITInnovation_XNetPro2;
using NSS_3330S.MOTION;

namespace NSS_3330S
{
    public class IFMgr
    {
        #region VAR
        private static volatile IFMgr instance;
        private static object syncRoot = new Object();

        IFSaw commSaw = new IFSaw();
        IFVision commVision = new IFVision();

        Thread threadRun;
        bool flagThreadAlive = false;      
        #endregion

        #region PROPERTY
        public static IFMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new IFMgr();
                        }
                    }
                }

                return instance;
            }
        }

        public IFSaw SAW
        {
            get
            {
                return commSaw;
            }
        }

        public IFVision VISION
        {
            get
            {
                return commVision;
            }
        } 
        #endregion

        #region TOR
        public IFMgr()
        {
        }

        ~IFMgr()
        {

        }
        #endregion

        #region CONNECT
        public void Open()
        {
            commVision.Start();
            commSaw.Start();
        }

        public void Close()
        {
            commSaw.Stop();
            commVision.Stop();
        }
        #endregion

        #region THREAD
        void StartStopManualThread(bool bStart)
        {
            if (threadRun != null)
            {
                // Join의 시간은 Thread에서 처리되는 최대 시간으로 지정
                flagThreadAlive = false;
                threadRun.Join(500);
                threadRun.Abort();
                threadRun = null;
            }

            if (bStart)
            {
                flagThreadAlive = true;
                threadRun = new Thread(new ParameterizedThreadStart(ThreadRun));
                threadRun.Name = "IF MGR THREAD";
                if (threadRun.IsAlive == false)
                    threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            IFMgr mgr = obj as IFMgr;

            while (mgr.flagThreadAlive)
            {
                Thread.Sleep(100);
                // 공통적으로 사용할 Thread 처리
            }
        }
        #endregion
    }
}
