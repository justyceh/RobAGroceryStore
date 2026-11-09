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
    public List<string> backpackItemIds = new List<string>();
}
