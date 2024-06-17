using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NSS_3330S
{
    public static class LogKeyWordMgr
    {
        public static Dictionary<string, Property> KeyWordDic= new Dictionary<string, Property>();
        public static string LogPaths = Directory.GetCurrentDirectory() + "\\KeyWordList.bin";

        public static bool bPathChanging = false;
        public static bool bPathChangeConfirm = false;

        private static object locker = new Object();

        /// <summary>
        /// Save Dictionary
        /// </summary>
        /// <param name="file"></param>
        public static void Write(string file)
        {
            lock (locker)
            {
                try
                {
                    using (FileStream fs = File.OpenWrite(file))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        // Put count.
                        writer.Write(KeyWordDic.Count);
                        // Write pairs.
                        foreach (var pair in KeyWordDic)
                        {
                            writer.Write(pair.Key);
                            writer.Write(pair.Value.Color.ToString());
                            //writer.Write(pair.Value.Find.ToString());
                            writer.Write(pair.Value.Usage.ToString());
                        }
                    }
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// Load Dictionary
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Dictionary<string, Property> Read(string file)
        {
            try
            {
                lock (locker)
                {
                    if (!File.Exists(file))
                    {
                        return new Dictionary<string, Property>();
                    }
                    var result = new Dictionary<string, Property>();
                    using (FileStream fs = File.OpenRead(file))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        // Get count.
                        int count = reader.ReadInt32();
                        Property saveP;
                        // Read in all pairs.
                        for (int i = 0; i < count; i++)
                        {
                            string key = reader.ReadString();
                            saveP = new Property();
                            saveP.Color = Convert.ToInt32(reader.ReadString());
                            //saveP.Find = Convert.ToBoolean(reader.ReadString());
                            saveP.Usage = Convert.ToBoolean(reader.ReadString());

                            result[key] = saveP;
                        }

                        return result;
                    }
                }
            }
            catch (System.Exception ex)
            {
                return new Dictionary<string, Property>();
            }
        }
    }

    public class Property
    {

        public Property()
        {
        }


        //public Property(int color, bool find, bool Usage)
        //{
        //    this.Color = color;
        //    this.Find = find;
        //    this.Usage = Usage;
        //}
        public Property(int color, bool Usage)
        {
            this.Color = color;
            this.Usage = Usage;
        }

        #region __Property__
        public int Color { get; set; }

        //public bool Find { get; set; }

        public bool Usage { get; set; }
        #endregion
    }
}
