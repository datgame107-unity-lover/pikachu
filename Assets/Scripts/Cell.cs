using UnityEngine;

/// <summary>
/// Represents a single tile cell on the board. Handles visual state and forwards
/// click events to the <see cref="Board"/> singleton.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Cell : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Serialized fields
    // -------------------------------------------------------------------------

    [SerializeField] private SpriteRenderer pokemonRenderer;
    [SerializeField] private SpriteRenderer selectionHighlight;

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public Pokemon Pokemon { get; private set; }
    public Vector2Int GridPosition { get; set; }
    public Tile Tile { get; set; }

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    /// <summary>Configures the cell with a Pokémon and its board position.</summary>
    public void Initialise(Pokemon pokemon, Vector2Int gridPosition)
    {
        Pokemon = pokemon;
        GridPosition = gridPosition;

        SetSprite(pokemon.PokemonSprite);
        Deselect();
    }

    // -------------------------------------------------------------------------
    // Visual state
    // -------------------------------------------------------------------------

    /// <summary>Marks this cell as selected (shows highlight).</summary>
    public void Select() => SetHighlight(true);

    /// <summary>Clears the selected state (hides highlight).</summary>
    public void Deselect() => SetHighlight(false);

    /// <summary>Assigns the Pokémon sprite to the renderer.</summary>
    public void SetSprite(Sprite sprite)
    {
        if (pokemonRenderer != null)
            pokemonRenderer.sprite = sprite;
    }

    // -------------------------------------------------------------------------
    // Unity messages
    // -------------------------------------------------------------------------

    private void OnMouseDown() => Board.Instance.OnCellClicked(this);

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void SetHighlight(bool isActive)
    {
        if (selectionHighlight != null)
            selectionHighlight.gameObject.SetActive(isActive);
    }
}