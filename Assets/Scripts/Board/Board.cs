using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the game board: creates / destroys tiles, handles cell selection,
/// drives BFS path-finding, and applies post-match board updates
/// (Normal / Gravity / Spin).
/// </summary>
public class Board : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static Board Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Grid")]
    [SerializeField, Min(4)] private int width = 8;
    [SerializeField, Min(4)] private int height = 8;

    [Header("Visuals")]
    [SerializeField] private Cell cellPrefab;
    [SerializeField] public LineRenderer pathLine;
    [SerializeField, Min(0.05f)] private float cellPadding = 0.05f;

    [Header("Pokémon Pool")]
    [SerializeField] private List<Pokemon> pokemonPool;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private AudioClip matchSfx;
    [SerializeField] private AudioClip failSfx;

    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private static readonly Vector2Int[] CardinalDirections =
    {
        new( 1,  0),
        new(-1,  0),
        new( 0,  1),
        new( 0, -1),
    };

    private const float PathDisplayDuration = 0.25f;
    private const float DeleteDelay = 0.20f;
    private const float ShuffleCooldown = 1.00f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Tile[,] _tiles;
    private LevelType _currentLevelType = LevelType.Normal;

    private Cell _firstSelected;
    private Cell _secondSelected;

    private bool _isProcessingMatch;
    private bool _isShuffling;

    private float _offsetX;
    private float _offsetY;

    private readonly List<Vector3> _linePoints = new();

    // -------------------------------------------------------------------------
    // Unity messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _offsetX = -(width - 1) / 2f;
        _offsetY = -(height - 1) / 2f + 0.5f;
    }

    private void Start()
    {
        _currentLevelType = LevelType.Normal;
        InitialiseBoard();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Starts a completely new game from level 1.</summary>
    public void NewGame()
    {
        GameManager.Instance.StartGame();
        _currentLevelType = LevelType.Normal;
        InitialiseBoard();
    }

    /// <summary>Called by <see cref="Cell"/> when the player clicks a tile.</summary>
    public void OnCellClicked(Cell cell)
    {
        if (GameManager.Instance.State == GameState.GameOver) return;
        if (_isProcessingMatch) return;
        if (cell == null) return;

        // If a full pair was already highlighted, clear it first
        if (_firstSelected != null && _secondSelected != null)
        {
            ClearSelection();
            return;
        }

        if (_firstSelected == null)
        {
            SelectFirst(cell);
        }
        else if (cell == _firstSelected)
        {
            DeselectFirst();
        }
        else
        {
            SelectSecond(cell);
            TryMatch();
        }
    }

    /// <summary>Randomly repositions all remaining tiles.</summary>
    public void ShuffleTiles()
    {
        if (_isShuffling) return;
        _isShuffling = true;

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (!_tiles[x, y].IsOccupied || _tiles[x, y].Cell == null) continue;

                int rx = Random.Range(1, width - 1);
                int ry = Random.Range(1, height - 1);

                Cell cellA = _tiles[x, y].Cell;
                Cell cellB = _tiles[rx, ry].Cell;

                if (cellB != null && cellA.Tile != null && cellB.Tile != null)
                    SwapCells(cellA, cellB);
            }
        }

        StartCoroutine(ResetShuffleFlagAfter(ShuffleCooldown));
    }

    // -------------------------------------------------------------------------
    // Board initialisation
    // -------------------------------------------------------------------------

    private void InitialiseBoard()
    {
        ClearBoard();

        _tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y] = new Tile();

        var interiorCells = BuildInteriorPositionList();

        while (interiorCells.Count > 1)
        {
            SpawnPair(interiorCells);
            SpawnPair(interiorCells);
        }
    }

    private List<Vector2Int> BuildInteriorPositionList()
    {
        var list = new List<Vector2Int>((width - 2) * (height - 2));
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                list.Add(new Vector2Int(x, y));
        return list;
    }

    private void SpawnPair(List<Vector2Int> availablePositions)
    {
        Pokemon pokemon = pokemonPool[Random.Range(0, pokemonPool.Count)];
        SpawnCell(pokemon, availablePositions);
        SpawnCell(pokemon, availablePositions);
    }

    private void SpawnCell(Pokemon pokemon, List<Vector2Int> availablePositions)
    {
        int idx = Random.Range(0, availablePositions.Count);
        Vector2Int gridPos = availablePositions[idx];
        availablePositions.RemoveAt(idx);

        Cell cell = CreateCellAt(gridPos, pokemon);
        _tiles[gridPos.x, gridPos.y].IsOccupied = true;
        _tiles[gridPos.x, gridPos.y].Cell = cell;
    }

    private Cell CreateCellAt(Vector2Int gridPos, Pokemon pokemon)
    {
        Vector3 worldPos = GridToWorld(gridPos.x, gridPos.y);
        Cell cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);

        cell.Initialise(pokemon, gridPos);
        cell.Tile = _tiles[gridPos.x, gridPos.y];

        return cell;
    }

    private void ClearBoard()
    {
        _firstSelected = null;
        _secondSelected = null;

        foreach (Cell cell in GetComponentsInChildren<Cell>())
            Destroy(cell.gameObject);

        if (_tiles == null) return;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y]?.Clear();
    }

    // -------------------------------------------------------------------------
    // Selection & matching
    // -------------------------------------------------------------------------

    private void SelectFirst(Cell cell)
    {
        PlaySfx(clickSfx);
        _firstSelected = cell;
        _firstSelected.Select();
    }

    private void DeselectFirst()
    {
        PlaySfx(clickSfx);
        PlaySfx(failSfx);
        _firstSelected.Deselect();
        _firstSelected = null;
    }

    private void SelectSecond(Cell cell)
    {
        _secondSelected = cell;
        _secondSelected.Select();
    }

    private void ClearSelection()
    {
        _firstSelected?.Deselect();
        _secondSelected?.Deselect();
        _firstSelected = null;
        _secondSelected = null;
    }

    private void TryMatch()
    {
        if (_firstSelected.Pokemon != _secondSelected.Pokemon)
        {
            PlaySfx(failSfx);
            ClearSelection();
            return;
        }

        List<Vector2Int> path = BFS.FindPath(_tiles, _firstSelected.GridPosition, _secondSelected.GridPosition);

        if (path == null)
        {
            PlaySfx(failSfx);
            ClearSelection();
            return;
        }

        _isProcessingMatch = true;
        DrawPath(path);
        PlaySfx(matchSfx);
        GameManager.Instance.AddScore(1);

        StartCoroutine(ProcessMatchAfterDelay(DeleteDelay, _firstSelected, _secondSelected));
    }

    // -------------------------------------------------------------------------
    // Post-match board updates
    // -------------------------------------------------------------------------

    private void ApplyBoardUpdate(LevelType levelType, Vector2Int posA, Vector2Int posB)
    {
        switch (levelType)
        {
            case LevelType.Gravity:
                ApplyGravityToColumn(posA);
                ApplyGravityToColumn(posB);
                break;

            case LevelType.Spin:
                ApplySpinShift(CardinalDirections[Random.Range(0, CardinalDirections.Length)]);
                break;

            case LevelType.Normal:
            default:
                break;
        }
    }

    private void ApplyGravityToColumn(Vector2Int pos)
    {
        for (int y = 1; y < height - 1; y++)
        {
            if (_tiles[pos.x, y].Cell != null) continue;

            for (int above = y + 1; above < height - 1; above++)
            {
                if (_tiles[pos.x, above].Cell == null) continue;

                Cell fallingCell = _tiles[pos.x, above].Cell;

                _tiles[pos.x, y].Cell = fallingCell;
                _tiles[pos.x, above].Cell = null;
                _tiles[pos.x, y].IsOccupied = true;
                _tiles[pos.x, above].IsOccupied = false;

                fallingCell.GridPosition = new Vector2Int(pos.x, y);
                fallingCell.transform.position = GridToWorld(pos.x, y);
                fallingCell.Tile = _tiles[pos.x, y];
                break;
            }
        }
    }

    private void ApplySpinShift(Vector2Int shiftDir)
    {
        int startX = shiftDir.x > 0 ? width - 2 : 1;
        int endX = shiftDir.x > 0 ? 0 : width - 1;
        int stepX = shiftDir.x > 0 ? -1 : 1;

        int startY = shiftDir.y > 0 ? height - 2 : 1;
        int endY = shiftDir.y > 0 ? 0 : height - 1;
        int stepY = shiftDir.y > 0 ? -1 : 1;

        for (int x = startX; x != endX; x += stepX)
        {
            for (int y = startY; y != endY; y += stepY)
            {
                if (_tiles[x, y].Cell != null) continue;

                int nx = x - shiftDir.x;
                int ny = y - shiftDir.y;

                while (IsInterior(nx, ny))
                {
                    if (_tiles[nx, ny].Cell != null)
                    {
                        Cell shiftingCell = _tiles[nx, ny].Cell;

                        _tiles[x, y].Cell = shiftingCell;
                        _tiles[nx, ny].Cell = null;
                        _tiles[x, y].IsOccupied = true;
                        _tiles[nx, ny].IsOccupied = false;

                        shiftingCell.GridPosition = new Vector2Int(x, y);
                        shiftingCell.transform.position = GridToWorld(x, y);
                        shiftingCell.Tile = _tiles[x, y];
                        break;
                    }

                    nx -= shiftDir.x;
                    ny -= shiftDir.y;
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Cell manipulation helpers
    // -------------------------------------------------------------------------

    private void RemoveCell(Cell cell)
    {
        _tiles[cell.GridPosition.x, cell.GridPosition.y].Clear();
        Destroy(cell.gameObject);
    }

    private void SwapCells(Cell a, Cell b)
    {
        (a.Tile, b.Tile) = (b.Tile, a.Tile);
        (a.transform.position, b.transform.position) = (b.transform.position, a.transform.position);
        (a.GridPosition, b.GridPosition) = (b.GridPosition, a.GridPosition);
        (a.Tile.Cell, b.Tile.Cell) = (b.Tile.Cell, a.Tile.Cell);
    }

    // -------------------------------------------------------------------------
    // Path visualisation
    // -------------------------------------------------------------------------

    private void DrawPath(IEnumerable<Vector2Int> path)
    {
        _linePoints.Clear();
        foreach (Vector2Int p in path)
            _linePoints.Add(GridToWorld(p.x, p.y));

        pathLine.positionCount = _linePoints.Count;
        pathLine.SetPositions(_linePoints.ToArray());

        StartCoroutine(ClearPathAfter(PathDisplayDuration));
    }

    // -------------------------------------------------------------------------
    // Win / lose checks
    // -------------------------------------------------------------------------

    private bool IsBoardCleared() => GetComponentInChildren<Cell>() == null;

    private bool HasAvailablePairs()
    {
        for (int x1 = 1; x1 < width - 1; x1++)
            for (int y1 = 1; y1 < height - 1; y1++)
            {
                Tile a = _tiles[x1, y1];
                if (!a.IsOccupied || a.Cell == null) continue;

                for (int x2 = 1; x2 < width - 1; x2++)
                    for (int y2 = 1; y2 < height - 1; y2++)
                    {
                        if (x1 == x2 && y1 == y2) continue;

                        Tile b = _tiles[x2, y2];
                        if (!b.IsOccupied || b.Cell == null) continue;
                        if (a.Cell.Pokemon != b.Cell.Pokemon) continue;

                        if (BFS.FindPath(_tiles, a.Cell.GridPosition, b.Cell.GridPosition) != null)
                            return true;
                    }
            }

        return false;
    }

    // -------------------------------------------------------------------------
    // Utility
    // -------------------------------------------------------------------------

    private Vector3 GridToWorld(int x, int y) =>
        new(x + _offsetX + x * cellPadding,
            y + _offsetY + y * cellPadding,
            0f);

    private bool IsInterior(int x, int y) =>
        x >= 1 && x < width - 1 && y >= 1 && y < height - 1;

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
            SoundManagerSO.Instance.PlaySoundFX(clip, transform.position, GameManager.Instance.Volume);
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator ProcessMatchAfterDelay(float delay, Cell first, Cell second)
    {
        Vector2Int posA = first.GridPosition;
        Vector2Int posB = second.GridPosition;

        yield return new WaitForSeconds(delay - 0.02f);

        first.Deselect();
        second.Deselect();
        RemoveCell(first);
        RemoveCell(second);
        _firstSelected = null;
        _secondSelected = null;

        yield return new WaitForSeconds(0.02f);

        _isProcessingMatch = false;

        ApplyBoardUpdate(_currentLevelType, posA, posB);

        if (IsBoardCleared())
        {
            Debug.Log("[Board] Level complete!");
            _currentLevelType = GameManager.Instance.AdvanceToNextLevel();
            InitialiseBoard();
        }
    }

    private IEnumerator ClearPathAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        pathLine.positionCount = 0;
    }

    private IEnumerator ResetShuffleFlagAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isShuffling = false;
    }
}