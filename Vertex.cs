using System.Collections.Generic;

namespace Cuthill
{
    public class Vertex
    {
        public int ID { get; set; }
        public int NewID { get; set; }
        public int Bonds { get; set; }
        public List<Vertex> ConnectedVertices { get; set; }

        private bool Viewed { get; set; }
        private bool Cutted { get; set; }

        public Vertex(int id)
        {
            ID = id;
            NewID = 0;
            Bonds = 0;
            ConnectedVertices = new List<Vertex>();
        }

        public void View() => Viewed = true;
        public void Cut() => Cutted = true;
        public void Reset() { Cutted = false; Viewed = false; }
        public bool IsViewed() => Viewed;
        public bool IsCutted() => Cutted;
        public void CalculateBonds() => Bonds = ConnectedVertices.Count;
        public override string ToString() => $"Vertex: {ID} => {NewID}, bonds: {ConnectedVertices.Count}";
    }
}
