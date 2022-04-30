using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sho8lana.Models;

namespace Sho8lana.Data
{
    public class ApplicationDbContext : IdentityDbContext<Customer>
    {
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<CustomerRequest> CustomerRequests { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Media> Medias { get; set; }
        public virtual DbSet<ServiceMessage> ServiceMessages { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Governorate> Governorates { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<OnlineUser> OnlineUsers { get; set; }
        public virtual DbSet<Payments> Payments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Customer>().Ignore(i => i.PhoneNumberConfirmed);
            builder.Entity<OnlineUser>().HasKey(k => new {k.UserId,k.ConnectionId});
        }

    }
}