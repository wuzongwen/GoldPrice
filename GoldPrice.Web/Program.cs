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
// ��� SQLite ���ݿ�֧��
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

// ��Ӻ�̨����
builder.Services.AddHostedService<PriceUpdateService>();

// ��� HttpClient ����
builder.Services.AddHttpClient();

// ���AntDesign
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

// ȷ�����ݿ��Ѵ���
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
