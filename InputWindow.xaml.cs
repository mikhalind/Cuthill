using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cuthill
{
    public partial class InputWindow : Window
    {
        public int[,] ResultMatrix;
        public List<string> NamesAccordance = new List<string>();

        private void SetButtonEnabled(bool isEnabled)
        {
            ButtonOK.IsEnabled = isEnabled;
            ImageOK.Opacity = (isEnabled) ? 1 : 0.5;
        }

        private void UpdateMatrix(int[,] matrix)
        {
            int rank = matrix.GetLength(0);

            DynamicGrid.Children.Clear();
            DynamicGrid.ColumnDefinitions.Clear();
            DynamicGrid.RowDefinitions.Clear();

            DynamicGrid.Background = Application.Current.Resources["BackgroundGray"] as SolidColorBrush;

            for (int i = 0; i < rank; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                RowDefinition row = new RowDefinition();
                DynamicGrid.ColumnDefinitions.Add(col);
                DynamicGrid.RowDefinitions.Add(row);
            }

            SolidColorBrush crosBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            SolidColorBrush noneBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));

            for (int i = 0; i < rank; i++)
            {
                for (int j = 0; j < rank; j++)
                {
                    Border border = new Border
                    {
                        CornerRadius = new CornerRadius(50 / rank),
                        Margin = new Thickness(0.5),
                        Background = (matrix[i, j] == 1) ? crosBrush : noneBrush
                    };
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    DynamicGrid.Children.Add(border);
                }
            }
        }

        public InputWindow()
        {
            InitializeComponent();
            SetButtonEnabled(false);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ResultMatrix = null;
            Close();
        }

        private void ButtonInfo_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Вводите матрицу цифрами 0 и 1, построчно без пробелов\n" +
            "ИЛИ\nпары названий смежных вершин через символ двоеточия", "Информация");

        private void UpdateStatus(bool correct, string error = "Все хорошо")
        {
            BarText.Text = error;
            BarIcon.Source = new BitmapImage(new Uri($"Icons/{(correct ? "okay" : "cross")}.png", UriKind.Relative));
            SetButtonEnabled(correct);
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int[,] mx;
            string[] lines = InputBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                UpdateStatus(false, "Матрица не введена");
                ResultMatrix = null;
                return;
            }
            try
            {
                mx = Matrix.ReadMatrixFromPairs(lines, out NamesAccordance);
            }
            catch (Exception outerException)
            {
                try
                {
                    mx = Matrix.ReadMatrixFromArray(lines);
                    BarText.Text = outerException.Message;
                }
                catch (Exception innerException)
                {
                    ResultMatrix = null;
                    UpdateStatus(false, innerException.Message);
                    return;
                }
            }
            ResultMatrix = mx;
            UpdateStatus(true);
            UpdateMatrix(mx);
        }
    }
}
