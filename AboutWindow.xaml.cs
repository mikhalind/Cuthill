using System.Windows;
using System.Diagnostics;

namespace Cuthill
{
    public partial class AboutWindow : Window
    {
        public AboutWindow() => InitializeComponent();

        private void ButtonOK_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonContact_Click(object sender, RoutedEventArgs e) => Process.Start("http://vk.com/die.waschzeit");
    }
}
