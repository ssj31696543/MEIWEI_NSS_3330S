using Keyence.AutoID.SDK;
using DionesTool.DATA;
using DionesTool.SOCKET;
using DionesUtil.DATA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NSS_3330S
{

    public class cKeyenceSR100
    {
        //public delegate void BCR_Start_DisplayHandler();
        //public event BCR_Start_DisplayHandler On_MGZ_BCR_Start_Display;
        //public delegate void BCR_Stop_DisplayHandler();
        //public event BCR_Stop_DisplayHandler On_MGZ_BCR_Stop_Display;

        public bool IsConnect = false;

        ReaderAccessor ReaderAcc;

        public cKeyenceSR100()
        {
            ReaderAcc = new ReaderAccessor();
        }


        public void Connect(string ip)
        {
            ReaderAcc.IpAddress = ip;

            IsConnect = ReaderAcc.Connect();
        }
        public void Disconnect()
        {
            IsConnect = false;
            ReaderAcc.Disconnect();
        }
        public string Send_ReadStart()
        {
            string STR = ReaderAcc.ExecCommand("LON", 100);

            if (STR == "")
            {
                ReaderAcc.ExecCommand("LOFF");
                STR = "FAIL";
            }

            if(STR.Contains('\r'))
            {
                STR = STR.Replace("\r", "");
            }
            return STR;
        }
        public void Send_ReadStop()
        {
            ReaderAcc.ExecCommand("LOFF");
        }
        public void Send_Reset()
        {
            ReaderAcc.ExecCommand("RESET");
        }
        public void Send_BufferClear()
        {
            ReaderAcc.ExecCommand("BCLR");
        }
        public string Send_ErrState()
        {
            return ReaderAcc.ExecCommand("ERRSTAT");
        }
        public string Send_keyence()
        {
            return ReaderAcc.ExecCommand("KEYENCE");
        }
        public string Send_Tune()
        {
            return ReaderAcc.ExecCommand("TUNE,01");
        }

    }
}
