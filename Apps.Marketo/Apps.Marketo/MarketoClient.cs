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
            var munchkin = authenticationCredentialsProvider.First(v => v.KeyName == "munchkin").Value;
            return new Uri($"https://{munchkin}.mktorest.com");
        }
    }
}
