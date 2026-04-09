/// <summary>
/// Lightweight data container for a single grid position on the board.
/// Holds occupancy state and a reference to the <see cref="Cell"/> sitting on it.
/// </summary>
public class Tile
{
    public bool IsOccupied { get; set; }
    public Cell Cell { get; set; }

    public Tile()
    {
        IsOccupied = false;
        Cell       = null;
    }

    /// <summary>Resets the tile to its empty, unoccupied state.</summary>
    public void Clear()
    {
        IsOccupied = false;
        Cell       = null;
    }
}