using AppOwnsData.Services;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbiembeddedtoken
{
    public static class Token
    {
        private static readonly string urlPowerBiServiceApiRoot = ConfigurationManager.AppSettings["urlPowerBiServiceApiRoot"];
        public static readonly Guid ReportId = GetParamGuid(ConfigurationManager.AppSettings["reportId"]);
        public static readonly Guid WorkspaceId = GetParamGuid(ConfigurationManager.AppSettings["workspaceId"]);

        private static readonly string m_authorityUrl = ConfigurationManager.AppSettings["authorityUrl"];
        private static readonly string[] m_scope = ConfigurationManager.AppSettings["scope"].Split(';');

        private static Guid GetParamGuid(string param)
        {
            Guid paramGuid = Guid.Empty;
            Guid.TryParse(param, out paramGuid);
            return paramGuid;
        }

        public static EmbedToken GiveMeToken()
        {
            return GetToken().Result;
        }

        public static async Task<EmbedToken> GetToken()

        {
            // For app only authentication, we need the specific tenant id in the authority url
            var tenantSpecificURL = m_authorityUrl.Replace("organizations", ConfigValidatorService.Tenant);

            IConfidentialClientApplication clientApp = ConfidentialClientApplicationBuilder
                                                                            .Create(ConfigValidatorService.ApplicationId)
                                                                            .WithClientSecret(ConfigValidatorService.ApplicationSecret)
                                                                            .WithAuthority(tenantSpecificURL)
                                                                            .Build();

            var authenticationResult = await clientApp.AcquireTokenForClient(m_scope).ExecuteAsync();

            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");
            using (var pbiClient = new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials))
            {
                var pbiReport = pbiClient.Reports.GetReportInGroup(WorkspaceId, ReportId);
                // Create list of dataset
                var datasetIds = new List<Guid>();
                int lifetimeInMinutes ;
                // Add dataset associated to the report
                datasetIds.Add(Guid.Parse(pbiReport.DatasetId));
                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(ReportId) },

                datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

                targetWorkspaces: new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(WorkspaceId) }

                    , lifetimeInMinutes: 3600
                );

                // Generate Embed token
                var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

                return embedToken;
            }



        }
    }
}
