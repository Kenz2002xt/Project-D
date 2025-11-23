using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float sanity = 100f;
    public float hunger = 100f;
    public float fire = 100f;

    public float sanityDecay = 0.5f;
    public float hungerDecay = 0.7f;
    public float fireDecay = 1.2f;

    public float sunriseTime = 480f;
    private float currentTime = 0f;

    public Slider sanitySlider;
    public Slider hungerSlider;
    public Slider fireSlider;
    public TextMeshProUGUI timerText;

    public bool isAtCampfire = true;
    public bool isAlive = true;
    public bool hasWon = false;

    public bool whistlePerformed = false;
    public AudioClip whistleClip;
    public AudioSource whistleSource;

    public bool isExploring = false;
    public GameObject explorePanel;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (isExploring) return; //pauses everything

        if (!isAlive || hasWon) return;

        RunTime();
        DecayStats();
        CheckLose();
        CheckWin();
        UpdateUI();
    }


    void RunTime()
    {
        currentTime += Time.deltaTime;
    }


    void DecayStats()
    {
        if (isAtCampfire)
        {
            sanity -= sanityDecay * Time.deltaTime;
            hunger -= hungerDecay * Time.deltaTime;
            fire -= fireDecay * Time.deltaTime;
        }
    }

 
    public void Whistle(bool duringWhisper)
    {
        whistlePerformed = true;

        // play whistle audio
        if (whistleSource != null && whistleClip != null)
            whistleSource.PlayOneShot(whistleClip);

        // only restore sanity if done during whisper
        if (duringWhisper)
        {
            sanity += 10f;
            sanity = Mathf.Clamp(sanity, 0, 100);
        }
    }

    public void AddWood(int amount)
    {
        fire += amount;
        fire = Mathf.Clamp(fire, 0, 100);
    }

   
    void CheckWin()
    {
        if (currentTime >= sunriseTime)
        {
            hasWon = true;
            Debug.Log("You survived until sunrise!");
        }
    }

    
    void CheckLose()
    {
        if (sanity <= 0 || hunger <= 0 || fire <= 0)
        {
            isAlive = false;
            Debug.Log("You died.");
        }
    }

 
    void UpdateUI()
    {
        sanitySlider.value = sanity;
        hungerSlider.value = hunger;
        fireSlider.value = fire;

        float timeLeft = sunriseTime - currentTime;
        timerText.text = $"Sunrise in: {Mathf.Ceil(timeLeft)}s";
    }

    public void OpenExploreMenu()
    {
        isExploring = true;
        explorePanel.SetActive(true);
    }

    public void CloseExploreMenu()
    {
        isExploring = false;
        explorePanel.SetActive(false);
    }
}