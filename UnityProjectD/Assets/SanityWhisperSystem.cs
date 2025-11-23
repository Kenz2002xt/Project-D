using UnityEngine;

public class SanityWhisperSystem : MonoBehaviour
{
    public GameManager gameManager;

    public AudioSource audioSource;   
    public AudioClip[] whisperClips;

    public float minDelay = 10f;
    public float maxDelay = 25f;

    public float reactionTime = 3.0f;
    public float sanityPenalty = 25f;

    private float whisperTimer = 0f;
    private float currentDelay;

    private bool whisperActive = false;
    private float reactionTimer = 0f;

    void Start()
    {
        currentDelay = Random.Range(minDelay, maxDelay);
    }

    void Update()
    {
        if (gameManager.isExploring) return; //stop if exploring

        if (!gameManager.isAlive || gameManager.hasWon) return;

      
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // If they whistle during whisper = success
            gameManager.Whistle(whisperActive);

            // Mark that they reacted to the whisper for handling penalty later
            gameManager.whistlePerformed = true;
        }

        // --- Whisper countdown when no whisper playing ---
        if (!whisperActive)
        {
            RunWhisperCountdown();
        }
        else
        {
            RunReactionWindow();
        }
    }

    void RunWhisperCountdown()
    {
        whisperTimer += Time.deltaTime;

        if (whisperTimer >= currentDelay)
        {
            PlayRandomWhisper();
        }
    }

    void PlayRandomWhisper()
    {
        whisperActive = true;
        reactionTimer = 0f;
        whisperTimer = 0f;

        gameManager.whistlePerformed = false;

        currentDelay = Random.Range(minDelay, maxDelay);

        // play whisper audio
        if (whisperClips.Length > 0)
        {
            audioSource.PlayOneShot(whisperClips[Random.Range(0, whisperClips.Length)]);
        }
    }

    void RunReactionWindow()
    {
        reactionTimer += Time.deltaTime;

        // If they reacted correctly (Q pressed)
        if (gameManager.whistlePerformed)
        {
            whisperActive = false;
            return;
        }

        // If they ran out of time
        if (reactionTimer >= reactionTime)
        {
            // penalty
            gameManager.sanity -= sanityPenalty;
            gameManager.sanity = Mathf.Clamp(gameManager.sanity, 0, 100);

            whisperActive = false;
        }
    }
}