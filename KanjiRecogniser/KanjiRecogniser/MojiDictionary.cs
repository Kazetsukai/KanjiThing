using Svg;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Svg.Pathing;
using System.Collections.Generic;
using System.Drawing;

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

                Console.Clear();
                foreach (var path in paths)
                {
                    foreach (var pathSeg in path.PathData)
                    {
                        var points = ConvertToPoints(pathSeg).ToList();
                        // Now to do something with the points

                        foreach (var p in points)
                        {
                            Console.SetCursorPosition((int)(p.X / 2), (int)(p.Y / 3));
                            Console.Write('#');
                        }

                    }
                    Console.ReadLine();
                }
                Console.ReadLine();
            }
            return new Moji();
        }

        const int SegmentCount = 30;
        private IEnumerable<PointF> ConvertToPoints(SvgPathSegment arg)
        {
            if (arg is SvgMoveToSegment)
            {
                // Ignore this, as Svg library converts everything to absolute coords
            }
            else if (arg is SvgCubicCurveSegment)
            {
                var curve = arg as SvgCubicCurveSegment;

                for (int i = 0; i < SegmentCount; i++)
                {
                    // Cubic bezier yo
                    var t = i / (double)(SegmentCount - 1);
                    var c1 = Math.Pow(1 - t, 3);
                    var c2 = 3 * Math.Pow(1 - t, 2) * t;
                    var c3 = 3 * (1 - t) * Math.Pow(t, 2);
                    var c4 = Math.Pow(t, 3);

                    yield return new PointF((float)(
                        c1 * curve.Start.X +
                        c2 * curve.FirstControlPoint.X +
                        c3 * curve.SecondControlPoint.X +
                        c4 * curve.End.X), (float)(
                        c1 * curve.Start.Y +
                        c2 * curve.FirstControlPoint.Y +
                        c3 * curve.SecondControlPoint.Y +
                        c4 * curve.End.Y)
                    );
                }
            }
            else
                throw new NotImplementedException();
        }
    }
}