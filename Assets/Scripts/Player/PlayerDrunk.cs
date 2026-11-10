using UnityEngine;
using UnityEngine.Rendering;            // Volume (works with any Scriptable Render Pipeline)
using UnityEngine.Rendering.Universal;  // URP's post-processing effects, like LensDistortion

// Makes the screen blurry and wobbly while you're drunk.
//
// How it works: a URP Volume is a "layer" of post-processing effects (blur, color tint, distortion...)
// drawn on top of the camera image. Its Weight goes from 0 (no effect) to 1 (full effect), a bit like
// CSS opacity for a filter. The Drunk Volume always exists, but sits at weight 0 until you drink.
// This script fades the weight up, keeps it there for the duration, then fades it back down.
// While it's on, it also sways the Lens Distortion back and forth so the view "wobbles".
public class PlayerDrunk : MonoBehaviour
{
    [Tooltip("The Volume holding the drunk look (blur, distortion, color fringes). Usually the DrunkVolume child of the Player.")]
    [SerializeField] private Volume drunkVolume;

    [Tooltip("Seconds it takes for the blur to fully kick in after drinking.")]
    [SerializeField] private float fadeInTime = 1.5f;

    [Tooltip("Seconds it takes for the blur to fully go away after the effect ends.")]
    [SerializeField] private float fadeOutTime = 3f;

    [Tooltip("How fast the view sways back and forth. Higher = faster, sicker wobble.")]
    [SerializeField] private float wobbleSpeed = 1.2f;

    [Tooltip("How far the center of the distortion drifts from the middle of the screen. 0 = no sway, 0.3 = a lot.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float wobbleAmount = 0.2f;

    [Tooltip("How much the fisheye bulge pulses in and out on top of the profile's own Lens Distortion intensity.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float distortionPulse = 0.15f;

    // The Lens Distortion inside the Volume's profile, if it has one. We change it every frame to make the wobble.
    private LensDistortion lensDistortion;
    private float baseIntensity;

    private float timeLeft;
    private float wobbleTime;

    public bool IsDrunk => timeLeft > 0f;

    private void Awake()
    {
        if (drunkVolume == null)
        {
            Debug.LogError("PlayerDrunk has no Drunk Volume set, so alcohol won't do anything.");
            enabled = false;
            return;
        }

        drunkVolume.weight = 0f;

        // .profile (not .sharedProfile) gives this Volume its own copy of the profile asset,
        // so changing values while playing doesn't permanently edit the asset file on disk.
        // TryGet finds the Lens Distortion effect inside the profile, if one was added.
        if (drunkVolume.profile.TryGet(out lensDistortion))
        {
            baseIntensity = lensDistortion.intensity.value;

            // Each setting in a Volume only applies if its checkbox ("override") is ticked.
            // Tick them from code so the wobble works even if they were left unticked in the Inspector.
            lensDistortion.intensity.overrideState = true;
            lensDistortion.center.overrideState = true;
        }
    }

    // Called by DrunkEffect when you drink. Drinking again resets the timer to the longer of the two.
    public void MakeDrunk(float duration)
    {
        timeLeft = Mathf.Max(timeLeft, duration);
        Debug.Log($"You feel dizzy... ({duration:0.#}s)");
    }

    private void Update()
    {
        if (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                Debug.Log("You sobered up.");
            }
        }

        // MoveTowards slides the weight toward 1 (drunk) or 0 (sober) at a steady speed,
        // so 1 / fadeInTime per second means it takes fadeInTime seconds to go from 0 to 1.
        float target = IsDrunk ? 1f : 0f;
        float fadeTime = IsDrunk ? fadeInTime : fadeOutTime;
        drunkVolume.weight = Mathf.MoveTowards(drunkVolume.weight, target, Time.deltaTime / Mathf.Max(0.01f, fadeTime));

        if (drunkVolume.weight > 0f && lensDistortion != null)
        {
            Wobble();
        }
    }

    private void Wobble()
    {
        wobbleTime += Time.deltaTime * wobbleSpeed;

        // Sine waves at slightly different speeds make the sway loop in a lazy, uneven figure-eight
        // instead of a perfectly regular side-to-side, which feels more "drunk".
        // The center is measured in screen fractions: (0.5, 0.5) is the middle of the screen.
        float x = 0.5f + Mathf.Sin(wobbleTime) * wobbleAmount;
        float y = 0.5f + Mathf.Sin(wobbleTime * 1.3f) * wobbleAmount * 0.6f;
        lensDistortion.center.value = new Vector2(x, y);

        // Intensity must stay between -1 and 1, so clamp it.
        float pulse = Mathf.Sin(wobbleTime * 0.7f) * distortionPulse;
        lensDistortion.intensity.value = Mathf.Clamp(baseIntensity + pulse, -1f, 1f);
    }

    // E.g. caught by a guard: GameManager turns off the player's scripts. Clear the blur right away
    // so the game-over screen (and its buttons) are easy to read.
    private void OnDisable()
    {
        timeLeft = 0f;
        if (drunkVolume != null)
        {
            drunkVolume.weight = 0f;
        }
    }

    // Lets you test the look from the component's ⋮ menu during Play mode, without needing a bottle.
    [ContextMenu("Debug: Get Drunk (10s)")]
    private void DebugGetDrunk()
    {
        MakeDrunk(10f);
    }
}
