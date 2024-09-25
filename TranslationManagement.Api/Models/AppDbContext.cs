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

        public DbSet<ITranslationJob> TranslationJobs { get; set; }
        public DbSet<ITranslatorModel> Translators { get; set; }
    }
}