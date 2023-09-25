using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo
{
    public class MarketoApplication : IApplication
    {
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
}

	


