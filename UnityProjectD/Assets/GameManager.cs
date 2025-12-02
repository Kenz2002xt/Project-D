using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public float sanity = 100f;
    public float hunger = 100f;
    public float fire = 100f;

    public float sanityDecay = 0.5f;
    public float hungerDecay = 0.7f;
    public float fireDecay = 1.2f;

    public float sunriseTime = 100f;
    private float currentTime = 0f;

    public Light sunLight;
    public Color nightColor;
    public Color dayColor;
    public float nightIntensity = 0.1f;
    public float dayIntensity = 1.2f;

    public int exploreTimeCost;
    public int exploreSanityCost;
    public int exploreHungerCost;

    public int runTimeCost;
    public int runSanityCost;
    public int fightSanityCost;
    public int fightHungerCost;

    public TextMeshProUGUI exploreCostText;
    public TextMeshProUGUI predatorRunCostText;
    public TextMeshProUGUI predatorFightCostText;

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

    public Material fireMaterial;
    public ParticleSystem fireParticles;
    public Light fireLight;

    private SanityEffects sanityFX;
    public VideoPlayer transitionVideo;
    public VideoPlayer foodTransitionVideo;

    void Start()
    {
        UpdateUI();
        sanityFX = FindFirstObjectByType<SanityEffects>();
    }

    void Update()
    {
        if (isExploring)
        {
            if (sanityFX != null)
                sanityFX.sanityPercent = sanity / 100f; 
            return;
        }

        if (!isAlive || hasWon) return;

        RunTime();
        DecayStats();
        CheckLose();
        CheckWin();
        UpdateUI();
        UpdateDayNightCycle();
        UpdateFireEffects();
    }
    string FormatClock()
    {
        
        float startHour = 1f;

        
        float t = Mathf.Clamp01(currentTime / sunriseTime);
        float hour = Mathf.Lerp(startHour, 6f, t);

        int hourInt = Mathf.FloorToInt(hour);
        int minutes = Mathf.FloorToInt((hour - hourInt) * 60);

        string minuteString = minutes.ToString("00");

        return $"{hourInt}:{minuteString} AM";
    }

    void UpdateFireVisuals()
    {
        float intensity = fire / 100f;
        fireMaterial.SetFloat("_FireIntensity", 1);
        var main = fireParticles.main;
        main.startSize = Mathf.Lerp(0f, 0.3f, intensity);
    }

    void UpdateFireEffects()
    {
        float intensity = fire / 100f;

        fireLight.intensity = Mathf.Lerp(0f, 60f, intensity);
    }

    void UpdateDayNightCycle()
    {
        float t = currentTime / sunriseTime; 

        sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
        sunLight.color = Color.Lerp(nightColor, dayColor, t);
        RenderSettings.ambientLight = Color.Lerp(nightColor, dayColor, t);
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
        if (sanityFX != null)
            sanityFX.sanityPercent = sanity / 100f;
        hungerSlider.value = hunger;
        fireSlider.value = fire;

        timerText.text = FormatClock();
        UpdateFireVisuals();
    }

    public void OpenExploreMenu()
    {
        isExploring = true;
        // generate costs 1–15
        exploreTimeCost = Random.Range(1, 16);
        exploreSanityCost = Random.Range(1, 16);
        exploreHungerCost = Random.Range(1, 16);

        UpdateExploreCostUI();
        explorePanel.SetActive(true);
    }

    void GeneratePredatorCosts()
    {
        runTimeCost = Random.Range(1, 16);
        runSanityCost = Random.Range(1, 16);

        fightSanityCost = Random.Range(1, 16);
        fightHungerCost = Random.Range(1, 16);

        predatorRunCostText.text = $"Time -{runTimeCost} | Sanity -{runSanityCost}";
        predatorFightCostText.text = $"Sanity -{fightSanityCost} | Hunger -{fightHungerCost}";
    }

    void UpdateExploreCostUI()
    {
        exploreCostText.text =
            $"The Price You Pay:\n" + $"Time -{exploreTimeCost} " + $"Sanity -{exploreSanityCost} " + $"Hunger -{exploreHungerCost}";
    }

    void ApplyExploreCosts()
    {
        // Apply time
        currentTime += exploreTimeCost;

        // Apply sanity + hunger
        sanity -= exploreSanityCost;
        hunger -= exploreHungerCost;

        sanity = Mathf.Clamp(sanity, 0, 100);
        hunger = Mathf.Clamp(hunger, 0, 100);
    }

    public void CloseExploreMenu()
    {
        isExploring = false;
        explorePanel.SetActive(false);
    }

    // Called when player presses "Search for Wood"
    public void SearchForWood()
    {
        ApplyExploreCosts();

        // Hide initial panel
        explorePanel.SetActive(false);

        // Show transition screen
        transitionPanel.SetActive(true);

        // Play transition video
        transitionVideo.Play();

        // After delay get results
        Invoke(nameof(GenerateWoodResults), 3f);
    }

    void GenerateWoodResults()
    {
        transitionPanel.SetActive(false);
        resultsPanel.SetActive(true);

        int roll = Random.Range(0, 100);

        if (roll < 40)
        {
            resultsBackground.sprite = nothingSprite;
            WoodresultsText.text = "You found nothing...";
        }
        else if (roll < 75)
        {
            resultsBackground.sprite = stickSprite;
            WoodresultsText.text = "You found a stick (+5 fire)";
            AddWood(5);
        }
        else
        {
            resultsBackground.sprite = logSprite;
            WoodresultsText.text = "You found a log (+20 fire)";
            AddWood(20);
        }
    }

    public void SearchForFood()
    {
        ApplyExploreCosts();
        explorePanel.SetActive(false);
        foodTransitionPanel.SetActive(true);
        foodTransitionVideo.Play();
        StartCoroutine(FoodSearchSequence());
    }

    private System.Collections.IEnumerator FoodSearchSequence()
    {
        yield return new WaitForSeconds(3f);
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
            ShowFoodResults("You found berries (+10 hunger)", berriesSprite);
            hunger += 10;
        }
        // 25% rabbit
        else if (roll < 90)
        {
            ShowFoodResults("You caught a rabbit (+20 hunger)", rabbitSprite);
            hunger += 20;
        }
        // 10% predator (will probably change later)
        else
        {
            predatorPanel.SetActive(true);
            GeneratePredatorCosts();
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
        // apply fight costs
        sanity -= fightSanityCost;
        hunger -= fightHungerCost;

        sanity = Mathf.Clamp(sanity, 0, 100);
        hunger = Mathf.Clamp(hunger, 0, 100);

        predatorPanel.SetActive(false);

        float winChance = (sanity + hunger) / 2f; // 0–100%

        float roll = Random.Range(0, 100f);

        if (roll <= winChance)
        {
            // WIN = large animal
            foodResultsPanel.SetActive(true);
            foodResultsText.text = "You defeated the predator (+40 hunger)";
            foodResultsBackground.sprite = largeAnimalSprite;
            hunger += 40;
        }
        else
        {
            // LOSE = nothing
            foodResultsPanel.SetActive(true);
            foodResultsText.text = "The predator scared you off (+0 hunger)";
            foodResultsBackground.sprite = nothingSprite;
        }
    }

    public void ReturnToCamp()
    {
        currentTime += runTimeCost;
        sanity -= runSanityCost;

        resultsPanel.SetActive(false);
        foodResultsPanel.SetActive(false);
        predatorPanel.SetActive(false);

        isExploring = false;
    }
}