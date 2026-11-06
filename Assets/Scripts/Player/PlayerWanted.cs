using System;
using UnityEngine;

// Tracks whether security knows you're a thief. Once a guard (or later a camera) sees you
// steal, you're Wanted for the rest of the run, and any guard who sees you will chase you.
public class PlayerWanted : MonoBehaviour
{
    public bool IsWanted { get; private set; }

    // Fired once, the moment you become Wanted. The WANTED indicator listens for this.
    public event Action BecameWanted;

    public void MakeWanted()
    {
        // Already wanted? Don't fire the event again.
        if (IsWanted)
        {
            return;
        }

        IsWanted = true;
        Debug.Log("You've been spotted stealing! You are now WANTED.");
        BecameWanted?.Invoke();
    }

    // Lets you test the Wanted state from the component's ⋮ menu during Play mode.
    [ContextMenu("Debug: Make Wanted")]
    private void DebugMakeWanted()
    {
        MakeWanted();
    }
}
