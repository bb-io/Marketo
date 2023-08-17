using System;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;

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

	


