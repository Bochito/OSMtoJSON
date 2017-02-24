using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OSMtoJSON
{
    class Program
    {
        static void Main(string[] args)
        {
            var stp = new Stopwatch();
            var listOfTimes = new List<long>();
            IEnumerable<Node> nodes;
            List<string> tags = new List<string>();

            using (var file = new StreamReader(args[0]))
            {
                stp.Start();
                var xml = XDocument.Load(file);
                stp.Stop();

                listOfTimes.Add(stp.ElapsedMilliseconds);
                Console.WriteLine($"took {stp.ElapsedMilliseconds} Milliseconds to load file");
                stp.Reset();

                stp.Start();
                nodes = xml.Descendants()
                            .AsParallel()
                            .Where(node => node.Name.LocalName == "node")
                            .Select(node => new Node
                            {
                                id = node.Attribute("id").Value,
                                lat = node.Attribute("lat").Value,
                                lon = node.Attribute("lon").Value,
                                tags = node.Elements()
                                            .Select(innerTags => new Tuple<string, string>(innerTags.Attribute("k").Value, innerTags.Attribute("v").Value))
                                            .ToDictionary(tuple1 => tuple1.Item1, tuple2 => tuple2.Item2)
                            });
                stp.Stop();

                listOfTimes.Add(stp.ElapsedMilliseconds);
                Console.WriteLine($"took {stp.ElapsedMilliseconds} Milliseconds to build IEnumerable<Node>");
                stp.Reset();
            }

            Console.WriteLine($"Found {nodes.Count()} nodes");

            foreach (var node in nodes)
            {
                foreach (var tag in node.tags)
                {
                    tags.Add(tag.Key);
                }
            }

            var distinctTags = tags.Distinct();
            foreach (var tag in distinctTags)
            {
                Console.WriteLine(tag);
            }


            var json = JsonConvert.SerializeObject(nodes);

            Directory.CreateDirectory(@"C:\OutputMaps\");
            File.WriteAllText(@"C:\OutputMaps\map.json", json);

            long totalTime = 0;
            foreach (var time in listOfTimes)
            {
                totalTime += time;
            }

            Console.WriteLine($"Total time {TimeSpan.FromMilliseconds(totalTime)}");

            Console.ReadLine();
        }

        class Node
        {
            public string id { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public Dictionary<string, string> tags { get; set; } = new Dictionary<string, string>();
        }
    }
}
