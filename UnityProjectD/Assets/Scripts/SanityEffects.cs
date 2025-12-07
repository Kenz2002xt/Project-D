using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// This script controls post-processing effects for shader techniques (vignette, chromatic aberration, and color shift)
// based on the player's sanity value. As sanity decreases the visual distortion increases.
// CODE HELP: Unity Learn (Get Started with Post-Processing), Unity Manual (Controlling Effects Using Scripts and Post-processing in the Universal Render Pipeline) 

public class SanityEffects : MonoBehaviour
{
    // Reference to the URP Volume with post-processing overrides
    public Volume volume;
    Vignette vignette;                   // darkens the screen edges as sanity fades
    ChromaticAberration chromatic;       // splits the color channels to distort vision
    ColorAdjustments colorAdjust;        // controls saturation, contrast, and exposure

    // Sanity percentage (0–1) parameter controlled by GameManager
    public float sanityPercent = 1f;
    private GameManager gm;

    void Start()
    {
        // Locates the GameManager 
        gm = FindFirstObjectByType<GameManager>();

        // Extracts the volume effects to be modified 
        // If the effect exists in the profile the TryGet assigns it to the variables
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out chromatic);
        volume.profile.TryGet(out colorAdjust);
    }

    void Update()
    {
        // Stops effects during exploration UI 
        if (gm != null && gm.isExploring)
            return;

        // Convert sanity percentage into a intensity curve
        // t = 0 means sane and t =1 means low sanity (so full effects)
        float t = 1 - sanityPercent;

        // Increase vignette darkness as sanity decreases
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0f, 0.45f, t);

        // Increase chromatic aberration
        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(0f, 1f, t);

        // Reduce saturation
        if (colorAdjust != null)
            colorAdjust.saturation.value = Mathf.Lerp(0f, -50f, t);
    }
}