using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BusMapGenerator
{
    internal class Station
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("en_name")]
        public string EnName { get; set; } = "";

        [JsonProperty("road_id")]
        public int RoadId { get; set; }

        [JsonProperty("on_road_pos")]
        public decimal OnRoadPos { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; } = "";

        [JsonProperty("connects_mtr")]
        public string[] ConnectsMtr { get; set; } = [];

        [JsonProperty("note")]
        public string[] Note { get; set; } = [];

        [JsonIgnore]
        public Road Road => Program.Roads[RoadId];

        [JsonIgnore]
        public decimal[] JSONCoord
        {
            get
            {
                decimal[] roadStart = Road.JSONCoordStart;
                decimal[] roadEnd = Road.JSONCoordEnd;
                decimal x0 = roadStart[0];
                decimal y0 = roadStart[1];
                decimal x1 = roadEnd[0];
                decimal y1 = roadEnd[1];
                return [x0 + (x1 - x0) * OnRoadPos, y0 + (y1 - y0) * OnRoadPos];
            }
            set
            {
                decimal[] roadStart = Road.JSONCoordStart;
                decimal[] roadEnd = Road.JSONCoordEnd;
                decimal x0 = roadStart[0];
                decimal y0 = roadStart[1];
                decimal x1 = roadEnd[0];
                decimal y1 = roadEnd[1];

                decimal dx = x1 - x0;
                decimal dy = y1 - y0;

                if (dx == 0 && dy == 0) return;

                // 根据直线上的投影反推 OnRoadPos
                decimal px = value[0] - x0;
                decimal py = value[1] - y0;

                decimal lengthSquared = dx * dx + dy * dy;
                OnRoadPos = (dx * px + dy * py) / lengthSquared;
                //OnRoadPos = Math.Clamp(OnRoadPos, 0.03m, 0.97m);
                OnRoadPos = Math.Clamp(OnRoadPos, 3m / Road.Length.Coefficient, 1 - 3m / Road.Length.Coefficient);
            }
        }

        [JsonIgnore]
        public SvgPoint SVGCoord => Utils.CoordJSONToSvg(JSONCoord);

        [JsonIgnore]
        public SKPoint SkiaCoord => Utils.CoordJSONToSkia(JSONCoord);

        [JsonIgnore]
        public Point WPFCoord => Utils.CoordSkiaToWPF(SkiaCoord, Program.RPSkiaElement);

        [JsonIgnore]
        public int GeoSide => Side == "left" ? (Road.Direction + 6) % 8 : (Road.Direction + 2) % 8;

        [JsonIgnore]
        public SvgCircle RPGraph => new()
        {
            CenterX = SVGCoord.X,
            CenterY = SVGCoord.Y,
            Radius = 2,
            Fill = new SvgColourServer(System.Drawing.Color.Yellow),
            Stroke = new SvgColourServer(System.Drawing.Color.Black),
            StrokeWidth = 0.8f,
        };

        [JsonIgnore]
        public SvgTextSpan RPText1 => new()
        {
            Text = Name,
            X =
            [
                GeoSide is 0 or 4
                    ? SVGCoord.X
                    : GeoSide is 1 or 2 or 3
                        ? new SvgUnit(SVGCoord.X.Value + 4)
                        : new SvgUnit(SVGCoord.X.Value - 4)
            ],
            Dx =
            [
                GeoSide is 0 or 4
                    ? (Name.EndsWith('）') ? 1.2f : 0)
                    : GeoSide is 5 or 6 or 7
                        ? (Name.EndsWith('）') ? 2.4f : 0)
                        : 0
            ],

            FontSize = 4.7f,
            FontFamily = "SimHei"
        };

        [JsonIgnore]
        public SvgTextSpan RPText2 => new()
        {
            Text = EnName,
            X =
            [
                GeoSide is 0 or 4
                    ? SVGCoord.X
                    : GeoSide is 1 or 2 or 3
                        ? new SvgUnit(SVGCoord.X.Value + 4)
                        : new SvgUnit(SVGCoord.X.Value - 4)
            ],
            Dy = [new SvgUnit(4f)],
            FontSize = 3,
            FontFamily = "Arial"
        };

        [JsonIgnore]
        public SvgText RPText
        {
            get
            {
                var text = new SvgText
                {
                    Y =
                    [
                        GeoSide is 2 or 6
                            ? SVGCoord.Y
                            : GeoSide is 0 or 1 or 7
                        ? new SvgUnit(SVGCoord.Y.Value + 8)
                        : new SvgUnit(SVGCoord.Y.Value - 8)
                    ],
                    Fill = new SvgColourServer(System.Drawing.Color.Black),
                    TextAnchor = GeoSide is 0 or 4
                        ? SvgTextAnchor.Middle
                        : GeoSide is 1 or 2 or 3
                            ? SvgTextAnchor.Start
                            : SvgTextAnchor.End
                };

                text.Children.Add(RPText1);
                text.Children.Add(RPText2);

                return text;
            }
        }
    }
}
