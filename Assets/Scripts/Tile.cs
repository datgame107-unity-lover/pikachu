using UnityEngine;

public class Tile 
{
    public Cell cell;
    public bool Occupied = false;

    public Tile()
    {
        cell = null;
        Occupied = false;
    }
}
