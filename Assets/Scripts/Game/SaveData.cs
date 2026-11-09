using System;
using System.Collections.Generic;

// Everything written to the save file. A plain data class, like a TypeScript interface for
// the JSON. [Serializable] lets Unity's JsonUtility turn it into JSON text and back.
// Fields are public (not [SerializeField] private) because JsonUtility only saves public
// fields of plain classes. Add new fields here as the game grows (unlocked levels, etc.).
[Serializable]
public class SaveData
{
    public int money;

    // One entry per non-empty slot.
    public List<SavedSlot> hotbar = new List<SavedSlot>();
    public List<SavedSlot> backpack = new List<SavedSlot>();

    // The old save format (one ID per item, no stacks). Only read, so saves made before
    // stacking still load; new saves leave it empty.
    public List<string> backpackItemIds = new List<string>();
}

// One saved stack: which slot it was in, which item, and how many.
[Serializable]
public class SavedSlot
{
    public int index;
    public string itemId;
    public int count;
}
