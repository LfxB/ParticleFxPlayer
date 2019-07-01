﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace XMLSerializationHelper
{
    public static class XMLHelper
    {
        public static T LoadXMLToObject<T>(T theObject, string path)
        {
            try
            {
                //Reads XML into an object (in this case, the venueList)
                
                XmlSerializer x = new XmlSerializer(theObject.GetType());

                using (Stream stream = File.OpenRead(path))
                {
                    return (T)x.Deserialize(stream);
                    //stream.Dispose();
                }
            }
            catch { return default(T); }
        }

        public static void SaveObjectToXML<T>(T theObject, string path)
        {
            XmlSerializer x = new XmlSerializer(theObject.GetType());

            using (TextWriter writer = File.CreateText(path))
            {
                x.Serialize(writer, theObject);
                //writer.Dispose();
            }
        }

        private static void SaveTest()
        {
            List<string> temp = new List<string>()
            {
                "Relaxed",
                "Comfy",
                "Fancy",
                "Formal"
            };
            SaveObjectToXML(temp, @"C:\Users\Samuel\Desktop");
        }
    }
}