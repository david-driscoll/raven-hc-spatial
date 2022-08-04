using System.Linq.Expressions;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using Raven.Client.Documents.Queries.Spatial;

namespace HotChocolate.Raven.Spatial.Filtering;

public class RavenQueryableFilterContext : QueryableFilterContext
{
    public RavenQueryableFilterContext(IFilterInputType initialType, bool inMemory) : base(initialType, inMemory)
    {
    }

    public Expression<Func<SpatialCriteriaFactory, SpatialCriteria>>? SpatialExpression { get; set; }
    public DynamicSpatialField? SpatialFieldName { get; set; }
    public string? FieldName { get; set; }
    public ObjectFieldNode? NodeToRemove { get; set; }
}

public class RavenFilterVisitor : FilterVisitor<QueryableFilterContext, Expression>
{
    public RavenFilterVisitor(FilterOperationCombinator<QueryableFilterContext, Expression> combinator) : base(combinator)
    {
    }

    protected override ISyntaxVisitorAction Leave(ObjectValueNode node, QueryableFilterContext context)
    {
        if (node.Fields.Count == 0)
        {
            return Continue;
        }

        var result = base.Leave(node, context);

        if (context is RavenQueryableFilterContext { NodeToRemove: { } nodeToRemove } ravenQueryableFilterContext)
        {
            if (node.Fields is not List<ObjectFieldNode> list)
            {
                throw new Exception("Unable to mutate field node list");
            }

            list.Remove(nodeToRemove);
            ravenQueryableFilterContext.NodeToRemove = null;
        }


        if (context is RavenQueryableFilterContext filterContext)
        {
            if (node.Fields is not List<ObjectFieldNode> list)
            {
                throw new Exception("Unable to mutate field node list");
            }

            foreach (var item in node.Fields.ToArray())
            {
                if (item.Value is ObjectValueNode { Fields.Count: 0 } or ListValueNode { Items.Count: 0 })
                {
                    list.Remove(item);
                }
            }
        }

        return result;
    }

    protected override ISyntaxVisitorAction Leave(ListValueNode node, QueryableFilterContext context)
    {
        if (node.Items is not List<IValueNode> list)
        {
            throw new Exception("Unable to mutate field node list");
        }

        var result = base.Leave(node, context);
        foreach (var item in node.Items
                                 .OfType<ObjectValueNode>()
                                 .Where(z => z.Fields.Count == 0)
                                 .ToArray()
                )
        {
            list.Remove(item);
        }

        return result;
    }
}
