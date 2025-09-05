namespace GridDisplay
{
    public class GridCell
    {
        public bool Blocked { get; set; }
        public int Height { get; set; }
        public int Alignment { get; set; }

        public GridCell()
        {
            Blocked = false;
            Height = 0;
            Alignment = 0;
        }

        public GridCell(bool blocked, int height, int alignment)
        {
            Blocked = blocked;
            Height = height;
            Alignment = alignment;
        }
    }
}