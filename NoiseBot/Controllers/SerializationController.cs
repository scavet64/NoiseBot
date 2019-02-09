using Newtonsoft.Json;
using NoiseBot.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoiseBot.Controllers
{
    class SerializationController
    {
        public static T DeserializeFile<T>(string path)
        {
            T rVal;
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        string json = sr.ReadToEnd();
                        rVal = JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            catch (IOException ioex)
            {
                throw new InvalidConfigException("Could not read settings file: " + ioex.Message);
            }
            catch (JsonException jex)
            {
                throw new InvalidConfigException("settings file incorrectly formatted: " + jex.Message);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return rVal;
        }

        public static void SerializeFile<t>(t objectToSave, string path)
        {
            if (objectToSave != null)
            {
                lock (objectToSave)
                {
                    using (StreamWriter file = File.CreateText(path))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, objectToSave);
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
