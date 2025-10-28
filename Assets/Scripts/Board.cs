using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
public class Board : MonoBehaviour
{
    public static Board Instance;
    [SerializeField]
    private int width = 8;
    [SerializeField]
    private int height = 8;
    [SerializeField]
    private List<Pokemon> pokemons;
    [SerializeField]
    private Cell cellPrefab;
    [SerializeField]
    public LineRenderer lineRenderer;
    [SerializeField, Min(0.05f)]

    private float cellPadding = 0.05f;
    private Tile[,] tiles;
    private List<Vector2Int> emptyCells;
    private Tile firstSelected;
    private List<Vector2Int> connectPath;

    private List<Vector3> linePoints = new List<Vector3>();
    private bool isDeleting;
    private bool isShuffling;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        NewGame();
    }


    public void NewGame()
    {
        if (tiles != null)
        {
            DeleteCurrentTiles(tiles);
        }
        tiles = new Tile[width, height];
        int totalCell = (width - 2) * (height - 2);
        emptyCells = new List<Vector2Int>(totalCell);
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                emptyCells.Add(new Vector2Int(i, j));
            }
        }
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                tiles[i, j] = new Tile();

        while (emptyCells.Count > 1)
        {
            for (int i = 0; i < 2; i++)
            {
                CreatePair();

            }

        }
    }
    private void DeleteCurrentTiles(Tile[,] tiles)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.Occupied)
                Destroy(tile.cell.gameObject);
        }
        tiles = null;

    }
    private void CreatePair()
    {
        Pokemon pokemon = pokemons[UnityEngine.Random.Range(0, pokemons.Count)];

        CreateCellInfo(pokemon);
        CreateCellInfo(pokemon);
    }
    private void CreateCellInfo(Pokemon pokemon)
    {
        Vector2Int cellPos = emptyCells[Random.Range(0, emptyCells.Count)];
        emptyCells.Remove(cellPos);
        Cell cell = CreateCell(cellPos.x, cellPos.y, pokemon);
        tiles[cellPos.x, cellPos.y].Occupied = true;
        tiles[cellPos.x, cellPos.y].cell = cell;
    }
    public void FindConnectedPath(Cell cell)
    {

        if (isDeleting)
            return;
        if (cell == null)
            return;
        if (firstSelected == null)
        {
            firstSelected = cell.tile;
            cell.Choose();
            return;
        }

        if (cell.tile == firstSelected)
        {
            cell.Clear();
            firstSelected = null;
        }
        else
        {
            cell.Choose();

            if (cell.pokemon == firstSelected.cell.pokemon)
            {
                connectPath = BFS.FindPath(tiles, firstSelected.cell.pos, cell.pos);

                if (connectPath != null)
                {
                    isDeleting = true;
                    DrawPath(connectPath);
                    Debug.Log($"✅ Đường hợp lệ với {connectPath.Count} điểm!");
                    StartCoroutine(DeleteCellAfterDelay(0.3f, cell, firstSelected.cell));
                    GameManager.Instance.AddScore(1);

                }
                else
                {
                    Debug.Log("❌ Không tìm thấy đường nối hợp lệ.");
                }
            }

            cell.Clear();
            firstSelected.cell.Clear();
            firstSelected = null;
        }
    }
    private Cell CreateCell(int x, int y, Pokemon pokemon)
    {
        float offsetX = -(width - 1) / 2f;
        float offsetY = -(height - 1) / 2f;

        Vector3 pos = new Vector3(
            x + offsetX + x * cellPadding,
            y + offsetY + y * cellPadding,
            0
        );

        Cell cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
        cell.pokemon = pokemon;
        cell.SetPokemonSprite(pokemon.pokemonSprite);
        cell.pos = new(x, y);
        cell.tile = tiles[x, y];

        tiles[x, y].cell = cell;

        return cell;
    }


    private void DrawPath(List<Vector2Int> path)
    {
        lineRenderer.positionCount = 0;
        linePoints.Clear();

        foreach (Vector2Int p in path)
        {
            float offsetX = -(width - 1) / 2f;
            float offsetY = -(height - 1) / 2f;
            Vector3 worldPos = new(p.x + offsetX + p.x * cellPadding, p.y + offsetY + p.y * cellPadding, 0);
            linePoints.Add(worldPos);
        }

        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());

        StartCoroutine(ClearLineAfterDelay(0.25f));
    }

    private void DeleteCell(Cell cell)
    {
        cell.tile.Occupied = false;
        tiles[cell.pos.x, cell.pos.y].cell = null;
        Destroy(cell.gameObject);

    }

    public void ShuffleTiles()
    {
        if (isShuffling) return;
        isShuffling = true;
        List<Cell> availableCells = new();
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (tiles[i, j].Occupied && tiles[i, j].cell != null)
                {
                    int posX = Random.Range(1, width - 1);
                    int posY = Random.Range(1, height - 1);

                    var cellA = tiles[i, j].cell;
                    var cellB = tiles[posX, posY].cell;

                    // Kiểm tra null kỹ hơn
                    if (cellB != null && cellA.tile != null && cellB.tile != null)
                    {
                        Swap(cellA, cellB);
                    }
                }
            }

        }
       
        StartCoroutine(ShuffingEndsIn(1f));
        
    }
    private void Swap(Cell cell1, Cell cell2)
    {   
        Debug.Log("Swap" + cell1.pos + " " + cell2.pos);
        (cell2.tile, cell1.tile) = (cell1.tile, cell2.tile);
        (cell1.transform.position, cell2.transform.position) = (cell2.transform.position, cell1.transform.position);
        (cell1.pos, cell2.pos) = (cell2.pos, cell1.pos);
        (cell1.tile.cell, cell2.tile.cell) = (cell2.tile.cell, cell1.tile.cell);
    }
    private bool CheckAvailablePair(Tile[,] tiles)
    {
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);
        print("hehe");
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                Tile a = tiles[i, j];
                if (!a.Occupied || a.cell == null) continue;

                for (int k = 1; k < width - 1; k++)
                {
                    for (int l = 1; l < height - 1; l++)
                    {
                        if (i == k && j == l) continue;

                        Tile b = tiles[k, l];
                        if (!b.Occupied || b.cell == null) continue;

                        // So sánh cùng loại Pokémon
                        if (a.cell.pokemon == b.cell.pokemon)
                        {
                            var path = BFS.FindPath(tiles, a.cell.pos, b.cell.pos);
                            if (path != null)
                            {
                                Debug.Log($"✅ Cặp còn nối được: {a.cell.pokemon} ({i},{j}) ↔ ({k},{l})");
                                return true; // Ngưng ngay khi tìm thấy 1 cặp hợp lệ
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("❌ Không còn cặp nào có thể nối.");
        return false;
    }

    private IEnumerator ClearLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lineRenderer.positionCount = 0;
    }
    private IEnumerator DeleteCellAfterDelay(float delay, Cell cell, Cell firstSelected)
    {
        yield return new WaitForSeconds(delay - 0.02f);
        DeleteCell(cell);
        DeleteCell(firstSelected);
        yield return new WaitForSeconds(0.02f);
        isDeleting = false;

    }
    private IEnumerator ShuffingEndsIn(float delay)
    {
        yield return new WaitForSeconds(delay);
        isShuffling = false;
    }
}
