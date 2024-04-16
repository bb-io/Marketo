using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.Marketo;

public class MarketoApplication : IApplication, ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.Cms, ApplicationCategory.Marketing];
        set { }
    }
    
    public string Name
    {
        get => "Marketo";
        set { }
    }
    
    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}