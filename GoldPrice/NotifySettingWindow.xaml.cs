using System.Text.RegularExpressions;
using System.Windows;
namespace GoldPrice
{
    public partial class NotifySettingWindow : Window
    {
        public string NotifyPath { get; private set; }

        public NotifySettingWindow(string NotifyPath)
        {
            InitializeComponent();
            NotifyPathTextBox.Text = NotifyPath;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsHttpUrl(NotifyPathTextBox.Text))
            {
                NotifyPath = NotifyPathTextBox.Text;
                this.DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("请输入有效的通知地址！", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        public static bool IsHttpUrl(string url)
        {
            // 正则表达式参考了多个来源的常见URL结构规则:ml-citation{ref="1,2" data="citationList"}
            string pattern = @"^(http://|https://)(www\.)?([\w-]+\.)+[\w-]+(:\d+)?(/[\w-./?%&=]*)?$";
            return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
        }
    }
}
