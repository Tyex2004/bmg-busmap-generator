using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusMapGenerator
{
    internal class Route
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        private int[] _color = new int[3];  // 默认初始化为 [0,0,0]

        [JsonProperty("color")]
        public int[] Color
        {
            get => (int[])_color.Clone();  // 返回副本保护内部结构
            set
            {
                if (value == null || value.Length != 3)
                {
                    _color = new int[3];
                }
                else
                {
                    _color[0] = Clamp(value[0]);
                    _color[1] = Clamp(value[1]);
                    _color[2] = Clamp(value[2]);
                }
            }
        }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("path")]
        public List<(int, List<int>)> Path { get; set; } = new List<(int, List<int>)>();

        [JsonProperty("end")]
        public int End { get; set; }

        private static int Clamp(int x) => Math.Max(0, Math.Min(255, x));
    }
}
