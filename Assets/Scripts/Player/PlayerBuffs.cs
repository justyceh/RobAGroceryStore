using System;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of the timed boosts currently on the player and counts them down.
// PlayerMovement and PlayerDash read the combined multipliers below every frame.
// Different buffs multiply together (Red Bull 1.5x speed + another 1.2x = 1.8x);
// using the SAME buff again just restarts its timer, so chugging three Red Bulls isn't 3.4x speed.
public class PlayerBuffs : MonoBehaviour
{
    // One running buff: which effect asset it came from and how long it has left.
    private class ActiveBuff
    {
        public BuffEffect Source;
        public float TimeLeft;
    }

    private readonly List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    // All active buffs multiplied together. 1 = no buff. Read-only from other scripts.
    public float SpeedMultiplier { get; private set; } = 1f;
    public float JumpMultiplier { get; private set; } = 1f;
    public float DashMultiplier { get; private set; } = 1f;

    // Fired whenever a buff starts or runs out, so a buff icon UI can redraw later.
    public event Action BuffsChanged;

    public void AddBuff(BuffEffect buff)
    {
        // List.Find returns the first entry that matches, or null. "b => ..." is a lambda, like a JS arrow function.
        ActiveBuff existing = activeBuffs.Find(b => b.Source == buff);
        if (existing != null)
        {
            existing.TimeLeft = buff.Duration;
            Debug.Log($"{buff.name} refreshed: {buff.Duration:0.#}s left.");
        }
        else
        {
            activeBuffs.Add(new ActiveBuff { Source = buff, TimeLeft = buff.Duration });
            Debug.Log($"{buff.name} started for {buff.Duration:0.#}s.");
        }

        Recalculate();
    }

    private void Update()
    {
        bool anyExpired = false;

        // Loop backwards so removing an entry doesn't skip the next one
        // (removing index 2 shifts index 3 down into 2).
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].TimeLeft -= Time.deltaTime;
            if (activeBuffs[i].TimeLeft <= 0f)
            {
                Debug.Log($"{activeBuffs[i].Source.name} wore off.");
                activeBuffs.RemoveAt(i);
                anyExpired = true;
            }
        }

        // Only redo the math when something actually changed, not every frame.
        if (anyExpired)
        {
            Recalculate();
        }
    }

    private void Recalculate()
    {
        SpeedMultiplier = 1f;
        JumpMultiplier = 1f;
        DashMultiplier = 1f;

        foreach (ActiveBuff active in activeBuffs)
        {
            SpeedMultiplier *= active.Source.SpeedMultiplier;
            JumpMultiplier *= active.Source.JumpMultiplier;
            DashMultiplier *= active.Source.DashMultiplier;
        }

        BuffsChanged?.Invoke();
    }
}
