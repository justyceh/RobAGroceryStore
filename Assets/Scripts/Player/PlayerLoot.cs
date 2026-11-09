using System;
using System.Collections.Generic;
using UnityEngine;

// Connects stealing in this level to the backpack that carries over between levels.
// It puts stolen items into the backpack, remembers which ones were taken THIS level
// (so they can be taken back if you're caught), and tells guards when a theft happens.
public class PlayerLoot : MonoBehaviour
{
    // How many items were stolen in this level. ExitZone uses this to decide if you've earned an escape.
    // "get; private set;" means any script can read it, but only this script can change it.
    public int LootCount { get; private set; }

    // Fired at the moment of a theft, so guards and cameras can check whether they saw it.
    public event Action ItemStolen;

    // Fired when you try to steal but there's no room, so the UI can say "Backpack full!".
    public event Action BackpackFull;

    private readonly List<ItemData> stolenThisLevel = new List<ItemData>();

    // The backpack lives in GameSession, not here, because this Player is destroyed when the level changes.
    private Backpack Backpack => GameSession.Instance.Backpack;

    // Returns true if the item went into the backpack. PlayerInteract only removes the item
    // from the scene when this succeeds.
    public bool TrySteal(ItemData item)
    {
        if (!Backpack.TryAdd(item))
        {
            BackpackFull?.Invoke();
            return false;
        }

        stolenThisLevel.Add(item);
        LootCount++;
        Debug.Log($"Stole {item.DisplayName} (${item.SellValue}). Backpack: {Backpack.Items.Count}/{Backpack.Capacity}");

        // "?." only calls Invoke if at least one script has subscribed, otherwise the event is null.
        ItemStolen?.Invoke();
        return true;
    }

    // Called when an item leaves the backpack some other way (e.g. a tool was used), so getting
    // caught later doesn't try to take it back a second time.
    public void ForgetItem(ItemData item)
    {
        stolenThisLevel.Remove(item);
    }

    // Called when you're caught: hand back everything grabbed in this level.
    // Items you brought in from earlier levels stay in the backpack.
    public void LoseThisLevelsLoot()
    {
        foreach (ItemData item in stolenThisLevel)
        {
            Backpack.Remove(item);
        }
        stolenThisLevel.Clear();
        LootCount = 0;
    }
}
