using UnityEngine;

// A timed boost to the player's own stats, e.g. Red Bull (faster + longer dash) or Jordans (higher jump).
// A multiplier of 1 means "no change", 2 means "double", 0.5 means "half".
// Create one with right-click in the Project window > Create > Heist > Effects > Buff.
[CreateAssetMenu(fileName = "NewBuff", menuName = "Heist/Effects/Buff")]
public class BuffEffect : ItemEffect
{
    [Tooltip("How many seconds the boost lasts. Using the same item again while it's active restarts the timer instead of stacking.")]
    [SerializeField] private float duration = 8f;

    [Tooltip("Multiplies walking and sprinting speed. 1.5 = 50% faster.")]
    [SerializeField] private float speedMultiplier = 1f;

    [Tooltip("Multiplies jump height. 2 = jump twice as high.")]
    [SerializeField] private float jumpMultiplier = 1f;

    [Tooltip("Multiplies how far a dash (Q) goes. 1.5 = 50% farther.")]
    [SerializeField] private float dashMultiplier = 1f;

    // Mathf.Max stops a 0 or negative number typed in the Inspector from freezing or reversing the player.
    public float Duration => Mathf.Max(0.1f, duration);
    public float SpeedMultiplier => Mathf.Max(0.1f, speedMultiplier);
    public float JumpMultiplier => Mathf.Max(0.1f, jumpMultiplier);
    public float DashMultiplier => Mathf.Max(0.1f, dashMultiplier);

    public override bool Use(GameObject user)
    {
        // TryGetComponent is GetComponent that also tells you whether it found one.
        // It's fine here because this only runs once per key press, not every frame.
        if (!user.TryGetComponent(out PlayerBuffs buffs))
        {
            Debug.LogWarning($"{name} needs a PlayerBuffs component on the Player.");
            return false;
        }

        buffs.AddBuff(this);
        return true;
    }
}
