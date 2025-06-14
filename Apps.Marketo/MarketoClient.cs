﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models;
using Apps.Marketo.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.Json;

namespace Apps.Marketo;

public class MarketoClient : RestClient
{
    public MarketoClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, bool throwOnAnyError)
        : base(
            new RestClientOptions { ThrowOnAnyError = throwOnAnyError, BaseUrl = GetUri(authenticationCredentialsProviders) },
            configureSerialization: s => s.UseSystemTextJson(new(JsonSerializerDefaults.Web)
                { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
        )
    {
    }

    public MarketoClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        : base(
            new RestClientOptions { ThrowOnAnyError = true, BaseUrl = GetUri(authenticationCredentialsProviders) },
            configureSerialization: s => s.UseSystemTextJson(new(JsonSerializerDefaults.Web)
            { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
        )
    {
    }

    private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider)
    {
        var url = authenticationCredentialsProvider.First(v => v.KeyName == "Munchkin Account ID").Value;
        return new($"https://{url}.mktorest.com");
    }

    public  T GetSingleEntity<T>(RestRequest request)
    {
        return ErrorHandler.ExecuteWithErrorHandling<T>(() =>
        {
            var result = ExecuteWithError<T>(request);
        if (result.Result == null || !result.Result.Any())
        {
            if (result.Warnings.Any())
            {
                throw new Exception(result.Warnings.First());
            }
            throw new Exception("No single result found");
        }
        return result.Result!.First();
        });
    }
   
    
    public BaseResponseDto<T> ExecuteWithError<T>(RestRequest request)
    {
        var response = ExecuteWithErrorHandling(request);
        return JsonConvert.DeserializeObject<BaseResponseDto<T>>(response.Content)!;
    }

    public List<T> Paginate<T>(RestRequest request)
    {
        var offset = 0;
        var limit = 200;

        var baseUrl = request.Resource.SetQueryParameter("maxReturn", limit.ToString());

        var result = new List<T>();
        BaseResponseDto<T> response;
        do
        {
            request.Resource = baseUrl.SetQueryParameter("offset", offset.ToString());

            response = ExecuteWithError<T>(request);
            result.AddRange(response.Result ?? Enumerable.Empty<T>());

            offset += limit;
        } while (response.Result?.Any() is true);

        return result;
    }

    public RestResponse ExecuteWithErrorHandling(RestRequest request)
    {
        var response = this.Execute(request);
        var errors = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);

        if (errors.Errors.Any())
        {
            if(errors.Errors.Count() == 1 && errors.Errors.First().Code == "709")
            {
                throw new BusinessRuleViolationException(709, errors.Errors.First().Message);
            }
            if(errors.Errors.Count() == 1 && errors.Errors.First().Code == "611")
            {
                throw new BusinessRuleViolationException(611, errors.Errors.First().Message);
            }
            if (errors.Errors.Count() == 1 && errors.Errors.First().Code == "702")
            {
                throw new BusinessRuleViolationException(702, errors.Errors.First().Message);
            }

            if (errors.Errors.Any(error => error.Code == "606"))
            {
                Thread.Sleep(TimeSpan.FromSeconds(20));
                var retryResponse = this.Execute(request);
                var retryErrors =
                    JsonConvert.DeserializeObject<ErrorResponse>(retryResponse.Content);

                if (retryErrors.Errors.Any())
                    throw new PluginMisconfigurationException(errors.Errors.First().Message);

                return retryResponse;
            }

            throw new PluginMisconfigurationException($"Error message: {errors.Errors.First().Message}, Error code: {errors.Errors.First().Code}");
        }

        return response;
    }
}