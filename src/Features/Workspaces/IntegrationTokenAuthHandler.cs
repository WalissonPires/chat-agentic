using System.Security.Claims;
using System.Text.Encodings.Web;
using ChatAgentic.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChatAgentic.Features.Workspaces
{
    public static class IntegrationTokenDefaults
    {
        public const string AuthenticationScheme = "IntegrationToken";
        public const string WorkspaceIdClaim = "WorkspaceId";
        public const string WorkspaceNameClaim = "WorkspaceName";
    }

    public class IntegrationTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AppDbContext _db;

        public IntegrationTokenAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            AppDbContext db)
            : base(options, logger, encoder)
        {
            _db = db;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return AuthenticateResult.NoResult();
            }

            string token;
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader["Bearer ".Length..].Trim();
            }
            else
            {
                token = authHeader.Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Empty token.");
            }

            var workspace = await _db.Workspaces
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IntegrationToken == token);

            if (workspace == null)
            {
                return AuthenticateResult.Fail("Invalid integration token.");
            }

            var claims = new[]
            {
                new Claim(IntegrationTokenDefaults.WorkspaceIdClaim, workspace.Id.ToString()),
                new Claim(IntegrationTokenDefaults.WorkspaceNameClaim, workspace.Name)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }

    public static class ClaimsPrincipalExtensions
    {
        public static int GetWorkspaceId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirstValue(IntegrationTokenDefaults.WorkspaceIdClaim)
                ?? throw new InvalidOperationException("WorkspaceId claim not found.");
            return int.Parse(claim);
        }
    }
}
