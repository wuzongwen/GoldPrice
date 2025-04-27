using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GoldPrice.WebApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<GoldPriceSettings> GoldPriceSettings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<GoldPriceSettings>().HasData(new GoldPriceSettings
            //{
            //    Id = 1,
            //    UpperThreshold = 850m,
            //    LowerThreshold = 700m,
            //    NotifyPath = "http://example.com/notify",
            //    UpdateInterval = 5
            //});
            modelBuilder.Entity<GoldPriceSettings>(model =>
            {
                model.HasKey(e => e.Id);
                model.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }

    public class GoldPriceSettings
    {
        public int Id { get; set; }
        [Range(0, 10000, ErrorMessage = "上限阈值必须在 0 到 10000 之间")]
        public decimal UpperThreshold { get; set; }

        [Range(0, 10000, ErrorMessage = "下限阈值必须在 0 到 10000 之间")]
        public decimal LowerThreshold { get; set; }

        [Required(ErrorMessage = "通知地址是必填项")]
        public string NotifyPath { get; set; }

        [Range(1, 3600, ErrorMessage = "更新间隔必须在 1 到 3600 秒之间")]
        public int UpdateInterval { get; set; }
    }
}
