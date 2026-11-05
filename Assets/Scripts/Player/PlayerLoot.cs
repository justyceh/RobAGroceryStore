using System;
using UnityEngine;

// Keeps track of how much the player has stolen.
// Kept separate from PlayerInteract so other scripts (the on-screen counter, and later the
// win check at the exit) can read the count without caring how items were picked up.
public class PlayerLoot : MonoBehaviour
{
    // "get; private set;" means any script can read LootCount, but only this script can change it,
    // like exposing a getter without a setter in TypeScript.
    public int LootCount { get; private set; }

    // Fired whenever LootCount changes, passing the new count. Other scripts subscribe with
    // "LootChanged += MyMethod", like addEventListener. PlayerLoot doesn't need to know who's listening.
    public event Action<int> LootChanged;

    public void AddLoot()
    {
        LootCount++;
        Debug.Log($"Stole an item! Loot: {LootCount}");

        // "?." only calls Invoke if at least one script has subscribed, otherwise LootChanged is null.
        LootChanged?.Invoke(LootCount);
    }
}
