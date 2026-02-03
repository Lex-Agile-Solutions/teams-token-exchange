using System.Security.Claims;
using System.Text.Json;
using Azure;
using Azure.Communication.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ExchangeTeamsToken;

public class ExchangeTeamsToken(ILogger<ExchangeTeamsToken> logger, CommunicationIdentityClient identityClient)
{
    [Function("ExchangeTeamsToken")]
    [Authorize]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest request,
        FunctionContext context
    )
    {
        var principal = context.Features.Get<ClaimsPrincipal>();
        var callerTenantId = principal?.FindFirst("tid")?.Value;
        var callerUserId = principal?.FindFirst("oid")?.Value;

        logger.LogInformation(
            "Exchanging token for Teams Token. Tenant ID: {CallerTenantId}, User ID: {CallerUserId}",
            callerTenantId,
            callerUserId
        );

        try
        {
            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                logger.LogError("Request body is empty.");
                return new BadRequestObjectResult("Request body is empty.");
            }

            var tokenRequest = JsonSerializer.Deserialize<TeamsTokenRequest>(requestBody);

            if (string.IsNullOrEmpty(tokenRequest?.ClientId) ||
                string.IsNullOrEmpty(tokenRequest.TeamsAccessToken) ||
                string.IsNullOrEmpty(tokenRequest.UserObjectId))
            {
                logger.LogError("ClientId, TeamsAccessToken, and UserObjectId are required.");
                return new BadRequestObjectResult("ClientId, TeamsAccessToken, and UserObjectId are required.");
            }

            var options = new GetTokenForTeamsUserOptions(
                tokenRequest.TeamsAccessToken,
                tokenRequest.ClientId,
                tokenRequest.UserObjectId
            );

            var accessToken = await identityClient.GetTokenForTeamsUserAsync(options);

            return new OkObjectResult(new TeamsTokenResponse
            {
                Token = accessToken.Value.Token,
                ExpiresAt = accessToken.Value.ExpiresOn.ToString("o"),
            });
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Request body is not valid JSON.");
            return new BadRequestObjectResult("Request body is not valid JSON.");
        }
        catch (RequestFailedException e)
        {
            logger.LogError(e, "Azure Communication Services error: {Message}", e.Message);
            return e.Status switch
            {
                401 => new UnauthorizedObjectResult("Invalid Teams access token."),
                403 => new ObjectResult("Forbidden: Unable to exchange token.") { StatusCode = 403 },
                _ => new BadRequestObjectResult("Unexpected error occurred.") { StatusCode = 500 },
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error occurred.");
            return new BadRequestObjectResult(e.Message);
        }
    }
}