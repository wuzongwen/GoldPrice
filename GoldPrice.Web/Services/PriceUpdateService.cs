using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GoldPrice.Web.Data;
using Microsoft.AspNetCore.WebUtilities;

namespace GoldPrice.Web.Services
{
    public class PriceUpdateService : BackgroundService
    {
        private readonly ILogger<PriceUpdateService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _appDbContext;

        public PriceUpdateService(
            ILogger<PriceUpdateService> logger,
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateAmountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台任务执行时发生错误");
                }

                // 从数据库中获取更新间隔
                int updateInterval = await GetUpdateIntervalAsync();
                await Task.Delay(updateInterval * 1000, stoppingToken);
            }
        }

        private async Task UpdateAmountAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 从数据库中获取最新配置
            var settings = await dbContext.GoldPriceSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return;
            }

            // 检查是否启用通知
            if (!settings.EnableNotification) 
            {
                return;
            }

            decimal upperThreshold = settings.UpperThreshold;
            decimal lowerThreshold = settings.LowerThreshold;
            string notifyPath = settings.NotifyPath;

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Host", "api.jdjygold.com");
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");

            var uri = new Uri("https://api.jdjygold.com/gw/generic/hj/h5/m/latestPrice");
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GoldPriceDto>(responseContent);

            decimal currentPrice = Convert.ToDecimal(data.resultData.datas.price);
            string message = string.Empty;

            if (currentPrice >= upperThreshold)
            {
                message = $"当前价格 {currentPrice} 超过上限 {upperThreshold}";
            }
            else if (currentPrice <= lowerThreshold)
            {
                message = $"当前价格 {currentPrice} 低于下限 {lowerThreshold}";
            }

            if (!string.IsNullOrEmpty(message)&& settings.EnableNotification&& IsNowInTimeRange(settings.NotifyStartTime, settings.NotifyEndTime))
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(settings.Sound)) 
                {
                    queryParams.Add("sound", settings.Sound);
                }
                string url = QueryHelpers.AddQueryString($"{notifyPath}{message}", queryParams);
                // 发送通知
                using var notifyClient = _httpClientFactory.CreateClient();
                await notifyClient.GetAsync(url);
            }
        }

        private async Task<int> GetUpdateIntervalAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var settings = await dbContext.GoldPriceSettings.FirstOrDefaultAsync();
            return settings?.UpdateInterval ?? 10; // 默认间隔为 10 秒
        }

        /// <summary>
        /// 判断当前本地时间是否在指定的时段区间内。
        /// </summary>
        /// <param name="start">时段开始，例如 "08:30"</param>
        /// <param name="end">时段结束，例如 "17:45"</param>
        /// <returns>如果当前时间在区间内（含边界），返回 true；否则 false。</returns>
        bool IsNowInTimeRange(string start, string end)
        {
            // 解析用户输入的 hh:mm 字符串
            if (!TimeSpan.TryParse(start, out var tsStart) ||
                !TimeSpan.TryParse(end, out var tsEnd))
            {
                // 如果解析失败，可以抛异常或按业务需求返回 false
                return false;
            }

            var now = DateTime.Now.TimeOfDay;

            if (tsStart <= tsEnd)
            {
                // 普通区间：例如 08:00–17:00
                return now >= tsStart && now <= tsEnd;
            }
            else
            {
                // 跨午夜区间：例如 20:00–02:00
                return now >= tsStart || now <= tsEnd;
            }
        }


        public class GoldPriceDto
        {
            public ResultData resultData { get; set; }

            public class ResultData
            {
                public Datas datas { get; set; }

                public class Datas
                {
                    public string price { get; set; }
                }
            }
        }
    }
}