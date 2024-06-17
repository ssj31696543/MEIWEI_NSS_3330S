using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using LinkMessage;
using DionesTool.SOCKET;
using NSS_3330S.MOTION;
using System.Collections.Concurrent;
using DionesTool.UTIL;
using DionesTool.Objects;
using System.IO;

namespace NSS_3330S
{
    public class IFVision
    {

        #region SOCKET

        #region ENUM
        public enum eFROMVISION
        {
            RCP_PARACHANGE,
            RCP_DELETE,
            ALARM_SET,
            ALARM_RESET,
            MACHINE_STATUS,
            ANSWER,
        }

        public enum eTOVISION
        {
            RCP_PARACHANGE,
            RCP_CHANGE,
            RCP_CREATE,
            RCP_DELETE,
            RCP_UPLOAD,
            RCP_SELECTED,
            RCP_LIST,
        }

        /// <summary>
        /// [2022.04.06. kmlee] UDP 통신 명령 추가
        /// </summary>
        public enum E_VS_SEND_CMD
        {
            TOP_LIVE = 0,
            BTM_LIVE,
            HD1_LIVE,
            HD2_LIVE,
            PREALIGN_LIVE,
            JOB_CHANGE,
            GET_SVID_VISION,
            JOG_ERROR,
        }

        public enum E_VS_RECV_CMD
        {
            MAPBLOCK1_TEACHING = 0,
            MAPBLOCK2_TEACHING,
            X_PITCH,
            Y1_PITCH,
            Y2_PITCH,
            Z_PITCH,
            SVID,
        }

        public enum eREQ_IDX_FROMVISION
        {
            CMD = 0,
            DATA
        }

        public enum eRES_IDX_FROMVISION
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

        public delegate void LogAddVision(string strLog);
        public event LogAddVision OnLogAddVision;
        #endregion

        #region VAR
        public SocketUDP m_Receiver = null;
        public SocketUDP m_Sender = null;
        //public SocketUDP m_UDP = null;

//#if TEST
//        private string m_strIpLocal = "127.0.0.1";
//        private string m_strIpRemote = "127.0.0.1";
//#else
        private string m_strIpLocal = "192.168.100.1";
        private string m_strIpRemote = "192.168.100.3";
//#endif

		// 6000번 대가 최종
        private int m_nLocalPort = 6000;  //수신포트
        private int m_nRemotePort = 6002; //송신포트


        Thread threadRun;
        bool flagThreadAlive = false;
        bool bCheckAllBack = false;

        private string strRecv = "";

        ConcurrentQueue<string> CmdQueue = new ConcurrentQueue<string>();

        public IFSeqBase ifEqRcpParaChange = new IFSeqBase();
        public IFSeqBase ifEqRcpDelete = new IFSeqBase();
        public IFSeqBase ifAlarmSet = new IFSeqBase();
        public IFSeqBase ifAlarmReset = new IFSeqBase();
        public IFSeqBase ifMachineStatus = new IFSeqBase();

        public IFSeqBase ifHostRcpParaChange = new IFSeqBase();
        public IFSeqBase ifRcpChange = new IFSeqBase();
        public IFSeqBase ifRcpDelete = new IFSeqBase();
        public IFSeqBase ifRcpCreate = new IFSeqBase();
        public IFSeqBase ifRcpUpload = new IFSeqBase();
        public IFSeqBase ifRcpSelected = new IFSeqBase();
        public IFSeqBase ifRcpList = new IFSeqBase();
        #endregion

        #region TOR
        public IFVision()
        {
            m_Receiver = new SocketUDP();
            m_Sender = new SocketUDP();
            //m_UDP = new SocketUDP();
        }

        #endregion

        #region CONNECTION
        public void Start()
        {
            m_Receiver.m_EventRecvMsg += OnRecvData;
            m_Sender.m_EventSentMsg += OnSentData;

            m_Receiver.IPSetting(m_strIpLocal, m_nLocalPort, m_strIpRemote, m_nRemotePort, true);
            m_Sender.IPSetting(m_strIpLocal, m_nLocalPort, m_strIpRemote, m_nRemotePort, false);

            //m_UDP.m_EventRecvMsg += OnRecvData;
            //m_UDP.m_EventSentMsg += OnSentData;
            //m_UDP.IPSetting(m_strIpLocal, m_nLocalPort, m_strIpRemote, m_nRemotePort, true);

            StartStopThread(true);
        }

        public void Stop()
        {
            StartStopThread(false);

            m_Receiver.m_EventRecvMsg -= OnRecvData;
            m_Sender.m_EventSentMsg -= OnSentData;

            m_Receiver.Close();
            m_Sender.Close();

            //m_UDP.m_EventRecvMsg -= OnRecvData;
            //m_UDP.m_EventSentMsg -= OnSentData;
            //m_UDP.Close();   
        }

        #endregion

        #region THREAD
        void StartStopThread(bool bStart)
        {
            if (threadRun != null)
            {
                // Join의 시간은 Thread에서 처리되는 최대 시간으로 지정
                flagThreadAlive = false;
                threadRun.Join(1500);
                threadRun.Abort();
                threadRun = null;
            }

            if (bStart)
            {
                flagThreadAlive = true;
                threadRun = new Thread(new ParameterizedThreadStart(ThreadRun));
                threadRun.Name = "VISION COMM THREAD";
                if (threadRun.IsAlive == false)
                    threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            IFVision ReadRun = obj as IFVision;

            while (ReadRun.flagThreadAlive)
            {
                Thread.Sleep(10);

                //if(m_UDP.IsConnected() == false)
                //{
                //    m_UDP.Connect(m_strIpLocal, m_strIpRemote, 8000);
                //}

                #region DEQUEUE
                if (CmdQueue.Count > 0)
                {
                    string strQueue = "";
                    if (CmdQueue.TryDequeue(out strQueue))
                    {
                        Parsing(strQueue);
                    }
                }
                #endregion

                #region RECV
                EqRecipeParaChange();
                EqRecipeDelete();
                AlarmSet();
                AlarmReset();
                MachineStatus();
                #endregion

                #region SEND
                HostRcpParamChange();
                RcpCreate();
                RcpChange();
                RcpDelete();
                RcpUpload();
                RcpSelected();
                RcpList();
                JogError();
                #endregion
            }
        }
        #endregion

        #region VISION -> SORTER CYCLE

        private int EqRecipeParaChange()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifEqRcpParaChange.SEQNO)
            //{
            //    case 0:
            //        string strAnswer = "";
            //        if (!ifEqRcpParaChange.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("PARA CHANGE | START"));
            //        break;

            //    case 1:
            //        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_SKIP].bOptionUse == true) break;

            //        string strRecipeName = ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA];
            //        MESSAGE_PARA_CHANGE mes = new MESSAGE_PARA_CHANGE();
            //        mes.PPID_CURRENT = strRecipeName;
            //        IFMgr.Inst.MES.SendQueue.Enqueue(mes);

            //        AddVisionLog(string.Format("PARA CHANGE | PARA CHANGE REQUEST TO MES, PPID {0}", strRecipeName));

            //        if(!isParaChangeUseOption())
            //        {
            //            bCheckAllBack = false;
            //            ifEqRcpParaChange.SEQNO = 4;
            //            return FNC.BUSY;
            //        }

            //        ifEqRcpParaChange.swTimeStamp.Restart();
            //        break;

            //    case 2:
            //        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_SKIP].bOptionUse == true) break;
                    
            //        if (ifEqRcpParaChange.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MES_REPLY_TIME].lValue)
            //        {
            //            strRecipeName = ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA];
            //            AddVisionLog(string.Format("PARA CHANGE | MES REPLY TIME-OUT MESSAGE_PARA_CHANGE, PPID {0}", strRecipeName));
            //            ifEqRcpParaChange.swTimeStamp.Stop();

            //            strAnswer = string.Format("{0},{1}", ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA]);
            //            SendERROR(strAnswer);

            //            ifEqRcpParaChange.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifEqRcpParaChange.SEQREPLY) return FNC.BUSY;
            //        ifEqRcpParaChange.swTimeStamp.Stop();
            //        break;


            //    case 3:
            //        bCheckAllBack = CheckAllBack();
            //        break;

            //    case 4:
            //        if (bCheckAllBack == false)
            //        {
            //            MESSAGE_RECIPE_CHANGE report = new MESSAGE_RECIPE_CHANGE();
            //            report.PP_CHANGE_CODE = ((int)MESDF.ePPChangeCode.SAVE).ToString();
            //            report.PPID_CURRENT = ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA];
            //            IFMgr.Inst.MES.SendQueue.Enqueue(report);
            //        }
            //        strAnswer = string.Format("{0},{1}", ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], ifEqRcpParaChange.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA]);
            //        SendOK(strAnswer);
            //        break;

            //    case 5:
            //        ifEqRcpParaChange.SEQSTART = false;
            //        ifEqRcpParaChange.ResetCmd();
            //        AddVisionLog(string.Format("PARA CHANGE | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}

            ////wrong seq check
            //if (ifEqRcpParaChange.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("PARA CHANGE | SEQ FAULT OCCUR"));

            //    ifEqRcpParaChange.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifEqRcpParaChange.SEQNO++;
            return FNC.BUSY;
        }

        private int EqRecipeDelete()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifEqRcpDelete.SEQNO)
            //{
            //    case 0:
            //        if (!ifEqRcpDelete.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP DELETE | START"));
            //        break;

            //    case 1:
            //        string strRecipeName = ifEqRcpDelete.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA];
            //        string strAnswer = string.Format("{0},{1}", ifEqRcpDelete.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], strRecipeName);

            //        if (strRecipeName == RecipeMgr.Inst.Rcp.strRecipeId)
            //        {
            //            SendERROR(strAnswer);
            //        }
            //        else
            //        {
            //            SendOK(strAnswer);
            //        }
            //        break;

            //    case 5:
            //        ifEqRcpDelete.SEQSTART = false;
            //        ifEqRcpDelete.ResetCmd();
            //        AddVisionLog(string.Format("RCP DELETE | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifEqRcpDelete.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("RCP DELETE | SEQ FAULT OCCUR"));

            //    ifEqRcpDelete.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifEqRcpDelete.SEQNO++;
            return FNC.BUSY;
        }

        private int AlarmSet()
        {
            // [2022.04.06. kmlee] 주석
            //string strAnswer = "";

            //switch (ifAlarmSet.SEQNO)
            //{
            //    case 0:
            //        if (!ifAlarmSet.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("ALARM SET | START"));
            //        break;

            //    case 1:
            //        int nAlarmNo = 0;
            //        if (!int.TryParse(ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA], out nAlarmNo))
            //        {
            //            strAnswer = string.Format("{0},{1}", ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA]);
            //            SendERROR(strAnswer);
            //        }
            //        else
            //        {
            //            if(!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_SKIP].bOptionUse)
            //            {
            //                ErrStruct err = new ErrStruct(nAlarmNo + (int)ERDF.VISION_ALARM_OFFSET, 1);
            //                ErrMgr.Inst.ErrStatus.Add(err);

            //                IFMgr.Inst.MES.AlarmReport();

            //                AddVisionLog(string.Format("ALARM SET | REPORT TO MES"));
            //            }

            //            strAnswer = string.Format("{0},{1}", ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], ifAlarmSet.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA]);
            //            SendOK(strAnswer);   
            //        }
            //        break;

            //    case 5:
            //        ifAlarmSet.SEQSTART = false;
            //        ifAlarmSet.ResetCmd();
            //        AddVisionLog(string.Format("ALARM SET | INIT"));

            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifAlarmSet.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("ALARM SET | SEQ FAULT OCCUR"));

            //    ifAlarmSet.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifAlarmSet.SEQNO++;
            return FNC.BUSY;
        }

        private int AlarmReset()
        {
            // [2022.04.06. kmlee] 주석
            //string strAnswer = "";

            //switch (ifAlarmReset.SEQNO)
            //{
            //    case 0:
            //        if (!ifAlarmReset.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("ALARM RESET | START"));

            //        break;
            //    case 1:
            //        int nAlarmNo = 0;
            //        if (!int.TryParse(ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA], out nAlarmNo))
            //        {
            //            strAnswer = string.Format("{0},{1}", ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA]);
            //            SendERROR(strAnswer);
            //        }
            //        else
            //        {
            //            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_SKIP].bOptionUse)
            //            {
            //                ErrStruct err = new ErrStruct(nAlarmNo + (int)ERDF.VISION_ALARM_OFFSET, 0);
            //                ErrMgr.Inst.ErrStatus.Add(err);

            //                IFMgr.Inst.MES.AlarmReport();

            //                AddVisionLog(string.Format("ALARM RESET | REPORT TO MES"));
            //            }

            //            //0번째가 받은 커멘드 위치
            //            strAnswer = string.Format("{0},{1}", ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], ifAlarmReset.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA]);
            //            SendOK(strAnswer);
            //        }
            //        break;

            //    case 5:
            //        ifAlarmReset.SEQSTART = false;
            //        ifAlarmReset.ResetCmd();
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifAlarmReset.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("ALARM RESET | SEQ FAULT OCCUR"));

            //    ifAlarmReset.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifAlarmReset.SEQNO++;
            return FNC.BUSY;
        }

        private int MachineStatus()
        {
            // [2022.04.06. kmlee] 주석
            //string strAnswer = "";
            //switch (ifMachineStatus.SEQNO)
            //{
            //    case 0:
            //        if (!ifMachineStatus.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("MC STATUS | START"));

            //        break;

            //    case 1:
            //        string strStatus = ifMachineStatus.SEQDATA[(int)eREQ_IDX_FROMVISION.DATA];
            //        strAnswer = string.Format("{0},{1}", ifMachineStatus.SEQDATA[(int)eREQ_IDX_FROMVISION.CMD], strStatus);
            //        SendOK(strAnswer);

            //        break;

            //    case 5:
            //        ifMachineStatus.SEQSTART = false;
            //        ifMachineStatus.ResetCmd();
            //        AddVisionLog(string.Format("MC STATUS | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifMachineStatus.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("MC STATUS | SEQ FAULT OCCUR"));
            //    ifMachineStatus.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifMachineStatus.SEQNO++;
            return FNC.BUSY;
        }
        #endregion

        #region SORTER -> VISION CYCLE
        private int HostRcpParamChange()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifHostRcpParaChange.SEQNO)
            //{
            //    case 0:
            //        if (!ifHostRcpParaChange.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("HOST PARA CHANGE | START"));

            //        break;

            //    case 1:
            //        SendCMD(eTOVISION.RCP_PARACHANGE, ifHostRcpParaChange.PPID);//1번 배열에 레시피 이름
            //        AddVisionLog(string.Format("HOST PARA CHANGE | HOST PARA CHANGE REQUEST TO VISION, PPID : {0}", ifHostRcpParaChange.PPID));

            //        ifHostRcpParaChange.swTimeStamp.Restart();
            //        break;

            //    case 2:
            //        if (ifHostRcpParaChange.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue)
            //        {
            //            AddVisionLog(string.Format("HOST PARA CHANGE | VISION REPLY TIME-OUT!!"));

            //            ifHostRcpParaChange.swTimeStamp.Stop();
            //            ifHostRcpParaChange.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifHostRcpParaChange.SEQREPLY) return FNC.BUSY;
            //        AddVisionLog(string.Format("HOST PARA CHANGE | RECEIVE VISION REPLY"));
            //        ifHostRcpParaChange.swTimeStamp.Stop();
            //        break;

            //    case 4:
            //        //if (ifHostRcpParaChange.RETURN != "OK")
            //        //{
            //        //    AddVisionLog(string.Format("HOST PARA CHANGE | VISION RETURN IS ERROR!!"));
            //        //}
            //        //else
            //        //{
            //        //    AddVisionLog(string.Format("HOST PARA CHANGE | VISION RETURN IS OK"));
            //        //}
            //        break;

            //    case 5:
            //        ifHostRcpParaChange.SEQSTART = false;
            //        ifHostRcpParaChange.SemiResetCmd();
            //        AddVisionLog(string.Format("HOST PARA CHANGE | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifHostRcpParaChange.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("HOST PARA CHANGE | SEQ FAULT OCCURS"));
            //    ifHostRcpParaChange.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifHostRcpParaChange.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpCreate()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifRcpCreate.SEQNO)
            //{
            //    case 0:
            //        if (!ifRcpCreate.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP CREATE | START"));

            //        break;

            //    case 1:
            //        SendCMD(eTOVISION.RCP_CREATE, ifRcpCreate.PPID);
            //        ifRcpCreate.swTimeStamp.Restart();
            //        AddVisionLog(string.Format("RCP CREATE | RCP CREATE REQUEST TO VISION, PPID : {0}", ifRcpCreate.PPID));
            //        break;

            //    case 2:
            //        if (ifRcpCreate.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue)
            //        {
            //            AddVisionLog(string.Format("RCP CREATE | VISION REPLY TIME-OUT!!, PPID : {0}", ifRcpCreate.PPID));

            //            ifRcpCreate.swTimeStamp.Stop();
            //            ifRcpCreate.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifRcpCreate.SEQREPLY) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP CREATE | RECEIVE VISION REPLY, PPID : {0}", ifRcpCreate.PPID));

            //        ifRcpCreate.swTimeStamp.Stop();
            //        break;

            //    case 3:
            //        //if (ifRcpCreate.RETURN != "OK")
            //        //{
            //        //    AddVisionLog(string.Format("RCP CREATE | VISION RETURN IS ERROR!!, PPID : {0}", ifRcpCreate.PPID));
            //        //}
            //        //else
            //        //{
            //        //    AddVisionLog(string.Format("RCP CREATE | VISION RETURN IS OK, PPID : {0}", ifRcpCreate.PPID));
            //        //}
            //        break;
                
            //    case 5:
            //        ifRcpCreate.SEQSTART = false;
            //        ifRcpCreate.SemiResetCmd();
            //        //ifRcpCreate.ResetCmd();
            //        AddVisionLog(string.Format("RCP CREATE | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifRcpCreate.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("RCP CREATE | SEQ FAULT OCCURS"));

            //    ifRcpCreate.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifRcpCreate.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpChange()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifRcpChange.SEQNO)
            //{
            //    case 0:
            //        if (!ifRcpChange.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP CHANGE | START"));

            //        break;

            //    case 1:
            //        SendCMD(eTOVISION.RCP_CHANGE, ifRcpChange.PPID);
            //        AddVisionLog(string.Format("RCP CHANGE | RCP CHANGE REQUEST TO VISION, PPID : {0}", ifRcpChange.PPID));

            //        ifRcpChange.swTimeStamp.Restart();

            //        break;

            //    case 2:
            //        if (ifRcpChange.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue)
            //        {
            //            AddVisionLog(string.Format("RCP CHANGE | VISION REPLY TIME-OUT!!, PPID : {0}", ifRcpChange.PPID));

            //            ifRcpChange.swTimeStamp.Stop();
            //            ifRcpChange.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifRcpChange.SEQREPLY) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP CHANGE | RECEIVE VISION REPLY, PPID : {0}", ifRcpChange.PPID));

            //        ifRcpChange.swTimeStamp.Stop();
            //        break;

            //    case 3:
            //        //if (ifRcpChange.RETURN != "OK")
            //        //{
            //        //    AddVisionLog(string.Format("RCP CHANGE | VISION RETURN IS ERROR!!, PPID : {0}", ifRcpChange.PPID));
            //        //}
            //        //else
            //        //{
            //        //    AddVisionLog(string.Format("RCP CHANGE | VISION RETURN IS OK, PPID : {0}", ifRcpChange.PPID));
            //        //}
            //        break;

            //    case 5:
            //        ifRcpChange.SEQSTART = false;
            //        ifRcpChange.SemiResetCmd();
            //        AddVisionLog(string.Format("RCP CHANGE | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifRcpChange.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("RCP CHANGE | SEQ FAULT OCCURS"));

            //    ifRcpChange.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifRcpChange.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpDelete()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifRcpDelete.SEQNO)
            //{
            //    case 0:
            //        if (!ifRcpDelete.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP DELETE | START"));

            //        break;

            //    case 1:
            //        SendCMD(eTOVISION.RCP_DELETE, ifRcpDelete.PPID);
            //        AddVisionLog(string.Format("RCP DELETE | RCP DELETE REQUEST TO VISION, PPID : {0}", ifRcpDelete.PPID));

            //        ifRcpDelete.swTimeStamp.Restart();
            //        break;

            //    case 2:
            //        if (ifRcpDelete.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue)
            //        {
            //            AddVisionLog(string.Format("RCP DELETE | VISION REPLY TIME-OUT!!, PPID : {0}", ifRcpDelete.PPID));

            //            ifRcpDelete.swTimeStamp.Stop();
            //            ifRcpDelete.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifRcpDelete.SEQREPLY) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP DELETE | RECEIVE VISION REPLY, PPID : {0}", ifRcpDelete.PPID));

            //        ifRcpDelete.swTimeStamp.Stop();
            //        break;

            //    case 3:
            //        //if (ifRcpDelete.RETURN != "OK")
            //        //{
            //        //    AddVisionLog(string.Format("RCP DELETE | VISION RETURN IS ERROR!!, PPID : {0}", ifRcpDelete.PPID));
            //        //}
            //        //else
            //        //{
            //        //    AddVisionLog(string.Format("RCP DELETE | VISION RETURN IS OK, PPID : {0}", ifRcpDelete.PPID));
            //        //}
            //        break;

            //    case 5:
            //        ifRcpDelete.SEQSTART = false;
            //        ifRcpDelete.SemiResetCmd();
            //        AddVisionLog(string.Format("RCP DELETE | INIT"));

            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifRcpDelete.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("RCP DELETE | SEQ FAULT OCCURS"));

            //    ifRcpDelete.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifRcpDelete.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpUpload()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifRcpUpload.SEQNO)
            //{
            //    case 0:
            //        if (!ifRcpUpload.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP UPLOAD | START"));

            //        break;

            //    case 1:
            //        SendCMD(eTOVISION.RCP_UPLOAD, ifRcpUpload.PPID);
            //        AddVisionLog(string.Format("RCP UPLOAD | RCP UPLOAD REQUEST TO VISION, PPID : {0}", ifRcpUpload.PPID));

            //        ifRcpUpload.swTimeStamp.Restart();
            //        break;

            //    case 2:
            //        if (ifRcpUpload.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue)
            //        {
            //            AddVisionLog(string.Format("RCP UPLOAD | VISION REPLY TIME-OUT!!, PPID : {0}", ifRcpUpload.PPID));

            //            ifRcpUpload.swTimeStamp.Stop();
            //            ifRcpUpload.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifRcpUpload.SEQREPLY) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP UPLOAD | RECEIVE VISION REPLY, PPID : {0}", ifRcpUpload.PPID));

            //        ifRcpUpload.swTimeStamp.Stop();
            //        break;
            //    case 3:
            //        //if (ifRcpUpload.RETURN != "OK")
            //        //{
            //        //    AddVisionLog(string.Format("RCP UPLOAD | VISION RETURN IS ERROR!!, PPID : {0}", ifRcpUpload.PPID));
            //        //}
            //        //else
            //        //{
            //        //    AddVisionLog(string.Format("RCP UPLOAD | VISION RETURN IS OK, PPID : {0}", ifRcpUpload.PPID));
            //        //}
            //        break;
            //    case 5:
            //        ifRcpUpload.SEQSTART = false;
            //        ifRcpUpload.SemiResetCmd();
            //        AddVisionLog(string.Format("RCP UPLOAD | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifRcpUpload.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("RCP UPLOAD | SEQ FAULT OCCURS"));

            //    ifRcpUpload.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifRcpUpload.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpSelected()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifRcpSelected.SEQNO)
            //{
            //    case 0:
            //        if (!ifRcpSelected.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP SELECTED | START"));

            //        break;

            //    case 1:
            //        SendCMD(eTOVISION.RCP_SELECTED);
            //        AddVisionLog(string.Format("RCP SELECTED | RCP SELECTED REQUEST TO VISION"));

            //        ifRcpSelected.swTimeStamp.Restart();

            //        break;

            //    case 2:
            //        if (ifRcpSelected.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue)
            //        {
            //            AddVisionLog(string.Format("RCP SELECTED | VISION REPLY TIME-OUT!!"));

            //            ifRcpSelected.swTimeStamp.Stop();
            //            ifRcpSelected.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifRcpSelected.SEQREPLY) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP SELECTED | RECEIVE VISION REPLY"));

            //        ifRcpSelected.swTimeStamp.Stop();
            //        break;

            //    case 3:
            //        if (ifRcpSelected.RETURN != "OK")
            //        {
            //            AddVisionLog(string.Format("RCP SELECTED | VISION RETURN IS ERROR!!"));
            //        }
            //        else
            //        {
            //            string str = ifRcpSelected.SEQDATA[(int)eRES_IDX_FROMVISION.DATA];
            //            AddVisionLog(string.Format("RCP SELECTED | VISION RETURN IS OK, PPID : {0}", str));
            //        }
            //        break;

            //    case 5:
            //        ifRcpSelected.SEQSTART = false;
            //        ifRcpSelected.ResetCmd();
            //        AddVisionLog(string.Format("RCP SELECTED | INIT"));

            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifRcpSelected.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("RCP SELECTED | SEQ FAULT OCCURS"));

            //    ifRcpSelected.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifRcpSelected.SEQNO++;
            return FNC.BUSY;
        }

        private int RcpList()
        {
            // [2022.04.06. kmlee] 주석
            //switch (ifRcpList.SEQNO)
            //{
            //    case 0:
            //        if (!ifRcpList.SEQSTART) return FNC.BUSY;
            //        AddVisionLog(string.Format("RCP LIST | START"));

            //        break;

            //    case 1:
            //        SendCMD(eTOVISION.RCP_LIST);
            //        AddVisionLog(string.Format("RCP LIST | RCP LIST REQUEST TO VISION"));

            //        ifRcpList.swTimeStamp.Restart();
            //        break;

            //    case 2:
            //        if (ifRcpList.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue)
            //        {
            //            AddVisionLog(string.Format("RCP LIST | VISION REPLY TIME-OUT!!"));

            //            ifRcpList.swTimeStamp.Stop();
            //            ifRcpList.SEQNO = 5;
            //            return FNC.BUSY;
            //        }
            //        if (!ifRcpList.SEQREPLY) return FNC.BUSY;
            //        ifRcpList.swTimeStamp.Stop();
            //        break;

            //    case 3:
            //        if (ifRcpList.RETURN != "OK")
            //        {
            //            AddVisionLog(string.Format("RCP LIST | VISION RETURN IS ERROR!!"));
            //        }
            //        else
            //        {
            //            AddVisionLog(string.Format("RCP LIST | VISION RETURN IS OK!!"));

            //            AddVisionLog(string.Format("----------- RECEIVED RCP LIST -----------"));

            //            //받은 레시피 리스트 이름확인
            //            string str = "";
            //            for (int i = (int)eRES_IDX_FROMVISION.DATA; i < ifRcpList.SEQDATA.Length - 1; i++)
            //            {
            //                str = ifRcpList.SEQDATA[i];
            //                AddVisionLog(string.Format("{0}", str));
            //            }
            //        }
            //        break;

            //    case 5:
            //        ifRcpList.SEQSTART = false;
            //        ifRcpList.ResetCmd();
            //        AddVisionLog(string.Format("RCP LIST | INIT"));
            //        return FNC.SUCCESS;

            //    default:
            //        break;
            //}
            ////wrong seq check
            //if (ifRcpList.SEQNO > 10)
            //{
            //    AddVisionLog(string.Format("RCP LIST | SEQ FAULT OCCURS"));

            //    ifRcpList.SEQFAULT = true;
            //    return FNC.BUSY;
            //}
            //ifRcpList.SEQNO++;
            return FNC.BUSY;
        }

        int JogError()
        {
            if (GbVar.g_bResponseJogError)
            {
                SendCMD(E_VS_SEND_CMD.JOG_ERROR);
                GbVar.g_bResponseJogError = false;
            }

            return FNC.BUSY;
        }
        #endregion

        #region SEND RECV
        [Obsolete("사용안함", true)]
        public void SendOK(string strMsg)
        {
            strMsg = "ANSWER," + strMsg + ",OK";
            Send(strMsg);

            AddVisionLog(string.Format("OK SEND TO VISION | {0}", strMsg));
        }

        [Obsolete("사용안함", true)]
        public void SendERROR(string strMsg)
        {
            strMsg = "ANSWER," + strMsg + ",ERROR";
            Send(strMsg);

            AddVisionLog(string.Format("ERROR SEND TO VISION | {0}", strMsg));
        }

        [Obsolete("사용안함", true)]
        public void SendCMD(eTOVISION eCmd)
        {
            string strMsg = "";
            strMsg = eCmd.ToString();
            Send(strMsg);

            AddVisionLog(string.Format("CMD SEND TO VISION | {0}", strMsg));
        }

        [Obsolete("사용안함", true)]
        public void SendCMD(eTOVISION eCmd, string strData)
        {
            string strMsg = "";
            strMsg = eCmd.ToString() + "," + strData;
            Send(strMsg);

            AddVisionLog(string.Format("CMD SEND TO VISION | {0}", strMsg));
        }

        /// <summary>
        /// [2022.04.06. kmlee] UDP 명령 추가
        /// 현재는 strData에 *만 들어간다
        /// ex) TOP_LIVE,*
        /// </summary>
        /// <param name="eCmd"></param>
        /// <param name="strData"></param>
        public void SendCMD(E_VS_SEND_CMD eCmd)
        {
            string strMsg = "";
            strMsg = eCmd.ToString();
            Send(strMsg);

            GbVar.g_queueMsgLogVisionUdp.Enqueue(new MessageLog(true, strMsg));
            AddVisionLog(string.Format("CMD SEND TO VISION | {0}", strMsg));
        }

        public void Send(string strMsg)
        {
            ASCIIEncoding encondeing = new ASCIIEncoding();

            string sSend = string.Format("{0},*", strMsg);
            byte[] bBuffer = encondeing.GetBytes(sSend);
            
            //m_UDP.SendData(bBuffer);
            m_Sender.SendData(bBuffer);
        }

        private void OnRecvData(byte[] buffer)
        {
            if (buffer.Length <= 0) return;

            string strbuffer = Encoding.ASCII.GetString(buffer);
            strRecv += strbuffer;

            #region PARSING
            if (strRecv.Length > 0)
            {
                //int nPosStart = strRecv.IndexOf(STX);
                int nPosEnd = strRecv.IndexOf(ETX);

                //if (nPosStart >= 0 && nPosEnd >= 0 && (nPosEnd > nPosStart))
                //if (nPosEnd >= 0)
                while (nPosEnd >= 0)
                {
                    //nPosStart = strRecv.IndexOf(STX);
                    //nPosEnd = strRecv.IndexOf(ETX);

                    string strCmd = strRecv.Substring(0, nPosEnd + 1);
                    CmdQueue.Enqueue(strCmd);
                    GbVar.g_queueMsgLogVisionUdp.Enqueue(new MessageLog(false, strCmd));

                    if (strRecv.Length > nPosEnd + 1)
                    {
                        strRecv = strRecv.Substring(nPosEnd + 1, strRecv.Length - (nPosEnd + 1));
                    }
                    else
                    {
                        strRecv = "";
                    }

                    nPosEnd = strRecv.IndexOf(ETX);
                }
            }
            #endregion
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
            double dMovePos = 0.0;

            AddVisionLog(string.Format("MESSAGE RECEIVED FROM VISION | {0}", strRecv));

            //string[] Data = strRecv.Substring(2, strRecv.Length - 4).Split(',');//시작기호, 끝기호 삭제
            string[] strData = strRecv.Split(new char[] { ',' } , StringSplitOptions.RemoveEmptyEntries);
            //if (strData.Length < 2 || strData[strData.Length - 1] != "*")
			if (strData.Length < 2)
            {
                return;
            }

            if (Enum.IsDefined(typeof(E_VS_RECV_CMD), strData[0]))
            {
                E_VS_RECV_CMD eRecvCmd;

                if(!Enum.TryParse<E_VS_RECV_CMD>(strData[0], out eRecvCmd))
                    return;

                switch (eRecvCmd)
                {
                    case E_VS_RECV_CMD.MAPBLOCK1_TEACHING:
                        {
                            GbVar.g_queueVisionMoveCmd.Enqueue(new VisionMoveCmd(eRecvCmd));
                        }
                        break;
                    case E_VS_RECV_CMD.MAPBLOCK2_TEACHING:
                        {
                            GbVar.g_queueVisionMoveCmd.Enqueue(new VisionMoveCmd(eRecvCmd));
                        }
                        break;
                    case E_VS_RECV_CMD.X_PITCH:
                        {
                            if (strData.Length < 3)
                                return;

                            if (!double.TryParse(strData[1], out dMovePos))
                                return;

                            GbVar.g_queueVisionMoveCmd.Enqueue(new VisionMoveCmd(eRecvCmd, dMovePos));
                        }
                        break;
                    case E_VS_RECV_CMD.Y1_PITCH:
                        {
                            if (strData.Length < 3)
                                return;

                            if (!double.TryParse(strData[1], out dMovePos))
                                return;

                            GbVar.g_queueVisionMoveCmd.Enqueue(new VisionMoveCmd(eRecvCmd, dMovePos));
                        }
                        break;
                    case E_VS_RECV_CMD.Y2_PITCH:
                        {
                            if (strData.Length < 3)
                                return;

                            if (!double.TryParse(strData[1], out dMovePos))
                                return;

                            GbVar.g_queueVisionMoveCmd.Enqueue(new VisionMoveCmd(eRecvCmd, dMovePos));
                        }
                        break;
                    case E_VS_RECV_CMD.Z_PITCH:
                        {
                            if (strData.Length < 3)
                                return;

                            if (!double.TryParse(strData[1], out dMovePos))
                                return;

                            GbVar.g_queueVisionMoveCmd.Enqueue(new VisionMoveCmd(eRecvCmd, dMovePos));
                        }
                        break;
                    case E_VS_RECV_CMD.SVID:
                        {
                            string[] strArr;
                            for (int nCnt = 1; nCnt < strData.Length; nCnt++)
                            {
                                strArr = strData[nCnt].Split('=');

                                if (strArr.Length != 2)
                                    continue;

                                GbVar.g_dictVisionSVID.AddOrUpdate(strArr[0], strArr[1], 
                                    (strSVID_Key, strSVID_Value) =>
                                    {
                                        strSVID_Value = strArr[1];

                                        return strSVID_Value;
                                    });
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //if (Enum.IsDefined(typeof(eFROMVISION), Data[(int)eREQ_IDX_FROMVISION.CMD]))
            //{
            //    eFROMVISION efromVision = (eFROMVISION)Enum.Parse(typeof(eFROMVISION), Data[(int)eREQ_IDX_FROMVISION.CMD]);

            //    #region VISION -> SORTER ANSWER
            //    if (efromVision == eFROMVISION.ANSWER)
            //    {
            //        if (Enum.IsDefined(typeof(eTOVISION), Data[(int)eRES_IDX_FROMVISION.CMD]))
            //        {
            //            //보낸 문자를 응답으로 다시 받아 파징.
            //            eTOVISION efromVisionRes = (eTOVISION)Enum.Parse(typeof(eTOVISION), Data[(int)eRES_IDX_FROMVISION.CMD]);

            //            switch (efromVisionRes)
            //            {
            //                case eTOVISION.RCP_PARACHANGE:
            //                    ifHostRcpParaChange.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자

            //                    ifHostRcpParaChange.SEQREPLY = true;

            //                    break;
            //                case eTOVISION.RCP_CHANGE:
            //                    ifRcpChange.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

            //                    ifRcpChange.SEQREPLY = true;

            //                    break;
            //                case eTOVISION.RCP_CREATE:
            //                    ifRcpCreate.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

            //                    ifRcpCreate.SEQREPLY = true;

            //                    break;
            //                case eTOVISION.RCP_DELETE:
            //                    ifRcpDelete.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

            //                    ifRcpDelete.SEQREPLY = true;

            //                    break;
            //                case eTOVISION.RCP_UPLOAD:
            //                    ifRcpUpload.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;

            //                    ifRcpUpload.SEQREPLY = true;

            //                    break;
            //                case eTOVISION.RCP_SELECTED:
            //                    ifRcpSelected.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;
            //                    ifRcpSelected.SEQDATA = Data;
            //                    ifRcpSelected.SEQREPLY = true;

            //                    break;
            //                case eTOVISION.RCP_LIST:
            //                    ifRcpList.RETURN = Data[Data.Length - 1];//Data의 마지막 배열 = 회신 문자;
            //                    ifRcpList.SEQDATA = Data;
            //                    ifRcpList.SEQREPLY = true;

            //                    break;
            //                default:
            //                    break;

            //            }
            //        }
            //    }
            //    #endregion

            //    #region VISION -> SORTER REQ
            //    else
            //    {
            //        switch (efromVision)
            //        {
            //            case eFROMVISION.RCP_PARACHANGE:
            //                //if (ifEqRcpParaChange.SEQSTART == true) break;

            //                ifEqRcpParaChange.SEQDATA = Data;
            //                ifEqRcpParaChange.SEQSTART = true;
            //                break;
            //            case eFROMVISION.RCP_DELETE:
            //                //if (ifEqRcpDelete.SEQSTART == true) break;

            //                ifEqRcpDelete.SEQDATA = Data;
            //                ifEqRcpDelete.SEQSTART = true;
            //                break;
            //            case eFROMVISION.ALARM_SET:
            //                //if (ifAlarmSet.SEQSTART == true) break;

            //                ifAlarmSet.SEQDATA = Data;
            //                ifAlarmSet.SEQSTART = true;
            //                break;
            //            case eFROMVISION.ALARM_RESET:
            //                //if (ifAlarmReset.SEQSTART == true) break;

            //                ifAlarmReset.SEQDATA = Data;
            //                ifAlarmReset.SEQSTART = true;
            //                break;
            //            case eFROMVISION.MACHINE_STATUS:
            //                //if (ifMachineStatus.SEQSTART == true) break;

            //                ifMachineStatus.SEQDATA = Data;
            //                ifMachineStatus.SEQSTART = true;
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    #endregion
            //}
        }
        #endregion

        #endregion

        #region PIO
        #region INPUT
        // [2022.04.06. kmlee] 삭제
        //public bool IsLotStartEndSet
        //{
        //    get { return GbVar.GB_INPUT[(int)IODF.INPUT.SET_LOT_START_END_INFO] == 1; }
        //}

        public bool IsVisionReady
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.VISION_READY] == 1; }
        }

        public bool IsPreAlignGrabReq
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.PRE_INSP_GRAB_REQ] == 1; }

        }
        public bool IsPreAlignCompleted
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.PRE_INSP_COMP] == 1; }
        }

        public bool IsTopAlignCountReset
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.TOP_ALIGN_CNT_RESET] == 1; }
        }

        public bool IsTopAlignCompleted
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.TOP_ALIGN_COMP] == 1; }
        }

        public bool IsMapInspCountReset
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.MAP_CNT_RESET] == 1; }
        }

        public bool IsMapInspCompleted
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.MAP_INSP_COMP] == 1; }
        }

        public bool IsBallCountReset
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.BALL_CNT_RESET] == 1; }
        }

        public bool IsBallInspCompleted
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.BALL_INSP_COMP] == 1; }
        }

        public bool IsPickerCalReset
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.PICKER_CAL_RESET] == 1; }
        }

        public bool IsPickerCalCompleted
        {
            get { return GbVar.GB_INPUT[(int)IODF.INPUT.PICKER_CAL_COMP] == 1; }
        }
        #endregion

        #region OUTPUT

        // [2022.04.06. kmlee] 삭제
        //public void SetLotStartEndInfo(bool bOn)
        //{
        //    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.SET_LOT_START_END_INFO, bOn);
        //}

        public void SetMapStageNo(MCDF.eUNIT eNo)
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_STAGE_NO, Convert.ToBoolean((int)eNo));
        }

        public void SetPreAlignGrabReq(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.PRE_INSP_COMP] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.PRE_INSP_GRAB_REQ, bOn);
            }
        }

        public void SetPreAlignComplete(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.PRE_INSP_COMP] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.PRE_INSP_COMP, bOn);
            }
        }

        public void SetTopAlignCountReset(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_CNT_RESET] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TOP_ALIGN_CNT_RESET, bOn);
            }
        }

        public void SetTopAlignCountComplete(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_INSP_COMP] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.TOP_ALIGN_COMP, bOn);
            }
        }

        public void SetMapInspCountReset(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_CNT_RESET] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_CNT_RESET, bOn);
            }
        }

        public void SetMapInspCountComplete(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_INSP_COMP] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_INSP_COMP, bOn);
            }
        }

        public void SetBallHeadNo(MCDF.eUNIT eNo)
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.BALL_HEAD_NO, Convert.ToBoolean((int)eNo));
        }

        public void SetBallInspCountReset(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.BALL_CNT_RESET] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.BALL_CNT_RESET, bOn);
            }
        }

        public void SetBallInspCountComplete(bool bOn)
        {
            //if ((GbVar.GB_OUTPUT[(int)IODF.OUTPUT.BALL_INSP_COMP] == 1) != bOn)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.BALL_INSP_COMP, bOn);
            }
        }

        public void SetPickerCalReset(bool bOn)
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.PICKER_CAL_RESET, bOn);
        }

        public void SetPickerCalComplete(bool bOn)
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.PICKER_CAL_COMP, bOn);
        }

        public void SetPickerCalLedOn(bool bOn)
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.PICKER_CAL_LED_ON, bOn);
        }

        public void SetPickerCalLedOff(bool bOn)
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.PICKER_CAL_LED_OFF, bOn);
        }

        public void ResetVisionIF()
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_STAGE_NO, false);
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.PRE_INSP_COMP, false);
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_CNT_RESET, false);
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_INSP_COMP, false);
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.BALL_HEAD_NO, false);
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.BALL_CNT_RESET, false);
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.BALL_INSP_COMP, false);
        }

        #endregion
        #endregion

        #region DB

        /// <summary>
        /// pjh 220728 수정
        /// </summary>
        /// <returns></returns>
        public bool UpdateRecipeData()
        {
            bool bRet = false;

            string strMapInfo = string.Format("{0},{1},{2},{3}", RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX, RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY, 
                                                                 RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX , RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY);
            string strUnitSize = string.Format("{0},{1}", RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX.ToString("0.00"), RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY.ToString("0.00"));

            bRet = GbVar.dbSetVision.UpdateMapBlock1(strMapInfo);
            bRet &= GbVar.dbSetVision.UpdateMapBlock2(strMapInfo);
            bRet &= GbVar.dbSetVision.UpdateUnitSize(strUnitSize);
            return bRet;
        }

        public bool UpdateRecipeName()
        {
            bool bRet = false;

            bRet = GbVar.dbSetVision.UpdateTrainName(RecipeMgr.Inst.Rcp.strRecipeId);
            return bRet;
        }


        string[] Map1_Host_Result = new string[500];
        public string[] MAP1_HOST_RESULT
        {
            get { return Map1_Host_Result; }
            set { Map1_Host_Result = value; }
        }

        string[] Map2_Host_Result = new string[500];
        public string[] MAP2_HOST_RESULT
        {
            get { return Map2_Host_Result; }
            set { Map2_Host_Result = value; }
        }

        public string[] GetPreInspResult(VSDF.ePRE_INSP_MODE mode)
        {
            string[] ret = null;
            ret = GbVar.dbGetPreAlign.SelectResult((int)mode);
            return ret;
        }

        public bool SetPreAlignMode(VSDF.ePRE_INSP_MODE mode)
        {
            bool ret = true;

            ret = GbVar.dbSetPreAlign.Update((int)mode);
            return ret;
        }

        public string[] GetHeadInspResult(int nHead)
        {
            string[] ret = null;
            //ret = GbVar.dbHeadVision[nHead].SelectResult();
            return ret;
        }

        public bool SetHeadInspMode(int nHead, int nInspMode = 1)
        {
            bool ret = true;
            //ret = GbVar.dbHeadMain[nHead].Update(nInspMode);
            return ret;
        }

        public tagRetAlignData[] GetTopAlignResult()
        {
            tagRetAlignData[] stAlignData = null;
            stAlignData = GbVar.dbGetTopAlign.Select();

                return stAlignData;
        }

        public tagRetAlignData GetPickerCalResult()
        {
            tagRetAlignData stAlignData = GbVar.dbGetPickerCal.Select();

            return stAlignData;
        }

        StringBuilder strbVisionLog = new StringBuilder();
        /// <summary>
        /// 상부 인스펙션의 결과값을 가져옵니다.
        /// 맵비젼 결과를 StripInfo의 UnitInfo에 저장합니다.
        /// [2022.05.04.kmlee] Edge의 Offset을 받고 Center의 Offset을 구합니다.
        /// </summary>
        /// <param name="nStage"></param>
        /// <returns></returns>
        public bool GetMapInspAllResult(int nStage)
        {
            try
            {
                Random rnd = new Random();
                int[] nArrResult = new int[10] { 0, 0, 0, 0, 0, 1, 0, 1, 0, 0 };

                string[,] strResult = new string[RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX];
                strResult = GbVar.dbGetTopInspection[nStage].SelectAllJudges(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY);

                int count = 0;
               
                for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY; nRow++)
                {
                    for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX; nCol++)
                    {
                        count++;
                        strbVisionLog.Clear();
                        strbVisionLog.Append("===================   ");
                        strbVisionLog.AppendLine(count.ToString());

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_RANDOM_SORT].bOptionUse)
                            {
                                int nResult = rnd.Next(0, 9);

                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = nArrResult[nResult];
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X = (double)nRow + (double)nCol / 10;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y = (double)nRow + (double)nCol / 10;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_UNIT = true;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_SKIP_UNIT = false;//220621
                            }
                            else
                            {
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = 0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X = (double)nRow + (double)nCol / 10;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y = (double)nRow + (double)nCol / 10;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_UNIT = true;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_SKIP_UNIT = false;//220621
                            }
                        }
                        else
                        {
                            if (strResult == null) return false;
                            string[] strData = strResult[nRow, nCol].Split(',');

                            strbVisionLog.AppendFormat("Vision Data [{0},{1}] \r\n Data:", nRow, nCol);
                            strbVisionLog.AppendLine(strResult[nRow, nCol]);

                            // [2022.04.06. kmlee] 변경. JUDGE,X1,Y1,X2,Y2 (5개. LT, RT)
                            // [2022.05.04. kmlee] 변경. JUDGE,X1,Y1,X2,Y2,X3,Y3,X4,Y4 (9개. LT, RT, LB, RB)
                            //if (strData.Length == 6)
                            //if (strData.Length == 5)
                            if (strData.Length == 9)
                            {
                                if (strData[0].Contains("1"))
                                {
                                    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.OK;
                                    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_NG_CODE = "00";

                                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MARK_VISION_OK_COUNT++;
                                }
                                else if (strData[0].Contains("0") || strData[0].Contains("2"))
                                {
                                    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.RW;
                                    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_NG_CODE = "00";

                                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MARK_VISION_RW_COUNT++;
                                }
                                else
                                {
                                    // [2022.04.06. kmlee] 주석
                                    //string[] sResult = new string[2];
                                    //sResult = strData[0].Split('_');

                                    // [2022.06.04.kmlee] 그 외에는 들어오지 않으나 혹시 몰라 넣음
                                    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.RW;

                                    // [2022.04.06. kmlee] 주석
                                    //if (sResult[0] == "NG")
                                    //{
                                    //    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.NG;

                                    //}
                                    //else
                                    //{
                                    //    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.RW;
                                    //}

                                    //if(sResult.Length == 2)
                                    //{
                                    //    GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_NG_CODE = sResult[1];
                                    //}

                                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MARK_VISION_RW_COUNT++;
                                }


                                int nDataCount = 1;
                                int nEdgeCount = 0;
                                double dValue = 0.0;

                                #region 각 Edge의 화면 센터와의 거리를 가져온다
                                // LEFT TOP
                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[nEdgeCount] = dValue;
                                strbVisionLog.AppendFormat("Left TOP X:{0} Y:", dValue);

                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[nEdgeCount] = dValue;
                                strbVisionLog.AppendLine(dValue.ToString());


                                nEdgeCount++;

                                // RIGHT TOP
                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[nEdgeCount] = dValue;
                                strbVisionLog.AppendFormat("RIGHT TOP X:{0} Y:", dValue);


                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[nEdgeCount] = dValue;
                                strbVisionLog.AppendLine(dValue.ToString());

                                nEdgeCount++;

                                // LEFT BOTTOM
                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[nEdgeCount] = dValue;
                                strbVisionLog.AppendFormat("LEFT BOTTOM X:{0} Y:", dValue);

                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[nEdgeCount] = dValue;
                                strbVisionLog.AppendLine(dValue.ToString());

                                nEdgeCount++;

                                // RIGHT BOTTOM
                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[nEdgeCount] = dValue;
                                strbVisionLog.AppendFormat("RIGHT BOTTOM X:{0} Y:", dValue);

                                if (!double.TryParse(strData[nDataCount++], out dValue)) dValue = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[nEdgeCount] = dValue;
                                strbVisionLog.AppendLine(dValue.ToString());
                                #endregion

                                #region Edge 기준으로 실제 오프셋을 구한다
                                // Center Offset 위치 계산
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X = 0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y = 0;

                                // Inspection Position에서 Edge까지의 거리를 계산한다
                                double dDistInspPosToEdgeX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX - RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX;
                                double dDistInspPosToEdgeY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY - RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY;

                                strbVisionLog.AppendFormat("Inspection Position에서 Edge까지의 거리 X:{0} Y:{1} \r\n", dDistInspPosToEdgeX, dDistInspPosToEdgeY);

                                // LT와 RT의 위치를 구하고 Unit Size의 중간 위치와 얼마나 떨어져있는지 계산한다
                                double dLT_X = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[0];
                                double dLT_Y = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[0];

                                double dRT_X = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[1];
                                double dRT_Y = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[1];

                                strbVisionLog.AppendFormat("LT와 RT의 위치를 구하고 Unit Size의 중간 위치와 얼마나 떨어져있는지  LTX:{0} LTY:{1} RTX:{2} RTY:{3}  \r\n", dLT_X, dLT_Y, dRT_X, dRT_Y);

                                // 티칭의 EDGE 위치를 구하기 위해 인스펙션 위치를 가져온다
                                //double dTeachCenterX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1];
                                //double dTeachCenterY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y + nStage].dPos[POSDF.MAP_STAGE_MAP_VISION_START];

                                double dTeachCenterX = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                                double dTeachCenterY = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                                if (nStage == 1)
                                {
                                    dTeachCenterX = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                                    dTeachCenterY = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                                }
                                strbVisionLog.AppendFormat("TEACH CENTER  X:{0} Y:{1}  \r\n", dTeachCenterX, dTeachCenterY);



                                // Unit Pitch와 Unit Size의 거리를 계산하여 EDGE로 만든다
                                double dTeachEdgeX = dTeachCenterX + dDistInspPosToEdgeX;
                                double dTeachEdgeY = dTeachCenterY - dDistInspPosToEdgeY;

                                // 실제 Edge 위치를 구하기 위해 인스펙션 위치를 가져온다
                                //double dInspPosEdgeX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1];
                                //double dInspPosEdgeY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y + nStage].dPos[POSDF.MAP_STAGE_MAP_VISION_START];

                                double dInspPosEdgeX = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                                double dInspPosEdgeY = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                                if (nStage == 1)
                                {
                                    dInspPosEdgeX = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                                    dInspPosEdgeX = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                                }

                            
                                // 티칭 인스펙션 위치에서 Top Align 정보를 적용하여 실제 인스펙션 위치를 구한다~
                                if (GbVar.Seq.sMapVisionTable[nStage].Info.bTopAlignCorrectResult)
                                {
                                    dInspPosEdgeX += GbVar.Seq.sMapVisionTable[nStage].Info.xyTopAlignCorrectOffset.x;
                                    dInspPosEdgeY += GbVar.Seq.sMapVisionTable[nStage].Info.xyTopAlignCorrectOffset.y;
                                }

                                // Map Inspection의 좌상단 Offset을 적용하여 실제 EDGE로 이동한다
                                dInspPosEdgeX += GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[0];
                                dInspPosEdgeY += GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[0];

                                // Angle을 구한다
                                double dAngle = MathUtil.GetAngle(dLT_X, dLT_Y, dRT_X + RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX, dRT_Y);
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.T = dAngle;


                                strbVisionLog.AppendFormat("인스펙션 위치  X:{0} Y:{1}  \r\n", dInspPosEdgeX, dInspPosEdgeY);


                                // Angle을 돌렸을 때의 EDGE 위치를 구한다
                                dxy xyAnglePos = MathUtil.GetAnglePos(dInspPosEdgeX, dInspPosEdgeY, -dAngle, dTeachCenterX, dTeachCenterY);

                                // 두 Edge의 거리 차를 계산한다 (실제 Edge 위치 - 티칭 Edge 위치)
                                // Pick Up에서 적용할 때는 X +, Y + 부호
                                double dRealPosX = dInspPosEdgeX - dTeachEdgeX;
                                double dRealPosY = dInspPosEdgeY - dTeachEdgeY;

                                dxy xy1 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[0] - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2),
                                                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[0] - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2));
                                dxy xy2 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[1] + (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2),
                                                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[1] - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2));
                                dxy xy3 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[2] - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2),
                                                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[2] + (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2));
                                dxy xy4 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[3] + (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2),
                                                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[3] + (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2));

                                bool bLineInter = false;
                                bool bSegInter = false;

                                dxy xyIntersect = new dxy();
                                dxy xyCloseP1 = new dxy();
                                dxy xyCloseP2 = new dxy();
                                MathUtil.FindIntersection(xy1, xy4, xy2, xy3, out bLineInter, out bSegInter, out xyIntersect, out xyCloseP1, out xyCloseP2);

                               
                                strbVisionLog.AppendFormat("XY1 위치 값 X:{0} Y:{1}  \r\n", xy1.x, xy1.y);
                                strbVisionLog.AppendFormat("XY4 위치 값 X:{0} Y:{1}  \r\n", xy4.x, xy4.y);
                                strbVisionLog.AppendFormat("XY2 위치 값 X:{0} Y:{1}  \r\n", xy2.x, xy2.y);
                                strbVisionLog.AppendFormat("XY3 위치 값 X:{0} Y:{1}  \r\n", xy3.x, xy3.y);



                                if (bLineInter)
                                {
                                    dRealPosX = xyIntersect.x;
                                    dRealPosY = xyIntersect.y;

                                    strbVisionLog.AppendFormat("칩 센터 #1 오프세 값   X1:{0} Y1:{1}  \r\n", xyIntersect.x, xyIntersect.y);
                                }

                                strbVisionLog.AppendFormat("칩 센터 #2 오프세 값   X1:{0} Y1:{1}  \r\n", dRealPosX, dRealPosY);

                                //dxy xy1 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[0],
                                //                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[0]);
                                //dxy xy2 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[1] + RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX,
                                //                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[1]);
                                //dxy xy3 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[2],
                                //                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[2] + RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY);
                                //dxy xy4 = new dxy(GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_X[3] + RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX,
                                //                  GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.EDGE_Y[3] + RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY);

                                //bool bLineInter = false;
                                //bool bSegInter = false;

                                //dxy xyIntersect = new dxy();
                                //dxy xyCloseP1 = new dxy();
                                //dxy xyCloseP2 = new dxy();
                                //MathUtil.FindIntersection(xy1, xy4, xy2, xy3, out bLineInter, out bSegInter, out xyIntersect, out xyCloseP1, out xyCloseP2);

                                //if (bLineInter)
                                //{
                                //    dRealPosX = xyIntersect.x - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2.0);
                                //    dRealPosY = xyIntersect.y - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2.0);
                                //}
                                //if (Math.Abs(dRealPosX) > 5 || Math.Abs(dRealPosY) > 5)
                                //{
                                //    //
                                //}

                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X = dRealPosX;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y = dRealPosY;
                                #endregion

                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_UNIT = true;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_SKIP_UNIT = false;//220621

                                //파일 로그 남기기
                                //SetStringToTxt(strbVisionLog.ToString());

                                continue;
                            }
                            else
                            {
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT = (int)VSDF.eJUDGE_MAP.RW;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.T = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_SIZE.Width = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_SIZE.Height = 0.0;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_NG_CODE = "00";
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_UNIT = true;
                                GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].IS_SKIP_UNIT = false;//220621

                                return false;
                            }
                        }                
                    } // for nCol
                } // for nRow

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_VISION_USE].bOptionUse)
                {
                    #region 정상적인 OFFSET을 하나 가져온다
                    double dCorrectOffsetX = 0;
                    double dCorrectOffsetY = 0;
                    for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY; nRow++)
                    {
                        for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX; nCol++)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                                ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_RANDOM_SORT].bOptionUse)
                            {
                            }
                            else
                            {
                                if (GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK)
                                {
                                    dCorrectOffsetX = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                                    dCorrectOffsetY = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;

                                    break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region REWORK 자재의 Offset을 주변의 정상적인 Offset으로 지정한다
                    for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY; nRow++)
                    {
                        for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX; nCol++)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_RANDOM_SORT].bOptionUse)
                            {
                            }
                            else
                            {
                                if (GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.RW)
                                {
                                    int[] nNearRowIndex = { -1, +1, 0, 0, -1, -1, +1, +1, -2, +2, 0, 0, -2, -2, +2, +2 };
                                    int[] nNearColIndex = { 0, 0, -1, +1, -1, +1, -1, +1, 0, 0, -2, +2, -2, +2, -2, +2 };

                                    bool bFindNear = false;
                                    for (int nCnt = 0; nCnt < nNearRowIndex.Length; nCnt++)
                                    {
                                        if (IsMapInspNearestDataOK(nStage, nNearRowIndex[nCnt], nNearColIndex[nCnt]))
                                        {
                                            bFindNear = true;

                                            GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nNearRowIndex[nCnt]][nNearColIndex[nCnt]].MAP_COORDI.X;
                                            GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y = GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nNearRowIndex[nCnt]][nNearColIndex[nCnt]].MAP_COORDI.Y;

                                            break;
                                        }
                                    }

                                    // 주변에 정상적인 칩이 없을 때는 정상적인 첫번째 칩으로 지정한다
                                    if (!bFindNear)
                                    {
                                        GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.X = dCorrectOffsetX;
                                        GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].MAP_COORDI.Y = dCorrectOffsetY;
                                    }
                                }
                            }
                        }
                    }
                    #endregion   
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        
        /// <summary>
        /// Vision Log write
        /// </summary>
        /// <param name="strMsg"></param>
        public  void SetStringToTxt(string strMsg)
        {
            try
            {
                //string strPath = string.Format("C:\\MACHINE\\TTLog\\Vision_{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"));

                //FileStream objFileStream = new FileStream(strPath, FileMode.Append, FileAccess.Write);
                //StreamWriter objStreamWriter = new StreamWriter(objFileStream, Encoding.Default);
                //objStreamWriter.WriteLine(strMsg);
                //objStreamWriter.Close();
                //objFileStream.Close();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 맵 인스펙션의 결과에서 에러 개수를 반환한다
        /// [2022.05.13.kmlee] 추가
        /// </summary>
        /// <param name="nStage"></param>
        /// <returns></returns>
        public int GetMapInspErrorCount(int nStage)
        {
            int nErrCnt = 0;

            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
            {
                return 0;
            }

            for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY; nRow++)
            {
                for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX; nCol++)
                {
                    if (GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK)
                    {

                    }
                    else if (GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK)
                    {
                        // Rework도 카운트해야 하는지 확인 필요
                        nErrCnt++;
                    }
                    else
                    {
                        nErrCnt++;
                    }

                }
            }

            return nErrCnt;
        }

        /// <summary>
        /// 근처의 데이터가 정상인지 확인한다
        /// </summary>
        /// <param name="nStage"></param>
        /// <param name="nRow"></param>
        /// <param name="nCol"></param>
        /// <returns></returns>
        bool IsMapInspNearestDataOK(int nStage, int nRow, int nCol)
        {
            if (nRow < 0 || nRow >= RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY)
                return false;

            if (nCol < 0 || nCol >= RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX)
                return false;

            if (GbVar.Seq.sMapVisionTable[nStage].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT != (int)VSDF.eJUDGE_MAP.OK)
                return false;

            return true;
        }

        public bool UpdateHostCodeToMapMainDB(int nStage, int nStart, string strData)
        {
            bool ret = true;

            // [2022.04.06. kmlee] 주석
            //string[] strJudge = new string[RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY];
            //for (int i = 0; i < strJudge.Length; i++)
            //{
            //    strJudge[i] = strData.Substring(i, 1);
            //}

            //ret = GbVar.dbMapMain[nStage].UpdateJudge(nStart, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY, strJudge, true);
            return ret;
        }

        /// <summary>
        /// Host로부터 전달 받은 SubMap Result 코드를 Map Main DB에 업데이트 합니다.
        /// </summary>
        /// <param name="nStage"></param>
        /// <param name="nUnitHostCode"></param>
        /// <returns></returns>
        public bool UpdateHostCodeToMapMainDB(int nStage, UnitInfo[][] nUnitHostCode)
        {
            bool ret = true;

            // [2022.04.06. kmlee] 주석
            //ret = GbVar.dbMapMain[nStage].UpdateHostCode(nUnitHostCode, true);
            return ret;
        }

        /// <summary>
        /// 하부 인스펙션의 결과 값을 가져옵니다.
        /// [2022.06.05.kmlee] 하부 인스펙션 결과는 OK와 NG로만 판단. 매칭 실패 시 알람을 띄우고 비전 프로그램에서 티칭 후 재시도
        /// </summary>
        /// <param name="nHead"></param>
        /// <returns></returns>
        public int GetBallInspResult(int nHead)
        {
            int nFuncResult = FNC.SUCCESS;

            try
            {
                //양품일 시            //불량 일 시
                //[0] = OK             //[0] = NG_NGCODE
                //[1] = Coordi X       //[1] = 0
                //[2] = Coordi Y       //[2] = 0
                //[3] = Coordi Y       //[3] = 0
                //[4] = Size X         //[4] = 0
                //[5] = Size Y         //[5] = 0
                //[6] = Saw Offset X   //[6] = 0
                //[7] = Saw Offset Y   //[7] = 0
                //...                    ...

                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    // 하부 비전을 사용하지 않을 경우 결과값을 전부 OK로 한다.
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse)
                    {
                        UpdateAllBallInspResult(nHead, VSDF.eJUDGE_BALL.OK);
                        return FNC.SUCCESS;
                    }
                }

                string[] ret = null;

                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    ret = GbVar.dbGetBtmInspection[nHead].SelectAllChip();
                }
                Random rnd = new Random();
                int[] nArrResult = new int[6] { 0, 1, 0, 0, 1, 0 };


                for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                {
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_RANDOM_SORT].bOptionUse)
                        {
                            int nResult = rnd.Next(0, 5);
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT = nArrResult[nResult];
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.X = 0.0f;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.Y = 0.0f;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.T = 0.0f;
                        }
                        else
                        {
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT = 0;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.X = 0.0f;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.Y = 0.0f;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.T = 0.0f;
                        }
                    }
                    else
                    {
                        string[] strData = ret[i].Split(',');

                        // [2022.04.06. kmlee] 변경
                        //if (strData.Length == 14)
                        if (strData.Length == 4)
                        {
                            if (strData[0].Equals("1"))
                            {
                                GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT = (int)VSDF.eJUDGE_BALL.OK;
                                GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_NG_CODE = "00";
                            }
                            else if (strData[0].Equals("3"))
                            {
                                // X MARK 에러
                                GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT = (int)VSDF.eJUDGE_BALL.NG;
                            }
                            else // 0 (칩 매칭 실패)
                            {
                                // [2022.06.04.kmlee] 자재가 있거나 배큠 상태일 때만 알람 발생
                                if (GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].IS_UNIT ||
                                    GbVar.GB_CHIP_PK_VAC[(CFG_DF.MAX_PICKER_PAD_CNT * nHead) + i])
                                {
                                    // [2022.06.04.kmlee] 알람 발생 안할 경우 리웍 트레이에 안착
                                    GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT = (int)VSDF.eJUDGE_BALL.RW;

                                    // [2022.06.04.kmlee] 알람 발생
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_ALARM_USE].bOptionUse)
                                    {
                                        return (int)ERDF.E_BALL_INSP_MATCH_FAIL_HEAD1_PAD1 + (nHead * 8) + i;
                                    }
                                }
                            }

                            double dValue = 0.0;
                            if (!double.TryParse(strData[1], out dValue)) dValue = 0.0;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.X = dValue;

                            if (!double.TryParse(strData[2], out dValue)) dValue = 0.0;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.Y = dValue;

                            if (!double.TryParse(strData[3], out dValue)) dValue = 0.0;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.T = dValue;

                            // [2022.05.12.kmlee] 강제로 유무를 지정하면 안된다
                            //GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].IS_UNIT = true;
                        }
                        else
                        {
                            double dValue = 0.0;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT = (int)VSDF.eJUDGE_BALL.RW;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.X = dValue;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.Y = dValue;
                            GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.T = dValue;

                            // [2022.05.12.kmlee] 강제로 유무를 지정하면 안된다
                            //GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].IS_UNIT = true;

                            return (int)ERDF.E_VISION_PROGRAM_BALL_INSPECTION_PARSE_ERROR;
                        }
                    }
                }
                return nFuncResult;
            }

            catch (Exception)
            {
                return (int)ERDF.E_VISION_PROGRAM_BALL_INSPECTION_PARSE_ERROR;
            }
        }

        public void IncreaseBallInspResultCount(int nHead)
        {
            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                    {
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].BALL_VISION_OK_COUNT++;
                    }
                    else if (GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.NG)
                    {
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].BALL_VISION_NG_COUNT++;
                    }
                }
            }
        }

        /// <summary>
        /// 하부 인스펙션의 판정값을 전부 지정합니다. 오프셋은 초기화합니다.
        /// </summary>
        /// <param name="nHead"></param>
        /// <param name="eJudge"></param>
        public void UpdateAllBallInspResult(int nHead, VSDF.eJUDGE_BALL eJudge)
        {
            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            {
                double dValue = 0.0;
                GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BTM_INSP_RESULT = (int)eJudge;
                GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.X = dValue;
                GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.Y = dValue;
                GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].BALL_COORDI.T = dValue;

                // [2022.05.12.kmlee] 강제로 유무를 지정하면 안된다
                //GbVar.Seq.sPkgPickNPlace.pInfo[nHead].unitPickUp[i].IS_UNIT = true;
            }
        }

        public bool UpdateHostnMapRetToBall(int nHead, UnitInfo[] info,  int nStart = 0)
        {
            bool ret1 = true;
            bool ret2 = true;

            // [2022.04.06. kmlee] 주석
            //string[] strHostCode = new string[10];
            //string[] strMapNgCode = new string[10];

            //for (int i = 0; i < info.Length; i++)
            //{
            //    strHostCode[i] = info[i].HOST_CODE.ToString();
            //    strMapNgCode[i] = info[i].TOP_NG_CODE;
            //}

            //ret1 = GbVar.dbBallMain[nHead].UpdateAllChip(strHostCode, (int)DBDF.eBALL_MAIN_ROW.HOST_CODE, true);
            //ret2 = GbVar.dbBallMain[nHead].UpdateAllChip(strMapNgCode, (int)DBDF.eBALL_MAIN_ROW.MAP_NG_CODE, true);

            return ret1 || ret2;
        }

        /// <summary>
        /// Pick Up한 유닛의 Mark 결과를 Main Ball DB Table에 업데이트 합니다.
        /// </summary>
        /// <param name="nHead"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool SetMapResult(int nHead, UnitInfo[] info)
        {
            bool ret = true;

            // [2022.04.06. kmlee] 주석
            //string[] strResult = new string[10];

            //for (int i = 0; i < strResult.Length; i++)
            //{
            //    strResult[i] = info[i].TOP_INSP_RESULT.ToString();
            //}

            //ret = GbVar.dbBallMain[nHead].UpdateAllChip(strResult, 1, true);
            return ret;
        }

        #endregion

        #region TRIGGER
        //public bool TrgPreOneShot(bool bOn = true)
        //{
        //    return MotionMgr.Inst.SetCntTriggerOutput((int)VSDF.eTRIGGER.PRE_ALIGN, bOn);
        //}
        public bool TrgMapOneShot(bool bOn = true)
        {
            //return MotionMgr.Inst.SetCntTriggerOutput((int)VSDF.eTRIGGER.MAP, bOn);
            return MotionMgr.Inst.SetTriggerOneShotMap();
        }
        public bool TrgBallOneShot(bool bOn = true)
        {
            //return MotionMgr.Inst.SetCntTriggerOutput((int)VSDF.eTRIGGER.BALL, bOn);
            return MotionMgr.Inst.SetTriggerOneShotBall();
        }

        #endregion

        private void AddVisionLog(string strLog)
        {
            if (OnLogAddVision != null)
                OnLogAddVision(strLog);

            GbFunc.WriteIFLog((int)DBDF.LOG_TYPE.IF_VISION, DateTime.Now.ToString("yyyyMMddHHmmssfff"), strLog);

        }
    }
}
