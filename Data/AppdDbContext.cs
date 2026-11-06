using AsyncProductApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AsyncProductApi.Data;

public class AppDbContext :DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<ListingRequest> ListingRequests  => Set<ListingRequest>();
}