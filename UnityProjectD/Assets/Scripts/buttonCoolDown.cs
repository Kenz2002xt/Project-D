using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Disables the explore button after its pressed which forces a cooldown before it can be used again.
// Makes sure theres no exploration spam
//CODE HELP: Unity Documentation (Cotoutine, CanvasGroup) and How to Disable a Button in Unity (ZDev-9 Youtube)

public class buttonCoolDown : MonoBehaviour
{
    public Button button;                 // Reference to the explore button
    public float cooldownTime = 8f;       // Seconds the button remains disabled 
    public float disabledAlpha = 0.35f;   // Button transparency while disabled 

    private CanvasGroup canvasGroup;      // Controls button fade and visibility
    private float originalAlpha;          // Stores original transparency so there can be a restore

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalAlpha = canvasGroup.alpha;  // Save original state

        // Listen for button click and start cooldown when clicked
        button.onClick.AddListener(() => StartCoroutine(CooldownRoutine()));
    }

    // Coroutine that handles cooldown logic.
    IEnumerator CooldownRoutine()
    {
        // Disable button
        button.interactable = false;
        canvasGroup.alpha = disabledAlpha;

        // Wait for cooldown duration
        yield return new WaitForSeconds(cooldownTime);

        // Re-enable button
        button.interactable = true;
        canvasGroup.alpha = originalAlpha;
    }
}
