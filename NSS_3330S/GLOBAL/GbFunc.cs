using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DionesTool.Objects;
using System.Drawing;
using System.Runtime.InteropServices;
using DionesTool.UTIL;
using System.Windows.Forms;
using System.Reflection;
using DionesTool.LOG.VSLogger;
using System.ComponentModel;
using System.IO;
using System.Data;
using NSS_3330S;
using SQLiteDB;
using System.Globalization;
using NSS_3330S.MOTION;
using System.Reflection;
using System.Xml.Serialization;

namespace NSS_3330S
{
    public static class GbFunc
    {
        private static readonly object padlock = new object();

        public static void Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

        public static void Populate<T>(this T[,] arr, T value)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    arr[i, j] = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct SYM
        {
            public short IsMatrixFixed;     /* FALSE: Try each valid from list; TRUE: Use FixedMatrixSize */
            public short FixedMatrixSize;   /* # to use for IsMatrixFixed == TRUE */
            /* Valid sizes are even numbers from 10 to 48, and sizes */
            /*  13(=26x8), 17(=34x10), and 21(=42x12) - Rectangular VeriCode */
            /*           and; 9(=18x8), 11(=32x8), 13(=26x12), 15(=36x12),   */
            /*            17(=36x16), 19(=48x16) - Rectangular DataMatrix    */

            public short MarkingStyle;      /*  (not used) */
            public short IndexContrast;     /* sets vcObject_thres = (## * 1024 * 5 / 100) or (0 == Auto) */
            public short IsContrastNormal;  /* 1 == Black on White, 0 == Inverse, (2 == Edge, 3 == Auto) */
            public short EdacLevel;         /* 0 == Auto, 1 == 12.5%, 2 == 25%, (?? == None)  */
            public short NumSymbols;        /* 1 == Return first symbol found; n == concatnate upto "n" symbols */
            public short IsSizeFixed;       /*  (not used) */
            public short Compression;       /* 0 == None (8-bit), -1 == Numeric (4-bit), -2 == UC Alpha (6-bit) */
            public double FixedSize;        /*  (not used) */
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct OPTIONS
        {
            public short SymbolType;        /* 0 or 4 == Vericode, 1 == DataMatrix, 5 == Auto */
            public short EdgeMethod;        /*  (not used) */
            public short SampleMethod;      /*  (not used) */
            public short BitsPerCell;       /* == 2, 4, 8, 16 (used in locate::MapHorizontal,Vert & DispObjectMap) */
            public short SampleWidth;       /*  (used extensivly by locate) */
            public short AorLeft;
            public short AorRight;
            public short AorTop;
            public short AorBottom;
            public short Noise;             /*  (not used) */
            public short Prefiltering;
            public short FilterSize;
            public short FilterIterations;
            public short UsePacket;
            public short TriggerCharacter;
            public short Terminate1;
            public short Terminate2;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string NoReadString;
            public short Port;
            public long BaudRate;
            public char Parity;
            public short DataBits;
            public short StopBits;
            public short NumRetry;
        }

        public struct UserSet
        {
            public int nSampleWidth;
            public int nBitsPerCell;
            public int nWidth;
            public int nHieght;
            public int nPrefilering;
            public int nMarixSize;
            public int nEDAC;
            public int nContrastNoraml;
            public int nSymbolType;
        }

        //외부 함수 Import
        [DllImport(("VRdll.dll"), CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern short vcRead(short xSize,
                                          short ySize,
                                          byte[] _input,
                                          StringBuilder _ouput);

        public static int S2D(string sData)
        {
            return 0;
        }

        public static int S2D(string sData, int nStart, int nCount)
        {
            return 0;
        }

        public static int S2DHEX(string sData)
        {
            return (int)Convert.ToInt64(sData, 16);
        }

        public static int S2DHEX(string sData, int nStart, int nCount)
        {
            return (int)Convert.ToInt64(sData.Substring(nStart, nCount), 16);
        }

        public static int C2D(char[] pzData, int nLen)
        {
            return 0;
        }

        public static int C2DHEX(char[] pzData, int nLen)
        {
            return 0;
        }

        public static string D2SHEX(Int32 dwData, int nLen, string sFill)
        {
            string sRtn = "";
            char[] pzData = new char[33];
            int nSize = 0;

            sRtn = Convert.ToString(dwData, 16);
            nSize = Math.Abs(nLen - sRtn.Length);

            if (sRtn.Length > 4)
            {
                sRtn = sRtn.Substring(sRtn.Length - 4, 4);
            }

            nSize = Math.Abs(nLen - sRtn.Length);
            for (int nFillCnt = 0; nFillCnt < nSize; nFillCnt++)
            {
                sRtn = sRtn.Insert(0, sFill);
            }

            return sRtn.ToUpper();
        }

        public static int HEX2Int(string sHexVal)
        {
            int nRtn = int.Parse(sHexVal, System.Globalization.NumberStyles.HexNumber);
            return nRtn;
        }

        public static double DoubleTryParse(string stringToConvert)
        {
            double number;

            bool success = double.TryParse(stringToConvert, out number);
            if (!success) number = 0;
            return number;
        }
        public static float FloatTryParse(string stringToConvert)
        {
            float number;

            bool success = float.TryParse(stringToConvert, out number);
            if (!success) number = 0;
            return number;
        }

        //VeriCode Read Function
        public static bool ReadVeriCode(string strImage, UserSet User, ref string ResultMsg)
        {
            try
            {
                //구조체 변수 설정
                GbFunc.OPTIONS g_vcOpt = new GbFunc.OPTIONS();
                GbFunc.SYM g_vcSym = new GbFunc.SYM();

                Bitmap bmp = new Bitmap(strImage);
                if (bmp == null) return false;
                Image image = (Image)bmp;

                StringBuilder sb_out = new StringBuilder();
                long imageSize = (new System.IO.FileInfo(strImage)).Length;
                byte[] pucMono = new byte[image.Size.Width * image.Size.Height + 1];
                byte[] pucImage = new byte[imageSize]; //bit size
                byte[] ResultData = new byte[5000];

                int nBitColor = Image.GetPixelFormatSize(image.PixelFormat);
                System.Drawing.Imaging.PixelFormat fm = image.PixelFormat;

                var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                fm);

                Marshal.Copy(data.Scan0, pucImage, 0, data.Stride * data.Height);
                bmp.UnlockBits(data);

                //bit 설정
                switch (nBitColor)
                {
                    case 8:
                        for (long lngIdx = 0; lngIdx < (image.Size.Width * image.Size.Height); lngIdx++)
                            pucMono[lngIdx] = pucImage[lngIdx];
                        break;
                    case 24:
                        for (long lngIdx = 0; lngIdx < (image.Size.Width * image.Size.Height); lngIdx++)
                        {
                            pucMono[lngIdx] = pucImage[lngIdx * 3];
                            if (pucMono[lngIdx] < 5)
                                pucMono[lngIdx] = 5;
                        }
                        break;
                    default:
                        return false;
                }

                g_vcOpt.SymbolType = (short)0;
                g_vcOpt.SampleWidth = (short)User.nSampleWidth;
                g_vcOpt.BitsPerCell = (short)User.nBitsPerCell;
                g_vcOpt.AorLeft = (short)7;
                g_vcOpt.AorRight = (short)(image.Size.Width - 8);
                g_vcOpt.AorTop = (short)7;
                g_vcOpt.AorBottom = (short)(image.Size.Height - 8);
                g_vcOpt.Prefiltering = (short)User.nPrefilering;
                g_vcOpt.FilterSize = (short)0;
                g_vcOpt.FilterIterations = (short)0;
                if (User.nMarixSize > 0)
                {
                    g_vcSym.IsMatrixFixed = (short)1;
                    g_vcSym.FixedMatrixSize = (short)User.nMarixSize;
                }
                else
                {
                    g_vcSym.IsMatrixFixed = (short)0;
                    g_vcSym.FixedMatrixSize = (short)16;
                }
                g_vcSym.IndexContrast = (short)0;

                switch (User.nContrastNoraml)
                {
                    case 0:
                        g_vcSym.IsContrastNormal = (short)1;
                        break;
                    case 1:
                        g_vcSym.IsContrastNormal = (short)0;
                        break;
                    case 2:
                        g_vcSym.IsContrastNormal = (short)2;
                        break;
                    default:
                        g_vcSym.IsContrastNormal = (short)1;
                        break;

                }
                g_vcSym.EdacLevel = (short)(User.nEDAC / 2);
                g_vcSym.Compression = (short)0;


                foreach (var value in ResultData)
                {
                    sb_out.Append(value);
                }

                switch (User.nSymbolType)
                {
                    case 0:
                        g_vcOpt.SymbolType = (short)5;
                        break;

                    case 1:
                        g_vcOpt.SymbolType = (short)4;
                        break;

                    case 2:
                        g_vcOpt.SymbolType = (short)1;
                        break;
                }

                int nRet = vcRead(Convert.ToInt16(image.Size.Width), Convert.ToInt16(image.Size.Height), pucMono, sb_out);
                if (nRet < 1) return false; //MessageBox.Show("Return Code Fail");
                ResultMsg = sb_out.ToString();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.ToString());
                return false;
            }

            return true;
        }

        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered",
                                         BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                                         null, control, new object[] { true });

        }
        public static DateTime GetBuildDateTime(Version ver = null)
        {
            if (ver == null)
                ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            //2. Version Text의 세번째 값(Build Number)은 2000년 1월 1일부터
            //Build된 날짜까지의 총 일(Days) 수 이다.
            int intDays = ver.Build;
            DateTime refDate = new DateTime(2000, 1, 1);
            DateTime dtBuildDate = refDate.AddDays(intDays);

            //3. Verion Text의 네번째 값(Revision NUmber)은 자정으로부터 Build된
            //시간까지의 지나간 초(Second) 값 이다.
            int intSeconds = ver.Revision;
            intSeconds = intSeconds * 2;
            dtBuildDate = dtBuildDate.AddSeconds(intSeconds);


            //4. 시차조정
            DaylightTime daylingTime = TimeZone.CurrentTimeZone
                    .GetDaylightChanges(dtBuildDate.Year);
            if (TimeZone.IsDaylightSavingTime(dtBuildDate, daylingTime))
                dtBuildDate = dtBuildDate.Add(daylingTime.Delta);


            return dtBuildDate;
        }

        /// <summary>
        /// Textbox에 한해서 입력된 값이 소수인가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsKeyInDouble(object sender, KeyPressEventArgs e)
        {

            if (sender is TextBox)
            {
                char ch = e.KeyChar;
                TextBox pTbx = new TextBox();
                pTbx = (TextBox)sender;

                if (pTbx.Text.Length <= pTbx.SelectionLength)
                {
                    pTbx.Text = ""; //모두 선택되어 있는 상태에서는 선두에 부호 "-" 입력가능하도록 문자를 지운다
                }

                //소수점 체크
                if (ch == 46 && pTbx.Text.IndexOf('.') != -1)
                {
                    return false; ;
                }
                //-부호 체크 (문자의 처음만 허용)
                if (ch == 45)
                {
                    string strVal = pTbx.Text + ch.ToString();
                    if (strVal.IndexOf('-') != 0)
                    {
                        return false;
                    }
                    if (pTbx.Text.IndexOf('-') != -1)
                    {
                        return false;
                    }
                }

                if (!Char.IsDigit(ch) && ch != 8 && ch != 46 && ch != 45)
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        public static bool IsKeyInUnsignedInt(object sender, KeyPressEventArgs e)
        {

            if (sender is TextBox)
            {
                char ch = e.KeyChar;
                TextBox pTbx = new TextBox();
                pTbx = (TextBox)sender;


                if (!Char.IsDigit(ch))
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        public static void ManualVacBlow(int nTag)
        {
            switch (nTag)
            {
                case 0:
                    {
                        // 스트핍 피커 스트립 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP, true);
                    }
                    break;
                case 1:
                    {
                        // 스트핍 피커 쿼드 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP, true);
                    }
                    break;
                case 2:
                    {
                        // 스트핍 피커 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_BLOW, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP, true);
                    }
                    break;
                case 3:
                    {
                        // 스트핍 피커 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP, true);
                    }
                    break;
                case 4:
                    {
                        // 유닛 피커 워크 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP, true);
                    }
                    break;
                case 5:
                    {
                        // 유닛 피커 워크 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_BLOW, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP, true);
                    }
                    break;
                case 6:
                    {
                        // 유닛 피커 워크 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP, true);
                    }
                    break;
                case 7:
                    {
                        // 유닛 피커 스크랩1 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP, true);
                    }
                    break;
                case 8:
                    {
                        // 유닛 피커 스크랩1 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP, true);
                    }
                    break;
                case 9:
                    {
                        // 유닛 피커 스크랩1 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP, true);
                    }
                    break;
                case 10:
                    {
                        // 유닛 피커 스크랩2 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP, true);
                    }
                    break;
                case 11:
                    {
                        // 유닛 피커 스크랩2 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP, true);
                    }
                    break;
                case 12:
                    {
                        // 유닛 피커 스크랩2 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP, true);
                    }
                    break;
                case 13:
                    {
                        // 드라이 블록 워크 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_VAC_PUMP, true);
                    }
                    break;
                case 14:
                    {
                        // 드라이 블록 워크 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_VAC_PUMP, true);
                    }
                    break;
                case 15:
                    {
                        // 드라이 블록 워크 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_VAC_PUMP, true);
                    }
                    break;
                case 16:
                    {
                        // 맵 피커 워크 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_BLOW, false);
                    }
                    break;
                case 17:
                    {
                        // 맵 피커 워크 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_BLOW, true);
                    }
                    break;
                case 18:
                    {
                        // 맵 피커 워크 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_PK_BLOW, false);
                    }
                    break;
                case 19:
                    {
                        // 좌측 맵 스테이지 워크 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_BLOW, false);
                    }
                    break;
                case 20:
                    {
                        // 좌측 맵 스테이지 워크 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_BLOW, true);
                    }
                    break;
                case 21:
                    {
                        // 좌측 맵 스테이지 워크 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_1_BLOW, false);
                    }
                    break;
                case 22:
                    {
                        // 우측 맵 스테이지 워크 배큠 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_VAC_ON, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_BLOW, false);
                    }
                    break;
                case 23:
                    {
                        // 우측 맵 스테이지 워크 블로우 온
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_BLOW, true);
                    }
                    break;
                case 24:
                    {
                        // 우측 맵 스테이지 워크 블로우 오프
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP, true);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_VAC_ON, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_2_BLOW, false);
                    }
                    break;
                case 25:
                    {
                        // 좌측 맵 스테이지 Air Knife On
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_AIR_KNIFE_1, true);
                    }
                    break;

                case 26:
                    {
                        // 좌측 맵 스테이지 Air Knife Off
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_AIR_KNIFE_1, false);
                    }
                    break;

                case 27:
                    {
                        // 우측 맵 스테이지 Air Knife Off
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_AIR_KNIFE_2, true);
                    }
                    break;

                case 28:
                    {
                        // 우측 맵 스테이지 Air Knife Off
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.MAP_ST_AIR_KNIFE_2, false);
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }

        public static bool IsVacOnOffColorOn(int nTag)
        {
            bool bRtn = false;
            switch (nTag)
            {
                case 0:
                    {
                        // 스트핍 피커 스트립 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP] == 1 )
                            bRtn = true;
                    }
                    break;
                case 1:
                    {
                        // 스트핍 피커 쿼드 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 2:
                    {
                        // 스트핍 피커 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_BLOW] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 3:
                    {
                        // 스트핍 피커 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 4:
                    {
                        // 유닛 피커 워크 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 5:
                    {
                        // 유닛 피커 워크 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_BLOW] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 6:
                    {
                        // 유닛 피커 워크 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 7:
                    {
                        // 유닛 피커 스크랩1 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON] == 1 && 
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 8:
                    {
                        // 유닛 피커 스크랩1 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 9:
                    {
                        // 유닛 피커 스크랩1 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 10:
                    {
                        // 유닛 피커 스크랩2 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 11:
                    {
                        // 유닛 피커 스크랩2 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 12:
                    {
                        // 유닛 피커 스크랩2 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 13:
                    {
                        // 드라이 블록 워크 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_BLOW] == 0 && 
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 14:
                    {
                        // 드라이 블록 워크 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_BLOW] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 15:
                    {
                        // 드라이 블록 워크 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_BLOW] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP] == 1)
                            bRtn = true;
                    }
                    break;
                case 16:
                    {
                        // 맵 피커 워크 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_BLOW] == 0)
                            bRtn = true;
                    }
                    break;
                case 17:
                    {
                        // 맵 피커 워크 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_BLOW] == 1)
                            bRtn = true;
                    }
                    break;
                case 18:
                    {
                        // 맵 피커 워크 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_BLOW] == 0)
                            bRtn = true;
                    }
                    break;
                case 19:
                    {
                        // 좌측 맵 스테이지 워크 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP] == 1 && 
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_BLOW] == 0)
                            bRtn = true;
                    }
                    break;
                case 20:
                    {
                        // 좌측 맵 스테이지 워크 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_BLOW] == 1)
                            bRtn = true;
                    }
                    break;
                case 21:
                    {
                        // 좌측 맵 스테이지 워크 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_BLOW] == 0)
                            bRtn = true;
                    }
                    break;
                case 22:
                    {
                        // 우측 맵 스테이지 워크 배큠 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_BLOW] == 0)
                            bRtn = true;
                    }
                    break;
                case 23:
                    {
                        // 우측 맵 스테이지 워크 블로우 온
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_BLOW] == 1)
                            bRtn = true;
                    }
                    break;
                case 24:
                    {
                        // 우측 맵 스테이지 워크 블로우 오프
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_BLOW] == 0)
                            bRtn = true;
                    }
                    break;
                default:
                    break;

            }
            return bRtn;
        }

        public static void ManualConvOnOff(int nTag)
        {
            switch (nTag)
            {
                case 0:
                    {
                        // 1F CONV CW (FORWARD)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL, true);
                    }
                    break;
                case 1:
                    {
                        // 1F CONV CCW (BACKWARD)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL, true);
                    }
                    break;
                case 2:
                    {
                        // 1F CONV STOP
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL, true);
                    }
                    break;
                case 3:
                    {
                        // 2F CONV CW (FORWARD)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL, true);
                    }
                    break;
                case 4:
                    {
                        // 2F CONV CCW (BACKWARD)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL, true);
                    }
                    break;
                case 5:
                    {
                        // 2F CONV STOP
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL, true);
                    }
                    break;
                case 6:
                    {
                        // ELV CONV CW (FORWARD)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL, true);
                    }
                    break;
                case 7:
                    {
                        // ELV CONV CCW (BACKWARD)
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL, true);
                    }
                    break;
                case 8:
                    {
                        // ELV CONV STOP
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL, false);
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL, true);
                    }
                    break;
                default:
                    break;
            }
        }

        public static bool IsConvOnOffColorOn(int nTag)
        {
            bool bRtn = false;
            switch (nTag)
            {
                case 0:
                    {
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3100] == 1 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3101] == 0)
                            bRtn = true;
                    }
                    break;
                case 1:
                    {
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3100] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3101] == 1)
                            bRtn = true;
                    }
                    break;
                case 2:
                    {
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3100] == 0 && 
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3101] == 0 &&
                            GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3102] == 1)
                            bRtn = true;
                    }
                    break;
                default:
                    break;
            }
            return bRtn;
        }

        //로그 파일 출력

        /// <summary>
        /// 220513
        /// 발생한 알람이 해당되는 모듈 번호를 반환 합니다.
        /// </summary>
        /// <param name="nErrorNo">에러 번호</param>
        /// <param name="eMdl"></param>
        /// <returns>피커 또는 스테이지테이블의 베큠 알람일 경우 모듈 번호를 반환 합니다. 아닐 경우 -1을 반환 합니다.</returns>
        public static bool IsPickerAndStageVacError(int nErrorNo, ref STRIP_MDL eMdl)
        {
            bool bRet = false;
            eMdl = STRIP_MDL.NONE;
            if (nErrorNo == (int)ERDF.E_LD_RAIL_VAC_NOT_ON)
                eMdl = STRIP_MDL.STRIP_RAIL;
            else if (nErrorNo == (int)ERDF.E_STRIP_PK_VAC_NOT_ON)
                eMdl = STRIP_MDL.STRIP_RAIL;

            else if (nErrorNo == (int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT)
                eMdl = STRIP_MDL.STRIP_TRANSFER;
            else if (nErrorNo == (int)ERDF.E_STRIP_PK_UNLOAD_TO_RT_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT)
                eMdl = STRIP_MDL.STRIP_TRANSFER;

            else if (nErrorNo == (int)ERDF.E_UNIT_PK_VAC_NOT_ON_1)
                eMdl = STRIP_MDL.CUTTING_TABLE;
            else if (nErrorNo == (int)ERDF.E_DRY_BLOCK_WORK_VAC_NOT_ON)
                eMdl = STRIP_MDL.UNIT_DRY;
            else if (nErrorNo == (int)ERDF.E_MAP_PK_WORK_VAC_NOT_ON)
                eMdl = STRIP_MDL.MAP_TRANSFER;
            else if (nErrorNo == (int)ERDF.E_MAP_STAGE_1_WORK_VAC_NOT_ON)
                eMdl = STRIP_MDL.MAP_VISION_TABLE_1;
            else if (nErrorNo == (int)ERDF.E_MAP_STAGE_2_WORK_VAC_NOT_ON)
                eMdl = STRIP_MDL.MAP_VISION_TABLE_2;

            if (eMdl != STRIP_MDL.NONE)
            {
                bRet = true;
            }
            return bRet;
        }


        /// <summary>
        /// 모듈의 이름을 반환합니다.
        /// </summary>
        /// <param name="eMdl"></param>
        /// <returns></returns>
        public static string GetStripName(STRIP_MDL eMdl)
        {
            string strName = eMdl.ToString();

            switch (eMdl)
            {
                case STRIP_MDL.NONE:
                    break;
                case STRIP_MDL.STRIP_RAIL:
                    strName = "스트립 레일";
                    break;
                case STRIP_MDL.STRIP_TRANSFER:
                    strName = "스트립 피커";
                    break;
                case STRIP_MDL.CUTTING_TABLE:
                    strName = "쏘우 커팅 테이블 1";
                    break;
                case STRIP_MDL.UNIT_TRANSFER:
                    strName = "유닛 피커";
                    break;
                //case STRIP_MDL.SECOND_CLEAN_ZONE:
                //    strName = "2차 세정존 테이블";
                //    break;
                //case STRIP_MDL.SECOND_ULTRA_ZONE:
                //    strName = "2차 초음파 테이블";
                //    break;
                //case STRIP_MDL.CLEANER_PICKER:
                //    strName = "클린 피커";
                //    break;
                case STRIP_MDL.UNIT_DRY:
                    strName = "드라이 블럭";
                    break;
                case STRIP_MDL.MAP_TRANSFER:
                    strName = "맵 피커";
                    break;
                case STRIP_MDL.MAP_VISION_TABLE_1:
                    strName = "맵 테이블 1";
                    break;
                case STRIP_MDL.MAP_VISION_TABLE_2:
                    strName = "맵 테이블 2";
                    break;
                case STRIP_MDL.MAX:
                    break;
                default:
                    break;
            }

            return strName;
        }

        public static bool IsStrip(STRIP_MDL eMdl)
        {
            bool isStrip = false;

            switch (eMdl)
            {
                case STRIP_MDL.STRIP_RAIL:
                    isStrip = GbVar.Seq.sStripRail.Info.IsStrip();
                    break;

                case STRIP_MDL.STRIP_TRANSFER:
                    isStrip = GbVar.Seq.sStripTransfer.Info.IsStrip();
                    break;

                case STRIP_MDL.CUTTING_TABLE:
                    isStrip = GbVar.Seq.sCuttingTable.Info.IsStrip();
                    break;

                case STRIP_MDL.UNIT_TRANSFER:
                    isStrip = GbVar.Seq.sUnitTransfer.Info.IsStrip();

                    break;

                case STRIP_MDL.UNIT_DRY:
                    isStrip = GbVar.Seq.sUnitDry.Info.IsStrip();

                    break;
                case STRIP_MDL.MAP_TRANSFER:
                    isStrip = GbVar.Seq.sMapTransfer.Info.IsStrip();

                    break;
                case STRIP_MDL.MAP_VISION_TABLE_1:
                    isStrip = GbVar.Seq.sMapVisionTable[0].Info.IsStrip();
                    break;
                case STRIP_MDL.MAP_VISION_TABLE_2:
                    isStrip = GbVar.Seq.sMapVisionTable[1].Info.IsStrip();
                    break;
                case STRIP_MDL.MAX:
                    break;
                default:
                    break;
            }

            return isStrip;
        }

        /// <summary>
        /// 해당 모듈의 자재 유무 및 정보를 삭제합니다.
        /// </summary>
        /// <param name="nMdlNo"></param>
        public static void StripScrap(STRIP_MDL eMdl)
        {
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;
            int nUnitCount = nTotMapCountX * nTotMapCountY;
            switch (eMdl)
            {
                case STRIP_MDL.STRIP_RAIL:
                    GbVar.Seq.sStripRail.Info.Clear();
                    break;

                case STRIP_MDL.STRIP_TRANSFER:
                    GbVar.Seq.sStripTransfer.Info.Clear();
                    break;

                case STRIP_MDL.CUTTING_TABLE:
                    GbVar.Seq.sCuttingTable.Info.Clear();
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sCuttingTable.Info.ClearItem();

                    break;

                case STRIP_MDL.UNIT_TRANSFER:
                    GbVar.Seq.sUnitTransfer.Info.Clear();
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sUnitTransfer.Info.ClearItem();

                    break;

                case STRIP_MDL.UNIT_DRY:
                    GbVar.Seq.sUnitDry.Info.Clear();
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sUnitDry.Info.ClearItem();

                    break;

                case STRIP_MDL.MAP_TRANSFER:
                    GbVar.Seq.sMapTransfer.Info.Clear();
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sMapTransfer.Info.ClearItem();

                    break;

                case STRIP_MDL.MAP_VISION_TABLE_1:
                    GbVar.Seq.sMapVisionTable[0].Info.Clear();
                    for (int nRow = GbVar.Seq.sMapVisionTable[0].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                    {
                        for (int nCol = GbVar.Seq.sMapVisionTable[0].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                        {
                            if (GbVar.Seq.sMapVisionTable[0].Info.UnitArr[nRow][nCol].IS_UNIT)
                            {
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                            }
                        }
                    }
                    GbVar.Seq.sMapVisionTable[0].Info.ClearItem();

                    break;

                case STRIP_MDL.MAP_VISION_TABLE_2:
                    GbVar.Seq.sMapVisionTable[1].Info.Clear();
                    for (int nRow = GbVar.Seq.sMapVisionTable[1].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                    {
                        for (int nCol = GbVar.Seq.sMapVisionTable[1].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                        {
                            if (GbVar.Seq.sMapVisionTable[1].Info.UnitArr[nRow][nCol].IS_UNIT)
                            {
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                            }
                        }
                    }
                    GbVar.Seq.sMapVisionTable[1].Info.ClearItem();

                    break;

                case STRIP_MDL.MAX:
                    break;
                default:
                    break;
            }
        }

        public static void SetScrapNeed(STRIP_MDL eMdl, int nValue)
        {
            if (eMdl <= STRIP_MDL.NONE || eMdl >= STRIP_MDL.MAX)
                return;

            GbVar.g_nScrapNeed[(int)eMdl] = nValue;
        }

        public static int GetScrapNeed(STRIP_MDL eMdl)
        {
            if (eMdl <= STRIP_MDL.NONE || eMdl >= STRIP_MDL.MAX)
                return 0;

            return GbVar.g_nScrapNeed[(int)eMdl];
        }

        public static bool IsScrapNeed(STRIP_MDL eMdl)
        {
            if (eMdl <= STRIP_MDL.NONE || eMdl >= STRIP_MDL.MAX)
                return false;

            return GbVar.g_nScrapNeed[(int)eMdl] == 1;
        }

        public static void ClearScrapNeed()
        {
            for (int nCnt = 0; nCnt < GbVar.g_nScrapNeed.Length; nCnt++)
            {
                GbVar.g_nScrapNeed[nCnt] = 0;
            }
        }

        public static StripInfo GetStripInfo(STRIP_MDL eMdl)
        {
            StripInfo stripInfo = null;
            switch (eMdl)
            {
                case STRIP_MDL.STRIP_RAIL:
                    stripInfo = GbVar.Seq.sStripRail.Info;
                    break;

                case STRIP_MDL.STRIP_TRANSFER:
                    stripInfo = GbVar.Seq.sStripTransfer.Info;
                    break;

                case STRIP_MDL.CUTTING_TABLE:
                    stripInfo = GbVar.Seq.sCuttingTable.Info;
                    break;

                case STRIP_MDL.UNIT_TRANSFER:
                    stripInfo = GbVar.Seq.sUnitTransfer.Info;
                    break;

                case STRIP_MDL.UNIT_DRY:
                    stripInfo = GbVar.Seq.sUnitDry.Info;
                    break;
                case STRIP_MDL.MAP_TRANSFER:
                    stripInfo = GbVar.Seq.sMapTransfer.Info;
                    //GbVar.Seq.sUnitDry.Info.Clear();
                    break;

                case STRIP_MDL.MAP_VISION_TABLE_1:
                    stripInfo = GbVar.Seq.sMapVisionTable[0].Info;
                    //GbVar.Seq.sMapTransfer.Info.Clear();
                    break;

                case STRIP_MDL.MAP_VISION_TABLE_2:
                    stripInfo = GbVar.Seq.sMapVisionTable[1].Info;
                    //GbVar.Seq.sMapTransfer.Info.Clear();
                    break;

                case STRIP_MDL.MAX:
                    break;
                default:
                    break;
            }

            return stripInfo;
        }
        public static bool bAllStripCheck()
        {
            bool bIsStrip = false;

            bIsStrip |= GbVar.Seq.sStripRail.Info.IsStrip();
            bIsStrip |= GbVar.Seq.sStripTransfer.Info.IsStrip();
            bIsStrip |= GbVar.Seq.sCuttingTable.Info.IsStrip();
            bIsStrip |= GbVar.Seq.sUnitTransfer.Info.IsStrip();
            bIsStrip |= GbVar.Seq.sUnitDry.Info.IsStrip();
            bIsStrip |= GbVar.Seq.sMapTransfer.Info.IsStrip();
            bIsStrip |= GbVar.Seq.sMapVisionTable[0].Info.IsStrip();
            bIsStrip |= GbVar.Seq.sMapVisionTable[1].Info.IsStrip();
            
            return bIsStrip;
        }
        public static bool bIsTrayMapUnitCheck()
        {
            bool bIsMap = false;
            try
            {
                for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX; nRow++)
                {
                    for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY; nCol++)
                    {
                        // 배열 참조가 잘못됨
                        bIsMap |= GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nCol][nRow].IS_UNIT;
                        bIsMap |= GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr[nCol][nRow].IS_UNIT;
                        bIsMap |= GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr[nCol][nRow].IS_UNIT;
                    }
                }
            }
            catch (Exception)
            {
                return true;
            }
         
            return bIsMap;
        }
        public static void WriteLogging(string strMsg)
        {
            FileLogger log = new FileLogger();

            string strLogWrite = "";
            string strLogin = "";
            string strSplit = ", ";

            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_ENG) strLogin = "ENGINEER";
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_MAK) strLogin = "MASTER";
            else strLogin = "OPERATOR";

            strLogWrite += DateTime.Now.ToString("HH:mm:ss.fff");
            strLogWrite += strSplit;

            strLogWrite += RecipeMgr.Inst.LAST_MODEL_NAME;
            strLogWrite += strSplit;

            strLogWrite += strLogin;
            strLogWrite += strSplit;

            strLogWrite += GbVar.strLoggingUerID;
            strLogWrite += strSplit;

            strLogWrite += strMsg;

            if (!Directory.Exists(PathMgr.Inst.PATH_LOG_LOGGING)) Directory.CreateDirectory(PathMgr.Inst.PATH_LOG_LOGGING);

            string sFileName = string.Format("{0}\\{1}.log", PathMgr.Inst.PATH_LOG_LOGGING, DateTime.Now.ToString("yyyyMMddHH"));
            log.Log(strLogWrite, sFileName);
        }
        public static void WriteErrorLog(string strMsg)
        {
            FileLogger log = new FileLogger();

            string strLogWrite = "";
            string strLogin = "";
            string strSplit = ", ";

            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_ENG) strLogin = "ENGINEER";
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_MAK) strLogin = "MASTER";
            else strLogin = "OPERATOR";

            strLogWrite += DateTime.Now.ToString("HH:mm:ss.fff");
            strLogWrite += strSplit;

            strLogWrite += RecipeMgr.Inst.LAST_MODEL_NAME;
            strLogWrite += strSplit;

            strLogWrite += strLogin;
            strLogWrite += strSplit;

            strLogWrite += GbVar.strLoggingUerID;
            strLogWrite += strSplit;

            strLogWrite += strMsg;

            if (!Directory.Exists(PathMgr.Inst.PATH_LOG_ERROR)) Directory.CreateDirectory(PathMgr.Inst.PATH_LOG_ERROR);

            string sFileName = string.Format("{0}\\{1}.log", PathMgr.Inst.PATH_LOG_ERROR, DateTime.Now.ToString("yyyyMMdd"));
            log.Log(strLogWrite, sFileName);
        }

        public static void WriteEventLog(string strSection, string strMsg)
        {
            EventProperties m_colInfo = new EventProperties();

            string strNow = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            m_colInfo.Time = strNow;
            m_colInfo.Section = strSection;
            m_colInfo.Detail = strMsg;

            GbVar.dbEventLog2.Enqueue_InsertDB(m_colInfo);
        }

        public static void WriteErrorLog(string strMsg, string strNow, int nErrNo)
        {
            AlarmProperties m_colInfo = new AlarmProperties();

            m_colInfo.Time = strNow;
            m_colInfo.No = nErrNo.ToString();
            m_colInfo.Name = strMsg;
            m_colInfo.Cause = ErrMgr.Inst.Get(nErrNo).CAUSE;

            GbVar.dbAlarmLog.Enqueue_InsertDB(m_colInfo);
        }

        public static void WriteExeptionLog(string strMsg)
        {
            FileLogger log = new FileLogger();

            string strLogWrite = "";
            string strLogin = "";
            string strSplit = ", ";

            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_ENG) strLogin = "ENGINEER";
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_MAK) strLogin = "MASTER";
            else strLogin = "OPERATOR";

            strLogWrite += DateTime.Now.ToString("HH:mm:ss.fff");
            strLogWrite += strSplit;

            strLogWrite += RecipeMgr.Inst.LAST_MODEL_NAME;
            strLogWrite += strSplit;

            strLogWrite += strLogin;
            strLogWrite += strSplit;

            strLogWrite += GbVar.strLoggingUerID;
            strLogWrite += strSplit;

            strLogWrite += strMsg;

            if (!Directory.Exists(PathMgr.Inst.PATH_LOG_EXCEPTION)) Directory.CreateDirectory(PathMgr.Inst.PATH_LOG_EXCEPTION);

            string sFileName = string.Format("{0}\\{1}.log", PathMgr.Inst.PATH_LOG_EXCEPTION, DateTime.Now.ToString("yyyyMMdd"));
            log.Log(strLogWrite, sFileName);
        }


        /// <summary>
        /// 20220106 pjh NEW SEQLOG FUNCTION 
        /// </summary>
        /// <param name="nProc"></param>
        /// <param name="nSeq"></param>
        /// <param name="nSeqSub"></param>
        /// <param name="strTime"></param>
        /// <param name="strCmd"></param>
        /// <param name="strPart"></param>
        /// <param name="strStatus"></param>
        public static void WriteProcLog(int nProc, int nSeq, int nSeqSub, string strTime, string strCmd, string strPart, string strStatus)
        {
            string strLogin = "";
            string strPath = "";
            string strPreFileName = "";
            string strSeqName = "";

            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_ENG) strLogin = "ENGINEER";
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_MAK) strLogin = "MAKER";
            else strLogin = "OPERATOR";

            strPreFileName = ((SEQ_ID)nProc).ToString();
            strPath = string.Format("{0}\\{1}", PathMgr.Inst.PATH_LOG_SEQ, ((SEQ_ID)nProc).ToString());
            strSeqName = nSeq.ToString();

            LogProperties m_colInfo = new LogProperties();

            m_colInfo.Time = strTime;
            m_colInfo.Model = RecipeMgr.Inst.LAST_MODEL_NAME;
            m_colInfo.Cell_Id = "";
            m_colInfo.Level = strLogin;
            m_colInfo.User_Id = GbVar.strLoggingUerID;
            m_colInfo.Log = "";
            m_colInfo.Command = strCmd;
            m_colInfo.Part = strPart;
            m_colInfo.Status = strStatus;

            GbVar.dbLogData[nProc].Enqueue_InsertDB(DBDF.LogDbName[nProc], m_colInfo);
        }


        public static void WriteProcLog(int nProc, int nSeq, int nSeqSub, string strTime, string strProc, string strMsg)
        {
            string strLogin = "";
            string strPath = "";
            string strPreFileName = "";
            string strSeqName = "";

            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_ENG) strLogin = "ENGINEER";
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_MAK) strLogin = "MAKER";
            else strLogin = "OPERATOR";

            strPreFileName = ((SEQ_ID)nProc).ToString();
            strPath = string.Format("{0}\\{1}", PathMgr.Inst.PATH_LOG_SEQ, ((SEQ_ID)nProc).ToString());
            strSeqName = nSeq.ToString();

            LogProperties m_colInfo = new LogProperties();

            m_colInfo.Time = strTime;
            m_colInfo.Model = RecipeMgr.Inst.LAST_MODEL_NAME;
            m_colInfo.Cell_Id = "";
            m_colInfo.Level = strLogin;
            m_colInfo.User_Id = GbVar.strLoggingUerID;
            m_colInfo.Log = string.Format("[SEQ {0} - {1}] LOG = {2}", nSeq, nSeqSub, strMsg);

            GbVar.dbLogData[nProc].Enqueue_InsertDB(DBDF.LogDbName[nProc], m_colInfo);
        }


        public static void WriteIFLog(int nProc, string strTime, string strMsg)
        {
            string strLogin = "";
            string strPath = "";
            string strPreFileName = "";

            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_ENG) strLogin = "ENGINEER";
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_MAK) strLogin = "MAKER";
            else strLogin = "OPERATOR";

            strPreFileName = ((SEQ_ID)nProc).ToString();
            strPath = string.Format("{0}\\{1}", PathMgr.Inst.PATH_LOG_SEQ, ((SEQ_ID)nProc).ToString());

            LogProperties m_colInfo = new LogProperties();

            m_colInfo.Time = strTime;
            m_colInfo.Model = RecipeMgr.Inst.LAST_MODEL_NAME;
            m_colInfo.Cell_Id = "";
            m_colInfo.Level = strLogin;
            m_colInfo.User_Id = GbVar.strLoggingUerID;
            m_colInfo.Log = strMsg;

            GbVar.dbLogData[nProc].Enqueue_InsertDB(DBDF.LogDbName[nProc], m_colInfo);
        }

        public static void TACT_TIME_LogWrite(string strProcess, DateTime dtStart, DateTime dtEnd)
        {
            TactTimeProperties m_colInfo = new TactTimeProperties();

            m_colInfo.Process = strProcess;
            m_colInfo.StartTime = dtStart.ToString("yyyyMMddHHmmssfff");
            m_colInfo.EndTime = dtEnd.ToString("yyyyMMddHHmmssfff");
            m_colInfo.TactTime = string.Format("{0}:{1}:{2}.{3}",
                            (dtEnd - dtStart).Hours.ToString("00"),
                            (dtEnd - dtStart).Minutes.ToString("00"),
                            (dtEnd - dtStart).Seconds.ToString("00"),
                            (dtEnd - dtStart).Milliseconds.ToString("000"));

            GbVar.dbTactTime.Enqueue_UpdateDB(m_colInfo);
        }

        public static string GetFileNameInPathWithoutExt(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public static bool SetAnalogLaserPower(int nHeadNo)
        {
            //

            return false;
        }

        /// <summary>
        /// 패턴은 쓰면 안된다..
        /// </summary>
        /// <param name="nScannerHead"></param>
        /// <param name="dPower"></param>
        /// <returns></returns>
        public static bool SetAnalogLaserPower(int nScannerHead, double dPower)
        {
            //

            return false;
        }

        /// <summary>
        /// 패턴은 쓰면 안된다..
        /// </summary>
        /// <param name="nScannerHead"></param>
        /// <returns></returns>
        public static bool WaitSetAnalogPower(int nScannerHead)
        {
            //

            return false;
        }
        public static int GetLaserHead(int nScannerHead)
        {
            int nLaserMotorHead = nScannerHead;
            return nScannerHead;
        }

        public static int GetTableNo(int nLaserHead)
        {
            int nTableNo = nLaserHead * 2;
            return nTableNo;
        }

        //public static bool SetTempValue(ucNumericKeyInputButtonEx ucNumTarget, ref int nData)
        //{
        //    int nTmp;
        //    if (!int.TryParse(ucNumTarget.Text, out nTmp)) return false;
        //    nData = nTmp;
        //    return true;
        //}

        //public static bool SetTempValue(ucNumericKeyInputButtonEx ucNumTarget, ref double dData)
        //{
        //    double dTmp;
        //    if (!double.TryParse(ucNumTarget.Text, out dTmp)) return false;
        //    dData = dTmp;
        //    return true;
        //}

        //public static bool SetTempValue(ucNumericKeyInputButtonEx ucNumTarget, ref long lData)
        //{
        //    long lTmp;
        //    if (!long.TryParse(ucNumTarget.Text, out lTmp)) return false;
        //    lData = lTmp;
        //    return true;
        //}
        public static bool GetIn(int nAddress)
        {
            //if (!GbDevice.devCCLinkIO.IsOpen) return false;
            if (GbVar.GB_INPUT[nAddress] > 0) return true;
            return false;
        }

        public static bool GetIn(IODF.INPUT nAddress)
        {
            //if (!GbDevice.devCCLinkIO.IsOpen) return false;
            if (GbVar.GB_INPUT[(int)nAddress] > 0) return true;
            return false;
        }

        public static bool GetOut(int nAddress)
        {
            //if (!GbDevice.devCCLinkIO.IsOpen) return false;
            if (GbVar.GB_OUTPUT[nAddress] > 0) return true;
            return false;
        }

        public static bool GetOut(IODF.OUTPUT nAddress)
        {
            //if (!GbDevice.devCCLinkIO.IsOpen) return false;
            if (GbVar.GB_OUTPUT[(int)nAddress] > 0) return true;
            return false;
        }

        public static void SetOut(int nAddress, bool bState)
        {
            //if (!GbDevice.devCCLinkIO.IsOpen) return;

            //버저스킵 옵션 사용시
            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BUZZER_USE].bOptionUse)
            {
                if (nAddress == (int)IODF.OUTPUT.END_BUZZER || nAddress == (int)IODF.OUTPUT.ERROR_BUZZER)
                {
                    MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
                    MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, false);
                    return;
                }
            }

            if (nAddress < 0) return;
            MOTION.MotionMgr.Inst.SetOutput(nAddress, bState);
        }

        public static void SetOut(IODF.OUTPUT output, bool bState)
        {
            //if (!GbDevice.devCCLinkIO.IsOpen) return;
            if ((int)output < 0) return;
            MOTION.MotionMgr.Inst.SetOutput((int)output, bState);
        }

        public static bool IsGdTrayTableYMoveSafe()
        {
#if _NOTEBOOK
            return true;
#endif
            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN])
                //GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] ||
                //GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN] ||
                //GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] ||
                //GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN])
            {
                return false;
            }

            return true;
        }
		
		public static bool IsGdTrayTableYMoveSafe(int nTableNo, int nTargetPosIdx)
        {
#if _NOTEBOOK
            return true;
#endif

            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN])
            {
                // Target Pos와 Interlock Pos 비교
                int nTrayStageNo = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;
                int nCheckTableNo = nTableNo == 0 ? 1 : 0;

                double dGdTrayTableCurrPos1 = RecipeMgr.Inst.Rcp.dMotPos[nTrayStageNo].dPos[nTargetPosIdx];
                double dGdTrayTableCurrPos2 = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nCheckTableNo].GetRealPos();

                double dGdTrayTableDist = Math.Abs(dGdTrayTableCurrPos1 - dGdTrayTableCurrPos2);

                if (dGdTrayTableDist >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public static int GetGdTrayTableYMoveSafe()
        {
            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] ||
                GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] == GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] ||
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] == GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN])
            {
                if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == 0 && GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] == 0)
                {
                    return (int)ERDF.E_INTL_GD_TRAY_STG_1_OFF_UP_DOWN_OUTPUT;
                }

                if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] == 0 && GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN] == 0)
                {
                    return (int)ERDF.E_INTL_GD_TRAY_STG_2_OFF_UP_DOWN_OUTPUT;
                }

                return FNC.BUSY;
            }

            return FNC.SUCCESS;
        }


        public static bool IsGdTrayTableUpDownSafe(int nTable, bool bUp)
        {
            double dGd1 = 0;
            double dGd2 = 0;

            dGd1 = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetEncPos();
            dGd2 = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetEncPos();

            if (bUp)
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP + (nTable * 7)] == 1 &&
                    GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN + (nTable * 7)] == 0)
                {
                    //이미 업 되어있음
                    return true;
                }

                //트레이 겹쳐져있음
                if (Math.Abs(dGd1 - dGd2) < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                {
                    return false;
                }
            }
            else
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP + (nTable * 7)] == 0 &&
                    GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN + (nTable * 7)] == 1)
                {
                    //이미 다운 되어있음
                    return true;
                }

                //트레이 겹쳐져있음
                if (Math.Abs(dGd1 - dGd2) < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                {
                    return false;
                }
            }

            return true;
        }

        public static int IsDoorOpenOrPressEmo(bool bCheckDoorSafety)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            IODF.INPUT[] inputEms = { 
                IODF.INPUT.EMO_FRONT_SWITCH_SAW, 
                IODF.INPUT.EMO_REAR_SWITCH_SAW,           
                IODF.INPUT.EMO_SWITCH_1,
                IODF.INPUT.EMO_SWITCH_2,
                IODF.INPUT.EMO_SWITCH_3
            };

            // EMO 알람
            for (int nCnt = 0; nCnt < inputEms.Length; nCnt++)
            {
                if (GbVar.IO[inputEms[nCnt]] == 0)
                {
                    bool bIsOn = MotionMgr.Inst.GetInput(inputEms[nCnt]);
                    if (!bIsOn)
                    {
                        WriteEventLog("EMO ALARM OCCURED LOG", string.Format("EMO BIT {0} IS {1}", inputEms[nCnt], MotionMgr.Inst.GetInput(inputEms[nCnt]), bIsOn));
                        return (int)ERDF.E_EMO_1 + nCnt;
                    }
                    else
                    {
                        WriteEventLog("EMO ALARM UNEXPECTED LOG", string.Format("EMO BIT {0} IS {1}", inputEms[nCnt], MotionMgr.Inst.GetInput(inputEms[nCnt]), bIsOn));
                    }
                }
            }

            //IODF.INPUT[] inputDoor = { IODF.INPUT.FORNT_MONITOR_DOOR, IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR, IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR, IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR,
            //                            /*IODF.INPUT.FRONT_MONITOR_DOOR_SAW, IODF.INPUT.LF_SIDE_LF_DOOR, IODF.INPUT.LF_SIDE_MID_DOOR,*/
            //                         /*IODF.INPUT.LF_SIDE_RT_DOOR,IODF.INPUT.OHT_FENCE_FRONT_DOOR,*/ IODF.INPUT.REAR_CENTER_DOOR,IODF.INPUT.REAR_LF_DOOR,IODF.INPUT.REAR_RT_DOOR,
            //                         IODF.INPUT.RT_SIDE_CENTER_DOOR,IODF.INPUT.RT_SIDE_LF_DOOR,IODF.INPUT.RT_SIDE_RT_DOOR};
            
            if(bCheckDoorSafety)
            {
                IODF.INPUT[] inputDoor = {
                IODF.INPUT.FORNT_MONITOR_DOOR,
                IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR,
                IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR,
                IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR,
                IODF.INPUT.REAR_CENTER_DOOR,
                IODF.INPUT.REAR_LF_DOOR,
                IODF.INPUT.REAR_RT_DOOR,
                IODF.INPUT.RT_SIDE_CENTER_DOOR,
                IODF.INPUT.RT_SIDE_LF_DOOR,
                IODF.INPUT.RT_SIDE_RT_DOOR
            };

                for (int nCnt = 0; nCnt < inputDoor.Length; nCnt++)
                {
                    if (GbVar.IO[inputDoor[nCnt]] == 0)
                    {
                        return (int)ERDF.E_DOOR_OPEN_1 + nCnt;
                    }
                }
            }


            return FNC.SUCCESS;
        }

        // Reallocates an array with a new size, and copies the contents
        // of the old array to the new array.
        // Arguments:
        //  oldArray  the old array, to be reallocated.
        //  newSize  the new array size.
        // Returns    A new array with the same contents.
        public static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
                System.Array.Copy(oldArray, newArray, preserveLength);
            return newArray;
        }

        public static void ListViewToCSV(ListView listView, string filePath, bool includeHidden)
        {
            //make header string
            StringBuilder result = new StringBuilder();
            WriteCSVRow(result, listView.Columns.Count, i => includeHidden || listView.Columns[i].Width > 0, i => listView.Columns[i].Text);

            //export data rows
            foreach (ListViewItem listItem in listView.Items)
                WriteCSVRow(result, listView.Columns.Count, i => includeHidden || listView.Columns[i].Width > 0, i => listItem.SubItems[i].Text);

            File.WriteAllText(filePath, result.ToString());
        }

        private static void WriteCSVRow(StringBuilder result, int itemsCount, Func<int, bool> isColumnNeeded, Func<int, string> columnValue)
        {
            bool isFirstTime = true;
            for (int i = 0; i < itemsCount; i++)
            {
                if (!isColumnNeeded(i))
                    continue;

                if (!isFirstTime)
                    result.Append(",");
                isFirstTime = false;

                result.Append(String.Format("\"{0}\"", columnValue(i)));
            }
            result.AppendLine();
        }

        static string GetFilePathMemoryCheck(string strPath, string strFileName, double dFileSizeMb = 10.0, string strExt = "log")
        {
            if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);
            string strFullFileName = string.Format("{0}\\{1}.{2}", strPath, strFileName, strExt);

            // 용량 체크
            try
            {
                int nCountName = 1;

                while (true)
                {
                    FileInfo fi = new FileInfo(strFullFileName);
                    if (!fi.Exists) break;

                    // Default : 10 MB
                    if (fi.Length < 1048576 * dFileSizeMb)
                        break;

                    strFullFileName = string.Format("{0}\\{1}_{2}.{3}", strPath, strFileName, nCountName++, strExt);
                }

            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                return string.Format("{0}\\{1}.{2}", strPath, strFileName, strExt);
            }

            return strFullFileName;
        }

        /// <summary>
        /// LOT END 조건인지 확인 (자재가 없는지 확인)
        /// </summary>
        /// <returns></returns>
        public static bool IsLotEndCondition(bool bIncludeVac, bool bIsOnlySorterCheck = false, bool bExcludeLoader = false)
        {
            bool bExistStrip = false;

            // 감지 센서, VACUUM, 자재 데이터

            if (bIsOnlySorterCheck == false && bExcludeLoader == false)
            {
                #region 로더
                // 매거진이 있는지 확인
                //if (bIncludeVac)
                {
                    bExistStrip |= GbVar.IO[IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR] == 1;
                    bExistStrip |= GbVar.IO[IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR] == 1;
                    bExistStrip |= GbVar.IO[IODF.INPUT.MATERIAL_PROTRUSION_CHECK_SENSOR] == 0;
                }

                // 매거진 트랜스퍼와 레일 사이의 자재 감지 유무
                //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE].bOptionUse)
                //{
                //    //bExistStrip |= GbVar.IO[IODF.INPUT.LD_VISON_X_GRIP_MATERIAL_CHECK] == 0;
                //    bExistStrip |= GbVar.IO[IODF.INPUT.SPARE_3111] == 1; // A접 변경
                //}


                #endregion

                if (bExistStrip) return false;
            }

            if (bIsOnlySorterCheck == false)
            {
                #region 다이싱
                // 레일에 자재가 있는지 확인
                //if (bIncludeVac)
                {
                    bExistStrip |= GbVar.IO[IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 1;
                    bExistStrip |= GbVar.IO[IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1;

                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE].bOptionUse)
                    //{
                    //    //bExistStrip |= GbVar.IO[IODF.INPUT.LD_VISON_X_GRIP_MATERIAL_CHECK] == 0;
                    //    bExistStrip |= GbVar.IO[IODF.INPUT.SPARE_3111] == 1; // A접 변경
                    //}
                }

                bExistStrip |= GbVar.Seq.sStripRail.Info.IsStrip();

                // Strip Picker
                if (bIncludeVac)
                {
                    bExistStrip |= GbVar.IO[IODF.OUTPUT.STRIP_PK_VAC_ON] == 1;
                    bExistStrip |= GbVar.IO[IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP] == 1;
                    bExistStrip |= GbVar.IO[IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD] == 1;
                    bExistStrip |= IsAVacOn((int)IODF.A_INPUT.STRIP_PK_VAC);
                }

                bExistStrip |= GbVar.Seq.sStripTransfer.Info.IsStrip();

                // Saw
                bExistStrip |= GbVar.Seq.sCuttingTable.Info.IsStrip();

                // Unit Picker
                if (bIncludeVac)
                {
                    bExistStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP] == 1;
                    bExistStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_VAC_ON] == 1;
                    bExistStrip |= IsAVacOn((int)IODF.A_INPUT.UNIT_PK_VAC);
                }

                bExistStrip |= GbVar.Seq.sUnitTransfer.Info.IsStrip();

                //// Unit Picker Scrap 1
                //bExistStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_SCRAP_VAC_PUMP_1] == 1;
                //bExistStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_SCRAP_VAC_1_OFF_2] == 1;

                //// Unit Picker Scrap 2
                //bExistStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_SCRAP_VAC_PUMP_2] == 1;
                //bExistStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_SCRAP_VAC_2_OFF_2] == 1;
                #endregion

                if (bExistStrip) return false;

                #region 수세기
                //// Second Cleaning Table
                //if (bIncludeVac)
                //{
                //    bExistStrip |= GbVar.IO[IODF.OUTPUT.SECOND_CLEANING_ZONE_KIT_PURGE_ON_OFF_SOL] == 1;
                //    bExistStrip |= GbVar.IO[IODF.OUTPUT.SECOND_CLEANING_ZONE_KIT_VACUUM_PUMP_ON_SOL] == 1;
                //    bExistStrip |= IsAVacOn((int)IODF.A_INPUT.SECOND_CLEANING_ZONE_KIT_MATERIAL_VACUUM_SENSOR);
                //}

                //bExistStrip |= GbVar.Seq.sCleaning[0].Info.IsStrip();

                //// Second Ultrasonic Table
                //if (bIncludeVac)
                //{
                //    bExistStrip |= GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_KIT_VACUUM_PUMP_ON_SOL] == 1;
                //    bExistStrip |= GbVar.IO[IODF.OUTPUT.SECOND_ULTRASONIC_ZONE_KIT_PURGE_ON_OFF_SOL] == 1;
                //    bExistStrip |= IsAVacOn((int)IODF.A_INPUT.SECOND_ULTRASONIC_ZONE_KIT_MATERIAL_VACUUM_SENSOR);
                //}

                //bExistStrip |= GbVar.Seq.sCleaning[1].Info.IsStrip();

                //// Cleaner Picker
                //if (bIncludeVac)
                //{
                //    bExistStrip |= GbVar.IO[IODF.OUTPUT.PICKER_VACUUM_PUMP_ON_SOL] == 1;
                //    bExistStrip |= GbVar.IO[IODF.OUTPUT.PICKER_KIT_VACUUM_PURGE_ON_OFF_SOL] == 1;
                //    bExistStrip |= IsAVacOn((int)IODF.A_INPUT.CLEAN_PICKER_VACUUM_SENSOR);
                //}

                //bExistStrip |= GbVar.Seq.sCleaning[2].Info.IsStrip();
                #endregion

                if (bExistStrip) return false;
            }

            #region 소터
            // Dry Block Table
            if (bIncludeVac)
            {
                bExistStrip |= GbVar.IO[IODF.OUTPUT.DRY_BLOCK_VAC_PUMP] == 1;
                bExistStrip |= GbVar.IO[IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 1;
                bExistStrip |= IsAVacOn((int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC);
            }

            bExistStrip |= GbVar.Seq.sUnitDry.Info.IsStrip();

            // Map Picker
            if (bIncludeVac)
            {
                //bExistStrip |= GbVar.IO[IODF.OUTPUT.MAP_PK_PUMP] == 1;
                bExistStrip |= GbVar.IO[IODF.OUTPUT.MAP_PK_VAC_ON] == 1;
                bExistStrip |= IsAVacOn((int)IODF.A_INPUT.MAP_PK_WORK_VAC);
            }

            bExistStrip |= GbVar.Seq.sMapTransfer.Info.IsStrip();

            // Map Table 1
            if (bIncludeVac)
            {
                //bExistStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_1_WORK_VAC_PUMP] == 1;
                bExistStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_1_VAC_ON] == 1;
                bExistStrip |= IsAVacOn((int)IODF.A_INPUT.MAP_STG_1_WORK_VAC);
            }

            bExistStrip |= GbVar.Seq.sMapVisionTable[CFG_DF.MAP_STG_1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START];

            bExistStrip |= GbVar.Seq.sMapVisionTable[0].Info.IsStrip();

            // Map Table 2
            if (bIncludeVac)
            {
                //bExistStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_2_WORK_VAC_PUMP] == 1;
                bExistStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_2_VAC_ON] == 1;
                bExistStrip |= IsAVacOn((int)IODF.A_INPUT.MAP_STG_2_WORK_VAC);
            }

            bExistStrip |= GbVar.Seq.sMapVisionTable[CFG_DF.MAP_STG_2].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START];

            bExistStrip |= GbVar.Seq.sMapVisionTable[1].Info.IsStrip();

            // Chip Picker
            if (bIncludeVac)
            {
                for (int nPickerHeadNo = 0; nPickerHeadNo < CFG_DF.MAX_PICKER_HEAD_CNT; nPickerHeadNo++)
                {
                    for (int nPickerPadNo = 0; nPickerPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPickerPadNo++)
                    {
                        bExistStrip |= IsChipPickerVacOn(nPickerHeadNo, nPickerPadNo);
                    }
                }
            }

            bExistStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[0].IsPickerAllEmptyUnit();
            bExistStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[1].IsPickerAllEmptyUnit();
            #endregion

            if (bExistStrip) return false;

            return bExistStrip == false;
        }

        /// <summary>
        /// P&P 관련 자재가 없는지
        /// 트레이 테이블의 트레이 배출해도 안전한지
        /// </summary>
        /// <param name="bIncludeVac">배큠 포함</param>
        /// <returns></returns>
        public static bool IsPnpNoStrip(bool bIncludeVac)
        {
            bool bExistStrip = false;

            #region 소터

            if (GbVar.Seq.sMapVisionTable[0].Info.IsStrip())
            {
                bExistStrip |= GbVar.Seq.sMapVisionTable[CFG_DF.MAP_STG_1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START];
            }

            if (GbVar.Seq.sMapVisionTable[1].Info.IsStrip())
            {
                bExistStrip |= GbVar.Seq.sMapVisionTable[CFG_DF.MAP_STG_2].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START];
            }

            // Chip Picker
            if (bIncludeVac)
            {
                for (int nPickerHeadNo = 0; nPickerHeadNo < CFG_DF.MAX_PICKER_HEAD_CNT; nPickerHeadNo++)
                {
                    for (int nPickerPadNo = 0; nPickerPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPickerPadNo++)
                    {
                        bExistStrip |= IsChipPickerVacOn(nPickerHeadNo, nPickerPadNo);
                    }
                }
            }

            bExistStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[0].IsPickerAllEmptyUnit();
            bExistStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[1].IsPickerAllEmptyUnit();

            for (int nHeadCnt = 0; nHeadCnt < CFG_DF.MAX_PICKER_HEAD_CNT; nHeadCnt++)
            {
                //bExistStrip |= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadCnt].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START];
                bExistStrip |= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadCnt].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START];

                bExistStrip |= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadCnt].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START];
                bExistStrip |= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadCnt].bSeqIfVar[seqPkgPickNPlace.SYNC_RW_PLACE_START];
                bExistStrip |= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadCnt].bSeqIfVar[seqPkgPickNPlace.SYNC_NG_PLACE_START];
            }
            #endregion

            if (bExistStrip) return false;

            return bExistStrip == false;
        }

        public static bool IsAInputLow(int nInputNo)
        {
            return ((GbVar.GB_AINPUT[nInputNo] - ConfigMgr.Inst.Cfg.Vac[nInputNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nInputNo].dRatio)
                < ConfigMgr.Inst.Cfg.Vac[nInputNo].dVacLevelLow;
        }

        public static bool IsAVacOn(int nInputNo)
        {
            return IsAInputLow(nInputNo);
        }

        public static bool IsChipPickerVacOn(int nPkNo, int nPadNo)
        {
            int nInputNo = (int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM + (CFG_DF.MAX_PICKER_PAD_CNT * nPkNo) + nPadNo;
            return ((GbVar.GB_AINPUT[nInputNo] - ConfigMgr.Inst.Cfg.Vac[nInputNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nInputNo].dRatio)
                < ConfigMgr.Inst.Cfg.Vac[nInputNo].dVacLevelLow;
        }

        public static bool IsChipPickerSkip(int nHeadNo, int nPadNo)
        {
            OPTNUM[] optNums;
            if (nHeadNo == CFG_DF.HEAD_1)
            {
                optNums = new OPTNUM[] { OPTNUM.PICKER_1_PAD_1_USE, OPTNUM.PICKER_1_PAD_2_USE, OPTNUM.PICKER_1_PAD_3_USE, OPTNUM.PICKER_1_PAD_4_USE, OPTNUM.PICKER_1_PAD_5_USE, OPTNUM.PICKER_1_PAD_6_USE, OPTNUM.PICKER_1_PAD_7_USE, OPTNUM.PICKER_1_PAD_8_USE };
            }
            else
            {
                optNums = new OPTNUM[] { OPTNUM.PICKER_2_PAD_1_USE, OPTNUM.PICKER_2_PAD_2_USE, OPTNUM.PICKER_2_PAD_3_USE, OPTNUM.PICKER_2_PAD_4_USE, OPTNUM.PICKER_2_PAD_5_USE, OPTNUM.PICKER_2_PAD_6_USE, OPTNUM.PICKER_2_PAD_7_USE, OPTNUM.PICKER_2_PAD_8_USE };
            }

            return !ConfigMgr.Inst.Cfg.itemOptions[(int)optNums[nPadNo]].bOptionUse;
        }

        #region 트레이 비전 (KEYENCE)
        public static void SetTrayInspCountReset(int nMeasureTableNo = 0)
        {
            //for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1; nCol++)
            //{
            //    for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - 1; nRow++)
            //    {
            //        GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE = -1;
            //    }
            //}
            GbDev.trayVision.ListMeasure.Clear();
        }
        //
        public static void SetTrayInspReq(bool bManual = false)
        {
            GbDev.trayVision.MZeroCommandFormat(bManual);
        } 
        public static void GetCurrentInspResult(int nMeasureTableNo = 0)
        {
            for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX; nCol++)
            {
                if (GbDev.trayVision.TOOL_VALUE != null)
                {
                    // 0 : 정상, 1 없음, 2 들뜸
                    if (GbDev.trayVision.TOOL_VALUE[nCol] < RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue[nMeasureTableNo])
                    {
                        GbDev.trayVision.MEASURE_CODE[nCol] = 1;
                    }
                    else if (GbDev.trayVision.TOOL_VALUE[nCol] > RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue[nMeasureTableNo])
                    {
                        GbDev.trayVision.MEASURE_CODE[nCol] = 2;
                    }
                    else
                    {
                        GbDev.trayVision.MEASURE_CODE[nCol] = 0;
                    }
                    //칩오프셋이 0이면 짝수번째 측정한건 쓰지 않는다.
                    if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY != 0)
                    {
                        // 정상이지만 다음 스텝은 비정상 일수도 있으니 체크
                        if (GbDev.trayVision.MEASURE_CODE[nCol] == 0)
                        {
                            if (GbDev.trayVision.TOOL_VALUE[nCol] < RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue[nMeasureTableNo])
                            {
                                GbDev.trayVision.MEASURE_CODE[nCol] = 1;
                            }
                            else if (GbDev.trayVision.TOOL_VALUE[nCol] > RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue[nMeasureTableNo])
                            {
                                GbDev.trayVision.MEASURE_CODE[nCol] = 2;
                            }
                            else
                            {
                                GbDev.trayVision.MEASURE_CODE[nCol] = 0;
                            }
                        }
                    }
                }
            }
        }
        public static void GetTrayInspResult(int nMeasureTableNo = 0)
        {
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
            {
                if (GbDev.trayVision.ListMeasure.Count != 0)
                {
                    for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY; nRow++)
                    {
                        for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX; nCol++)
                        {
                            // 0 : 정상, 1 없음, 2 들뜸
                            if (GbDev.trayVision.ListMeasure[nRow * 2][nCol] < RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue[nMeasureTableNo])
                            {
                                GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE = 1;
                            }
                            else if (GbDev.trayVision.ListMeasure[nRow * 2][nCol] > RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue[nMeasureTableNo])
                            {
                                GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE = 2;
                            }
                            else
                            {
                                GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE = 0;
                            }
                            //칩오프셋이 0이면 짝수번째 측정한건 쓰지 않는다.
                            if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY != 0)
                            {
                                // 정상이지만 다음 스텝은 비정상 일수도 있으니 체크
                                if (GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE == 0)
                                {
                                    if (GbDev.trayVision.ListMeasure[nRow * 2 + 1][nCol] < RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue[nMeasureTableNo])
                                    {
                                        GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE = 1;
                                    }
                                    else if (GbDev.trayVision.ListMeasure[nRow * 2 + 1][nCol] > RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue[nMeasureTableNo])
                                    {
                                        GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE = 2;
                                    }
                                    else
                                    {
                                        GbVar.Seq.sUldGDTrayTable[nMeasureTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE = 0;
                                    }
                                }
                            }
                        }
                    }
                   
                }

            }
        }
        public static void DeleteAllDB(int nPeriod)
        {
            //GbVar.dbAlarmLog.Enqueue_DeleteDB(GbVar.dbAlarmLog.TABLE_NAME, nPeriod);
            //GbVar.dbEventData.Enqueue_DeleteDB(GbVar.dbEventData.TABLE_NAME, nPeriod);
            ////for (int nIdx = 0; nIdx < GbVar.dbLogData.Length; nIdx++)
            ////{
            ////    GbVar.dbLogData[nIdx].Enqueue_DeleteDB(GbVar.dbLogData[nIdx].TABLE_NAME, nPeriod);
            ////}
            ////GbVar.dbMgzInfo.Enqueue_DeleteDB(GbVar.dbMgzInfo.TABLE_NAME, nPeriod);
            //for (int nidx = 0; nidx < GbVar.dbStripInfo.Length; nidx++)
            //{
            //    GbVar.dbStripInfo[nidx].Enqueue_DeleteDB(GbVar.dbStripInfo[nidx].TABLE_NAME, nPeriod);
            //}
        }

        #endregion
    }


    public class SystemTime_Change
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEMTIME
        {
            internal ushort wYear;
            internal ushort wMonth;
            internal ushort wDayOfWeek;
            internal ushort wDay;
            internal ushort wHour;
            internal ushort wMinute;
            internal ushort wSecond;
            internal ushort wMilliseconds; //yyyymmddhhmmss
        }

        [DllImport("kernel32.dll")]
        internal static extern Boolean SetLocalTime(ref SYSTEMTIME lpSystemTime);
        public static void SetTime(DateTime dtNewTime)
        {
            // Call the native GetSystemTime method
            // with the defined structure.
            SYSTEMTIME systime = new SYSTEMTIME();

            systime.wYear = (ushort)dtNewTime.Year;
            systime.wMonth = (ushort)dtNewTime.Month;
            systime.wDay = (ushort)dtNewTime.Day;
            systime.wHour = (ushort)dtNewTime.Hour;
            systime.wMinute = (ushort)dtNewTime.Minute;
            systime.wSecond = (ushort)dtNewTime.Second;
            systime.wMilliseconds = (ushort)dtNewTime.Millisecond;

            SetLocalTime(ref systime);
        }

        //SystemTime_Change.SetTime(2018, 2, 5, 15, 28, 30);
        public static bool SetTime(short sYear, short sMonth, short sDay, short sHour, short sMinute, short sSecond)
        {
            // Call the native GetSystemTime method
            // with the defined structure.
            SYSTEMTIME systime = new SYSTEMTIME();

            systime.wYear = (ushort)sYear;
            systime.wMonth = (ushort)sMonth;
            systime.wDay = (ushort)sDay;
            systime.wHour = (ushort)sHour;
            systime.wMinute = (ushort)sMinute;
            systime.wSecond = (ushort)sSecond;
            systime.wMilliseconds = 0;

            bool bRet = false;
            bRet = SetLocalTime(ref systime);
            return bRet;
        }
    }

    public static class SaveFile
    {
        public static string GET_DirNameDate(string sPreDir)
        {
            string s = sPreDir + DateTime.Now.Year.ToString() + "_" + string.Format("{0:00}", DateTime.Now.Month) + "\\";
            if (!Directory.Exists(s)) Directory.CreateDirectory(s);
            return s + DateTime.Now.Year.ToString() + string.Format("{0:00}", DateTime.Now.Month) + string.Format("{0:00}", DateTime.Now.Day) + "_";
        }
        public static string GET_PathOperation(double dt, string preDir)
        {
            DateTime d = DateTime.FromOADate(dt);
            string s = preDir + d.Year.ToString() + "_" + string.Format("{0:00}", d.Month) + "\\";
            return s + d.Year.ToString() + string.Format("{0:00}", d.Month) + string.Format("{0:00}", d.Day) + "_";
        }
        public static string GET_LogTime(string str)
        {
            string s = DateTime.Now.Year.ToString() + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "\t";
            s += string.Format("{0:00}", DateTime.Now.Hour) + ":" + string.Format("{0:00}", DateTime.Now.Minute)
              + ":" + string.Format("{0:00}", DateTime.Now.Second) + "." + string.Format("{0:00}", DateTime.Now.Millisecond)
              + "\t" + str + "\r\n";
            return s;
        }


        //public static void SaveITSInfo(string fName, string sLogs)
        //{
        //    string s = PathMgr.ITSInfo + DateTime.Now.Year.ToString() + "_" + string.Format("{0:00}", DateTime.Now.Month) + "\\";
        //    if (!Directory.Exists(s)) Directory.CreateDirectory(s);
        //    string fn = s + fName + ".txt";
        //    try
        //    {
        //        WR_File(fn, sLogs, false);
        //    }
        //    catch (Exception E) 
        //    {
        //    }
        //}

        //public static void SaveStripDefectCount(string sITSID, string sLogs)
        //{
        //    string fn = SaveFile.GET_DirNameDate(PathMgr.LogStripDefectCount) + sITSID + ".TXT";
        //    try
        //    {
        //        if (Directory.Exists(fn)) return;
        //        WR_File(fn, sLogs, false);
        //    }
        //    catch (Exception e) 
        //    { 
        //    }
        //}

        //public static void SaveITS_StripCount(string sData)
        //{
        //    WR_File(PathMgr.ITSCount, sData, false);
        //}

        //public static void SaveStripDefectLocationList(string sITSID, string sLogs)
        //{
        //    string fn = SaveFile.GET_DirNameDate(PathMgr.LogStripDefectLocationList) + sITSID + ".TXT";
        //    try
        //    {
        //        if (Directory.Exists(fn)) return;
        //        WR_File(fn, sLogs, false);
        //        WR_File(fn, sLogs, false);
        //    }
        //    catch (Exception e) 
        //    {

        //    }
        //}
        //public static void SaveITS_StripLocation(string sData)
        //{
        //    WR_File(PathMgr.ITSLocation, sData, false);
        //}

        public static void WR_File(string path, string s, bool appeded)
        {
            int fm = (int)FileMode.Create;          // 파일 만듬. 같은 이름 파일이 있으면 이전 파일 지우고 만듬.
            if (appeded) fm = (int)FileMode.Append; // 추가모드로 OPEN. 파일 없으면 만듬.
            try
            {
                FileStream fs = new FileStream(path, (FileMode)fm);
                StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
                sw.Write(s);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e) 
            {

            }
        }
    }

    public static class LoadnPutObjValue
    {
        /// <summary>
        /// 객체를 XML로 Serialize합니다.
        /// </summary>
        /// <param name="classObject"></param>
        /// <returns></returns>
        public static string ConvertObjectToXMLString(object classObject)
        {
            string xmlString = null;
            XmlSerializer xmlSerializer = new XmlSerializer(classObject.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, classObject);
                memoryStream.Position = 0;
                xmlString = new StreamReader(memoryStream).ReadToEnd();
            }
            return xmlString;
        }

        /// <summary>
        /// 스트링을 객체로 Deserialize합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T ConvertXmlStringtoObject<T>(string xmlString)
        {        
            T classObject;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(xmlString))
            {
                classObject = (T)xmlSerializer.Deserialize(stringReader);
            }
            return classObject;
        }


        /// <summary>
        /// 클래스 내의 선언된 변수의 이름과 값을 "name=value;name=value;" 형태의 문자열로 반환합니다.
        /// 단, 사용자 정의 Type은 불가
        /// </summary>
        /// <param name="tp">저장할 클래스 타입</param>
        /// <param name="obj">저장할 클래스 객체</param>
        /// <returns></returns>
        public static string LoadValue(Type tp, object obj)
        {
            string strContents = "";
            try
            {
                var bindingflags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

                foreach (FieldInfo info in tp.GetFields(bindingflags))
                {
                    Type type = info.FieldType;

                    #region IsArray
                    if (type.IsArray)
                    {
                        if (type.Name == "Int16[]")
                        {
                            Int16[] arr = (Int16[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                strContents += arr[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        if (type.Name == "Int32[]")
                        {
                            Int32[] arr = (Int32[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                strContents += arr[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        if (type.Name == "Int64[]")
                        {
                            Int64[] arr = (Int64[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                strContents += arr[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        if (type.Name == "Boolean[]")
                        {
                            Boolean[] arr = (Boolean[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                strContents += arr[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        if (type.Name == "Double[]")
                        {
                            Double[] arr = (Double[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                strContents += arr[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        if (type.Name == "Single[]")
                        {
                            Single[] arr = (Single[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                strContents += arr[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        if (type.Name == "Char[]")
                        {
                            Char[] arr = (Char[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i] == '\0')
                                {
                                    strContents += "\\0,";
                                }
                                else
                                {
                                    strContents += arr[i] + ",";
                                }
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        if (type.Name == "String[]")
                        {
                            String[] arr = (String[])info.GetValue(obj);

                            strContents += info.Name + "=";

                            for (int i = 0; i < arr.Length; i++)
                            {
                                strContents += arr[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }

                    }
                    #endregion

                    #region IsList
                    else if (type.FullName.Contains("List"))
                    {
                        if (type.FullName.Contains("Int16"))
                        {
                            List<short> list = new List<short>();
                            list = (List<short>)info.GetValue(obj);
                            strContents += info.Name + "=";

                            if (list == null)
                            {
                                strContents += ";";
                                continue;
                            }

                            for (int i = 0; i < list.Count; i++)
                            {
                                strContents += list[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        else if (type.FullName.Contains("Int32"))
                        {
                            List<int> list = new List<int>();
                            list = (List<int>)info.GetValue(obj);
                            strContents += info.Name + "=";

                            if (list == null)
                            {
                                strContents += ";";
                                continue;
                            }

                            for (int i = 0; i < list.Count; i++)
                            {
                                strContents += list[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        else if (type.FullName.Contains("Int64"))
                        {
                            List<long> list = new List<long>();
                            list = (List<long>)info.GetValue(obj);
                            strContents += info.Name + "=";

                            if (list == null)
                            {
                                strContents += ";";
                                continue;
                            }

                            for (int i = 0; i < list.Count; i++)
                            {
                                strContents += list[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        else if (type.FullName.Contains("Double"))
                        {
                            List<double> list = new List<double>();
                            list = (List<double>)info.GetValue(obj);
                            strContents += info.Name + "=";

                            if (list == null)
                            {
                                strContents += ";";
                                continue;
                            }

                            for (int i = 0; i < list.Count; i++)
                            {
                                strContents += list[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        else if (type.FullName.Contains("String"))
                        {
                            List<string> list = new List<string>();
                            list = (List<string>)info.GetValue(obj);
                            strContents += info.Name + "=";

                            if (list == null)
                            {
                                strContents += ";";
                                continue;
                            }

                            for (int i = 0; i < list.Count; i++)
                            {
                                strContents += list[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                        else if (type.FullName.Contains("Boolean"))
                        {
                            List<bool> list = new List<bool>();
                            list = (List<bool>)info.GetValue(obj);
                            strContents += info.Name + "=";

                            if (list == null)
                            {
                                strContents += ";";
                                continue;
                            }

                            for (int i = 0; i < list.Count; i++)
                            {
                                strContents += list[i] + ",";
                            }
                            strContents = strContents.TrimEnd(',') + ";";
                        }
                    }
                    #endregion

                    #region ETC
                    else
                    {
                        strContents += info.Name + "=" + info.GetValue(obj) + ";";
                    }
                    #endregion
                }
            }
            catch (Exception)
            {
                strContents = "";
            }

            return strContents;
        }

        /// <summary>
        /// 클래스 내의 선언된 변수 중 입력한 변수 명과 일치하는 변수에 값을 넣습니다.  
        /// 단, 사용자 정의 Type은 불가
        /// </summary>
        /// <param name="name">변수 명</param>
        /// <param name="value">변수 값</param>
        /// <param name="tp">입력할 클래스 타입</param>
        /// <param name="obj">입력할 클래스 객체</param>
        /// <returns></returns>
        public static bool PutValue(List<string> name, List<string> value, Type tp, object obj)
        {
            bool bret = true;

            try
            {
                var bindingflags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

                foreach (FieldInfo info in tp.GetFields(bindingflags))
                {
                    int nIndex = name.FindIndex(x => x == info.Name);
                    if (nIndex < 0) continue;
                    object point = info.GetValue(obj);

                    #region IsArray
                    if (point is Array)
                    {
                        string[] sArr = value[nIndex].Split(',');

                        if (point is System.Int16[])
                        {
                            short[] sValue = new short[sArr.Length];
                            for (int i = 0; i < sArr.Length; i++)
                            {
                                if (!short.TryParse(sArr[i], out sValue[i])) sValue[i] = 0;
                            }

                            info.SetValue(obj, sValue);
                        }
                        else if (point is System.Int32[])
                        {
                            int[] nValue = new int[sArr.Length];
                            for (int i = 0; i < sArr.Length; i++)
                            {
                                if (!int.TryParse(sArr[i], out nValue[i])) nValue[i] = 0;
                            }

                            info.SetValue(obj, nValue);
                        }
                        else if (point is System.Int64[])
                        {
                            long[] lValue = new long[sArr.Length];
                            for (int i = 0; i < sArr.Length; i++)
                            {
                                if (!long.TryParse(sArr[i], out lValue[i])) lValue[i] = 0;
                            }

                            info.SetValue(obj, lValue);
                        }
                        else if (point is System.Single[])
                        {
                            float[] fValue = new float[sArr.Length];
                            for (int i = 0; i < sArr.Length; i++)
                            {
                                if (!float.TryParse(sArr[i], out fValue[i])) fValue[i] = 0;
                            }

                            info.SetValue(obj, fValue);
                        }
                        else if (point is System.Double[])
                        {
                            double[] dValue = new double[sArr.Length];
                            for (int i = 0; i < sArr.Length; i++)
                            {
                                if (!double.TryParse(sArr[i], out dValue[i])) dValue[i] = 0;
                            }

                            info.SetValue(obj, dValue);
                        }
                        else if (point is System.String[])
                        {
                            info.SetValue(obj, sArr);
                        }
                        else if (point is System.Char[])
                        {
                            char[] cValue = new char[sArr.Length];
                            for (int i = 0; i < sArr.Length; i++)
                            {
                                if (sArr[i] == "\\0")
                                {
                                    cValue[i] = '\0';
                                }
                                else
                                {
                                    cValue[i] = Convert.ToChar(sArr[i]);
                                }
                            }

                            info.SetValue(obj, cValue);
                        }
                        else if (point is System.Boolean[])
                        {
                            bool[] bValue = new bool[sArr.Length];
                            for (int i = 0; i < sArr.Length; i++)
                            {
                                bValue[i] = Convert.ToBoolean(sArr[i]);
                            }

                            info.SetValue(obj, bValue);
                        }
                    }
                    else if (point is List<short>)
                    {
                        List<short> list = new List<short>();

                        if (value[nIndex] == "")
                        {
                            info.SetValue(obj, list);
                            continue;
                        }
                        string[] sArr = value[nIndex].Split(',');

                        for (int nCnt = 0; nCnt < sArr.Length; nCnt++)
                        {
                            short sValue = 0;
                            if (!short.TryParse(sArr[nCnt], out sValue)) sValue = 0;

                            list.Add(sValue);
                        }
                        info.SetValue(obj, list);
                    }
                    else if (point is List<int>)
                    {
                        List<int> list = new List<int>();

                        if (value[nIndex] == "")
                        {
                            info.SetValue(obj, list); 
                            continue;
                        }
                        string[] sArr = value[nIndex].Split(',');

                        for (int nCnt = 0; nCnt < sArr.Length; nCnt++)
                        {
                            int nValue = 0;
                            if (!int.TryParse(sArr[nCnt], out nValue)) nValue = 0;

                            list.Add(nValue);
                        }
                        info.SetValue(obj, list);
                    }
                    else if (point is List<double>)
                    {
                        List<double> list = new List<double>();

                        if (value[nIndex] == "")
                        {
                            info.SetValue(obj, list);
                            continue;
                        }
                        string[] sArr = value[nIndex].Split(',');

                        for (int nCnt = 0; nCnt < sArr.Length; nCnt++)
                        {
                            double dValue = 0.0;
                            if (!double.TryParse(sArr[nCnt], out dValue)) dValue = 0.0;

                            list.Add(dValue);
                        }
                        info.SetValue(obj, list);
                    }
                    else if (point is List<bool>)
                    {
                        List<bool> list = new List<bool>();

                        if (value[nIndex] == "")
                        {
                            info.SetValue(obj, list);
                            continue;
                        }
                        string[] sArr = value[nIndex].Split(',');

                        for (int nCnt = 0; nCnt < sArr.Length; nCnt++)
                        {
                            bool bValue = false;
                            if (!bool.TryParse(sArr[nCnt], out bValue)) bValue = false;

                            list.Add(bValue);
                        }
                        info.SetValue(obj, list);
                    }
                    else if (point is List<string>)
                    {
                        List<string> list = new List<string>();

                        if (value[nIndex] == "")
                        {
                            info.SetValue(obj, list);
                            continue;
                        }
                        string[] sArr = value[nIndex].Split(',');
                        list = sArr.ToList<string>();
                        info.SetValue(obj, list);
                    }
                    #endregion

                    #region IsNotArray
                    else if (point is int)
                    {
                        int nValue = 0;
                        if (!int.TryParse(value[nIndex], out nValue)) nValue = 0;
                        info.SetValue(obj, nValue);
                    }
                    else if (point is long)
                    {
                        long lValue = 0;
                        if (!long.TryParse(value[nIndex], out lValue)) lValue = 0;
                        info.SetValue(obj, lValue);
                    }
                    else if (point is string)
                        info.SetValue(obj, value[nIndex]);
                    else if (point is float)
                    {
                        float fValue = 0;
                        if (!float.TryParse(value[nIndex], out fValue)) fValue = 0;
                        info.SetValue(obj, fValue);
                    }
                    else if (point is double)
                    {
                        double dValue = 0;
                        if (!double.TryParse(value[nIndex], out dValue)) dValue = 0;
                        info.SetValue(obj, dValue);
                    }
                    else if (point is bool)
                    {
                        bool bValue = Convert.ToBoolean(value[nIndex]);
                        info.SetValue(obj, bValue);
                    }
                    #endregion

                    #region UserDef
                    else if (point is TrayBatchInfo)
                    {
                        TrayBatchInfo Value = new TrayBatchInfo();

                        info.SetValue(obj, Value);
                    }
                    #endregion
                }
            }
            catch (Exception)
            {
                bret = false;
            }

            return bret;
        }
    }

    public class SeqManager
    {
        #region 단일
        /// <summary>
        /// 객체를 시퀀스 DB에 업데이트 합니다.
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool UpdateSeqObj(Type tp, object obj)
        {
            bool bret = true;

            try
            {
                string strValue = LoadnPutObjValue.ConvertObjectToXMLString(obj);

                GbVar.dbSeqInfo.UpLoadSeq(tp.Name, strValue);
            }
            catch (Exception)
            {
                bret = false;
            }

            return bret;
        }

        /// <summary>
        /// 배열형 객체를 시퀀스DB에 업데이트 합니다.
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool UpdateSeqObj(Type tp, object[] obj)
        {
            bool bret = true;

            try
            {
                string strValue = "";
                for (int i = 0; i < obj.Length; i++)
                {
                    strValue = LoadnPutObjValue.ConvertObjectToXMLString(obj[i]);

                    //1번이 Base이므로 +1함
                    GbVar.dbSeqInfo.UpLoadSeq(tp.Name + (i + 1), strValue);
                }
            }
            catch (Exception)
            {
                bret = false;
            }

            return bret;
        }

        /// <summary>
        /// 시퀀스DB로 부터 XML을 불러와 객체에 넣습니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nCount"></param>
        /// <returns></returns>
        public object LoadSeqObj<T>(int nCount = -1)
        {
            object ret = new object();

            try
            {
                string strValue = "";

                if (nCount >= 0)
                {
                    strValue = GbVar.dbSeqInfo.LoadSeq(typeof(T).Name.ToString() + (nCount + 1).ToString());
                }
                else
                {
                    strValue = GbVar.dbSeqInfo.LoadSeq(typeof(T).Name.ToString());
                }

                ret = LoadnPutObjValue.ConvertXmlStringtoObject<T>(strValue);
            }
            catch (Exception)
            {
                ret = null;
            }
            return ret;
        }
        #endregion
   
        #region 다중
        /// <summary>
        /// 모든 시퀀스 객체의 변수 값을 DB에 업데이트합니다.  
        /// </summary>
        public void UploadSeq()
        {
            UpdateSeqObj(typeof(seqLdMzLoading), GbVar.Seq.sLdMzLoading);
            UpdateSeqObj(typeof(seqLdMzLotStart), GbVar.Seq.sLdMzLotStart);
            UpdateSeqObj(typeof(seqStripRail), GbVar.Seq.sStripRail);
            UpdateSeqObj(typeof(seqStripTransfer), GbVar.Seq.sStripTransfer);
            UpdateSeqObj(typeof(seqSawCuttingTable), GbVar.Seq.sCuttingTable);
            UpdateSeqObj(typeof(seqUnitTransfer), GbVar.Seq.sUnitTransfer);
            UpdateSeqObj(typeof(seqUnitDry), GbVar.Seq.sUnitDry);
            UpdateSeqObj(typeof(seqMapTransfer), GbVar.Seq.sMapTransfer);
            UpdateSeqObj(typeof(seqMapVisionTable), GbVar.Seq.sMapVisionTable);
            UpdateSeqObj(typeof(seqPkgPickNPlace), GbVar.Seq.sPkgPickNPlace);
            UpdateSeqObj(typeof(seqUldGoodTrayTable), GbVar.Seq.sUldGDTrayTable);
            UpdateSeqObj(typeof(seqUldReworkTrayTable), GbVar.Seq.sUldRWTrayTable);
            UpdateSeqObj(typeof(seqUldRejectBoxTable), GbVar.Seq.sUldRjtBoxTable);
            UpdateSeqObj(typeof(seqUldTrayTransfer), GbVar.Seq.sUldTrayTransfer);
            UpdateSeqObj(typeof(seqUldTrayElvGood), GbVar.Seq.sUldTrayElvGood);
            UpdateSeqObj(typeof(seqUldTrayElvRework), GbVar.Seq.sUldTrayElvRework);
            UpdateSeqObj(typeof(seqUldTrayElvEmpty), GbVar.Seq.sUldTrayElvEmpty);
        }

        /// <summary>
        /// 모든 시퀀스 객체에 값을 넣습니다.
        /// </summary>
        public void DownLoadSeq()
        {
            try
            {
                object obj = LoadSeqObj<seqLdMzLoading>();
                if (obj != null) GbVar.Seq.sLdMzLoading = (seqLdMzLoading)obj;

                obj = LoadSeqObj<seqLdMzLotStart>();
                if (obj != null) GbVar.Seq.sLdMzLotStart = (seqLdMzLotStart)obj;

                obj = LoadSeqObj<seqStripRail>();
                if (obj != null) GbVar.Seq.sStripRail = (seqStripRail)obj;

                obj = LoadSeqObj<seqStripTransfer>();
                if (obj != null) GbVar.Seq.sStripTransfer = (seqStripTransfer)obj;

                obj = LoadSeqObj<seqUnitTransfer>();
                if (obj != null) GbVar.Seq.sUnitTransfer = (seqUnitTransfer)obj;

                obj = LoadSeqObj<seqUnitDry>();
                if (obj != null) GbVar.Seq.sUnitDry = (seqUnitDry)obj;

                obj = LoadSeqObj<seqMapTransfer>();
                if (obj != null) GbVar.Seq.sMapTransfer = (seqMapTransfer)obj;

                obj = LoadSeqObj<seqUldReworkTrayTable>();
                if (obj != null) GbVar.Seq.sUldRWTrayTable = (seqUldReworkTrayTable)obj;

                obj = LoadSeqObj<seqUldRejectBoxTable>();
                if (obj != null) GbVar.Seq.sUldRjtBoxTable = (seqUldRejectBoxTable)obj;

                obj = LoadSeqObj<seqUldTrayTransfer>();
                if (obj != null) GbVar.Seq.sUldTrayTransfer = (seqUldTrayTransfer)obj;

                obj = LoadSeqObj<seqUldTrayElvRework>();
                if (obj != null) GbVar.Seq.sUldTrayElvRework = (seqUldTrayElvRework)obj;

                obj = LoadSeqObj<seqSawCuttingTable>();
                if (obj != null) GbVar.Seq.sCuttingTable = (seqSawCuttingTable)obj;

                for (int i = 0; i < 2; i++)
                {
                    obj = LoadSeqObj<seqMapVisionTable>(i);
                    if (obj != null) GbVar.Seq.sMapVisionTable[i] = (seqMapVisionTable)obj;
                    obj = LoadSeqObj<seqPkgPickNPlace>(i);
                    if (obj != null) GbVar.Seq.sPkgPickNPlace.pInfo[i] = (PickerHeadInfo)obj;
                    obj = LoadSeqObj<seqUldGoodTrayTable>(i);
                    if (obj != null) GbVar.Seq.sUldGDTrayTable[i] = (seqUldGoodTrayTable)obj;
                    obj = LoadSeqObj<seqUldTrayElvEmpty>(i);
                    if (obj != null) GbVar.Seq.sUldTrayElvEmpty[i] = (seqUldTrayElvEmpty)obj;
                    obj = LoadSeqObj<seqUldTrayElvGood>(i);
                    if (obj != null) GbVar.Seq.sUldTrayElvGood[i] = (seqUldTrayElvGood)obj;

                }
            }
            catch (Exception)
            {
            }
        }
        #endregion   
    }

    public class StripManager
    {
        /// <summary>
        /// 스르립 정보를 해당하는 전용변수에 복사합니다.
        /// </summary>
        /// <param name="strip">스트립 정보</param>
        /// <param name="eMdl">복사할 모듈</param>
        /// <returns></returns>
        public bool DownloadStripInfo(StripInfo strip, STRIP_MDL eMdl)
        {
            bool bRet = true;
            try
            {
                switch (eMdl)
                {
                    case STRIP_MDL.STRIP_RAIL:
                        strip.CopyTo(ref GbVar.Seq.sStripRail.Info);

                        break;
                    case STRIP_MDL.STRIP_TRANSFER:
                        strip.CopyTo(ref GbVar.Seq.sStripTransfer.Info);

                        break;
                    case STRIP_MDL.CUTTING_TABLE:
                        strip.CopyTo(ref GbVar.Seq.sCuttingTable.Info);

                        break;

                    case STRIP_MDL.UNIT_TRANSFER:
                        strip.CopyTo(ref GbVar.Seq.sUnitTransfer.Info);

                        break;
                    //case STRIP_MDL.SECOND_CLEAN_ZONE:
                    //    strip.CopyTo(ref GbVar.Seq.sCleaning[0].Info);

                    //    break;
                    //case STRIP_MDL.SECOND_ULTRA_ZONE:
                    //    strip.CopyTo(ref GbVar.Seq.sCleaning[1].Info);

                    //    break;
                    //case STRIP_MDL.CLEANER_PICKER:
                    //    strip.CopyTo(ref GbVar.Seq.sCleaning[2].Info);

                    //    break;
                    case STRIP_MDL.UNIT_DRY:
                        strip.CopyTo(ref GbVar.Seq.sUnitDry.Info);

                        break;
                    case STRIP_MDL.MAP_TRANSFER:
                        strip.CopyTo(ref GbVar.Seq.sMapTransfer.Info);

                        break;
                    case STRIP_MDL.MAP_VISION_TABLE_1:
                        strip.CopyTo(ref GbVar.Seq.sMapVisionTable[0].Info);

                        break;
                    case STRIP_MDL.MAP_VISION_TABLE_2:
                        strip.CopyTo(ref GbVar.Seq.sMapVisionTable[1].Info);

                        break;
                    case STRIP_MDL.NONE:
                    case STRIP_MDL.MAX:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        /// <summary>
        /// 최근에 생산한 5개의 스트립 정보를 불러 옵니다.
        /// </summary>
        /// <param name="eMdl">불러올 모듈</param>
        /// <returns></returns>
        public StripInfo[] LoadStripsInfo(STRIP_MDL eMdl)
        {
            StripInfo[] strips;
            try
            {
                strips = GbVar.dbStripInfo[(int)eMdl].SelectStripsInfo();
            }
            catch (Exception)
            {
                strips = null;
            }

            return strips;
        }


        /// <summary>
        /// 가장 최근에 생산한 1개의 스트립의 정보를 불러 옵니다.
        /// </summary>
        /// <param name="eMdl">불러올 모듈</param>
        /// <returns></returns>
        public StripInfo LoadStripInfo(STRIP_MDL eMdl)
        {
            StripInfo strip;
            try
            {
                strip = GbVar.dbStripInfo[(int)eMdl].SelectRecentStripInfo();
            }
            catch (Exception)
            {
                strip = null;
            }

            return strip;
        }

        /// <summary>
        /// 모든 모듈의 스트립 정보를 스퀀스 객체에 다운로드 합니다.
        /// </summary>
        /// <returns></returns>
        public bool DownloadAllMdlStripInfo()
        {
            bool bRet = true;
            try
            {
                StripInfo strip = new StripInfo();

                strip = LoadStripInfo(STRIP_MDL.STRIP_RAIL);
                DownloadStripInfo(strip, STRIP_MDL.STRIP_RAIL);

                strip = LoadStripInfo(STRIP_MDL.STRIP_TRANSFER);
                DownloadStripInfo(strip, STRIP_MDL.STRIP_TRANSFER);

                strip = LoadStripInfo(STRIP_MDL.CUTTING_TABLE);
                DownloadStripInfo(strip, STRIP_MDL.CUTTING_TABLE);

                strip = LoadStripInfo(STRIP_MDL.UNIT_TRANSFER);
                DownloadStripInfo(strip, STRIP_MDL.UNIT_TRANSFER);

                //strip = LoadStripInfo(STRIP_MDL.SECOND_CLEAN_ZONE);
                //DownloadStripInfo(strip, STRIP_MDL.SECOND_CLEAN_ZONE);

                //strip = LoadStripInfo(STRIP_MDL.SECOND_ULTRA_ZONE);
                //DownloadStripInfo(strip, STRIP_MDL.SECOND_ULTRA_ZONE);

                //strip = LoadStripInfo(STRIP_MDL.CLEANER_PICKER);
                //DownloadStripInfo(strip, STRIP_MDL.CLEANER_PICKER);

                strip = LoadStripInfo(STRIP_MDL.UNIT_DRY);
                DownloadStripInfo(strip, STRIP_MDL.UNIT_DRY);

                strip = LoadStripInfo(STRIP_MDL.MAP_TRANSFER);
                DownloadStripInfo(strip, STRIP_MDL.MAP_TRANSFER);

                strip = LoadStripInfo(STRIP_MDL.MAP_VISION_TABLE_1);
                DownloadStripInfo(strip, STRIP_MDL.MAP_VISION_TABLE_1);

                strip = LoadStripInfo(STRIP_MDL.MAP_VISION_TABLE_2);
                DownloadStripInfo(strip, STRIP_MDL.MAP_VISION_TABLE_2);
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }
    }

    public class LotManager
    {

        #region 호스트정보
         public bool DeleteLotInfo(string strLotId)
        {
            bool bRet = true;
            try
            {
                GbVar.dbLotInfo.DeleteInfo(strLotId);
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// 새로운 Lot이 투입되었을 시 Lot정보를 추가 합니다.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public bool InsertLotInfo(HostLotInfo Info)
        {
            bool bRet = true;
            try
            {
                GbVar.dbLotInfo.InsertInfo(Info);
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// 기존 등록되어있는 모든 Lot의 정보를 삭제하고 새로 추가합니다.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool UpdateAllLotInfo(BindingList<HostLotInfo> info)
        {
            bool bRet = true;
            try
            {
                GbVar.dbLotInfo.UpdateAllInfo(info);
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }       
        
        /// <summary>
        /// DB에 저장되어있는 모든 Lot의 정보를 반환합니다.
        /// </summary>
        /// <param name="strLotId"></param>
        /// <returns></returns>
        public BindingList<HostLotInfo> DownloadLotInfo()
        {
            BindingList<HostLotInfo> bRet = new BindingList<HostLotInfo>();
            try
            {
                bRet = GbVar.dbLotInfo.DownloadInfo();
            }
            catch (Exception)
            {
            }
            return bRet;
        }
        #endregion

        #region 생산 수량
        /// <summary>
        /// 생산 수량 로그를 추가합니다.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public bool InsertProcQtyLog(EqpProcInfo Info)
        {
            bool bRet = true;
            try
            {
                GbVar.dbProcQtyLog2.InsertInfo(Info);
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
		/// 220611 pjh
        /// 등록되어있는 모든 생산 수량을 삭제하고 새로 추가합니다.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool UpdateAllProcQtyInfo(BindingList<EqpProcInfo> info)
        {
            bool bRet = true;
            try
            {
                GbVar.dbProcsQtyInfo.UpdateAllInfo(info);
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// 220611 pjh
        /// DB에 저장된 생산 수량 정보를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public BindingList<EqpProcInfo> DownloadProcQtyInfo()
        {
            BindingList<EqpProcInfo> bRet = new BindingList<EqpProcInfo>();
            try
            {
                bRet = GbVar.dbProcsQtyInfo.DownloadInfo();
            }
            catch (Exception)
            {
            }
            return bRet;
        }
        #endregion
    }

    #region 프린터

    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.
            di.pDocName = "My C#.NET RAW Document";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            // Open the file.
            FileStream fs = new FileStream(szFileName, FileMode.Open);
            // Create a BinaryReader on the file.
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            Byte[] bytes = new Byte[fs.Length];
            bool bSuccess = false;
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }

        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;

            // How many characters are in the string?
            // Fix from Nicholas Piasecki:
            // dwCount = szString.Length;
            dwCount = (szString.Length + 1) * Marshal.SystemMaxDBCSCharSize;

            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DOCINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string printerDocumentName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string printerDocumentDataType;
    }

    public class RawPrinter
    {
        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = false,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long OpenPrinter(string pPrinterName, ref IntPtr phPrinter, int pDefault);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = false,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long StartDocPrinter(IntPtr hPrinter, int Level, ref DOCINFO pDocInfo);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long StartPagePrinter(IntPtr hPrinter);

        [
            DllImport("winspool.drv", CharSet = CharSet.Ansi, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long WritePrinter(IntPtr hPrinter, string data, int buf, ref int pcWritten);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long EndPagePrinter(IntPtr hPrinter);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long EndDocPrinter(IntPtr hPrinter);

        [
            DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
        public static extern long ClosePrinter(IntPtr hPrinter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printerJobName">My C#.NET RAW Document</param>
        /// <param name="rawStringToSendToThePrinter"><보낼 글자/param>
        /// <param name="printerNameAsDescribedByPrintManager">v프린터 이름</param>
        public static void SendToPrinter(string rawStringToSendToThePrinter, string printerNameAsDescribedByPrintManager)
        {
            IntPtr handleForTheOpenPrinter = new IntPtr();
            DOCINFO documentInformation = new DOCINFO();
            int printerBytesWritten = 0;
            documentInformation.printerDocumentName = "My C#.NET RAW Document";
            documentInformation.printerDocumentDataType = "RAW";
            OpenPrinter(printerNameAsDescribedByPrintManager, ref handleForTheOpenPrinter, 0);
            StartDocPrinter(handleForTheOpenPrinter, 1, ref documentInformation);
            StartPagePrinter(handleForTheOpenPrinter);
            WritePrinter(handleForTheOpenPrinter, rawStringToSendToThePrinter, rawStringToSendToThePrinter.Length,
                         ref printerBytesWritten);
            EndPagePrinter(handleForTheOpenPrinter);
            EndDocPrinter(handleForTheOpenPrinter);
            ClosePrinter(handleForTheOpenPrinter);
        }
    }
    #endregion
}
