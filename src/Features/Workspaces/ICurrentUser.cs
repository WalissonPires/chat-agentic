namespace ChatAgentic.Features.Workspaces
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        int WorkspaceId { get; }
    }

    public class CurrentUser : ICurrentUser
    {
        private readonly bool _isAuthenticated;
        private readonly int _workspaceId;

        public CurrentUser(bool isAuthenticated, int workspaceId)
        {
            _isAuthenticated = isAuthenticated;
            _workspaceId = workspaceId;
        }

        public bool IsAuthenticated =>_isAuthenticated;
        public int WorkspaceId => _workspaceId;
    }
}
