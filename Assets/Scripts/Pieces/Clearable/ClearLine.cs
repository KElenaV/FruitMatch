namespace MatchForThree
{
    internal class ClearLine : Clearable
    {
        public bool isRow;

        public override void Clear()
        {
            base.Clear();

            if (isRow)
                Piece.GameGridRef.ClearRow(Piece.Y);
            else
                Piece.GameGridRef.ClearColumn(Piece.X);
        }
    }
}
