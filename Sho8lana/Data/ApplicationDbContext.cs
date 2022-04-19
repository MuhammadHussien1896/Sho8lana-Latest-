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
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


    }
}