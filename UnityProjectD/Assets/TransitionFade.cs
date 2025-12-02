using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        
        yield return StartCoroutine(Fade(1f, 0f));

        yield return new WaitForSeconds(waitAfterFadeIn);

        yield return StartCoroutine(Fade(0f, 1f));

    }

    IEnumerator Fade(float start, float end)
    {
        float t = 0f;
        Color c = fadePanel.color;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            c.a = Mathf.Lerp(start, end, t);
            fadePanel.color = c;
            yield return null;
        }
    }
}