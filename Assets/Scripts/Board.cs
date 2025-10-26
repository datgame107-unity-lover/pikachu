using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class Board : MonoBehaviour
{
    public static Board Instance;
    public int width = 8;
    public int height = 8;
    [SerializeField]
    private List<Pokemon> pokemons;
    public Cell cellPrefab;
    private Tile[,] tiles;
    private List<Vector2Int> emptyCells;
    private Tile firstSelected;
    private List<Vector2Int> connectPath;
    public LineRenderer lineRenderer; // Gắn từ Inspector
    private List<Vector3> linePoints = new List<Vector3>();

    private bool isDeleting;
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


    private void NewGame()
    {
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

        while (emptyCells.Count > 0)
        {
            for (int i = 0; i < 2; i++)
            {

                Vector2Int cellPos = emptyCells[Random.Range(0, emptyCells.Count)];
                emptyCells.Remove(cellPos);
                Pokemon pokemon = pokemons[Random.Range(0, pokemons.Count)];
                Cell cell = CreateCell(cellPos.x, cellPos.y, pokemon);
                tiles[cellPos.x, cellPos.y].Occupied = true;
                tiles[cellPos.x, cellPos.y].cell = cell;
            }

        }
    }

    public void FindConnectedPath(Cell cell)
    {
        if (isDeleting)
            return;
        if(cell == null)
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
                }
                else
                {
                    Debug.Log("❌ Không tìm thấy đường nối hợp lệ.");
                }
            }

            // Xóa chọn
            cell.Clear();
            firstSelected.cell.Clear();
            firstSelected = null;
        }
    }
    private Cell CreateCell(int x, int y, Pokemon pokemon)
    {
        float offsetX = -(width - 1) / 2f;
        float offsetY = -(height - 1) / 2f;

        Vector3 pos = new Vector3(x + offsetX, y + offsetY, 0);

        Cell cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
        cell.pokemon = pokemon;
        cell.SetPokemonSprite(pokemon.pokemonSprite);
        cell.pos = new Vector2Int(x, y);
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
            Vector3 worldPos = new Vector3(p.x + offsetX, p.y + offsetY, 0);
            linePoints.Add(worldPos);
        }

        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());

        StartCoroutine(ClearLineAfterDelay(0.25f));
    }
    private IEnumerator ClearLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lineRenderer.positionCount = 0;
    }
    private void DeleteCell(Cell cell)
    {   
        cell.tile.Occupied = false;
        Destroy(cell.gameObject);
        
    }
    private IEnumerator DeleteCellAfterDelay(float delay, Cell cell, Cell firstSelected)
    {
        yield return new WaitForSeconds(delay-0.02f);
        DeleteCell(cell);
        DeleteCell(firstSelected);
        yield return new WaitForSeconds(0.02f);
        isDeleting = false;
    }
}
