using NetTopologySuite.Geometries;

namespace HotChocolate.Raven.Spatial.Filtering.Types;

public class PolygonFilterInputType
    : GeometryFilterInputType<Polygon>
{
}
