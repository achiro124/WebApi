namespace UsersWebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            :base(options)    
        {
            
        }
        public DbSet<User> Users { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Login = "Admin",
                    Password="123",
                    Name="Admin",
                    Gender=1,
                    Birthday =new DateTime(2001,08,09),
                    Admin= true,
                    CreatedOn= DateTime.Now
                });
        }
    }
}
