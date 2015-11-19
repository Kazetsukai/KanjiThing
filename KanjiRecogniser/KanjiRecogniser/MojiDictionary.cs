using Svg;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace KanjiRecogniser
{
    internal class MojiDictionary
    {
        static XmlDocument _xmlDoc;
        static XmlDocument XmlDoc
        {
            get
            {
                if (_xmlDoc == null)
                {
                    var nameTable = new XmlNamespaceManager(new NameTable());
                    nameTable.AddNamespace("kvg", "urn:ignore");
                    _xmlDoc = new XmlDocument(nameTable.NameTable);
                    _xmlDoc.XmlResolver = null;
                }
                return _xmlDoc;
            }
        }

        public MojiDictionary(string folder)
        {
            if (Directory.Exists(folder))
            {
                var files = Directory.EnumerateFiles(folder, "*.svg");
                
                var kanji = files.Select(f => LoadMojiFile(f)).ToList();
            }
        }

        private Moji LoadMojiFile(string filename)
        {
            XmlDoc.Load(filename);

            var svg = SvgDocument.Open(XmlDoc);
            var code = filename.Split('\\').Last().Split('.').First();

            var elem = svg.GetElementById("kvg:" + code);
            if (elem == null)
            {
                Console.WriteLine("Warning! Couldn't find element at " + code);
                Console.ReadLine();
            }
            else
            {
                var paths = elem.Descendants().Where(d => d is SvgPath).Cast<SvgPath>();
                Console.WriteLine("Processing " + code + "...");
            }
            return new Moji();
        }
    }
}