using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;

namespace SimNite.Views
{
    public partial class WelcomeView : UserControl
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void ViewChangelogs_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/erwang64/SimNite/releases/latest", // Remplace par ton vrai repo
                UseShellExecute = true
            });
        }
    }
}
