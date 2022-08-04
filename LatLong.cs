using NetTopologySuite.Geometries;

namespace HotChocolate.Raven.Spatial;

public record LatLong(double Latitude, double Longitude)
{
    public static implicit operator Point(LatLong latLong)
    {
        return new Point(latLong.Longitude, latLong.Latitude);
    }

    public static implicit operator LatLong(Point latLong)
    {
        return FromPoint(latLong);
    }

    public static LatLong FromPoint(Point point)
    {
        return new(point.Y, point.X);
    }

    public Point ToPoint()
    {
        return new Point(Latitude, Longitude);
    }
}
