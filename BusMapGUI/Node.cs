using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BusMapGenerator
{
    public class Node
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<double> Coord { get; set; }
    }
}
