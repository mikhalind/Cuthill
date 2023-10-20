using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows;

namespace Cuthill
{
    public class Graph
    {
        public enum Mode
        { 
            Old = 0,
            New = 1
        };

        public int Width { get; set; }

        public List<Vertex> Vertices { get; set; }

        public Graph() => Vertices = new List<Vertex>();

        public Graph(int[,] matrix)
        {
            Vertices = new List<Vertex>();
            int rank = matrix.GetLength(0);
            for (int i = 0; i < rank; i++)
                AddVertex(i + 1);
            for (int i = 0; i < rank; i++)
                for (int j = 0; j < rank; j++)
                    if (matrix[i, j] == 1 && i != j)
                        AddConnection(i + 1, j + 1);
            CalculateBonds();
        }

        public Graph(List<Vertex> list) => Vertices = list;

        ~Graph() => Vertices.Clear();

        public void AddVertex(int id) => Vertices.Add(new Vertex(id));        

        public void AddConnection(int id, int to)
        {
            Vertex firstVertex = FindVertex(id, Mode.Old);
            Vertex secondVertex = FindVertex(to, Mode.Old);
            firstVertex.ConnectedVertices.Add(secondVertex);
        }

        public bool CheckConnection(int first, int second, Mode mode)
        {
            Vertex firstVertex = FindVertex(first, mode);
            return firstVertex.ConnectedVertices.Any(vx => vx.NewID == second);
        }

        public void CalculateWidth() // => Width = Matrix.GetWidth(GetMatrix(Graph.Mode.New));
        {
            int max = 0;
            foreach (var item in Vertices)
                foreach (var connected in item.ConnectedVertices)
                    if (Math.Abs(item.NewID - connected.NewID) > max)
                        max = Math.Abs(item.NewID - connected.NewID);
            Width = max;
        }

        public void CalculateBonds() 
        {
            foreach (var vertex in Vertices)
                vertex.CalculateBonds(); 
        }

        public Vertex FindMinimum() 
        {
            Vertex vertex;
            if (Vertices.Count == 0)
                vertex = null;
            else
            {
                int min = Vertices.Min(vx => vx.Bonds);
                vertex = Vertices.Where(vx => vx.Bonds == min).ElementAt(0);
            }
            return vertex;
        }

        public Vertex FindVertex(int id, Mode mode)
        {
            foreach (var v in Vertices)
                if (((mode == Mode.Old) ? v.ID : v.NewID) == id)
                    return v;
            return null;
        }
    }
}
