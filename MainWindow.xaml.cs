using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Cuthill
{
    public partial class MainWindow : Window
    {
        private int[,] InputMatrix = null;
        private int[,] OutputMatrix = null;
        private SuperGraph Supergraph;
        private List<string> NamesMatch = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            SetButtonEnabled(false, ButtonSave, ImageSave);
            SetButtonEnabled(false, ButtonStart, ImageStart);
            SetItemEnabled(false, ItemSave, ItemLog, ItemGraphs);
            UpdateStatus(false);
        }

        private void SetItemEnabled(bool isEnabled, params MenuItem[] items)
        {
            foreach (MenuItem item in items)
                item.IsEnabled = isEnabled;
        }

        private void SetButtonEnabled(bool isEnabled, Button aButton, Image aImage)
        {
            aButton.IsEnabled = isEnabled;
            aImage.Opacity = (isEnabled) ? 1 : 0.5;
        }

        private void UpdateStatus(bool isOpened, string filePath = "")
        {
            BarIcon.Source = new BitmapImage(new Uri($"Icons/{((isOpened) ? "okay" : "cross")}.png", UriKind.Relative));
            BarText.Text = (isOpened) ?
                (filePath.Length != 0) ? 
                    $"Файл {System.IO.Path.GetFileName(filePath)} открыт" : 
                    "Матрица успешно введена" :
                "Матрица не загружена";
        }

        private void ScaleMatrix(object sender, MouseButtonEventArgs e)
        {
            Image image = new Image
            {
                Source = (sender as Image).Source,
                Margin = new Thickness(1)
            };

            Border border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(127, 127, 127)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5),
                Child = image
            };

            Window scaledMatrixWindow = new Window
            {
                Title = "Просмотр матрицы",
                Height = 666,
                Width = 666,
                MaxHeight = 1000,
                MaxWidth = 1000,
                Content = border,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            scaledMatrixWindow.ShowDialog();
            scaledMatrixWindow.Close();
        }

        private void UpdateMatrix(int[,] matrix, bool isInput)
        {
            Grid dynamicGrid = (isInput) ? InputGrid : OutputGrid;
            dynamicGrid.Children.Clear();
            dynamicGrid.ColumnDefinitions.Clear();
            dynamicGrid.RowDefinitions.Clear();
            dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            dynamicGrid.RowDefinitions.Add(new RowDefinition());
            Image matrixImage = new Image() { Source = Matrix.RenderToBitmap(matrix) };
            RenderOptions.SetBitmapScalingMode(matrixImage, BitmapScalingMode.NearestNeighbor);
            matrixImage.MouseLeftButtonDown += ScaleMatrix;
            Grid.SetRow(matrixImage, 0);
            Grid.SetColumn(matrixImage, 0);
            dynamicGrid.Children.Add(matrixImage);
            if (isInput) { inputLabel.Text = $"Исходная матрица (m = {Matrix.GetWidth(matrix)})"; inputBorder.BorderThickness = new Thickness(0); }
            else { outputLabel.Text = $"Новая матрица (m = {Matrix.GetWidth(matrix)})"; outputBorder.BorderThickness = new Thickness(0); }
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "txt files (*.txt)|*.txt"
            };

            openFileDialog.ShowDialog();
            string filePath = openFileDialog.FileName;

            if (filePath == String.Empty) return;

            try
            {
                InputMatrix = Matrix.ReadMatrixFromFile(filePath, out NamesMatch);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка считывания", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UpdateStatus(true, filePath);
            UpdateMatrix(InputMatrix, true);
            SetButtonEnabled(true, ButtonStart, ImageStart);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OutputMatrix = CuthillMethod.Start(InputMatrix, NamesMatch, out Supergraph);
                UpdateMatrix(OutputMatrix, false);
                SetButtonEnabled(true, ButtonSave, ImageSave);
                SetItemEnabled(true, ItemSave, ItemLog, ItemGraphs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка преобразования", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = $"result_{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.txt",
                Filter = "Text file (*.txt)|*.txt|Bitmap Image (*.bmp)|*.bmp"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                if (String.IsNullOrEmpty(filePath)) return;
                string extension = Path.GetExtension(filePath);
                if (extension.ToLower() == ".txt")
                {
                    using (StreamWriter streamWriter = new StreamWriter(filePath))
                    {
                        int rank = OutputMatrix.GetLength(0);
                        for (int i = 0; i < rank; i++)
                        {
                            for (int j = 0; j < rank; j++)
                                streamWriter.Write(OutputMatrix[i, j]);
                            streamWriter.WriteLine();
                        }
                    }
                }
                else if (extension.ToLower() == ".bmp")
                {
                    Image image = new Image { Source = Matrix.RenderToBitmap(OutputMatrix) };
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    int resolution = 5 * OutputMatrix.GetLength(0);
                    image.Arrange(new Rect(new Size(resolution, resolution)));                    
                    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)image.ActualWidth,
                                                                       (int)image.ActualHeight, 
                                                                       96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(image);
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));

                    using (FileStream stream = File.Create(filePath)) { encoder.Save(stream); }
                }
                else throw new NotImplementedException();
                MessageBox.Show("Файл успешно сохранен", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ItemAbout_Click(object sender, RoutedEventArgs e) => new AboutWindow().ShowDialog();

        private void ItemExit_Click(object sender, RoutedEventArgs e) => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?", "Вопрос", 
                                                      MessageBoxButton.YesNo, 
                                                      MessageBoxImage.Question,
                                                      MessageBoxResult.Yes);
            e.Cancel = (result != MessageBoxResult.Yes);            
            base.OnClosing(e);
        }

        private void ItemEnter_Click(object sender, RoutedEventArgs e)
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.ShowDialog();
            if (inputWindow.ResultMatrix != null)
            {
                InputMatrix = inputWindow.ResultMatrix;
                NamesMatch = inputWindow.NamesAccordance;
                UpdateMatrix(InputMatrix, true);
                SetButtonEnabled(true, ButtonStart, ImageStart);
                UpdateStatus(true);
            }
        }

        private void ItemLog_Click(object sender, RoutedEventArgs e)
        {
            string logLines = String.Empty;
            foreach (string line in CuthillMethod.GetLog())
                logLines += line + Environment.NewLine;
            LogWindow logWindow = new LogWindow(logLines);
            logWindow.ShowDialog();
        }

        private void ItemGenerate_Click(object sender, RoutedEventArgs e)
        {
            RankWindow rankInput = new RankWindow();
            rankInput.ShowDialog();
            if (rankInput.Rank != 0)
                InputMatrix = Matrix.GenerateMatrix(rankInput.Rank);
            else
            { 
                MessageBox.Show("Неправильное число", "Ошибка");
                return;
            }
            UpdateMatrix(InputMatrix, true);
            SetButtonEnabled(true, ButtonStart, ImageStart);
            UpdateStatus(true);
        }

        private void ItemGraphs_Click(object sender, RoutedEventArgs e)
        {
            List<int[]> send = new List<int[]>();
            foreach (Graph graph in Supergraph.Graphs)
                send.Add(new int[3] { Supergraph.Graphs.IndexOf(graph), graph.Vertices.Count, graph.Width });
            GraphsWindow gw = new GraphsWindow();
            gw.SetData(send);
            gw.ShowDialog();
        }

        private void ItemHelp_Click(object sender, RoutedEventArgs e) => MessageBox.Show("В разработке", "Исключение");
    }
}