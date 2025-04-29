using GoldPrice.Web.Components;
using GoldPrice.Web.Data;
using GoldPrice.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
if (string.IsNullOrWhiteSpace(dbPath))
{
    dbPath = builder.Configuration.GetConnectionString("Sqlite");
}
else 
{
    dbPath = $"Data Source={dbPath}";
}
// 添加 SQLite 数据库支持
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

// 添加后台服务
builder.Services.AddHostedService<PriceUpdateService>();

// 添加 HttpClient 工厂
builder.Services.AddHttpClient();

// 添加AntDesign
builder.Services.AddAntDesign();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// 确保数据库已创建
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
