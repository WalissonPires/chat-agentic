using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Persistence
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Workspace> Workspaces { get; set; } = default!;
        public DbSet<AgentDefinition> Agents { get; set; } = default!;
        public DbSet<Person> People { get; set; } = default!;
        public DbSet<Contact> Contacts { get; set; } = default!;
        public DbSet<Conversation> Conversations { get; set; } = default!;
        public DbSet<ConversationMessage> ConversationMessages { get; set; } = default!;
        public DbSet<Knowledge> Knowledges { get; set; } = default!;
        public DbSet<AIUsageHistory> AIUsageHistories { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<Enum>().HaveConversion<string>();

            base.ConfigureConventions(configurationBuilder);
        }
    }
}