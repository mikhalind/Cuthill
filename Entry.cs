namespace Cuthill
{
    public class Entry
    {
        public int Graph { get; set; }
        public int Vertices { get; set; }
        public int BandWidth { get; set; }

        public Entry(int graph, int vertices, int bandwidth)
        {
            Graph = graph;
            Vertices = vertices;
            BandWidth = bandwidth;
        }
    }
}
