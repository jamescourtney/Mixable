using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Tester
{
    internal class Class1
    {
        public static void Main(string[] args)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Foo.Bar.Baz.Configuration));

            using (var sw = new StringWriter())
            {
                var c = (Foo.Bar.Baz.Configuration)serializer.Deserialize(File.OpenRead("derived.xml"));

                serializer.Serialize(sw, c);
                Console.WriteLine(sw.ToString());
            }
        }
    }
}
