using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LinkMessage;
using DionesTool.SOCKET;
using NSS_3330S.MOTION;

namespace NSS_3330S
{
    public class IFSaw
    {
        #region SOCKET

        #region ENUM
        public enum eFROMSAW
        {
            RCP_PARACHANGE,
            RCP_DELETE,
            ALARM_SET,
            ALARM_RESET,
            MACHINE_STATUS,
            BLADE_ID,
            ANSWER,
        }


        public enum eTOSAW
        {
            RCP_PARACHANGE,
            RCP_CHANGE,
            RCP_CREATE,
            RCP_DELETE,
            RCP_UPLOAD,
            RCP_SELECTED,
            RCP_LIST,
            BLADE_ID,
            WORKING_DATA,
            GET_SVID,
        }

        public enum eREQ_IDX_FROMSAW
        {
            CMD = 0,
            DATA
        }

        public enum eRES_IDX_FROMSAW
        {
            ANSWER = 0,
            CMD,
            DATA,
        }

        public enum ePPChangeCode
        {
            SELECT = 0,
            CREATE = 1,//1, 2, 3 모두 Offline
            DELETE = 2,
            SAVE = 3,
        }
        #endregion

        #region CONST
        char STX = '@';
        char ETX = '*';
        #endregion

        #region DELE
        public delegate void ErrorStopDelegate(int nErrNo);
        public event ErrorStopDelegate ErrStopEvent = null;

        public delegate void SendRecvEvent(bool bRecv, string strRecv);
        public event SendRecvEvent OnSendRecvEvent = null;

        public delegate void LogAddSaw(string strLog);
        public event LogAddSaw OnLogAddSaw;
        #endregion

        #region VAR
        public SocketUDP m_Receiver = null;
        public SocketUDP m_Sender = null;

#if TEST
        private string m_strIpLocal = "127.0.0.1";
        private string m_strIpRemote = "127.0.0.1";
#else
        private string m_strIpLocal = "192.168.1.1";
        private string m_strIpRemote = "192.168.1.2";
#endif

        private int m_nLocalPort = 5001;  //수신포트
        private int m_nRemotePort = 5000; //송신포트

        Thread threadRun;
        public bool flagThreadAlive = false;
        private string strRecv = "";

        bool bCheckAllBack = false;

        IFSeqBase ifEqRcpDelete = new IFSeqBase();
        IFSeqBase ifAlarmSet = new IFSeqBase();
        IFSeqBase ifAlarmReset = new IFSeqBase();
        IFSeqBase ifMachineStatus = new IFSeqBase();

        public IFSeqBase ifEqRcpParaChange = new IFSeqBase();
        public IFSeqBase ifHostRcpParaChange = new IFSeqBase();
        public IFSeqBase ifRcpChange = new IFSeqBase();
        public IFSeqBase ifRcpDelete = new IFSeqBase();
        public IFSeqBase ifRcpCreate = new IFSeqBase();
        public IFSeqBase ifRcpUpload = new IFSeqBase();
        public IFSeqBase ifRcpSelected = new IFSeqBase();
        public IFSeqBase ifRcpList = new IFSeqBase();
        public IFSeqBase ifBladeId = new IFSeqBase();
        public IFSeqBase ifWorkingData = new IFSeqBase();
        public IFSeqBase ifGetSvId = new IFSeqBase();

        Queue<string> CmdQueue = new Queue<string>();

        int nSawTableNo = 0;
        #endregion

        #region TOR
        public IFSaw()
        {
            m_Receiver = new SocketUDP();
            m_Sender = new SocketUDP();
        }

        ~IFSaw()
        {

        }
        #endregion

        #region CONNECTION

        public void Start()
        {
            m_Receiver.m_EventRecvMsg += OnRecvData;
            m_Sender.m_EventSentMsg += OnSentData;

            m_Receiver.IPSetting(m_strIpLocal, m_nLocalPort, m_strIpRemote, m_nRemotePort, true);
            m_Sender.IPSetting(m_strIpLocal, m_nLocalPort, m_strIpRemote, m_nRemotePort, false);

            StartStopManualThread(true);
        }

        public void Stop()
        {
            StartStopManualThread(false);

            m_Receiver.m_EventRecvMsg -= OnRecvData;
            m_Sender.m_EventRecvMsg -= OnSentData;

            m_Receiver.Close();
            m_Sender.Close();
        }

        void StartStopManualThread(bool bStart)
        {
            if (threadRun != null)
            {
                flagThreadAlive = false;
                threadRun.Join(2000);
                threadRun.Abort();
                threadRun = null;
            }

            if (bStart)
            {
                flagThreadAlive = true;
                threadRun = new Thread(new ThreadStart(Run));
                threadRun.Name = "SAW COMM THREAD";
                if (threadRun.IsAlive == false)
                    threadRun.Start();
            }
        }

        #endregion

        #region THREAD

        private void Run()
        {
            while (flagThreadAlive)
            {
                Thread.Sleep(100);


                if (strRecv.Length > 0)
                {
                    int nPosStart = strRecv.IndexOf(STX);
                    int nPosEnd = strRecv.IndexOf(ETX);

                    if (nPosStart >= 0 && nPosEnd >= 0 && (nPosEnd > nPosStart))
                    {
                        nPosStart = strRecv.IndexOf(STX);
                        nPosEnd = strRecv.IndexOf(ETX);

                        string strCmd = strRecv.Substring(nPosStart, nPosEnd - nPosStart + 1);
                        CmdQueue.Enqueue(strCmd);

                        if (strRecv.Length > nPosEnd + 1)
                        {
                            strRecv = strRecv.Substring(nPosEnd + 1, strRecv.Length - (nPosEnd + 1));
                        }
                        else
                        {
                            strRecv = "";
                        }
                    }
                }

                if (CmdQueue.Count > 0)
                {
                    string strQueue = "";
                    strQueue = CmdQueue.Peek();
                    Parsing(strQueue);
                }

                #region Recv
                //EqRecipeParaChange();
                //EqRecipeDelete();
                //AlarmSet();
                //AlarmReset();
                //MachineStatus();
                #endregion

                #region Send
                //HostRcpParamChange();
                //RcpCreate();
                //RcpChange();
                //RcpDelete();
                //RcpUpload();
                //RcpSelected();
                //RcpList();
                //BladeId();
                //WorkingData();
                GetSvId();
                #endregion
            }
        }

        #endregion

        #region SAW -> SORTER CYCLE
        private int EqRecipeParaChange()
        {
            string strAnswer = "";

            switch (ifEqRcpParaChange.SEQNO)
            {
                case 0:
                    if (!ifEqRcpParaChange.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("PARA CHANGE | START"));

                    break;
                    
                case 1:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse == false) break;

                    string strRecipeName = ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA];
                    AddSawLog(string.Format("PARA CHANGE | PARA CHANGE REQUEST TO MES, PPID {0}", strRecipeName));

                    ifEqRcpParaChange.swTimeStamp.Restart();
                    break;

                case 2:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse == false) break;

                    if (ifEqRcpParaChange.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MES_REPLY_TIME].lValue)
                    {
                        strRecipeName = ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA];
                        AddSawLog(string.Format("PARA CHANGE | MES REPLY TIME-OUT MESSAGE_PARA_CHANGE, PPID {0}", strRecipeName));
                        ifEqRcpParaChange.swTimeStamp.Stop();

                        strAnswer = string.Format("{0},{1}", ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA]);
                        SendERROR(strAnswer);

                        ifEqRcpParaChange.SEQNO = 5;
                        return FNC.BUSY;
                    }

                    if (!ifEqRcpParaChange.SEQREPLY) return FNC.BUSY;
                    ifEqRcpParaChange.swTimeStamp.Stop();

                    break;

                case 3:
                    break;

                case 4:
                    if (bCheckAllBack == false)
                    {
                    }
                    strAnswer = string.Format("{0},{1}", ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA]);
                    SendOK(strAnswer);
                    break;

                case 5:
                    ifEqRcpParaChange.SEQSTART = false;
                    ifEqRcpParaChange.ResetCmd();
                    AddSawLog(string.Format("PARA CHANGE | INIT"));
                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifEqRcpParaChange.SEQNO > 10)
            {
                AddSawLog(string.Format("PARA CHANGE | SEQ FAULT OCCUR"));
                ifEqRcpParaChange.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifEqRcpParaChange.SEQNO++;
            return FNC.BUSY;
        }

        private int EqRecipeDelete()
        {
            switch (ifEqRcpDelete.SEQNO)
            {
                case 0:
                    if (!ifEqRcpDelete.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("RCP DELETE | START"));

                    break;

                case 1:
                    string strRecipeName = ifEqRcpDelete.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA];
                    string strAnswer = string.Format("{0},{1}", ifEqRcpDelete.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], strRecipeName);

                    if (strRecipeName == RecipeMgr.Inst.Rcp.strRecipeId)
                    {
                        SendERROR(strAnswer);
                    }
                    else
                    {
                        SendOK(strAnswer);
                    }
                    break;

                case 5:
                    ifEqRcpDelete.SEQSTART = false;
                    ifEqRcpDelete.ResetCmd();
                    AddSawLog(string.Format("RCP DELETE | INIT"));

                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifEqRcpDelete.SEQNO > 10)
            {
                AddSawLog(string.Format("RCP DELETE | SEQ FAULT OCCUR"));

                ifEqRcpDelete.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifEqRcpDelete.SEQNO++;
            return FNC.BUSY;
        }

        private int AlarmSet()
        {
            string strAnswer = "";

            switch (ifAlarmSet.SEQNO)
            {
                case 0:
                    if (!ifAlarmSet.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("ALARM SET | START"));
                    break;

                case 1:
                  int nAlarmNo = 0;
                    if (!int.TryParse(ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA], out nAlarmNo))
                    {
                        strAnswer = string.Format("{0},{1}", ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA]);
                        SendERROR(strAnswer);
                    }
                    else
                    {
                        strAnswer = string.Format("{0},{1}", ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA]);
                        SendOK(strAnswer);   
                    }
                    break;

                case 5:
                    ifAlarmSet.SEQSTART = false;
                    ifAlarmSet.ResetCmd();
                    AddSawLog(string.Format("ALARM SET | INIT"));

                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifAlarmSet.SEQNO > 10)
            {
                AddSawLog(string.Format("ALARM SET | SEQ FAULT OCCUR"));

                ifAlarmSet.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifAlarmSet.SEQNO++;
            return FNC.BUSY;
        }

        private int AlarmReset()
        {
            string strAnswer = "";

            switch (ifAlarmReset.SEQNO)
            {
                case 0:
                    if (!ifAlarmReset.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("ALARM RESET | START"));

                    break;
                case 1:
                    int nAlarmNo = 0;
                    if (!int.TryParse(ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA], out nAlarmNo))
                    {
                        strAnswer = string.Format("{0},{1}", ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA]);
                        SendERROR(strAnswer);
                    }
                    else
                    {
                        //0번째가 받은 커멘드 위치
                        strAnswer = string.Format("{0},{1}", ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA]);
                        SendOK(strAnswer);
                    }
                    break;

                case 5:
                    ifAlarmSet.SEQSTART = false;
                    ifAlarmReset.ResetCmd();
                    AddSawLog(string.Format("ALARM RESET | INIT"));

                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifAlarmReset.SEQNO > 10)
            {
                AddSawLog(string.Format("ALARM RESET | SEQ FAULT OCCUR"));

                ifAlarmReset.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifAlarmReset.SEQNO++;
            return FNC.BUSY;
        }

        private int MachineStatus()
        {
            string strAnswer = "";
            switch (ifMachineStatus.SEQNO)
            {
                case 0:
                    if (!ifMachineStatus.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("MC STATUS | START"));

                    break;

                case 1:
                    string strStatus = ifMachineStatus.SEQDATA[(int)eREQ_IDX_FROMSAW.DATA];
                    strAnswer = string.Format("{0},{1}", ifMachineStatus.SEQDATA[(int)eREQ_IDX_FROMSAW.CMD], strStatus);
                    SendOK(strAnswer);

                    break;

                case 5:
                    ifMachineStatus.SEQSTART = false;
                    ifMachineStatus.ResetCmd();
                    AddSawLog(string.Format("MC STATUS | INIT"));

                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifMachineStatus.SEQNO > 10)
            {
                AddSawLog(string.Format("MC STATUS | SEQ FAULT OCCUR"));

                ifMachineStatus.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifMachineStatus.SEQNO++;
            return FNC.BUSY;
        }

        #endregion

        #region SORTER -> SAW CYCLE
        public void GetSvValue()
        {
            if (ifGetSvId.SEQSTART == true) return;
            ifGetSvId.SEQSTART = true;
        }

        private int HostRcpParamChange()
        {
            switch (ifHostRcpParaChange.SEQNO)
            {
                case 0:
                    if (!ifHostRcpParaChange.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("HOST PARA CHANGE | START"));

                    break;

                case 1:
                    SendCMD(eTOSAW.RCP_PARACHANGE, ifHostRcpParaChange.PPID);//1번 배열에 레시피 이름
                    AddSawLog(string.Format("HOST PARA CHANGE | HOST PARA CHANGE REQUEST TO SAW, PPID : {0}", ifHostRcpParaChange.PPID));

                    ifHostRcpParaChange.swTimeStamp.Restart();

                    break;

                case 2:
                    if (ifHostRcpParaChange.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("HOST PARA CHANGE | SAW REPLY TIME-OUT!!"));

                        ifHostRcpParaChange.swTimeStamp.Stop();
                        ifHostRcpParaChange.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifHostRcpParaChange.SEQREPLY) return FNC.BUSY;
                    AddSawLog(string.Format("HOST PARA CHANGE | RECEIVE SAW REPLY"));
                    ifHostRcpParaChange.swTimeStamp.Stop();
                    break;

                case 4:
                    //if (ifHostRcpParaChange.RETURN != "OK")
                    //{
                    //    AddSawLog(string.Format("HOST PARA CHANGE | SAW RETURN IS ERROR!!"));
                    //}
                    //else
                    //{
                    //    AddSawLog(string.Format("HOST PARA CHANGE | SAW RETURN IS OK"));
                    //}
                    break;

                case 5:
                    ifHostRcpParaChange.SEQSTART = false;
                    ifHostRcpParaChange.SemiResetCmd();
                    AddSawLog(string.Format("HOST PARA CHANGE | INIT"));
                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifHostRcpParaChange.SEQNO > 10)
            {
                AddSawLog(string.Format("HOST PARA CHANGE | SEQ FAULT OCCURS"));

                ifHostRcpParaChange.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifHostRcpParaChange.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpCreate()
        {
            switch (ifRcpCreate.SEQNO)
            {
                case 0:
                    if (!ifRcpCreate.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("RCP CREATE | START"));

                    break;

                case 1:
                    SendCMD(eTOSAW.RCP_CREATE, ifRcpCreate.PPID);
                    ifRcpCreate.swTimeStamp.Restart();
                    AddSawLog(string.Format("RCP CREATE | RCP CREATE REQUEST TO SAW, PPID : {0}", ifRcpCreate.PPID));
                    break;

                case 2:
                    if (ifRcpCreate.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("RCP CREATE | SAW REPLY TIME-OUT!!, PPID : {0}", ifRcpCreate.PPID));

                        ifRcpCreate.swTimeStamp.Stop();
                        ifRcpCreate.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifRcpCreate.SEQREPLY) return FNC.BUSY;
                    AddSawLog(string.Format("RCP CREATE | RECEIVE SAW REPLY, PPID : {0}", ifRcpCreate.PPID));

                    ifRcpCreate.swTimeStamp.Stop();
                    break;

                case 3:
                    //if (ifRcpCreate.RETURN != "OK")
                    //{
                    //    AddSawLog(string.Format("RCP CREATE | SAW RETURN IS ERROR!!, PPID : {0}", ifRcpCreate.PPID));
                    //}
                    //else
                    //{
                    //    AddSawLog(string.Format("RCP CREATE | SAW RETURN IS OK, PPID : {0}", ifRcpCreate.PPID));
                    //}
                    break;

                case 5:
                    ifRcpCreate.SEQSTART = false;
                    ifRcpCreate.SemiResetCmd();
                    AddSawLog(string.Format("RCP CREATE | INIT"));
                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifRcpCreate.SEQNO > 10)
            {
                AddSawLog(string.Format("RCP CREATE | SEQ FAULT OCCURS"));

                ifRcpCreate.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifRcpCreate.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpChange()
        {
            switch (ifRcpChange.SEQNO)
            {
                case 0:
                    if (!ifRcpChange.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("RCP CHANGE | START"));

                    break;

                case 1:
                    SendCMD(eTOSAW.RCP_CHANGE, ifRcpChange.PPID);
                    AddSawLog(string.Format("RCP CHANGE | RCP CHANGE REQUEST TO SAW, PPID : {0}", ifRcpChange.PPID));

                    ifRcpChange.swTimeStamp.Restart();
                    break;


                case 2:
                    if (ifRcpChange.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("RCP CHANGE | SAW REPLY TIME-OUT!!, PPID : {0}", ifRcpChange.PPID));

                        ifRcpChange.swTimeStamp.Stop();
                        ifRcpChange.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifRcpChange.SEQREPLY) return FNC.BUSY;
                    AddSawLog(string.Format("RCP CHANGE | RECEIVE SAW REPLY, PPID : {0}", ifRcpChange.PPID));

                    ifRcpChange.swTimeStamp.Stop();
                    break;

                case 3:
                    //if (ifRcpChange.RETURN != "OK")
                    //{
                    //    AddSawLog(string.Format("RCP CHANGE | SAW RETURN IS ERROR!!, PPID : {0}", ifRcpChange.PPID));
                    //}
                    //else
                    //{
                    //    AddSawLog(string.Format("RCP CHANGE | SAW RETURN IS OK, PPID : {0}", ifRcpChange.PPID));
                    //}
                    break;

                case 5:
                    ifRcpChange.SEQSTART = false;
                    ifRcpChange.SemiResetCmd();
                    AddSawLog(string.Format("RCP CHANGE | INIT"));
                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifRcpChange.SEQNO > 10)
            {
                AddSawLog(string.Format("RCP CHANGE | SEQ FAULT OCCURS"));

                ifRcpChange.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifRcpChange.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpDelete()
        {
            switch (ifRcpDelete.SEQNO)
            {
                case 0:
                    if (!ifRcpDelete.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("RCP DELETE | START"));

                    break;

                case 1:
                    SendCMD(eTOSAW.RCP_DELETE, ifRcpDelete.PPID);
                    AddSawLog(string.Format("RCP DELETE | RCP DELETE REQUEST TO SAW, PPID : {0}", ifRcpDelete.PPID));

                    ifRcpDelete.swTimeStamp.Restart();
                    break;

                case 2:
                    if (ifRcpDelete.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("RCP DELETE | SAW REPLY TIME-OUT!!, PPID : {0}", ifRcpDelete.PPID));

                        ifRcpDelete.swTimeStamp.Stop();
                        ifRcpDelete.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifRcpDelete.SEQREPLY) return FNC.BUSY;
                    AddSawLog(string.Format("RCP DELETE | RECEIVE SAW REPLY, PPID : {0}", ifRcpDelete.PPID));

                    ifRcpDelete.swTimeStamp.Stop();
                    break;

                case 3:
                    //if (ifRcpDelete.RETURN != "OK")
                    //{
                    //    AddSawLog(string.Format("RCP DELETE | SAW RETURN IS ERROR!!, PPID : {0}", ifRcpDelete.PPID));
                    //}
                    //else
                    //{
                    //    AddSawLog(string.Format("RCP DELETE | SAW RETURN IS OK, PPID : {0}", ifRcpDelete.PPID));
                    //}
                    break;

                case 5:
                    ifRcpDelete.SEQSTART = false;
                    ifRcpDelete.SemiResetCmd();
                    AddSawLog(string.Format("RCP DELETE | INIT"));

                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifRcpDelete.SEQNO > 10)
            {
                AddSawLog(string.Format("RCP DELETE | SEQ FAULT OCCURS"));

                ifRcpDelete.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifRcpDelete.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpUpload()
        {
            switch (ifRcpUpload.SEQNO)
            {
                case 0:
                    if (!ifRcpUpload.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("RCP UPLOAD | START"));

                    break;

                case 1:
                    SendCMD(eTOSAW.RCP_UPLOAD, ifRcpUpload.PPID);
                    AddSawLog(string.Format("RCP UPLOAD | RCP UPLOAD REQUEST TO SAW, PPID : {0}", ifRcpUpload.PPID));

                    ifRcpUpload.swTimeStamp.Restart();
                    break;

                case 2:
                    if (ifRcpUpload.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("RCP UPLOAD | SAW REPLY TIME-OUT!!, PPID : {0}", ifRcpUpload.PPID));

                        ifRcpUpload.swTimeStamp.Stop();
                        ifRcpUpload.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifRcpUpload.SEQREPLY) return FNC.BUSY;
                    AddSawLog(string.Format("RCP UPLOAD | RECEIVE SAW REPLY, PPID : {0}", ifRcpUpload.PPID));

                    ifRcpUpload.swTimeStamp.Stop();
                    break;
                case 3:
                    //if (ifRcpUpload.RETURN != "OK")
                    //{
                    //    AddSawLog(string.Format("RCP UPLOAD | SAW RETURN IS ERROR!!, PPID : {0}", ifRcpUpload.PPID));
                    //}
                    //else
                    //{
                    //    AddSawLog(string.Format("RCP UPLOAD | SAW RETURN IS OK, PPID : {0}", ifRcpUpload.PPID));
                    //}
                    break;
                case 5:
                    ifRcpUpload.SEQSTART = false;
                    ifRcpUpload.SemiResetCmd();
                    AddSawLog(string.Format("RCP UPLOAD | INIT"));
                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifRcpUpload.SEQNO > 10)
            {
                AddSawLog(string.Format("RCP UPLOAD | SEQ FAULT OCCURS"));

                ifRcpUpload.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifRcpUpload.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpSelected()
        {
            switch (ifRcpSelected.SEQNO)
            {
                case 0:
                    if (!ifRcpSelected.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("RCP SELECTED | START"));

                    break;

                case 1:
                    SendCMD(eTOSAW.RCP_SELECTED);
                    AddSawLog(string.Format("RCP SELECTED | RCP SELECTED REQUEST TO SAW"));

                    ifRcpSelected.swTimeStamp.Restart();
                    break;

                case 2:
                    if (ifRcpSelected.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("RCP SELECTED | SAW REPLY TIME-OUT!!"));

                        ifRcpSelected.swTimeStamp.Stop();
                        ifRcpSelected.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifRcpSelected.SEQREPLY) return FNC.BUSY;
                    AddSawLog(string.Format("RCP SELECTED | RECEIVE SAW REPLY"));

                    ifRcpSelected.swTimeStamp.Stop();
                    break;

                case 3:
                    if (ifRcpSelected.RETURN != "OK")
                    {
                        AddSawLog(string.Format("RCP SELECTED | SAW RETURN IS ERROR!!"));
                    }
                    else
                    {
                        string str = ifRcpSelected.SEQDATA[(int)eRES_IDX_FROMSAW.DATA];
                        AddSawLog(string.Format("RCP SELECTED | SAW RETURN IS OK, PPID : {0}", str));
                    }
                    break;

                case 5:
                    ifRcpSelected.SEQSTART = false;
                    ifRcpSelected.ResetCmd();
                    AddSawLog(string.Format("RCP SELECTED | INIT"));

                    return FNC.SUCCESS;


                default:
                    break;
            }
            //wrong seq check
            if (ifRcpSelected.SEQNO > 10)
            {
                AddSawLog(string.Format("RCP SELECTED | SEQ FAULT OCCURS"));

                ifRcpSelected.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifRcpSelected.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpList()
        {
            switch (ifRcpList.SEQNO)
            {
                case 0:
                    if (!ifRcpList.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("RCP LIST | START"));

                    break;

                case 1:
                    SendCMD(eTOSAW.RCP_LIST);
                    AddSawLog(string.Format("RCP LIST | RCP LIST REQUEST TO SAW"));

                    ifRcpList.swTimeStamp.Restart();
                    break;

                case 2:
                    if (ifRcpList.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("RCP LIST | SAW REPLY TIME-OUT!!"));

                        ifRcpList.swTimeStamp.Stop();
                        ifRcpList.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifRcpList.SEQREPLY) return FNC.BUSY;
                    ifRcpList.swTimeStamp.Stop();
                    break;

                case 3:
                    if (ifRcpList.RETURN != "OK")
                    {
                        AddSawLog(string.Format("RCP LIST | SAW RETURN IS ERROR!!"));
                    }
                    else
                    {
                        AddSawLog(string.Format("RCP LIST | SAW RETURN IS OK!!"));

                        AddSawLog(string.Format("----------- RECEIVED RCP LIST -----------"));

                        //받은 레시피 리스트 이름확인
                        string str = "";
                        for (int i = (int)eRES_IDX_FROMSAW.DATA; i < ifRcpList.SEQDATA.Length - 1; i++)
                        {
                            str = ifRcpList.SEQDATA[i];
                            AddSawLog(string.Format("{0}", str));
                        }
                    }
                    break;

                case 5:
                    ifRcpList.SEQSTART = false;
                    ifRcpList.ResetCmd();
                    AddSawLog(string.Format("RCP LIST | INIT"));
                    return FNC.SUCCESS;


                default:
                    break;
            }
            //wrong seq check
            if (ifRcpList.SEQNO > 10)
            {
                AddSawLog(string.Format("RCP LIST | SEQ FAULT OCCURS"));

                ifRcpList.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifRcpList.SEQNO++;
            return FNC.BUSY;
        }

        private int BladeId()
        {
            switch (ifBladeId.SEQNO)
            {
                case 0:
                    if (!ifBladeId.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("BLADE ID | START"));
                    break;

                case 1:
                    SendCMD(eTOSAW.BLADE_ID);
                    break;

                case 2:
                    if (ifBladeId.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("BLADE ID | SAW REPLY TIME-OUT!!"));

                        ifBladeId.swTimeStamp.Stop();
                        ifBladeId.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifBladeId.SEQREPLY) return FNC.BUSY;
                    ifBladeId.swTimeStamp.Stop();
                    break;

                case 3:
                    if (ifBladeId.RETURN != "OK")
                    {
                        AddSawLog(string.Format("BLADE ID | SAW RETURN IS ERROR!!"));
                    }
                    else
                    {
                        AddSawLog(string.Format("BLADE ID | SAW RETURN IS OK!!"));

                        //받은 BLADE ID 확인
                        string str = "";
                        for (int i = (int)eRES_IDX_FROMSAW.DATA; i < ifBladeId.SEQDATA.Length - 1; i++)
                        {
                            str += ifBladeId.SEQDATA[i] + ",";
                        }
                        str = str.TrimEnd(',');
                        AddSawLog(string.Format("BLADE ID | BLADE ID : {0}", str));
                    }
                    break;

                case 5:
                    ifBladeId.SEQSTART = false;
                    ifBladeId.ResetCmd();
                    AddSawLog(string.Format("BLADE ID | INIT"));
                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifBladeId.SEQNO > 10)
            {
                AddSawLog(string.Format("BLADE ID | SEQ FAULT OCCURS"));

                ifBladeId.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifBladeId.SEQNO++;
            return FNC.BUSY;
        }

        private int WorkingData()
        {
            switch (ifWorkingData.SEQNO)
            {
                case 0:
                    if (!ifWorkingData.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("WORKING DATA | START"));
                    break;

                case 1:
                    SendCMD(eTOSAW.WORKING_DATA, nSawTableNo.ToString());
                    break;

                case 2:
                    if (ifWorkingData.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("WORKING DATA | SAW REPLY TIME-OUT!!"));

                        ifWorkingData.swTimeStamp.Stop();
                        ifWorkingData.SEQNO = 5;
                        return FNC.BUSY;
                    }
                    if (!ifWorkingData.SEQREPLY) return FNC.BUSY;
                    ifWorkingData.swTimeStamp.Stop();
                    break;

                case 3:
                    if (ifWorkingData.RETURN != "OK")
                    {
                        AddSawLog(string.Format("WORKING DATA | SAW RETURN IS ERROR!!"));
                    }
                    else
                    {
                        MESDF.eWORKING_DATA Index = MESDF.eWORKING_DATA.MAX;

                        //0번째 = ANSWER, 1번째 = CMD, 2번째 = 테이블 번호, 3번째~끝까지 = 데이터
                        for (int i = 3; i < ifWorkingData.SEQDATA.Length; i++)
                        {
                            //아이템이름=값 과 같은 형태로 넘어옴
                            //ex) speed1=1,speed0=1,speed3=1,speed2=2...
                            string[] data = ifWorkingData.SEQDATA[i].Split('=');

                            int nValue = 0;
                            if (data.Length >= 2)
                            {
                                //0번째 = 아이템 이름, 1번째 = 값
                                if (!int.TryParse(data[1], out nValue)) nValue = 0;
                            }
                            if (!Enum.TryParse<MESDF.eWORKING_DATA>(data[0], out Index)) continue;

                            GbVar.WorkingData[(int)Index] = nValue;
                        }

                        AddSawLog(string.Format("WORKING DATA | SAW RETURN IS OK!!"));
                    }
                    break;

                case 5:
                    ifWorkingData.SEQSTART = false;
                    ifWorkingData.ResetCmd();
                    AddSawLog(string.Format("WORKING DATA | INIT"));

                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifWorkingData.SEQNO > 10)
            {
                AddSawLog(string.Format("WORKING DATA | SEQ FAULT OCCURS"));

                ifWorkingData.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifWorkingData.SEQNO++;
            return FNC.BUSY;
        }


        private int GetSvId()
        {
            switch (ifGetSvId.SEQNO)
            {
                case 0:
                    if (!ifGetSvId.SEQSTART) return FNC.BUSY;
                    AddSawLog(string.Format("GET SVID | START"));
                    break;

                case 1:
                    SendCMD(eTOSAW.GET_SVID);
                    ifGetSvId.swTimeStamp.Restart();
                    //AddSawLog(string.Format("GET SVID | TIMER RESTART!!"));
                    break;

                case 2:
                    if (ifGetSvId.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue)
                    {
                        AddSawLog(string.Format("GET SVID | SAW REPLY TIME-OUT!"));

                        ifGetSvId.swTimeStamp.Stop();
                        ifGetSvId.SEQNO = 6;
                        return FNC.BUSY;
                    }
                    if (!ifGetSvId.SEQREPLY) return FNC.BUSY;

                    AddSawLog(string.Format("GET SVID | VALUE RECEIVE!"));
                    ifGetSvId.swTimeStamp.Stop();
                    break;

                case 3:
                    if (ifGetSvId.RETURN != "OK")
                    {
                        AddSawLog(string.Format("GET SVID | SAW RETURN IS ERROR!!"));
                        ifGetSvId.SEQNO = 6;
                        return FNC.BUSY;
                    }
                    else
                    {
                        AddSawLog(string.Format("GET SVID | SAW RETURN IS OK!!"));

                        string[] strArr;
                        //처음과 마지막 배열은 SV데이터가 아님
                        for (int nCnt = (int)eRES_IDX_FROMSAW.DATA; nCnt < ifGetSvId.SEQDATA.Length - 1; nCnt++)
                        {
                            strArr = ifGetSvId.SEQDATA[nCnt].Split(':');

                            if (strArr.Length != 2)
                                continue;

                            GbVar.g_dictSawSVID.AddOrUpdate(strArr[0], strArr[1],
                                (strSVID_Key, strSVID_Value) =>
                                {
                                    strSVID_Value = strArr[1];

                                    return strSVID_Value;
                                });
                        }
                    }
                    break;

                case 5:
                    //MESDF.UpdatedSvIdSaw = true;
                    break;

                case 6:
                    ifGetSvId.SEQSTART = false;
                    ifGetSvId.ResetCmd();
                    AddSawLog(string.Format("GET SVID | INIT"));
                    return FNC.SUCCESS;

                default:
                    break;
            }
            //wrong seq check
            if (ifGetSvId.SEQNO > 10)
            {
                AddSawLog(string.Format("GET SVID | SEQ FAULT OCCURS"));

                ifGetSvId.SEQFAULT = true;
                return FNC.BUSY;
            }
            ifGetSvId.SEQNO++;
            return FNC.BUSY;
        }
        #endregion

        #region SEND RECV
        public void SendOK(string strMsg)
        {
            strMsg = "ANSWER," + strMsg + ",OK";
            Send(strMsg);

            AddSawLog(string.Format("OK SEND TO SAW | {0}", strMsg));
        }

        public void SendERROR(string strMsg)
        {
            strMsg = "ANSWER," + strMsg + ",ERROR";
            Send(strMsg);
            AddSawLog(string.Format("ERROR SEND TO SAW | {0}", strMsg));
        }

        public void SendCMD(eTOSAW eCmd, string strData)
        {
            string strMsg = "";
            strMsg = eCmd.ToString() + "," + strData;
            Send(strMsg);
            AddSawLog(string.Format("CMD SEND TO SAW | {0}", strMsg));
        }
            
        public void SendCMD(eTOSAW eCmd)
        {
            string strMsg = "";
            strMsg = eCmd.ToString();
            Send(strMsg);
            AddSawLog(string.Format("CMD SEND TO VISION | {0}", strMsg));
        }

        public void Send(string strMsg)
        {
            ASCIIEncoding encondeing = new ASCIIEncoding();
            string sSend = "@," + strMsg + ",*";
            byte[] bBuffer = encondeing.GetBytes(sSend);
            m_Sender.SendData(bBuffer);
        }

        private void OnRecvData(byte[] buffer)
        {
            if (buffer.Length <= 0) return;

            string strbuffer = Encoding.ASCII.GetString(buffer);
            strRecv += strbuffer;
        }

        private void OnSentData(byte[] buffer)
        {
            string strSendData = Encoding.ASCII.GetString(buffer);
            if (OnSendRecvEvent != null)
            {
                OnSendRecvEvent(false, strSendData);
            }
        }

        #endregion

        #region PARSING

        private void Parsing(string strRecv)
        {
            AddSawLog(string.Format("MESSAGE RECEIVED FROM SAW | {0}", strRecv));

            string[] Data = strRecv.Substring(2, strRecv.Length - 4).Split(',');
            if (Data.Length < 2)
            {
                CmdQueue.Dequeue();
                return;
            }

            if (Enum.IsDefined(typeof(eFROMSAW), Data[(int)eREQ_IDX_FROMSAW.CMD]))
            {
                eFROMSAW efromSaw = (eFROMSAW)Enum.Parse(typeof(eFROMSAW), Data[(int)eREQ_IDX_FROMSAW.CMD]);

                #region SAW -> SORTER ANSWER
                if (efromSaw == eFROMSAW.ANSWER)
                {
                    if (Enum.IsDefined(typeof(eTOSAW), Data[(int)eRES_IDX_FROMSAW.CMD]))
                    {
                        //보낸 문자를 응답으로 다시 받아 파징.
                        eTOSAW efromSawRes = (eTOSAW)Enum.Parse(typeof(eTOSAW), Data[(int)eRES_IDX_FROMSAW.CMD]);

                        switch (efromSawRes)
                        {
                            case eTOSAW.RCP_PARACHANGE:
                                ifHostRcpParaChange.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자

                                ifHostRcpParaChange.SEQREPLY = true;
                                CmdQueue.Dequeue();

                                break;
                            case eTOSAW.RCP_CHANGE:
                                ifRcpChange.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

                                ifRcpChange.SEQREPLY = true;
                                CmdQueue.Dequeue();

                                break;
                            case eTOSAW.RCP_CREATE:
                                ifRcpCreate.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

                                ifRcpCreate.SEQREPLY = true;
                                CmdQueue.Dequeue();
                                break;
                            case eTOSAW.RCP_DELETE:
                                ifRcpDelete.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

                                ifRcpDelete.SEQREPLY = true;
                                CmdQueue.Dequeue();
                                break;
                            case eTOSAW.RCP_UPLOAD:
                                ifRcpUpload.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

                                ifRcpUpload.SEQREPLY = true;
                                CmdQueue.Dequeue();

                                break;
                            case eTOSAW.RCP_SELECTED:
                                ifRcpSelected.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;
                                ifRcpSelected.SEQDATA = Data;

                                ifRcpSelected.SEQREPLY = true;
                                CmdQueue.Dequeue();

                                break;
                            case eTOSAW.RCP_LIST:
                                ifRcpList.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;
                                ifRcpList.SEQDATA = Data;

                                ifRcpList.SEQREPLY = true;
                                CmdQueue.Dequeue();
                                break;

                            case eTOSAW.BLADE_ID:
                                ifBladeId.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;
                                ifBladeId.SEQDATA = Data;

                                ifBladeId.SEQREPLY = true;
                                CmdQueue.Dequeue();

                                break;

                            case eTOSAW.WORKING_DATA:
                                ifWorkingData.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;
                                ifWorkingData.SEQDATA = Data;

                                ifWorkingData.SEQREPLY = true;
                                CmdQueue.Dequeue();
                                break;

                            case eTOSAW.GET_SVID:
                                ifGetSvId.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;
                                ifGetSvId.SEQDATA = Data;

                                ifGetSvId.SEQREPLY = true;
                                CmdQueue.Dequeue();
                                break;

                            default:
                                break;
                        }
                    }
                }
                #endregion

                #region SAW -> SORTER REQ
                else
                {
                    switch (efromSaw)
                    {
                        case eFROMSAW.RCP_PARACHANGE:
                            //if (ifEqRcpParaChange.SEQSTART == true) break;

                            ifEqRcpParaChange.SEQDATA = Data;
                            ifEqRcpParaChange.SEQSTART = true;
                            CmdQueue.Dequeue();
                            break;
                        case eFROMSAW.RCP_DELETE:
                            //if (ifEqRcpDelete.SEQSTART == true) break;

                            ifEqRcpDelete.SEQDATA = Data;
                            ifEqRcpDelete.SEQSTART = true;
                            CmdQueue.Dequeue();
                            break;
                        case eFROMSAW.ALARM_SET:
                            //if (ifAlarmSet.SEQSTART == true) break;

                            ifAlarmSet.SEQDATA = Data;
                            ifAlarmSet.SEQSTART = true;
                            CmdQueue.Dequeue();
                            break;
                        case eFROMSAW.ALARM_RESET:
                            //if (ifAlarmReset.SEQSTART == true) break;

                            ifAlarmReset.SEQDATA = Data;
                            ifAlarmReset.SEQSTART = true;
                            CmdQueue.Dequeue();
                            break;
                        case eFROMSAW.MACHINE_STATUS:
                            //if (ifMachineStatus.SEQSTART == true) break;

                            ifMachineStatus.SEQDATA = Data;
                            ifMachineStatus.SEQSTART = true;
                            CmdQueue.Dequeue();
                            break;

                        case eFROMSAW.BLADE_ID:
                            //if (ifBladeId.SEQSTART == true) break;

                            ifBladeId.SEQDATA = Data;
                            ifBladeId.SEQSTART = true;
                            CmdQueue.Dequeue();
                            break;

                        default:
                            break;
                    }
                }
                #endregion
            }
            else
            {
                CmdQueue.Dequeue();
            }
        }

        #endregion

        public void RequestWorkingData(int nTable)
        {
            nSawTableNo = nTable;
            ifWorkingData.SEQSTART = true;
        }

        #endregion

        #region PIO
        public bool IsProgramOn()
        {
            return MotionMgr.Inst.GetInput(IODF.INPUT.SAW_PROGRAM_ON);
        }

        public bool IsInitializing()
        {
            return MotionMgr.Inst.GetInput(IODF.INPUT.SAW_INITIALIZING);
        }

        public void SetEqpProgramOn(bool bOn)
        {
            //MotionMgr.Inst.SetOutput(IODF.OUTPUT.SORTER_PROGRAM_ON, bOn);
        }

        #region STRIP TRANSFER
        public bool IsSawRun()
        { 
            return MotionMgr.Inst.GetInput(IODF.INPUT.SAW_RUN);
        }

        public bool IsTableCuttingRun()
        {
            IODF.INPUT input = IODF.INPUT.SAW_CUTTING;

            return MotionMgr.Inst.GetInput(input);
        }

        public bool IsTableLoadingReq()
        {
            //IODF.INPUT input = IODF.INPUT.SAW_LD_REQUISITION_L;
            IODF.INPUT input = IODF.INPUT.SAW_LD_REQUISITION_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public bool IsTableLoadingPos()
        {
            //IODF.INPUT input = IODF.INPUT.SAW_LD_POS;
            IODF.INPUT input = IODF.INPUT.SAW_LD_POS_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public bool IsTableLoadingVacOn()
        {
            //IODF.INPUT input = IODF.INPUT.SAW_STAGE_VAC_ON;
            IODF.INPUT input = IODF.INPUT.SAW_STAGE_VAC_ON_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public bool IsTableLoadingComplete()
        {
            IODF.INPUT input = IODF.INPUT.SAW_ALIGN_COMP_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public bool IsTableReloadReq()
        {
            //IODF.INPUT input = IODF.INPUT.SAW_REPICK_ALIGN_L;
            IODF.INPUT input = IODF.INPUT.SAW_REPICK_ALIGN_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public void SetStripTransferXLoadPos(bool bOn)
        {
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_X;

            MotionMgr.Inst.SetOutput(output, bOn);
        }

        public void SetStripTransferZLoadPos(bool bOn)
        {
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_Z;

            MotionMgr.Inst.SetOutput(output, bOn);
        }

        public void SetStripTransferLoadComplete(bool bOn)
        {
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_LD_OK_R;

            MotionMgr.Inst.SetOutput(output, bOn);
        }

        public void SetTableBlowOn(bool bOn)
        {
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_BLOW_OFF_R;

            MotionMgr.Inst.SetOutput(output, bOn);
        }

        public void ResetStripTransferIF()
        {
            IODF.OUTPUT output1 = IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_X;
            IODF.OUTPUT output2 = IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_Z;
            IODF.OUTPUT output3 = IODF.OUTPUT.SORTER_LD_OK_R;
            IODF.OUTPUT output4 = IODF.OUTPUT.SORTER_BLOW_OFF_R;

            MotionMgr.Inst.SetOutput(output1, false);
            MotionMgr.Inst.SetOutput(output2, false);
            MotionMgr.Inst.SetOutput(output3, false);
            MotionMgr.Inst.SetOutput(output4, false);
        }
        #endregion

        #region UNIT TRANSFER
        public bool IsTableUnloadingReq()
        {
            //IODF.INPUT input = IODF.INPUT.SAW_ULD_REQUISITION;
            IODF.INPUT input = IODF.INPUT.SAW_ULD_REQUISITION_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public bool IsTableUnloadingPos()
        {
            //IODF.INPUT input = IODF.INPUT.SAW_ULD_POS;
            IODF.INPUT input = IODF.INPUT.SAW_ULD_POS_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public bool IsTableUnloadingBlowOn()
        {
            //IODF.INPUT input = IODF.INPUT.SAW_STG_BLOW_ON;
            IODF.INPUT input = IODF.INPUT.SAW_STG_BLOW_ON_R;

            return MotionMgr.Inst.GetInput(input);
        }

        public void SetUnitTransferXUnloadPos( bool bOn)
        {
            //IODF.OUTPUT output = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_X;
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_X;

            MotionMgr.Inst.SetOutput(output, bOn);
        }

        public void SetUnitTransferZUnloadPos(bool bOn)
        {
            //IODF.OUTPUT output = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_Z;
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_Z;

            MotionMgr.Inst.SetOutput(output, bOn);
        }

        public void SetUnitTransferUnloadComplete( bool bOn)
        {
            //IODF.OUTPUT output = IODF.OUTPUT.SORTER_ULD_OK;
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_ULD_OK_R;

            MotionMgr.Inst.SetOutput(output, bOn);
        }

        public void ResetUnitTransferIF()
        {
            //IODF.OUTPUT output1 = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_X;
            //IODF.OUTPUT output2 = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_Z;
            //IODF.OUTPUT output3 = IODF.OUTPUT.SORTER_ULD_OK;
            IODF.OUTPUT output1 = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_X;
            IODF.OUTPUT output2 = IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_Z;
            IODF.OUTPUT output3 = IODF.OUTPUT.SORTER_ULD_OK_R;
            if (GbVar.IO[output1] == 1)
                MotionMgr.Inst.SetOutput(output1, false);
            if (GbVar.IO[output2] == 1)
                MotionMgr.Inst.SetOutput(output2, false);
            if (GbVar.IO[output3] == 1)
                MotionMgr.Inst.SetOutput(output3, false);
        }

        #endregion

        public void SetTransferZDownInterlock(bool bOn)
        {
            IODF.OUTPUT output = IODF.OUTPUT.SORTER_PK_INTERLOCK;

            MotionMgr.Inst.SetOutput(output, bOn);
            output = IODF.OUTPUT.SORTER_PK_INTERLOCK_R;

            MotionMgr.Inst.SetOutput(output, bOn);
        }
        #endregion

        #region DB
        /// <summary>
        /// Reload X보정 값을 가져옵니다.
        /// </summary>
        /// <returns>X보정 값</returns>
        public double GetReloadOffset()
        {
            double dOffset = 0.0f;

            try 
	        {
                // 0번째 : X보정값, 1번째 : Y보정 값(사용안함), 2번째 : T보정값(사용 안함)
                string[] strResult = new string[3];
                strResult = GbVar.dbFromSaw.SelectResult();

                if (strResult != null)
                {
                    if (!double.TryParse(strResult[0], out dOffset)) 
                        dOffset = 0.0f;
                }

                return dOffset;
	        }
	        catch (Exception)
	        {
                dOffset = 0.0f;
	        }

            return dOffset;
        }

        public bool SetPreAlignOffset(double dOffsetY, double dOffsetT)
        {
            bool bret = true;

            try
            {
                bret = GbVar.dbFromSorter.UpdateAllData("0", dOffsetY.ToString(), dOffsetT.ToString());
            }
            catch (Exception)
            {
                bret = false;
            }

            return bret; 
        }

        #endregion

        private void AddSawLog(string strLog)
        {
            if (OnLogAddSaw != null)
                OnLogAddSaw(strLog);

            GbFunc.WriteIFLog((int)DBDF.LOG_TYPE.IF_SAW, DateTime.Now.ToString("yyyyMMddHHmmssfff"), strLog);
        }
    }
}
