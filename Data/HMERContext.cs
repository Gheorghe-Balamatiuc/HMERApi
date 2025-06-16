using HMERApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HMERApi.Data;

/// <summary>
/// Entity Framework Core database context for the HMER (Handwritten Math Expression Recognition) application
/// </summary>
public class HMERContext(DbContextOptions<HMERContext> options) : DbContext(options)
{
    /// <summary>
    /// DbSet for Products table
    /// </summary>
    public DbSet<Product> Products { get; set; }
}