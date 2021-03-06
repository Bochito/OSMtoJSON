﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OSMtoJSON
{
    class Program
    {
        static IEnumerable<XElement> SimpleStream(string resource, string nodeName){
            using(var reader = XmlReader.Create(resource)){
                reader.MoveToContent();
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:{
                            if(reader.Name == nodeName){
                                XElement el = XElement.ReadFrom(reader) as XElement;
                                if(el != null){
                                    yield return el;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var stp = new Stopwatch();

            IEnumerable<Node> nodes;
            List<string> tags = new List<string>();
            
            stp.Start();
            nodes = SimpleStream(args[0], "node")
                            .Select(node => new Node{
                                id = node.Attribute("id").Value,
                                lat = node.Attribute("lat").Value,
                                lon = node.Attribute("lon").Value,
                                tags = node.Elements()
                                            .Select(innerTags => new Tuple<string, string>(innerTags.Attribute("k").Value, innerTags.Attribute("v").Value))
                                            .ToDictionary(tuple1 => tuple1.Item1, tuple2 => tuple2.Item2)
                            });

            foreach (var node in nodes)
            {
                foreach (var tag in node.tags)
                {
                    tags.Add(tag.Key);
                }
            }

            var distinctTags = tags.Distinct();
            File.WriteAllLines(@".\tags.txt", distinctTags);

            var json = JsonConvert.SerializeObject(nodes);
            File.WriteAllText(@".\map.json", json);
            stp.Stop();

            Console.WriteLine($"Finished in {stp.Elapsed}");
            
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
