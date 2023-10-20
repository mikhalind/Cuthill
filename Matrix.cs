using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cuthill
{
    abstract class Matrix
    {
        public static int GetWidth(int[,] matrix)
        {
            int rank = matrix.GetLength(0);
            int width = 0;
            for (int i = 0; i < rank; i++)
                for (int j = 0; j < i; j++)
                    if (matrix[i, j] == 1)
                        if ((i - j) > width)
                            width = i - j;                        
            return width;
        }

        public static void CheckMatrix(int[,] matrix)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
                throw new InvalidDataException($"Количество рядов ({matrix.GetLength(0)}) и строк ({matrix.GetLength(1)}) не равно");
            else
            {
                int rank = matrix.GetLength(0);
                for (int i = 0; i < rank; i++)
                {
                    for (int j = 0; j < rank; j++)
                    {
                        if (i == j && matrix[i, j] != 1)
                            throw new InvalidDataException($"Нуль на главной диагонали, строка {i + 1}, ряд {j + 1}");
                        if (matrix[i, j] != matrix[j, i])
                            throw new InvalidDataException($"Матрица не симметричная, строка {i + 1}, ряд {j + 1}");
                        if (matrix[i, j] != 0 && matrix[i, j] != 1)
                            throw new InvalidDataException($"Недопустимый символ {matrix[i, j]}, строка {i + 1}, ряд {j + 1}");
                    }
                }
            }
        }

        public static int[,] ReadMatrixFromArray(string[] stringsArray)
        {
            int[,] matrix;
            int rank = stringsArray.Length;

            foreach (string firstLine in stringsArray)
                foreach (string secondLine in stringsArray)
                    if (firstLine.Length != secondLine.Length)
                        throw new InvalidDataException($"Разная длина входных строк, проверьте матрицу");

            if (!stringsArray.All(row => row.Length == stringsArray.Length))
                throw new InvalidDataException($"Количество рядов ({stringsArray.Length}) и столбцов ({stringsArray[0].Length}) не равно");

            for (int i = 0; i < stringsArray.Length; i++)
                for (int j = 0; j < stringsArray[i].Length; j++)
                    if (stringsArray[i][j] != '1' && stringsArray[i][j] != '0')
                        throw new InvalidDataException($"Найден недопустимый символ: \"{stringsArray[i][j]}\" (ряд {i + 1}, столбец {j + 1})");

            matrix = new int[rank, rank];
            for (int i = 0; i < rank; i++)
                for (int j = 0; j < rank; j++)
                    matrix[i, j] = Int32.Parse(stringsArray[i][j].ToString());
            CheckMatrix(matrix);
            return matrix;
        }

        public static int InputType(string[] lines) => lines.All(item => item.Count(ch => ch == ':') == 1) ? 1 : 0;
        
        public static int[,] ReadMatrixFromFile(string path, out List<string> elements)
        {
            int[,] matrix;
            string[] array = File.ReadAllLines(path);

            if (array.Length == 0)
                throw new FileFormatException($"Файл \"{Path.GetFileName(path)}\" пустой");

            if (InputType(array) == 0)
            {
                matrix = ReadMatrixFromArray(array);
                elements = new List<string>();
            }
            else
                matrix = ReadMatrixFromPairs(array, out elements);
            return matrix;
        }

        public static int[,] ReadMatrixFromPairs(string[] lines, out List<string> elements)
        {
            if (lines.Any(str => str.Count(ch => ch == ':') != 1))
                throw new Exception($"Невозможно считать строку: \"{lines.Where(s => s.Count(c => c == ':') != 1).ElementAt(0)}\"");

            List<string> elementsList = new List<string>();
            List<Tuple<string, string>> bindingsList = new List<Tuple<string, string>>();            
            foreach (string line in lines)
            {
                string[] pairs = line.Split(':');
                bindingsList.Add(new Tuple<string, string>(pairs[0], pairs[1]));
            }

            for (int i = 0; i < bindingsList.Count; i++)
            {
                Tuple<string, string> item = bindingsList.ElementAt(i);
                if (!elementsList.Contains(item.Item1))
                    elementsList.Add(item.Item1);
                if (!elementsList.Contains(item.Item2))
                    elementsList.Add(item.Item2);
            }

            int rank = elementsList.Count;
            int[,] matrix = new int[rank, rank];
            for (int i = 0; i < rank; i++)
                for (int j = 0; j < rank; j++)
                    matrix[i, j] = (i == j) ? 1 : 0;

            foreach (var pair in bindingsList)
            {
                int row = elementsList.IndexOf(pair.Item1);
                int col = elementsList.IndexOf(pair.Item2);
                matrix[row, col] = 1;
                matrix[col, row] = 1;
            }

            elements = elementsList;
            return matrix;
        }

        [Obsolete]
        public static string PrintMatrix(int[,] matrix, char cross = 'X', char empty = '.') 
        {
            CheckMatrix(matrix);
            string result = String.Empty;            
            int rank = matrix.GetLength(0);
            result += $"    ";
            for (int i = 0; i < rank; i++)
                result += $"{i + 1} ";
            result += Environment.NewLine;
            for (int i = 0; i < rank; i++)
            {
                result += $"{((i < 9)? " " : string.Empty)}{i + 1}| ";
                for (int j = 0; j < rank; j++)
                    result = (matrix[i, j] == 1) ? result + $"{cross} " : result + $"{empty} ";
                result += Environment.NewLine;
            }
            return result;
        }

        public static WriteableBitmap RenderToBitmap(int[,] matrix)
        {
            int side = matrix.GetLength(0);
            WriteableBitmap map = new WriteableBitmap(side, side, 72, 72, PixelFormats.BlackWhite, null);
            for (int i = 0; i < side; i++)
            {
                for (int j = 0; j < side; j++)
                {
                    int code = (matrix[i, j] == 1) ? 0 : 255;
                    byte[] color = { (byte)code, (byte)code, (byte)code, (byte)code };
                    Int32Rect rect = new Int32Rect(i, j, 1, 1);
                    map.WritePixels(rect, color, 4, 0);
                }
            }
            return map;
        }

        public static int[,] GenerateMatrix(int rank)
        {
            int[,] matrix = new int[rank, rank];
            for (int i = 0; i < rank; i++)
                for (int j = 0; j < rank; j++)                
                    matrix[i, j] = (i == j)? 1 : 0;

            Random rand = new Random();
            for (int i = 0; i < rank; i++)
            {
                for (int j = i + 1; j < rank; j++)
                {
                    matrix[i, j] = (rand.Next(0, 100) >= ((4.7 * Math.Log(rank) + 76 > 99) ? 99 : 4.7 * Math.Log(rank) + 76) ) ? 1 : 0;
                    matrix[j, i] = matrix[i, j];
                }
            }

            return matrix;
        }
    }
}
