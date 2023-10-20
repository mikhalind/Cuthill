using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace Cuthill
{
    public partial class LogWindow : Window
    {
        public LogWindow() => InitializeComponent();

        public LogWindow(string text)
        {
            InitializeComponent();
            LogBox.Text = text;
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(LogBox.Text);
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message, "Исключение", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
            MessageBox.Show("Текст скопирован в буфер обмена", "Прекрасно", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = $"log_{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.txt",
                Filter = "txt files (*.txt)|*.txt"
            };
            saveFileDialog.ShowDialog();
            string filePath = (String.IsNullOrEmpty(saveFileDialog.FileName)) ? null : saveFileDialog.FileName;
            if (filePath == null)
                return;
            else using (StreamWriter streamWriter = new StreamWriter(filePath))
                foreach (var line in LogBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                    streamWriter.WriteLine(line);
        }
    }
}
