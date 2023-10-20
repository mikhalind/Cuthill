using System.Collections.Generic;
using System.Linq;

namespace Cuthill
{
    class SuperGraph
    {
        public List<Vertex> Vertices { get; set; }
        public List<Vertex> ViewedVertices { get; set; }
        public List<Graph> Graphs { get; set; }

        public SuperGraph()
        {
            Graphs = new List<Graph>();
            Vertices = new List<Vertex>();
            ViewedVertices = new List<Vertex>();
        }

        public SuperGraph(int[,] matrix)
        {
            int rank = matrix.GetLength(0);
            Graphs = new List<Graph>();
            Vertices = new List<Vertex>();
            ViewedVertices = new List<Vertex>();

            for (int i = 1; i <= rank; i++)
            {
                Vertex vertex = new Vertex(i);
                Vertices.Add(vertex);
            }

            for (int i = 0; i < rank; i++)
                for (int j = 0; j < rank; j++)
                    if (matrix[i, j] == 1 && i != j)
                        Vertices.ElementAt(i).ConnectedVertices.Add(Vertices.ElementAt(j));

            foreach (var vertex in Vertices) vertex.CalculateBonds();

            while (Vertices.Any(vertex => !ViewedVertices.Contains(vertex)))
            {
                int startID = 0;
                for (int i = 1; i <= Vertices.Count; i++)
                    if (ViewedVertices.All(ch => ch.ID != i))
                        startID = i;
                    
                Vertex start = Vertices.Where(item => item.ID == startID).ElementAt(0);
                List<List<Vertex>> newOrder = new List<List<Vertex>>() { new List<Vertex>() { start } };
                int level = 0;
                start.View();
                while (level < newOrder.Count)
                {
                    foreach (var item in newOrder.ElementAt(level).OrderBy(u => u.Bonds))
                    {
                        if (item.ConnectedVertices.Where(u => !u.IsViewed()).Count() == 0)
                            continue;
                        newOrder.Add(item.ConnectedVertices.Where(u => u.NewID == 0 && !u.IsViewed()).OrderBy(u => u.Bonds).ToList());
                        item.ConnectedVertices.ForEach(u => u.View());
                    }
                    level++;
                }

                Graph tempGraph = new Graph();
                foreach (var row in newOrder)
                {
                    foreach (var vx in row)
                    {
                        ViewedVertices.Add(vx);
                        tempGraph.Vertices.Add(vx);
                    }
                }
                Graphs.Add(tempGraph);
            }

            foreach (var v in Vertices)
                v.Reset();
        }

        public SuperGraph(List<Graph> list) { Graphs = list; }

        public void Add(Graph graph) { Graphs.Add(graph); }

        public bool CheckConnection(int first, int second, Graph.Mode mode)
        {
            return (mode == Graph.Mode.Old) ?
                Vertices.Find(ch => ch.ID == first).ConnectedVertices.Any(vx => vx.ID == second):
                Vertices.Find(ch => ch.NewID == first).ConnectedVertices.Any(vx => vx.NewID == second);
        }
    }
}
