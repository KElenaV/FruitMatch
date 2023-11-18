using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MatchForThree
{
    public class GameGrid : MonoBehaviour
    {
        [System.Serializable]
        public struct PiecePrefab
        {
            public PieceType type;
            public GameObject prefab;
        };

        [System.Serializable]
        public struct PiecePosition
        {
            public PieceType type;
            public int x;
            public int y;
        };

        [SerializeField] private int _xDimension;
        [SerializeField] private int _yDimension;
        [SerializeField] private float _fillTime;

        [SerializeField] private Level _level;

        [SerializeField] private PiecePrefab[] _piecePrefabs;
        [SerializeField] private GameObject _background;

        [SerializeField] private PiecePosition[] _piecePositions;

        private Dictionary<PieceType, GameObject> _piecePrefabDict;

        private GamePiece[,] _pieces;

        private bool _inverse;

        private GamePiece _pressedPiece;
        private GamePiece _enteredPiece;

        private bool _gameOver;

        public bool IsFilling { get; private set; }
        public Level Level => _level;

        private void Awake()
        {
            _piecePrefabDict = _piecePrefabs.GroupBy(p => p.type)
                                            .ToDictionary(group => group.Key, group => group.First().prefab);

            for (int x = 0; x < _xDimension; x++)
            {
                for (int y = 0; y < _yDimension; y++)
                {
                    GameObject background = Instantiate(_background, GetWorldPosition(x, y), Quaternion.identity);
                    background.transform.parent = transform;
                }
            }

            _pieces = new GamePiece[_xDimension, _yDimension];

            for (int i = 0; i < _piecePositions.Length; i++)
            {
                if (_piecePositions[i].x >= 0 && _piecePositions[i].y < _xDimension
                                            && _piecePositions[i].y >= 0 && _piecePositions[i].y < _yDimension)
                {
                    SpawnNewPiece(_piecePositions[i].x, _piecePositions[i].y, _piecePositions[i].type);
                }
            }

            for (int x = 0; x < _xDimension; x++)
            {
                for (int y = 0; y < _yDimension; y++)
                {
                    if (_pieces[x, y] == null)
                    {
                        SpawnNewPiece(x, y, PieceType.Empty);
                    }
                }
            }

            StartCoroutine(Fill());
        }

        private IEnumerator Fill()
        {
            bool needsRefill = true;
            IsFilling = true;

            while (needsRefill)
            {
                yield return new WaitForSeconds(_fillTime);
                while (FillStep())
                {
                    _inverse = !_inverse;
                    yield return new WaitForSeconds(_fillTime);
                }

                needsRefill = ClearAllValidMatches();
            }

            IsFilling = false;
        }

        private bool FillStep()
        {
            bool movedPiece = false;

            for (int y = _yDimension - 2; y >= 0; y--)
            {
                for (int loopX = 0; loopX < _xDimension; loopX++)
                {
                    int x = loopX;
                    if (_inverse) { x = _xDimension - 1 - loopX; }
                    GamePiece piece = _pieces[x, y];

                    if (!piece.IsMovable()) continue;

                    GamePiece pieceBelow = _pieces[x, y + 1];

                    if (pieceBelow.Type == PieceType.Empty)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y + 1, _fillTime);
                        _pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.Empty);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag == 0) continue;

                            int diagX = x + diag;

                            if (_inverse)
                            {
                                diagX = x - diag;
                            }

                            if (diagX < 0 || diagX >= _xDimension) continue;

                            GamePiece diagonalPiece = _pieces[diagX, y + 1];

                            if (diagonalPiece.Type != PieceType.Empty) continue;

                            bool hasPieceAbove = true;

                            for (int aboveY = y; aboveY >= 0; aboveY--)
                            {
                                GamePiece pieceAbove = _pieces[diagX, aboveY];

                                if (pieceAbove.IsMovable())
                                {
                                    break;
                                }
                                else if (pieceAbove.Type != PieceType.Empty)
                                {
                                    hasPieceAbove = false;
                                    break;
                                }
                            }

                            if (hasPieceAbove) continue;

                            Destroy(diagonalPiece.gameObject);
                            piece.MovableComponent.Move(diagX, y + 1, _fillTime);
                            _pieces[diagX, y + 1] = piece;
                            SpawnNewPiece(x, y, PieceType.Empty);
                            movedPiece = true;
                            break;
                        }
                    }
                }
            }

            for (int x = 0; x < _xDimension; x++)
            {
                GamePiece pieceBelow = _pieces[x, 0];

                if (pieceBelow.Type != PieceType.Empty) continue;

                Destroy(pieceBelow.gameObject);
                GameObject newPiece = Instantiate(_piecePrefabDict[PieceType.Normal], GetWorldPosition(x, -1), Quaternion.identity, this.transform);

                _pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                _pieces[x, 0].Init(x, -1, this, PieceType.Normal);
                _pieces[x, 0].MovableComponent.Move(x, 0, _fillTime);
                _pieces[x, 0].ColorComponent.SetColor((ColorType)Random.Range(0, _pieces[x, 0].ColorComponent.NumColors));
                movedPiece = true;
            }

            return movedPiece;
        }

        public Vector2 GetWorldPosition(int x, int y)
        {
            return new Vector2(
                transform.position.x - _xDimension / 2.0f + x,
                transform.position.y + _yDimension / 2.0f - y);
        }

        private GamePiece SpawnNewPiece(int x, int y, PieceType type)
        {
            GameObject newPiece = Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity, this.transform);
            _pieces[x, y] = newPiece.GetComponent<GamePiece>();
            _pieces[x, y].Init(x, y, this, type);

            return _pieces[x, y];
        }

        private static bool IsAdjacent(GamePiece piece1, GamePiece piece2) =>
            (piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
            (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1);

        private void SwapPieces(GamePiece piece1, GamePiece piece2)
        {
            if (_gameOver) { return; }

            if (!piece1.IsMovable() || !piece2.IsMovable()) return;

            _pieces[piece1.X, piece1.Y] = piece2;
            _pieces[piece2.X, piece2.Y] = piece1;

            if (GetMatch(piece1, piece2.X, piece2.Y) != null ||
                GetMatch(piece2, piece1.X, piece1.Y) != null ||
                piece1.Type == PieceType.Rainbow ||
                piece2.Type == PieceType.Rainbow)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, _fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, _fillTime);

                if (piece1.Type == PieceType.Rainbow && piece1.IsClearable() && piece2.IsColored())
                {
                    ClearColor clearColor = piece1.GetComponent<ClearColor>();

                    if (clearColor)
                    {
                        clearColor.Color = piece2.ColorComponent.Color;
                    }

                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.Type == PieceType.Rainbow && piece2.IsClearable() && piece1.IsColored())
                {
                    ClearColor clearColor = piece2.GetComponent<ClearColor>();

                    if (clearColor)
                    {
                        clearColor.Color = piece1.ColorComponent.Color;
                    }

                    ClearPiece(piece2.X, piece2.Y);
                }

                ClearAllValidMatches();

                if (piece1.Type == PieceType.RowClear || piece1.Type == PieceType.ColumnClear)
                {
                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.Type == PieceType.RowClear || piece2.Type == PieceType.ColumnClear)
                {
                    ClearPiece(piece2.X, piece2.Y);
                }

                _pressedPiece = null;
                _enteredPiece = null;

                StartCoroutine(Fill());

                _level.OnMove();
            }
            else
            {
                _pieces[piece1.X, piece1.Y] = piece1;
                _pieces[piece2.X, piece2.Y] = piece2;
            }
        }

        public void PressPiece(GamePiece piece) => _pressedPiece = piece;

        public void EnterPiece(GamePiece piece) => _enteredPiece = piece;

        public void ReleasePiece()
        {
            if (IsAdjacent(_pressedPiece, _enteredPiece))
            {
                SwapPieces(_pressedPiece, _enteredPiece);
            }
        }

        private bool ClearAllValidMatches()
        {
            bool needsRefill = false;

            for (int y = 0; y < _yDimension; y++)
            {
                for (int x = 0; x < _xDimension; x++)
                {
                    if (!_pieces[x, y].IsClearable()) continue;

                    List<GamePiece> match = GetMatch(_pieces[x, y], x, y);

                    if (match == null) continue;

                    PieceType specialPieceType = PieceType.Count;
                    GamePiece randomPiece = match[Random.Range(0, match.Count)];
                    int specialPieceX = randomPiece.X;
                    int specialPieceY = randomPiece.Y;

                    if (match.Count == 4)
                    {
                        if (_pressedPiece == null || _enteredPiece == null)
                        {
                            specialPieceType = (PieceType)Random.Range((int)PieceType.RowClear, (int)PieceType.ColumnClear);
                        }
                        else if (_pressedPiece.Y == _enteredPiece.Y)
                        {
                            specialPieceType = PieceType.RowClear;
                        }
                        else
                        {
                            specialPieceType = PieceType.ColumnClear;
                        }
                    } 
                    else if (match.Count >= 5)
                    {
                        specialPieceType = PieceType.Rainbow;
                    }

                    foreach (var gamePiece in match)
                    {
                        if (ClearPiece(gamePiece.X, gamePiece.Y))
                            needsRefill = true;

                        if (gamePiece == _pressedPiece || gamePiece == _enteredPiece)
                        {
                            specialPieceX = gamePiece.X;
                            specialPieceY = gamePiece.Y;
                        }
                    }

                    if (specialPieceType == PieceType.Count) continue;

                    Destroy(_pieces[specialPieceX, specialPieceY]);
                    GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

                    if ((specialPieceType == PieceType.RowClear || specialPieceType == PieceType.ColumnClear)
                        && newPiece.IsColored() && match[0].IsColored())
                    {
                        newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                    }
                    else if (specialPieceType == PieceType.Rainbow && newPiece.IsColored())
                    {
                        newPiece.ColorComponent.SetColor(ColorType.Any);
                    }
                }
            }

            return needsRefill;
        }

        private List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
        {
            if (!piece.IsColored()) return null;
            var color = piece.ColorComponent.Color;
            var horizontalPieces = new List<GamePiece>();
            var verticalPieces = new List<GamePiece>();
            var matchingPieces = new List<GamePiece>();

            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < _xDimension; xOffset++)
                {
                    int x = (dir == 0) ? (newX - xOffset) : (newX + xOffset);

                    if (x < 0 || x >= _xDimension) break;

                    var newYPiece = _pieces[x, newY];

                    if (newYPiece.IsColored() && newYPiece.ColorComponent.Color == color)
                        horizontalPieces.Add(newYPiece);
                    else
                        break;
                }
            }

            if (horizontalPieces.Count >= 3)
            {
                matchingPieces.AddRange(horizontalPieces);
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int yOffset = 1; yOffset < _yDimension; yOffset++)
                        {
                            int y;


                            if (dir == 0)
                            {
                                y = newY - yOffset;
                            }
                            else
                            {
                                y = newY + yOffset;
                            }

                            if (y < 0 || y >= _yDimension)
                            {
                                break;
                            }

                            if (_pieces[horizontalPieces[i].X, y].IsColored() && _pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                            {
                                verticalPieces.Add(_pieces[horizontalPieces[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (verticalPieces.Count < 2)
                    {
                        verticalPieces.Clear();
                    }
                    else
                    {
                        matchingPieces.AddRange(verticalPieces);
                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < _xDimension; yOffset++)
                {
                    int y;

                    if (dir == 0)
                        y = newY - yOffset;
                    else
                        y = newY + yOffset;

                    if (y < 0 || y >= _yDimension) { break; }

                    if (_pieces[newX, y].IsColored() && _pieces[newX, y].ColorComponent.Color == color)
                        verticalPieces.Add(_pieces[newX, y]);
                    else
                        break;
                }
            }

            if (verticalPieces.Count >= 3)
                matchingPieces.AddRange(verticalPieces);

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 1; xOffset < _yDimension; xOffset++)
                        {
                            int x;

                            if (dir == 0)
                                x = newX - xOffset;
                            else
                                x = newX + xOffset;

                            if (x < 0 || x >= _xDimension)
                                break;

                            if (_pieces[x, verticalPieces[i].Y].IsColored() && _pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                                horizontalPieces.Add(_pieces[x, verticalPieces[i].Y]);
                            else
                                break;
                        }
                    }

                    if (horizontalPieces.Count < 2)
                    {
                        horizontalPieces.Clear();
                    }
                    else
                    {
                        matchingPieces.AddRange(horizontalPieces);
                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            return null;
        }

        private void ChangeValue(int dir, int value, int newValue, int offset)
        {
            if (dir == 0)
                value = newValue - offset;
            else
                value = newValue + offset;
        }

        private bool ClearPiece(int x, int y)
        {
            var piece = _pieces[x, y];

            if (piece.IsClearable() == false || piece.ClearableComponent.IsBeingCleared) return false;

            piece.ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.Empty);
            ClearObstacles(x, y);

            return true;
        }

        private void ClearObstacles(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
            {
                int adjacentX = x + i;
                int adjacentY = y + i;

                if (i != 0 && adjacentX >= 0 && adjacentX < _xDimension)
                    ClearObstacleAt(adjacentX, y);

                if (i != 0 && adjacentY >= 0 && adjacentY < _yDimension)
                    ClearObstacleAt(x, adjacentY);
            }
        }

        private void ClearObstacleAt(int x, int y)
        {
            if (_pieces[x, y].Type == PieceType.Bubble && _pieces[x, y].IsClearable())
            {
                _pieces[x, y].ClearableComponent.Clear();
                SpawnNewPiece(x, y, PieceType.Empty);
            }
        }

        public void ClearRow(int row)
        {
            for (int x = 0; x < _xDimension; x++)
                ClearPiece(x, row);
        }

        public void ClearColumn(int column)
        {
            for (int y = 0; y < _yDimension; y++)
                ClearPiece(column, y);
        }

        public void ClearColor(ColorType color)
        {
            var pieces = _pieces.Cast<GamePiece>()
                                .Where(p => p.IsColored() && p.ColorComponent.Color == color || color == ColorType.Any);

            foreach (var piece in pieces)
            {
                ClearPiece(piece.X, piece.Y);
            }
        }

        public void GameOver() => _gameOver = true;

        public List<GamePiece> GetPiecesOfType(PieceType type)
            => _pieces.Cast<GamePiece>().Where(p => p.Type == type).ToList();

    }
}
