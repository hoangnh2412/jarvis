// using System.Diagnostics;
// using Microsoft.AspNetCore.Http;
// using Jarvis.OpenTelemetry.Interfaces;
// using Jarvis.OpenTelemetry.SemanticConvention;

// namespace Jarvis.OpenTelemetry.Instrumentations;

// public class UserEnrichHttpResponse : IAspNetCoreEnrichHttpResponse
// {
//     public Task EnrichAsync(Activity activity, HttpResponse httpResponse)
//     {
//         var userName = ClaimsPrincipalExtension.GetClaim(IdentityExtension.GetClaims(httpResponse.HttpContext), AuthConstants.Claims.CognitoUsernameKey)?.Value;
//         if (string.IsNullOrEmpty(userName))
//             return Task.CompletedTask;

//         activity.SetTag(UserAttributes.Id, userName);
//         return Task.CompletedTask;
//     }
// }