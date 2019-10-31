using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BatteryPower.Helpers
{
    class XmlHelper
    {
        public static void SaveToXml(string filePath, object sourceObj)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && sourceObj != null)
            {
                FileInfo fi = new FileInfo(filePath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(sourceObj.GetType());
                    XmlSerializerNamespaces nameSpace = new XmlSerializerNamespaces();

                    nameSpace.Add("", ""); //not ot output the default namespace
                    xmlSerializer.Serialize(writer, sourceObj, nameSpace);
                }
            }
        }


        public static object LoadFromXml(string filePath, Type type)
        {
            object result = null;

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(type);
                    result = xmlSerializer.Deserialize(reader);
                }
            }
            return result;
        }
    }
}
