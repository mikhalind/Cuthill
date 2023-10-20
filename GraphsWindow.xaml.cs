using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Linq;

namespace Cuthill
{
    public partial class GraphsWindow : Window
    {
        public List<Entry> entries;
        public GraphsWindow() => InitializeComponent();
        public GraphsWindow(List<int[]> list)
        {
            InitializeComponent();
            SetData(list);
        }
        public void SetData(List<int[]> list)
        {
            entries = (from int[] item in list select new Entry(item[0], item[1], item[2])).ToList();
            listView.ItemsSource = entries;
        }
    }
}
