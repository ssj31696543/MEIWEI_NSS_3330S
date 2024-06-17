using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S
{
    public static class JSONHelper
    {
        private static Formatting _formatting = Formatting.Indented;

        public static string GetJSON(object sourceObject)
        {
            string json = JsonConvert.SerializeObject(sourceObject, _formatting);
            return json;
        }

        public static T GetObject<T>(string json)
        {
            T targetObject = JsonConvert.DeserializeObject<T>(json);
            return targetObject;
        }

        public static void Serialize(object sourceObject, Stream targetStream)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(targetStream))
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                    {
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        jsonSerializer.Formatting = _formatting;
                        jsonSerializer.Serialize(jsonTextWriter, sourceObject);
                        jsonTextWriter.Flush();
                    }
                }
            }
            catch(Exception ex)
            {
            }
        }

        public static T Deserialize<T>(Stream sourceStream)
        {
            using (StreamReader streamReader = new StreamReader(sourceStream))
            {
                using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    jsonSerializer.Formatting = _formatting;
                    T targetObject = jsonSerializer.Deserialize<T>(jsonTextReader);
                    return targetObject;
                }
            }
        }
    }
}
