using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SanityEffects : MonoBehaviour
{
    public Volume volume;
    Vignette vignette;
    ChromaticAberration chromatic;
    ColorAdjustments colorAdjust;

    public float sanityPercent = 1f;
    private GameManager gm;

    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out chromatic);
        volume.profile.TryGet(out colorAdjust);
    }

    void Update()
    {
        if (gm != null && gm.isExploring)
            return;
        float t = 1 - sanityPercent;

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0f, 0.45f, t);

        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(0f, 1f, t);

        if (colorAdjust != null)
            colorAdjust.saturation.value = Mathf.Lerp(0f, -50f, t);
    }
}