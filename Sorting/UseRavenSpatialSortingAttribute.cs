using System.Reflection;
using System.Runtime.CompilerServices;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace HotChocolate.Raven.Spatial.Sorting;

public class UseRavenSpatialSortingAttribute : ObjectFieldDescriptorAttribute
{
    public UseRavenSpatialSortingAttribute([CallerLineNumber] int order = 0)
    {
        Order = order;
    }

    public override void OnConfigure(
        IDescriptorContext context,
        IObjectFieldDescriptor descriptor, MemberInfo member
    )
    {
        descriptor.UseRavenSpatialSorting();
    }
}
