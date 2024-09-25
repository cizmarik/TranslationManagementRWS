using Microsoft.EntityFrameworkCore;
using TranslationManagement.Api.Controlers;
using TranslationManagement.Api.Controllers;

namespace TranslationManagement.Api.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TranslationJob> TranslationJobs { get; set; }
        public DbSet<TranslatorModel> Translators { get; set; }
    }
}