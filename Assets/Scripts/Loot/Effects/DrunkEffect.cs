using UnityEngine;

// Alcohol: makes the player's vision blurry and wobbly for a while (a debuff on yourself).
// This asset only says HOW LONG. What the screen looks like is set up on PlayerDrunk and its Volume.
// Create one with right-click in the Project window > Create > Heist > Effects > Drunk.
[CreateAssetMenu(fileName = "NewDrunk", menuName = "Heist/Effects/Drunk")]
public class DrunkEffect : ItemEffect
{
    [Tooltip("How many seconds your vision stays blurry (not counting the fade in and out). Drinking again while drunk restarts the timer.")]
    [SerializeField] private float duration = 15f;

    public override bool Use(GameObject user)
    {
        if (!user.TryGetComponent(out PlayerDrunk drunk))
        {
            Debug.LogWarning($"{name} needs a PlayerDrunk component on the Player.");
            return false;
        }

        drunk.MakeDrunk(Mathf.Max(0.1f, duration));
        return true;
    }
}
