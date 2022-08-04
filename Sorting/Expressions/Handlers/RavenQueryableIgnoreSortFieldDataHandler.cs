using HotChocolate.Configuration;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Data.Sorting;
using HotChocolate.Data.Sorting.Expressions;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using HotChocolate.Raven.Spatial.Sorting.Convention;

namespace HotChocolate.Raven.Spatial.Sorting.Expressions.Handlers;

public class RavenQueryableIgnoreSortFieldDataHandler : SortFieldHandler<QueryableSortContext, QueryableSortOperation>
{
    public override bool CanHandle(ITypeCompletionContext context, ISortInputTypeDefinition typeDefinition, ISortFieldDefinition fieldDefinition)
    {
        return false;
    }

    public override bool TryHandleEnter(QueryableSortContext context, ISortField field, ObjectFieldNode node, out ISyntaxVisitorAction? action)
    {
        action = SyntaxVisitor.SkipAndLeave;
        return true;
    }

    public override bool TryHandleLeave(QueryableSortContext context, ISortField field, ObjectFieldNode node, out ISyntaxVisitorAction? action)
    {
        action = SyntaxVisitor.Continue;
        return true;
    }
}
