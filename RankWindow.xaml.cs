using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cuthill
{
    public partial class RankWindow : Window
    {
        public int Rank;

        public RankWindow()
        {
            InitializeComponent();
            Rank = 0;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Rank = (!Int32.TryParse(InputBox.Text, out Rank)) ? 0 : Rank;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) => Close();
        
        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.Text = (box.Text.Length > 3)? box.Text.Substring(0, 3) : box.Text;
            if (box.Text.Any(ch => !Char.IsDigit(ch)))
                box.Text = new string(box.Text.Where(ch => Char.IsDigit(ch)).ToArray());
            if (box.Text.Length > 1)
                while (box.Text.ElementAt(0) == '0')
                    box.Text = box.Text.Substring(1, box.Text.Length - 1);
            box.CaretIndex = box.Text.Length;
        }
    }
}
