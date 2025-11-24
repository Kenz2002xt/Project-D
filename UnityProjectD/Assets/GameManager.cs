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
    public GameObject transitionPanel;    // placeholder transition 
    public GameObject resultsPanel;       // results screen

    public Image resultsBackground;       // background image that changes
    public Sprite nothingSprite;
    public Sprite stickSprite;
    public Sprite logSprite;
    public TextMeshProUGUI WoodresultsText;

    public GameObject foodTransitionPanel;
    public GameObject foodResultsPanel;
    public Image foodResultsBackground;
    public TextMeshProUGUI foodResultsText;

    public Sprite berriesSprite;
    public Sprite rabbitSprite;
    public Sprite largeAnimalSprite;

    public GameObject predatorPanel;


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

    // Called when player presses "Search for Wood"
    public void SearchForWood()
    {
        // Hide initial panel
        explorePanel.SetActive(false);

        // Show transition screen
        transitionPanel.SetActive(true);

        // After delay get results
        Invoke(nameof(GenerateWoodResults), 2f);
    }

    void GenerateWoodResults()
    {
        transitionPanel.SetActive(false);
        resultsPanel.SetActive(true);

        int roll = Random.Range(0, 100);

        if (roll < 40)
        {
            resultsBackground.sprite = nothingSprite;
            WoodresultsText.text = "You found nothing.";
        }
        else if (roll < 75)
        {
            resultsBackground.sprite = stickSprite;
            WoodresultsText.text = "You found a stick (+5 fire).";
            AddWood(5);
        }
        else
        {
            resultsBackground.sprite = logSprite;
            WoodresultsText.text = "You found a log! (+20 fire)";
            AddWood(20);
        }
    }

    public void SearchForFood()
    {
        explorePanel.SetActive(false);
        foodTransitionPanel.SetActive(true);
        StartCoroutine(FoodSearchSequence());
    }

    private System.Collections.IEnumerator FoodSearchSequence()
    {
        yield return new WaitForSeconds(2f);
        foodTransitionPanel.SetActive(false);

        int roll = Random.Range(0, 100);

        // 30% nothing
        if (roll < 30)
        {
            ShowFoodResults("You found nothing...", nothingSprite);
        }
        // 35% berries
        else if (roll < 65)
        {
            ShowFoodResults("You found berries!", berriesSprite);
            hunger += 10;
        }
        // 25% rabbit
        else if (roll < 90)
        {
            ShowFoodResults("You caught a rabbit!", rabbitSprite);
            hunger += 20;
        }
        // 10% predator (will probably change later)
        else
        {
            predatorPanel.SetActive(true);
        }
    }

    private void ShowFoodResults(string text, Sprite sprite)
    {
        foodResultsPanel.SetActive(true);
        foodResultsBackground.sprite = sprite;
        foodResultsText.text = text;
    }


    public void FightPredator()
    {
        predatorPanel.SetActive(false);

        float winChance = (sanity + hunger) / 2f; // 0–100%

        float roll = Random.Range(0, 100f);

        if (roll <= winChance)
        {
            // WIN = large animal
            foodResultsPanel.SetActive(true);
            foodResultsText.text = "You defeated the predator and got a large animal!";
            foodResultsBackground.sprite = largeAnimalSprite;
            hunger += 40;
        }
        else
        {
            // LOSE = nothing
            foodResultsPanel.SetActive(true);
            foodResultsText.text = "The predator scared you off — you found nothing.";
            foodResultsBackground.sprite = nothingSprite;
        }
    }

    public void ReturnToCamp()
    {
        resultsPanel.SetActive(false);
        foodResultsPanel.SetActive(false);
        predatorPanel.SetActive(false);

        isExploring = false;
    }
}