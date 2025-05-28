using Newtonsoft.Json;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusMapGenerator
{
    internal class Road
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("nodes")]
        public int[] NodesId { get; set; } = new int[2];

        [JsonIgnore]
        public Node[] Nodes => NodesId.Select(nodesId => Program.Nodes[nodesId]).ToArray();

        [JsonIgnore]
        public decimal[] JSONCoordStart => Nodes[0].JSONCoord;

        [JsonIgnore]
        public decimal[] JSONCoordEnd => Nodes[1].JSONCoord;

        [JsonIgnore]
        public SvgPoint SvgCoordStart => Nodes[0].SvgCoord;

        [JsonIgnore]
        public SvgPoint SvgCoordEnd => Nodes[1].SvgCoord;

        [JsonIgnore]
        public int Direction  // 从零向北开始顺时针
        {
            get
            {
                decimal dx = JSONCoordEnd[0] - JSONCoordStart[0]; // 东西方向分量
                decimal dy = JSONCoordEnd[1] - JSONCoordStart[1]; // 南北方向分量（上北）

                // 计算 atan2(Δy, Δx)，返回角度（单位：度）
                double angleRad = Math.Atan2((double)dy, (double)dx);  // [-π, π]
                double angleDeg = angleRad * (180.0 / Math.PI);        // 转换为角度

                // 将角度转换为从正东开始顺时针的角度
                // 例如：正东=0，正北=90，正西=180/-180，正南=-90
                double clockwiseFromEast = (90 - angleDeg + 360) % 360;

                // 以每45度为一个方向区间，四舍五入后再模8，得到方向编号
                int direction = (int)Math.Round(clockwiseFromEast / 45.0) % 8;

                return direction; // 0=北，1=东北，2=东，3=东南，4=南，5=西南，6=西，7=西北
            }
        }

        [JsonIgnore]
        public SvgLine RPGraph => new()
        {
            StartX = SvgCoordStart.X,
            StartY = SvgCoordStart.Y,
            EndX = SvgCoordEnd.X,
            EndY = SvgCoordEnd.Y,
            Stroke = new SvgColourServer(Color.FromArgb(200, 0, 200)),
            StrokeWidth = 3
        };
    }
}
