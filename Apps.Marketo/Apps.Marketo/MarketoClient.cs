using System;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Marketo
{
	public class MarketoClient : RestClient
	{
		public MarketoClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
		: base(
			  new RestClientOptions() { ThrowOnAnyError = true, BaseUrl = GetUri(authenticationCredentialsProviders) }
			  ) { }

        private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider)
        {
            var url = authenticationCredentialsProvider.First(v => v.KeyName == "URL").Value;
            return new Uri($"https://{url}.mktorest.com");
        }
    }
}
