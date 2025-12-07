using UnityEngine;

// Handles sanity draining hallucination events and checks if the player responds by whistling.
// If the player reacts in time, then they avoid losing sanity.
// CODE HELP: Timers in Unity (simplysystemsstudios5122 on Youtube) and Unity Documentation (AudioSource.PlayOneShot)


public class SanityWhisperSystem : MonoBehaviour
{
    public GameManager gameManager;         // Reference to main game manager
    public AudioSource audioSource;         // audio source used to play whispers
    public AudioClip[] whisperClips;        // array of whisper sounds to choose from randomly

    public float minDelay = 10f;            // Min seconds between whispers
    public float maxDelay = 25f;            // Max seconds between whispers

    public float reactionTime = 3.0f;       // The time the player has to whistle after hearing a whisper
    public float sanityPenalty = 25f;       // Sanity reduction if player doesn't whisper

    private float whisperTimer = 0f;        // Tracks time since last hallucination
    private float currentDelay;             // Randomly chosen wait time until next hallucination

    private bool whisperActive = false;     // Tracking whether a whisper is currently happening
    private float reactionTimer = 0f;       // Timer that tracks how long the player has to react


    void Start()
    {
        // Picks a random delay for the first whisper
        currentDelay = Random.Range(minDelay, maxDelay);
    }

    void Update()
    {
        if (gameManager.isExploring) return; //stop if exploring
         // Stop behavior if player died or won
        if (!gameManager.isAlive || gameManager.hasWon) return;

      
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // If they whistle during whisper = success
            gameManager.Whistle(whisperActive);

            // Mark that they reacted to the whisper for handling penalty later
            gameManager.whistlePerformed = true;
        }

        // If no hallucination is happening then count down to the next one
        if (!whisperActive)
        {
            RunWhisperCountdown();
        }
        // Otherwise tracking the time the player has to react
        else
        {
            RunReactionWindow();
        }
    }

    void RunWhisperCountdown()
    {
        whisperTimer += Time.deltaTime;
        // If countdown meets the delay then trigger whisper event
        if (whisperTimer >= currentDelay)
        {
            PlayRandomWhisper(); //play auditory hallucination
        }
    }

    void PlayRandomWhisper()
    {
        whisperActive = true;
        reactionTimer = 0f;
        whisperTimer = 0f;

        gameManager.whistlePerformed = false; // Reseting player response state

        currentDelay = Random.Range(minDelay, maxDelay); // The next hallucination time is randomized again

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