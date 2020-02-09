using Microsoft.EntityFrameworkCore;

namespace poc_azure_function
{
    using Microsoft.EntityFrameworkCore.Design;
    using Model;
    public class DBContext : DbContext
    {
        private DBContext dBContext;

        public DbSet<Board> Boards { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // TODO: DbContextOptions 初期化ここでも良いはず。。。
        }
    }

    public class DBContextFactory : IDesignTimeDbContextFactory<DBContext>
    {
        DBContext IDesignTimeDbContextFactory<DBContext>.CreateDbContext(string[] args)
        {
            return this.CreateDbContext();
        }

        public DBContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
            optionsBuilder.UseSqlServer("server=localhost,1433;database=test01;uid=sa;pwd=yourStrong(!)Password;");

            return new DBContext(optionsBuilder.Options);
        }

    }
}