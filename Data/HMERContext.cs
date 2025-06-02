using HMERApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HMERApi.Data;

public class HMERContext(DbContextOptions<HMERContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}