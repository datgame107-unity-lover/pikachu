using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Cell : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer pokemonSpite;
    [SerializeField]
    private SpriteRenderer frontground;
    public Pokemon pokemon { get;  set; }
    public Vector2Int pos { get;  set; }
    public Tile tile;

    public Cell(Pokemon pokemon, Vector2Int pos)
    {
        this.pokemon = pokemon;
        this.pos = pos;
        this.pokemonSpite.sprite = pokemon.pokemonSprite;
    }
    
    private void OnMouseDown()
    {
        Board.Instance.FindConnectedPath(this);
    }
    public void Clear()
    {
        ChangeFrontgroundState(false);
    }
    public void Choose()
    {
        ChangeFrontgroundState(true);

    }
    public void SetPokemonSprite(Sprite sprite)
    {
        this.pokemonSpite.sprite = sprite;
    }
    public void ChangeFrontgroundState(bool state)
    {
        frontground.gameObject.SetActive(state);
    }
}
