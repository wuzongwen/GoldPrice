using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using System.IO;
using GoldPrice.Model;
using System.Globalization;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Microsoft.Playwright;

namespace GoldPrice
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _updateTimer;
        private NotifyIcon _notifyIcon;

        // 阈值字段
        private decimal _upperThreshold; // 上限阈值
        private decimal _lowerThreshold; // 下限阈值
        private string _notifyPath;//通知地址

        // 配置文件路径
        private readonly string iniFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

        private int _updateInterval = 10; // 10秒

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();

            // 加载配置（如果 ini 文件不存在，则使用默认值）
            LoadThresholdFromIni();

            // 初始化定时器
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_updateInterval)
            };
            _updateTimer.Tick += async (s, e) => await UpdateAmount();
            _updateTimer.Start();

            Loaded += async (s, e) => await UpdateAmount();
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = Icon(),
                Visible = true,
                Text = $"实时金价 - 更新间隔：{_updateInterval}秒"
            };

            // 创建右键菜单
            var contextMenu = new ContextMenuStrip();

            // 添加“切换间隔”子菜单项
            var intervalMenuItem = new ToolStripMenuItem("更新间隔");
            // 添加具体选项：
            intervalMenuItem.DropDownItems.Add(CreateIntervalMenuItem("5秒", 5));
            ToolStripMenuItem defaultItem = CreateIntervalMenuItem("10秒", 10);
            defaultItem.Checked = true;
            intervalMenuItem.DropDownItems.Add(defaultItem);
            intervalMenuItem.DropDownItems.Add(CreateIntervalMenuItem("30秒", 30));
            intervalMenuItem.DropDownItems.Add(CreateIntervalMenuItem("60秒", 30));
            contextMenu.Items.Add(intervalMenuItem);

            // 添加设置阈值菜单项
            var thresholdMenuItem = new ToolStripMenuItem("设置阈值");
            thresholdMenuItem.Click += OnSetThresholdClick;
            contextMenu.Items.Add(thresholdMenuItem);

            // 添加设置通知地址菜单项
            var notifyPathMenuItem = new ToolStripMenuItem("设置通知地址");
            notifyPathMenuItem.Click += OnSetNofityPathClick;
            contextMenu.Items.Add(notifyPathMenuItem);

            // 添加“修改 Cookie”菜单项
            //var cookieMenuItem = new ToolStripMenuItem("修改 Cookie");
            //cookieMenuItem.Click += OnModifyCookieClick;
            //contextMenu.Items.Add(cookieMenuItem);

            contextMenu.Items.Add("退出", null, OnExitClick);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private async Task UpdateAmount()
        {
            try
            {
                bool needNotify = false;
                string message = string.Empty;
                // 可以通过此 handler 创建 HttpClient 实例
                using (HttpClient client = new HttpClient())
                {
                    // 添加自定义请求头
                    client.DefaultRequestHeaders.Add("Host", "api.jdjygold.com");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh-Hans;q=0.9");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.Add("Origin", "https://m.jdjygold.com");
                    client.DefaultRequestHeaders.Add("Sec", "same");
                    client.DefaultRequestHeaders.Add("Referer", "https://m.jdjygold.com/");
                    //client.DefaultRequestHeaders.Add("Cookie", cookie);

                    var uri = new Uri("https://api.jdjygold.com/gw/generic/hj/h5/m/latestPrice");
                    // 发送 GET 请求
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(uri);
                        response.EnsureSuccessStatusCode(); // 确保响应状态为 200 OK，否则抛出异常

                        string responseContent = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<GoldPriceDto>(responseContent);

                        Dispatcher.Invoke(() =>
                        {
                            AmountText.Text = ConvertToAmountString(data.resultData.datas.price);
                            var upAndDownAmt = data.resultData.datas.upAndDownAmt;
                            if (data.resultData.datas.upAndDownAmt.StartsWith("+"))
                            {
                                SubAmountText.Foreground = System.Windows.Media.Brushes.Red;
                            }
                            else if (data.resultData.datas.upAndDownAmt.StartsWith("-"))
                            {
                                SubAmountText.Foreground = System.Windows.Media.Brushes.Green;
                            }

                            if (Convert.ToDecimal(data.resultData.datas.price) > Convert.ToDecimal(data.resultData.datas.yesterdayPrice))
                            {
                                AmountText.Foreground = System.Windows.Media.Brushes.Red;
                            }
                            else
                            {
                                AmountText.Foreground = System.Windows.Media.Brushes.Green;
                            }
                            SubAmountText.Text = upAndDownAmt;

                            // 检查阈值
                            if (Convert.ToDecimal(data.resultData.datas.price) >= _upperThreshold)
                            {
                                needNotify = true;
                                message = $"当前价格 {data.resultData.datas.price} 超过上限 {_upperThreshold}";
                            }
                            else if (Convert.ToDecimal(data.resultData.datas.price) <= _lowerThreshold)
                            {
                                needNotify = true;
                                message = $"当前价格 {data.resultData.datas.price} 低于下限 {_lowerThreshold}";
                            }
                        });
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine("请求出错：" + ex.Message);
                    }
                }

                if (needNotify)
                {
                    _notifyIcon.ShowBalloonTip(3000, "价格警告", message, ToolTipIcon.Warning);

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(2);
                        await client.GetAsync($"{_notifyPath}{message}");
                    }
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    AmountText.Text = "Error";
                    SubAmountText.Text = "";
                    AmountText.Foreground = System.Windows.Media.Brushes.Red;
                    SubAmountText.Foreground = System.Windows.Media.Brushes.Green;
                });
            }
        }

        // 窗口拖动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // 退出处理
        private void OnExitClick(object sender, EventArgs e)
        {
            _notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        private ToolStripMenuItem CreateIntervalMenuItem(string text, int seconds)
        {
            // 创建一个可勾选的菜单项
            var menuItem = new ToolStripMenuItem(text)
            {
                CheckOnClick = false // 不使用自动切换，需要手动控制
            };
            menuItem.Click += (s, e) =>
            {
                // 获取父下拉菜单
                if (menuItem.Owner is ToolStripDropDown parent)
                {
                    // 遍历所有子项，将它们取消选中
                    foreach (ToolStripItem item in parent.Items)
                    {
                        if (item is ToolStripMenuItem mi)
                        {
                            mi.Checked = false;
                        }
                    }
                }
                // 设置当前点击项为选中状态
                menuItem.Checked = true;
                // 更新定时器间隔
                _updateInterval = seconds;
                _updateTimer.Interval = TimeSpan.FromSeconds(_updateInterval);
                // 更新托盘图标提示文本（可选）
                _notifyIcon.Text = $"实时金价 - 更新间隔：{_updateInterval}秒";
            };

            return menuItem;
        }

        /// <summary>
        /// 当点击“修改 Cookie”菜单项时调用，此方法弹出输入框让用户输入新的 Cookie 字符串。
        /// </summary>
        private void OnModifyCookieClick(object sender, EventArgs e)
        {
            // 因为托盘菜单事件可能不在UI线程中，所以使用Dispatcher调用UI线程
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 使用新建的美化对话框显示 Cookie 修改窗口，设置 Owner 保证居中显示
                var dialog = new CookieModificationWindow("");
                dialog.Owner = this;
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    var input = dialog.CookieValue;
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        if (input.Length > 0)
                        {
                            string value = input;

                            // 可选：更新托盘提示
                            _notifyIcon.ShowBalloonTip(3000, "修改 Cookie", $"新的 Cookie 已更新：{input}", ToolTipIcon.Info);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("格式不正确");
                        }
                    }
                }
            });
        }

        // 防止直接关闭窗口
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // 阻止直接关闭
            this.Hide();    // 最小化到托盘
            base.OnClosing(e);
        }

        // API响应模型
        public class AmountData
        {
            public decimal MainAmount { get; set; }
            public decimal SubAmount { get; set; }
        }

        static Icon Icon()
        {
            try
            {
                // 使用MemoryStream和Icon类创建图标对象
                using (MemoryStream memoryStream = new MemoryStream(GoldPriceRes.logo))
                {
                    return new Icon(memoryStream);
                }
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("图标数据无效：" + ex.Message);
            }
        }

        private void LoadThresholdFromIni()
        {
            if (!File.Exists(iniFilePath))
            {
                _upperThreshold = 800;
                _lowerThreshold = 700;
                _notifyPath = "https://api.day.app/token/";
                SaveThresholdToIni();
            }
            else
            {
                string upper = IniFile.ReadValue(iniFilePath, "Notify", "GPUpper", "800.00");
                string lower = IniFile.ReadValue(iniFilePath, "Notify", "GPLower", "700.00");
                _notifyPath = IniFile.ReadValue(iniFilePath, "Notify", "NotifyPath", "https://api.day.app/token/");
                if (!decimal.TryParse(upper, out _upperThreshold))
                    _upperThreshold = 800M;
                if (!decimal.TryParse(lower, out _lowerThreshold))
                    _lowerThreshold = 700M;
            }
        }

        private void SaveThresholdToIni()
        {
            IniFile.WriteValue(iniFilePath, "Notify", "GPUpper", _upperThreshold.ToString());
            IniFile.WriteValue(iniFilePath, "Notify", "GPLower", _lowerThreshold.ToString());
            IniFile.WriteValue(iniFilePath, "Notify", "NotifyPath", _notifyPath);
        }

        private void OnSetThresholdClick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ThresholdSettingWindow(_upperThreshold, _lowerThreshold);
                dialog.Owner = this;
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    _upperThreshold = dialog.UpperThreshold;
                    _lowerThreshold = dialog.LowerThreshold;
                    SaveThresholdToIni();
                    _notifyIcon.ShowBalloonTip(1500, "设置金价通知值", $"已更新值：上限 {_upperThreshold}，下限 {_lowerThreshold}", ToolTipIcon.Info);
                }
            });
        }

        private void OnSetNofityPathClick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new NotifySettingWindow(_notifyPath);
                dialog.Owner = this;
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    _notifyPath = dialog.NotifyPath;
                    SaveThresholdToIni();
                    _notifyIcon.ShowBalloonTip(1500, "设置通知地址", $"已更新通知地址：{_notifyPath}", ToolTipIcon.Info);
                }
            });
        }

        private static string ConvertToAmountString(string input)
        {
            // 尝试解析输入字符串为decimal，支持货币符号和千位分隔符
            if (decimal.TryParse(input, NumberStyles.Currency | NumberStyles.Number, CultureInfo.CurrentCulture, out decimal result))
            {
                // 格式化为两位小数，使用不变文化确保小数点格式为"."
                return result.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// 获取国际金价
        /// </summary>
        /// <returns></returns>
        static async Task<(string priceText, string changeAmt,string changePct)> GetXAUUSD() 
        {
            try
            {
                using var pw = await Playwright.CreateAsync();
                await using var browser =
                    await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    { Headless = true });
                var page = await browser.NewPageAsync();
                await page.GotoAsync(
                  "https://www.tradingview.com/symbols/XAUUSD/?exchange=OANDA"
                );

                // 自动等待并获取文本
                var priceText = await page.TextContentAsync(
                    "div.js-symbol-header-ticker span.js-symbol-last"
                );
                var changeAmt = await page.TextContentAsync(
                    "div.js-symbol-header-ticker div.js-symbol-change-direction span:nth-child(1)"
                );
                var changePct = await page.TextContentAsync(
                    "div.js-symbol-header-ticker div.js-symbol-change-direction span:nth-child(2)"
                );

                await browser.CloseAsync();

                return (priceText, changeAmt, changePct);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}