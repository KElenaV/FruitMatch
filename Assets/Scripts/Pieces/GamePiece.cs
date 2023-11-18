using UnityEngine;

namespace MatchForThree
{
    public class GamePiece : MonoBehaviour
    {
        [SerializeField] private int _score;

        private int _x;
        private int _y;
        private PieceType _type;
        private GameGrid _gameGrid;
        private MovablePiece _movableComponent;
        private Clearable _clearableComponent;
        private ColorPiece _colorComponent;

        public int X
        {
            get => _x;
            set { if (IsMovable()) { _x = value; } }
        }

        public int Y
        {
            get => _y;
            set { if (IsMovable()) { _y = value; } }
        }
        
        public int Score => _score;

        public PieceType Type => _type;

        public GameGrid GameGridRef => _gameGrid;

        public MovablePiece MovableComponent => _movableComponent;

        public Clearable ClearableComponent => _clearableComponent;
        
        public ColorPiece ColorComponent => _colorComponent;

        private void Awake()
        {
            _movableComponent = GetComponent<MovablePiece>();
            _colorComponent = GetComponent<ColorPiece>();
            _clearableComponent = GetComponent<Clearable>();
        }

        public void Init(int x, int y, GameGrid gameGrid, PieceType type)
        {
            _x = x;
            _y = y;
            _gameGrid = gameGrid;
            _type = type;
        }

        private void OnMouseEnter() => _gameGrid.EnterPiece(this);

        private void OnMouseDown() => _gameGrid.PressPiece(this);

        private void OnMouseUp() => _gameGrid.ReleasePiece();

        public bool IsMovable() => _movableComponent != null;

        public bool IsColored() => _colorComponent != null;

        public bool IsClearable() => _clearableComponent != null;
    }
}
