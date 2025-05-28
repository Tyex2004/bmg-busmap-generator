# 建模说明

## Node（道路节点）

### JSON 字段

| 字段名 | 类型 | 说明 |
| --- | --- | --- |
| `Id` | `int` | 唯一标识 |
| `Name` | `string` | 名称 |
| `JSONCoord` | `decimal[]` | JSON 坐标 |

### 其它属性

| 属性名 | 类型 | 说明 |
| --- | --- | --- |
| `SVGCoord` | `SvgPoint` | SVG 坐标 |
| `SkiaCoord` | `SKPoint` | Skia 坐标 |
| `WPFCoord` | `Point` | WPF 坐标 |
| `RoadsId` | `int[]` | 连接道路 Id 数组 |
| `RPGraph` | `SvgCircle` | 在道路预览图中的几何 |

## Road（道路）

### JSON 字段

| 字段名 | 类型 | 说明 |
| --- | --- | --- |
| `Id` | `int` | 唯一标识 |
| `Name` | `string` | 名称 |
| `NodesId` | `int[]` | 两端道路节点 Id |

## 其它属性

| 属性名 | 类型 | 说明 |
| --- | --- | --- |
| `Nodes` | `Node[]` | 两端道路节点数组 |
| `JSONCoordStart` | `decimal[]` | JSON 起点坐标 |
| `JSONCoordEnd` | `decimal[]` | JSON 终点坐标 |
| `SVGCoordStart` | `SvgPoint` | SVG 起点坐标 |
| `SVGCoordEnd` | `SvgPoint` | SVG 终点坐标 |
| `SkiaCoordStart` | `SKPoint` | Skia 起点坐标 |
| `SkiaCoordEnd` | `SKPoint` | Skia 终点坐标 |
| `WPFCoordStart` | `Point` | WPF 起点坐标 |
| `WPFCoordEnd` | `Point` | WPF 终点坐标 |
| `Direction` | `int` | 方向（0北顺时针）|
| `RPGraph` | `SvgLine` | 在道路预览图中的几何 |

## Station（站点）

### JSON 字段

| 字段名 | 类型 | 说明 |
| --- | --- | --- |
| `Id` | `int` | 唯一标识 |
| `Name` | `string` | 名称 |
| `EnName` | `string` | 英文名称 |
| `RoadId` | `int` | 所属道路 ID |
| `OnRoadPos` | `double` | 道路上位置 |
| `Side` | `string` | 站名标注方向 |
| `ConnectsMtr` | `string[]` | 连接地铁站名数组 |
| `Note` | `string[]` | 备注 |

### 其它属性

| 属性名 | 类型 | 说明 |
| --- | --- | --- |
| `Road` | `Road` | 所属道路 |
| `JSONCoord` | `decimal[]` | JSON 坐标 |
| `SVGCoord` | `SvgPoint` | SVG 坐标 |
| `SkiaCoord` | `SKPoint` | Skia 坐标 |
| `WPFCoord` | `Point` | WPF 坐标 |
| `GeoSide` | `GeoSide` | 站名几何标注方向 |
| `RPGraph` | `SvgCircle` | 在道路预览图中的几何 |
| `RPText1` | `SvgTextSpan` | 中文站名文本对象 |
| `RPText2` | `SvgTextSpan` | 英文站名文本对象 |
| `RPText` | `SvgText` | 站名标注文本对象 |