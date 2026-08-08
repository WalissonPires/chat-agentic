using System.Security.Claims;

namespace ChatAgentic.Features.Workspaces
{
    public class CurrentUserFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ICurrentUser Create()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirstValue(IntegrationTokenDefaults.WorkspaceIdClaim) ?? throw new InvalidOperationException("WorkspaceId claim not found.");

            return new CurrentUser(user?.Identity?.IsAuthenticated ?? false, int.Parse(claim ?? "0"));
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
