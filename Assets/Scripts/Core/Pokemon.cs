using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single Pokémon type used on the board.
/// Create via <c>Assets → Create → Pokémon</c>.
/// </summary>
[CreateAssetMenu(menuName = "Pokémon/Pokémon Data", fileName = "New Pokémon")]
public class Pokemon : ScriptableObject
{
    [Tooltip("Unique numeric identifier for this Pokémon.")]
    public int Code;

    [Tooltip("The sprite displayed on the board tile.")]
    public Sprite PokemonSprite;
}