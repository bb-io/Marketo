using System.Text.Json;
using Apps.Marketo.Dtos;
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
            var url = authenticationCredentialsProvider.First(v => v.KeyName == "Munchkin Account ID").Value;
            return new Uri($"https://{url}.mktorest.com");
        }

        public BaseResponseDto<T> ExecuteWithError<T>(MarketoRequest request)
        {
            var res = this.Execute(request).Content;
            var deserialized = JsonSerializer.Deserialize<BaseResponseDto<T>>(res, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true});
            if (deserialized.Errors.Any())
            {
                throw new ArgumentException(deserialized.Errors.First().Message);
            }
            return deserialized;
        }
    }
}
