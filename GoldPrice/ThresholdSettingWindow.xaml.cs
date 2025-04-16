using System.Windows;

namespace GoldPrice
{
    public partial class ThresholdSettingWindow : Window
    {
        public decimal UpperThreshold { get; private set; }
        public decimal LowerThreshold { get; private set; }

        public ThresholdSettingWindow(decimal currentUpper, decimal currentLower)
        {
            InitializeComponent();
            UpperThresholdTextBox.Text = currentUpper.ToString();
            LowerThresholdTextBox.Text = currentLower.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(UpperThresholdTextBox.Text.Trim(), out decimal upper) &&
                decimal.TryParse(LowerThresholdTextBox.Text.Trim(), out decimal lower))
            {
                UpperThreshold = upper;
                LowerThreshold = lower;
                this.DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("请输入有效的数字！", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
