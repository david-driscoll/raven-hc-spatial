using NetTopologySuite.Geometries;

namespace HotChocolate.Raven.Spatial.Filtering.Types;

public class MultiPolygonFilterInputType
    : GeometryFilterInputType<MultiPolygon>
{
}
