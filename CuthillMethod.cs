using System.Linq;
using System.Collections.Generic;

namespace Cuthill
{
    internal abstract class CuthillMethod
    {
        private static SuperGraph Supergraph;

        private static readonly List<string> Protocol = new List<string>();

        private static List<string> Names = new List<string>();

        public static List<string> GetLog() => Protocol;       

        public static int[,] Start(int[,] matrix, List<string> names, out SuperGraph sgOutput)
        {
            int graphCounter = 1;
            Names.Clear();
            Names = names;
            Protocol.Clear();
            int rank = matrix.GetLength(0);
            int[,] newMatrix = new int[rank, rank]; 

            Protocol.Add("-=<< СТАРТ АЛГОРИТМА >>=-");            
            Protocol.Add($"Определена размерность матрицы: {rank}");

            Supergraph = new SuperGraph(matrix);
            if (Supergraph.Graphs.Count == 1)
                Protocol.Add($"Граф определен как связный");
            else
            {
                Protocol.Add($"Граф определен как не связный");
                Protocol.Add($"Компонент связности: {Supergraph.Graphs.Count}");
            }

            foreach (string item in Names)
                Protocol.Add($"Вершина {item} - номер {Names.IndexOf(item) + 1}");

            int counter = 1;
            foreach (Graph graph in Supergraph.Graphs)
            {
                Protocol.Add($">>> Анализ {graphCounter++} компоненты связности <<<");
                List<int> orderedList = new List<int>();
                for (int i = counter; i < counter + graph.Vertices.Count; i++)
                    orderedList.Add(i);
                Vertex startVertex = graph.FindMinimum();
                Protocol.Add($"Найдена вершина {startVertex.ID} с min(i)p(i)={startVertex.Bonds}");
                startVertex.NewID = counter;
                counter += graph.Vertices.Count;
                startVertex.Cut();
                Detour(startVertex, orderedList);
                graph.CalculateWidth();
            }
            for (int i = 0; i < rank; i++)
                for (int j = 0; j < rank; j++)
                    newMatrix[i, j] = (i == j) ?  1 :  Supergraph.CheckConnection(i + 1, j + 1, Graph.Mode.New) ? 1 : 0;
            Protocol.Add($"-=<< КОНЕЦ АЛГОРИТМА >>=-");
            sgOutput = Supergraph;
            return newMatrix;
        }
        
        private static void Detour(Vertex startVertex, List<int> orderedList)
        {
            List<List<Vertex>> newOrder = new List<List<Vertex>>() { new List<Vertex>() { startVertex } };
            int level = 0;
            startVertex.View();

            while (level < newOrder.Count)
            {
                foreach (var item in newOrder.ElementAt(level).OrderBy(u => u.Bonds))
                {
                    if (item.ConnectedVertices.Where(u => !u.IsViewed()).Count() == 0)
                    {
                        Protocol.Add($"Вершина {item.ID}: пропущена");
                        continue;
                    }
                    newOrder.Add(item.ConnectedVertices.Where(u => u.NewID == 0 && !u.IsViewed()).OrderBy(u => u.Bonds).ToList());

                    string temp = string.Empty;
                    foreach (Vertex it in item.ConnectedVertices.Where(u => u.NewID == 0 && !u.IsViewed()).OrderBy(u => u.Bonds).ToList())
                        temp += it.ID.ToString() + " ";
                    Protocol.Add($"Вершина {item.ID}: добавлены смежные: {temp}");
                    item.ConnectedVertices.ForEach(u => u.View());
                }
                level++;
            }

            Protocol.Add($"> Переименование вершин... <");

            int newCounter = 0;
            foreach (var subitem in from item in newOrder
                                    from subitem in item
                                    select subitem) {
                subitem.NewID = orderedList.ElementAt(newCounter++);
                Protocol.Add($"Вершине {((Names.Count != 0) ? $"{Names.ElementAt(subitem.ID - 1)} ({subitem.ID})" : $"{subitem.ID}")} " +
                       $"присвоено имя {((Names.Count != 0) ? $"{Names.ElementAt(subitem.NewID - 1)} ({subitem.NewID})" : $"{subitem.NewID}")}");
            }
        }
    }
}
