using System.Linq.Expressions;
using System.Reflection;
using Raven.Client.Documents.Queries.Spatial;

namespace HotChocolate.Raven.Spatial.Filtering.Expressions;

internal static class RavenExpressionBuilder
{
    public static Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> Contains(string text, double? buffer)
    {
        return CreateMethod(_contains, text, buffer);
    }

    public static Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> Disjoint(string text, double? buffer)
    {
        return CreateMethod(_disjoint, text, buffer);
    }

    public static Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> Intersects(string text, double? buffer)
    {
        return CreateMethod(_intersects, text, buffer);
    }

    public static Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> Within(string text, double? buffer)
    {
        return CreateMethod(_within, text, buffer);
    }

    private static readonly MethodInfo _contains = typeof(SpatialCriteriaFactory).GetMethods()
                                                                                 .First(
                                                                                      z => z.Name == nameof(SpatialCriteriaFactory.Contains)
                                                                                        && z.GetParameters().Length == 2
                                                                                  );

    private static readonly MethodInfo _disjoint = typeof(SpatialCriteriaFactory).GetMethods()
                                                                                 .First(
                                                                                      z => z.Name == nameof(SpatialCriteriaFactory.Disjoint)
                                                                                        && z.GetParameters().Length == 2
                                                                                  );


    private static readonly MethodInfo _intersects = typeof(SpatialCriteriaFactory).GetMethods()
                                                                                   .First(
                                                                                        z => z.Name == nameof(SpatialCriteriaFactory.Intersects)
                                                                                          && z.GetParameters().Length == 2
                                                                                    );

    private static readonly MethodInfo _within = typeof(SpatialCriteriaFactory).GetMethods()
                                                                               .First(
                                                                                    z => z.Name == nameof(SpatialCriteriaFactory.Within)
                                                                                      && z.GetParameters().Length == 2
                                                                                );

    private static Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> CreateMethod(MethodInfo method, string text, double? buffer)
    {
        var parameter = Expression.Parameter(typeof(SpatialCriteriaFactory), "factory");
        var factoryMethod = buffer.HasValue
            ? Expression.Call(parameter, method, Expression.Constant(text), Expression.Constant(buffer.Value))
            : Expression.Call(parameter, method, Expression.Constant(text), Expression.Constant(0.025));

        return Expression.Lambda<Func<SpatialCriteriaFactory, SpatialCriteria>>(factoryMethod, parameter);
    }
}
