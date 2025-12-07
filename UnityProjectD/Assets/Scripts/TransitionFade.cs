using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Simple fade panel logic for transition video
// Logic repurposed from Project A and Project B
// Used with reference to "Ending the Game" tutorial by Unity Learn

public class TransitionFade : MonoBehaviour
{
    public Image fadePanel;      
    public float fadeDuration = 1f;
    public float waitAfterFadeIn = 0.5f;

    void OnEnable()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {

        // Fade from fully opaque (1f) to transparent (0f)
        yield return StartCoroutine(Fade(1f, 0f));

        // Wait with full visibility before fading back out
        yield return new WaitForSeconds(waitAfterFadeIn);

        // Fade from transparent (0f) to fully opaque (1f)
        yield return StartCoroutine(Fade(0f, 1f));

    }

    IEnumerator Fade(float start, float end)
    {
        float t = 0f; // timer used for the lerping progress
        Color c = fadePanel.color; // color of the panel taken so the alpha can be modified

        // gradually adjusting transparency over the duration
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration; //increasing the time based on the duration
            c.a = Mathf.Lerp(start, end, t); // adjusting the transparency smoothly
            fadePanel.color = c;
            // Wait a frame before continuing the loop
            yield return null;
        }
    }
}