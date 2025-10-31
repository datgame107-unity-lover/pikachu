using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.PlayerSettings;
public class Board : MonoBehaviour
{

    private static readonly Vector2Int[] directions = {
        new (1, 0),   // phải
        new (-1, 0),  // trái
        new (0, 1),   // lên
        new (0, -1)   // xuống
    };

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
    [SerializeField]
    private AudioClip clickSound;
    [SerializeField]
    private AudioClip linkedSound;
    [SerializeField]
    private AudioClip oho;

    private Tile[,] tiles;
    private List<Vector2Int> emptyCells;
    private List<Vector2Int> connectPath;

    private List<Vector3> linePoints = new List<Vector3>();

    private LevelType level;
    private Cell firstSelected;
    private Cell secondSelected;
    private bool isDeleting;
    private bool isShuffling;
    float offsetX;
    float offsetY;
    private void Awake()
    {
        offsetX = -(width - 1) / 2f;
        offsetY = -(height - 1) / 2f;
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
        level = LevelType.Normal;
        NewGame(level);
    }


    public void NewGame(LevelType level)
    {   
        GameManager.Instance.StartGame();
        if (tiles != null)
        {
            DeleteCurrentTiles(tiles);
            firstSelected = null;
        }
        tiles = new Tile[width, height];

        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                tiles[i, j] = new Tile();

        int totalCell = (width - 2) * (height - 2);
        emptyCells = new List<Vector2Int>(totalCell);
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                emptyCells.Add(new Vector2Int(i, j));
            }
        }
     

        while (emptyCells.Count > 1)
        {
            for (int i = 0; i < 2; i++)
            {
                CreatePair();

            }

        }
    }
    public void NewGame()
    {
        GameManager.Instance.StartGame();
        if (tiles != null)
        {
            DeleteCurrentTiles(tiles);
            firstSelected = null;
        }
        tiles = new Tile[width, height];

        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                tiles[i, j] = new Tile();

        int totalCell = (width - 2) * (height - 2);
        emptyCells = new List<Vector2Int>(totalCell);
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                emptyCells.Add(new Vector2Int(i, j));
            }
        }


        while (emptyCells.Count > 1)
        {
            for (int i = 0; i < 2; i++)
            {
                CreatePair();

            }

        }
    }

    private void NextLevel()
    {
        level = GameManager.Instance.NextLevel();
        NewGame(level);
    }
    private bool CheckWin()
    {       
        Cell cell = gameObject.GetComponentInChildren<Cell>();
        if (cell != null)
            return false;
        else
            return true;
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
        if (GameManager.Instance.state == GameState.GameOver)
            return;
        if (isDeleting) return;
        if(cell == null) return;
        if ( secondSelected != null&&(firstSelected != null||firstSelected==null))
        {
            firstSelected.Clear();
            secondSelected.Clear();
            firstSelected = null;
            secondSelected= null;
        }
        if (firstSelected == null)
        {
            PlaySound(clickSound);
            firstSelected = cell;
            firstSelected.Choose();
        }
        else if (cell == firstSelected)
        {
            PlaySound(clickSound);
            PlaySound(oho);

            firstSelected.Clear();
            firstSelected = null;
        }
        else
        {
            SoundManagerSO.Instance.PlaySOundFXClip(clickSound, transform.position, 0.5f);

            secondSelected = cell;
            secondSelected.Choose();
            if (firstSelected.pokemon.Equals(secondSelected.pokemon))
            {
                connectPath = BFS.FindPath(tiles, firstSelected.pos, secondSelected.pos);
                if (connectPath!=null)
                {
                    isDeleting = true;
                    DrawPath(connectPath);
                    PlaySound(linkedSound);
                    StartCoroutine(DeleteCellAfterDelay(0.2f, firstSelected, secondSelected));
                }
            }
            else
            {

                PlaySound(oho);
                firstSelected.Clear();
                secondSelected.Clear();
                firstSelected = null;
                secondSelected = null;
            }
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
    private void DeleteCurrentTiles(Tile[,] tiles)
    {

        Cell[] cellList = GetComponentsInChildren<Cell>();
        foreach (Cell cell in cellList)
            Destroy(cell.gameObject);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].Occupied = false;
                tiles[i, j] = null;

            }
        }

    }
    public void ShuffleTiles()
    {
        if (isShuffling) return;
        isShuffling = true;
        List<Cell> availableCells = new();
        for (int i = 1; i < height - 1; i++)
        {
            for (int j = 1; j < width - 1; j++)
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
    private void UpdateBoard(LevelType levelType, Vector2Int posA, Vector2Int posB)
    {
        switch (levelType)
        {
            case LevelType.Normal:
                break;
            case LevelType.Gravity:
                BoardUpdateByGravity(posA, posB);
                break;
            case LevelType.Spin:
                BoardUpdateBySpinning(directions[Random.Range(0, directions.Length)]);
                break;
        }
    }

    private void BoardUpdateByGravity(Vector2Int posA, Vector2Int posB)
    {
        ColumnUpdateByGravity(posA);
        ColumnUpdateByGravity(posB);

    }
    private void ColumnUpdateByGravity(Vector2Int pos)
    {
    

        // duyệt từ dưới lên (bỏ hàng viền)
        for (int y = 1; y < height - 1; y++)
        {
            // nếu ô này trống
            if (tiles[pos.x, y].cell == null)
            {
                // tìm ô có cell ở trên để rơi xuống
                for (int above = y + 1; above < height - 1; above++)
                {
                    if (tiles[pos.x, above].cell != null)
                    {
                        Cell cell = tiles[pos.x, above].cell;

                        // Cập nhật mảng Tile
                        tiles[pos.x, y].cell = cell;
                        tiles[pos.x, above].cell = null;

                        // Cập nhật trạng thái Occupied
                        tiles[pos.x, y].Occupied = true;
                        tiles[pos.x, above].Occupied = false;

                        // Cập nhật vị trí, pos trong Cell
                        cell.pos = new Vector2Int(pos.x, y);
                        cell.transform.position = new Vector3(
                            pos.x + offsetX + pos.x * cellPadding,
                            y + offsetY + y * cellPadding,
                            0
                        );
                        cell.tile = tiles[pos.x, y];

                        break; // sau khi rơi xong, thoát ra kiểm tra tiếp
                    }
                }
            }
        }
    }

    private void BoardUpdateBySpinning(Vector2Int dir)
    {

        // Xác định hướng duyệt theo chiều rơi
        int startX = dir.x > 0 ? width - 2 : 1;
        int endX = dir.x > 0 ? 0 : width - 1;
        int stepX = dir.x > 0 ? -1 : 1;

        int startY = dir.y > 0 ? height - 2 : 1;
        int endY = dir.y > 0 ? 0 : height - 1;
        int stepY = dir.y > 0 ? -1 : 1;

        for (int i = startX; i != endX; i += stepX) // i là cột
        {
            for (int j = startY; j != endY; j += stepY) // j là hàng
            {
                if (tiles[i, j].cell == null)
                {
                    int nextX = i - dir.x;
                    int nextY = j - dir.y;

                    while (nextX >= 1 && nextX < width - 1 &&
                           nextY >= 1 && nextY < height - 1)
                    {
                        if (tiles[nextX, nextY].cell != null)
                        {
                            Cell cell = tiles[nextX, nextY].cell;

                            // Cập nhật mảng Tile
                            tiles[i, j].cell = cell;
                            tiles[nextX, nextY].cell = null;

                            tiles[i, j].Occupied = true;
                            tiles[nextX, nextY].Occupied = false;

                            // Cập nhật vị trí
                            cell.pos = new Vector2Int(i, j);
                            cell.transform.position = new Vector3(
                                i + offsetX + i * cellPadding,
                                j + offsetY + j * cellPadding,
                                0
                            );
                            cell.tile = tiles[i, j];

                            break;
                        }

                        nextX -= dir.x;
                        nextY -= dir.y;
                    }
                }
            }
        }
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

    #region Enumerator

    private IEnumerator DeleteCellAfterDelay(float delay, Cell firstSelected, Cell secondSelected)
    {
        Vector2Int posA = firstSelected.pos;
        Vector2Int posB = secondSelected.pos;
        yield return new WaitForSeconds(delay - 0.02f);
        firstSelected.Clear();
        secondSelected.Clear();
       
        DeleteCell(firstSelected);
        DeleteCell(secondSelected);
        firstSelected = null;
        secondSelected = null;
        Debug.Log(posA + " " + tiles[posA.x, posA.y].Occupied);
        yield return new WaitForSeconds(0.02f);
        isDeleting = false;

        UpdateBoard(level,posA, posB);
        if(CheckWin())
        {
            print("win");
            NextLevel();

        }
        //Debug.Log(posA + " " + tiles[posA.x, posA.y].Occupied);

    }
    private IEnumerator ClearLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lineRenderer.positionCount = 0;
    }

    private IEnumerator ShuffingEndsIn(float delay)
    {
        yield return new WaitForSeconds(delay);
        isShuffling = false;
    }

    #endregion
    public void PrintTile()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Debug.Log("[" + i + "," + j + "]" + "is" + tiles[i, j].Occupied);
            }
        }
    }
    private void PlaySound(AudioClip audioClip)
    {
        SoundManagerSO.Instance.PlaySOundFXClip(audioClip, transform.position, 0.75f);

    }
}
