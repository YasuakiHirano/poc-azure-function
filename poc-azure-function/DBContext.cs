using Microsoft.EntityFrameworkCore;

namespace poc_azure_function
{
    using Microsoft.EntityFrameworkCore.Design;
    using Model;
    public class DBContext : DbContext
    {
        public DbSet<Board> Boards { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }
    }

    public class DBContextFactory : IDesignTimeDbContextFactory<DBContext>
    {
        DBContext IDesignTimeDbContextFactory<DBContext>.CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
            optionsBuilder.UseSqlServer("server=localhost,1433;database=test01;uid=sa;pwd=yourStrong(!)Password;");

            return new DBContext(optionsBuilder.Options);
        }
    }
}