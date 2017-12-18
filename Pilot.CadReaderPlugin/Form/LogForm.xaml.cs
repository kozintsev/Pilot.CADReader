namespace Ascon.Pilot.SDK.CadReader.Form
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class LogForm
    {
        public LogForm()
        {
            InitializeComponent();
        }

        public void AddLog(string text)
        {
            ListBoxLog.Items.Add(text);
        }

        public void ClearLog()
        {
            ListBoxLog.Items.Clear();
        }
    }
}
