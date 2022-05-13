using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Tester
{
    internal class Class1
    {
        public static void Main(string[] args)
        {
            using Stream fs = File.OpenRead("derived2.xml");

            var doc = XDocument.Load(fs);
            var config = new Foo.Bar.Baz.Bat.Configuration(doc.Root);
        }
    }
}