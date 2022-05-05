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
            XmlSerializer serializer = new XmlSerializer(typeof(Foo.Bar.Baz.Bat.Configuration));

            using (var sw = new StringWriter())
            {
                var c = (Foo.Bar.Baz.Bat.Configuration)serializer.Deserialize(File.OpenRead("derived.xml"));
                var c2 = (Foo.Bar.Baz.Bat.Configuration)serializer.Deserialize(File.OpenRead("derived2.xml"));

                serializer.Serialize(sw, c);
                Console.WriteLine(sw.ToString());
            }
        }
    }
}
