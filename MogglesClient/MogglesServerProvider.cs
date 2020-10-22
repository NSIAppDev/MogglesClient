﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using MogglesClient.PublicInterface;

namespace MogglesClient
{
    public class MogglesServerProvider: IMogglesFeatureToggleProvider
    {
        private readonly IMogglesLoggingService _featureToggleLoggingService;
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;

        public MogglesServerProvider(IMogglesLoggingService featureToggleLoggingService, IMogglesConfigurationManager mogglesConfigurationManager)
        {
            _featureToggleLoggingService = featureToggleLoggingService;
            _mogglesConfigurationManager = mogglesConfigurationManager;
        }

        public List<FeatureToggle> GetFeatureToggles()
        {
            using (var client = new HttpClient())
            {
                SetRequestHeader(client);

                client.Timeout = _mogglesConfigurationManager.GetTimeoutValue();

                string urlWithParams = GetUrlParams();

                HttpResponseMessage response;
                string featureToggles;

                try
                {
                    response = client.GetAsync(urlWithParams).Result;
                    featureToggles = response.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException ex)
                {
                    _featureToggleLoggingService.TrackException(ex, _mogglesConfigurationManager.GetApplicationName(), _mogglesConfigurationManager.GetEnvironment());
                    throw new MogglesClientException("An error occurred while getting the feature toggles from the server!");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _featureToggleLoggingService.TrackException(new MogglesClientException("An error occurred while getting the feature toggles from the server!"), _mogglesConfigurationManager.GetApplicationName(), _mogglesConfigurationManager.GetEnvironment());
                    throw new MogglesClientException(
                        "An error occurred while getting the feature toggles from the server!");
                }

                return JsonConvert.DeserializeObject<List<FeatureToggle>>(featureToggles);
            }
        }

        private string GetUrlParams()
        {
            var applicationName = _mogglesConfigurationManager.GetApplicationName();
            var environment = _mogglesConfigurationManager.GetEnvironment();

            return $"?applicationName={applicationName}&environment={environment}";
        }

        private void SetRequestHeader(HttpClient client)
        {
            client.BaseAddress = new Uri(_mogglesConfigurationManager.GetTogglesUrl());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = GetSecurityToken();
        }

        private AuthenticationHeaderValue GetSecurityToken() => new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

        private string GenerateJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _mogglesConfigurationManager.GetTokenSigningKey();
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature);

            var  username = HttpContext.Current.User.Identity.Name ?? "";
            var claims = new[] {new Claim("sub", username), new Claim("unique_name", username)};

            var securityToken = new JwtSecurityToken(null, null, claims, new DateTime?(), DateTime.Now.AddMinutes(1.0), credentials);

            return tokenHandler.WriteToken(securityToken);
        }
    }
}
