namespace MatchForThree
{
    public class ClearColor : Clearable
    {
        public ColorType Color { get; set; }

        public override void Clear()
        {
            base.Clear();

            Piece.GameGridRef.ClearColor(Color);
        }
    }
}
