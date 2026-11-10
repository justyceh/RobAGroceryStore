using UnityEngine;

// The base for every tool's effect (Red Bull's speed boost, a thrown banana, a smoke cloud...).
// "abstract" means you never create a plain ItemEffect, only one of its subclasses, like
// BuffEffect. It works like a TypeScript interface that every effect must implement:
// BackpackSelector just calls Use() and doesn't need to know which effect it is.
//
// Effects are ScriptableObject assets (like ItemData), so one "RedBullBuff" asset holds the
// numbers (how fast, how long) and the Red Bull ItemData points at it in the Inspector.
public abstract class ItemEffect : ScriptableObject
{
    // Runs the effect. "user" is the Player GameObject, so an effect can find the components it
    // needs on it (PlayerBuffs, the camera, a throw point...).
    // Returns true if the item was used up, false if it couldn't be used (then it stays in the slot).
    public abstract bool Use(GameObject user);
}
