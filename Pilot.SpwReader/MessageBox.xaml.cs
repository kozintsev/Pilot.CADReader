using System.Diagnostics;
using System.Windows;

namespace Ascon.Pilot.SDK.SpwReader
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
            var strVersion = System.Reflection.Assembly.GetExecutingAssembly()
                .GetName().Version.ToString();
            VersionBlock.Text = "Версия: " + strVersion;

        }

        private void OnBtnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnHyperlinkRequestNavigateClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
