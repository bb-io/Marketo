using RestSharp;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Marketo
{
    [ActionList]
    public class Actions
    {
        [Action]
        public FileResponse fetchFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, [ActionParameter] string id)
        {
            var client = new MarketoClient(authenticationCredentialsProviders: authenticationCredentialsProviders);
            var clientId = authenticationCredentialsProviders.First(v => v.KeyName == "clientId");
            var clientSecret = authenticationCredentialsProviders.First(v => v.KeyName == "clientSecret");

            var authRequest = new RestRequest("/identity/oauth/token", Method.Get);
            authRequest.AddQueryParameter("grant_type", "client_credentials");
            authRequest.AddQueryParameter("client_id", $"{clientId}");
            authRequest.AddQueryParameter("client_secret", $"{clientSecret}");

            var authResponse = client.Execute<AuthResponse>(authRequest);
            if (authResponse.Data == null) throw new Exception("auth response was null");
            var accessToken = authResponse.Data.AccessToken;

            var request = new RestRequest($"/rest/asset/v1/file/{id}.json", Method.Get);
            request.AddQueryParameter("access_token", accessToken);

            var response = client.Execute<FetchFileResponse>(request);
            if (response.Data == null) throw new Exception("response data was null");

            return response.Data.Result[0];
        }
    }
}