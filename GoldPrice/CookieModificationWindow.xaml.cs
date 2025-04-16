using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace GoldPrice
{
    public partial class CookieModificationWindow : Window
    {
        public string CookieValue { get; private set; }

        public CookieModificationWindow(string existingCookie = "")
        {
            InitializeComponent();
            CookieTextBox.Text = existingCookie;
            this.Icon = LoadIcon();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CookieValue = CookieTextBox.Text.Trim();
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private BitmapImage LoadIcon()
        {
            using (MemoryStream ms = new MemoryStream(GoldPriceRes.logo))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
    }
}
