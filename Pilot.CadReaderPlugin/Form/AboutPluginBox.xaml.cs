using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Ascon.Pilot.SDK.CadReader.Form
{
    /// <inheritdoc cref="MessageBox.xaml" />
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class AboutPluginBox
    {
        public AboutPluginBox()
        {
            InitializeComponent();
            var strVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
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
