using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.IO;

/*
    GameManager
    This script is the central manager that drives the core survival loop of the game.
    Manages player stats (Sanity, Hunger, Fire), time progression
    from 3am to 6am (sunrise), UI updates, exploration events, resource
    outcomes, predator encounters, and win/lose conditions.

    The player must survive the night by maintaining fire, food,
    and sanity. Exploring the forest risks hallucinations,
    predators, and starvation. Basically a simple random gen loop with shader techniques added.

    The whistle mechanic is treated as a sanity restoration method-
    if used during hallucinations, it restores the sanity slider.


    CODE HELP:
     Code Inspiration and References:
     - Unity Documentation (Random.Range (int or float), Coroutines, Mathf.Clamp, Streaming Assets, and VideoPlayer)
     - BModev Tutorials Youtube (Flexible Loot System in Unity with Random)
     - Brackeys Tutorials Youtube (Basics of Shader Graph)
     - GameDevBill Youtube (Top Fire Shader Graphs for Unity)
     - PingSharp Youtube (Post Processing Scripting Unity Tutorial)
     - Mina Pecheux Youtube (Creating a Day and Night Cycle System [Unity C# Tutorial]

*/


public class GameManager : MonoBehaviour
{
    // Reference to separate scene controller for win/lose transitions.
    public SceneLoader sceneLoader;

    // decay rates applied over time.
    public float sanity = 100f;
    public float hunger = 100f;
    public float fire = 100f;

    public float sanityDecay = 0.18f;
    public float hungerDecay = 0.32f;
    public float fireDecay = 0.40f;

    // Used to prevent long streaks of bad RNG when searching for wood.
    int failedWoodAttempts = 0;

    // Time system (once currentTime hits sunriseTime, player wins).
    public float sunriseTime = 100f;
    private float currentTime = 0f;

    // Light + sky color that shifts as player makes it closer to dawn.
    public Light sunLight;
    public Color nightColor;
    public Color dayColor;
    public float nightIntensity = 0.2f;
    public float dayIntensity = 0.05f;


    // Exploration stat cost values (assigned at runtime for each exploration).
    public int exploreTimeCost;
    public int exploreSanityCost;
    public int exploreHungerCost;

    // Predator encounter costs if the player chooses to run or fight.
    public int runTimeCost;
    public int runSanityCost;
    public int fightSanityCost;
    public int fightHungerCost;

    // UI references for displaying the exploration decisions and costs.
    public TextMeshProUGUI exploreCostText;
    public TextMeshProUGUI predatorRunCostText;
    public TextMeshProUGUI predatorFightCostText;

    // Whistle hint that only appears when sanity gets low for the first time.
    public TextMeshProUGUI hintText;  
    private bool whistleHintShown = false;
    public float sanityTriggerLevel = 65f;

    // UI Sliders and time display.
    public Slider sanitySlider;
    public Slider hungerSlider;
    public Slider fireSlider;
    public TextMeshProUGUI timerText;

    // Bool state checks 
    public bool isAtCampfire = true;
    public bool isAlive = true;
    public bool hasWon = false;
    public bool whistlePerformed = false;

    // Whistle system and sanity restoration during hallucinations.
    public AudioClip whistleClip;
    public AudioSource whistleSource;

    // Exploration panels and result panels.
    public bool isExploring = false;
    public GameObject explorePanel;
    public GameObject transitionPanel;    // transition 
    public GameObject resultsPanel;       // results screen

    // Wood search and result sprites.
    public Image resultsBackground;       // background image that changes
    public Sprite nothingSprite;
    public Sprite stickSprite;
    public Sprite logSprite;
    public TextMeshProUGUI WoodresultsText;

    // Food exploration UI and result sprites.
    public GameObject foodTransitionPanel;
    public GameObject foodResultsPanel;
    public Image foodResultsBackground;
    public TextMeshProUGUI foodResultsText;
    public Sprite berriesSprite;
    public Sprite rabbitSprite;
    public Sprite largeAnimalSprite;

    // Predator event panel.
    public GameObject predatorPanel;

    // Fire shader and particle system thats controlled by player fire stat.
    public Material fireMaterial;
    public ParticleSystem fireParticles;
    public Light fireLight;

    // Visual hallucination/ sanity effect system.
    private SanityEffects sanityFX;
    public ParticleSystem breath;

    // Videos for exploration transitions (also written for WebGL build)
    public VideoPlayer transitionVideo;
    public VideoPlayer foodTransitionVideo;
    public string exploreVideoName = "explorevid.mp4";

    // background music and sound effect audio
    public AudioSource firewhoosh;
    public AudioSource sigh;
    public AudioSource food;
    public AudioSource heart;
    public AudioSource radio;
    private bool radioWasPlaying = false;


    void Start()
    {
        // Initialize sliders and visuals
        UpdateUI();
        sanityFX = FindFirstObjectByType<SanityEffects>();
        // Ambient radio
        radio?.Play();
    }

    void Update()
    {
        // Exploration acts like a temporary pause to normal decay.
        if (isExploring)
        {
            // Exploration turns off radio to increase immersion.
            if (radioWasPlaying)
            {
                radio.Pause();
                radioWasPlaying = false;
            }
            // Pass the sanity percentage to hallucination shader system.
            if (sanityFX != null)
                sanityFX.sanityPercent = sanity / 100f;

            return;
        }
        else
        {
            // Resume radio when player returns to campfire.
            if (!radioWasPlaying)
            {
                radio.Play();
                radioWasPlaying = true;
            }
        }

        // Stop the gameplay logic if game has already been decided
        if (!isAlive || hasWon) return;

        RunTime();              // Clock to sunrise.
        DecayStats();           // Player stat decay over time.
        CheckLose();            // Death conditions.
        CheckWin();             // Survival win.
        UpdateUI();             // Update sliders and time.
        UpdateDayNightCycle();  // Fades to sunrise.
        UpdateFireEffects();    // Fire brightness.

        // Teach whistle mechanic only when sanity has first deteriorated.
        if (!whistleHintShown && sanity < sanityTriggerLevel)
        {
            whistleHintShown = true;
            ShowWhistleHint();
        }
    }

    // Converts in game time to formatted clock (am).
    string FormatClock()
    {
        // Game always starts at 3:00AM.
        float startHour = 3f;

        // t = percentage of night passed.
        // currentTime increases over gameplay and sunriseTime is the total length of night.
        float t = Mathf.Clamp01(currentTime / sunriseTime);

        // Mathf.Lerp smoothly blends between two values.
        // Here it shifts time from 3 to 6 over the full length of the night.
        float hour = Mathf.Lerp(startHour, 6f, t);
        int hourInt = Mathf.FloorToInt(hour); // whole number
        int minutes = Mathf.FloorToInt((hour - hourInt) * 60); // Converts decimal remainder into minutes

        string minuteString = minutes.ToString("00"); // Formats 1 into "01" for a more real clock look

        return $"{hourInt}:{minuteString} AM"; // Final HUD clock text
    }

    // Displays a one time hint to teach player the whistle mechanic.
    void ShowWhistleHint()
    {
        hintText.text = "Press <b>Q</b> to whistle during auditory hallucinations";
        hintText.gameObject.SetActive(true);

        Invoke(nameof(HideHint), 7f); //hide after seven seconds
    }

    void HideHint()
    {
        hintText.gameObject.SetActive(false); // Removes UI from the screen
}

    // Controls shader & particle scaling for campfire.
    void UpdateFireVisuals()
    {
        // Fire stat (0–100) becomes 0–1 percentage.
        float intensity = fire / 100f;

        // Sends a value into the fire shader.
        // _FireIntensity is a property inside the shader graph (emmissions)
        // Increasing this makes flames appear bigger and brighter.
        fireMaterial.SetFloat("_FireIntensity", 1);
        // Accesses the particle system so I can modify flame size in real time.
        var main = fireParticles.main;

        // Lerp makes the flames shrink or grow depending on fire remaining.
        // 0 = no flame and 1 = full flame.
        main.startSize = Mathf.Lerp(0f, 0.3f, intensity);
    }

    // Separate function that targets light intensity response to fire stat smoothly.
    void UpdateFireEffects()
    {
        float intensity = fire / 100f;

        // Smoothly fades the light between no fire to bright campfire glow.
        // 60f is intentionally more dramatic.
        fireLight.intensity = Mathf.Lerp(0f, 60f, intensity);
    }

    // Dynamic fading from night to sunrise.
    void UpdateDayNightCycle()
    {
        float t = currentTime / sunriseTime; // percentage through the night
        // Lerp makes a smooth transition from dark to bright.
        sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
        sunLight.color = Color.Lerp(nightColor, dayColor, t); // Also blends sun color
        // Changes the global ambient light so this is what fills any unlit shadows.
        RenderSettings.ambientLight = Color.Lerp(nightColor, dayColor, t);
    }

    void RunTime()
    {
        // Each second adds Time.deltaTime 
        currentTime += Time.deltaTime;
    }

    // Survival decay over time.
    void DecayStats()
    {
        if (isAtCampfire)
        {
            // All three survival stats shrink over time while at campfire.
            sanity -= sanityDecay * Time.deltaTime;
            hunger -= hungerDecay * Time.deltaTime;
            fire -= fireDecay * Time.deltaTime;
            // Being near the fire slowly stabilizes sanity (but not enough to fully heal decay).
            if (!isExploring)
                sanity += 1f * Time.deltaTime;
        }
        // Prevents stats from exceeding bounds (no negatives or passing max).
        sanity = Mathf.Clamp(sanity, 0, 100); 
}

    // Whistle restores sanity if the player times it during hallucination events.
    public void Whistle(bool duringWhisper)
    {
        whistlePerformed = true;
        breath?.Play(); // UI breathing effect when player is whistling (mimics cold environment)

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

    // Called whenever the player finds wood during exploration. 
    public void AddWood(int amount)
    {
        fire += amount;                      // Add to total fire resource
        fire = Mathf.Clamp(fire, 0, 100);    // Clamp so shader intensity  and fire particles never exceed range
    }

    // If player makes it to sunrise then load win scene.
    void CheckWin()
    {
        if (currentTime >= sunriseTime)
        {
            hasWon = true;
            sceneLoader.OpenWin();
            Debug.Log("You survived until sunrise!");
            return;
        }
    }

    // If any survival stat reaches zero then load game over scene.
    void CheckLose()
    {
        if (sanity <= 0 || hunger <= 0 || fire <= 0)
        {
            isAlive = false;
            sceneLoader.OpenGameOver();
            Debug.Log("You died.");
        }
    }

    // Updates slider UI and fire effects.
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

    // Opens exploration UI and rolls random resource costs for the search.
    public void OpenExploreMenu()
    {
        isExploring = true;
        // generate costs 1–10
        exploreTimeCost = Random.Range(1, 10);
        exploreSanityCost = Random.Range(1, 10);
        exploreHungerCost = Random.Range(1, 10);

        UpdateExploreCostUI();
        explorePanel.SetActive(true);
    }

    // Cost generation for predator encounters (run vs fight).
    void GeneratePredatorCosts()
    {
        runTimeCost = Random.Range(1, 10);
        runSanityCost = Random.Range(1, 10);

        fightSanityCost = Random.Range(1, 10);
        fightHungerCost = Random.Range(1, 10);
        //displays cost
        predatorRunCostText.text = $"Time -{runTimeCost} | Sanity -{runSanityCost}";
        predatorFightCostText.text = $"Sanity -{fightSanityCost} | Hunger -{fightHungerCost}";
    }

    // Displays rolled costs in UI.
    void UpdateExploreCostUI()
    {
        exploreCostText.text =
            $"The Price You Pay:\n" + $"Time -{exploreTimeCost} " + $"Sanity -{exploreSanityCost} " + $"Hunger -{exploreHungerCost}";
    }

    // Apply exploration costs before showing results.
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

    //close the explore to show results or go back to camp
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

        string path = System.IO.Path.Combine(Application.streamingAssetsPath, exploreVideoName); // making file path for the transition vid in StreamingAssets.
        transitionVideo.source = VideoSource.Url; // Video will be loaded as a URL rather than a local VideoClip reference for WebGL.
        transitionVideo.url = path; // Assign the video file path to the player.
        transitionVideo.Play(); // Begin the transition video 
        // After delay get results
        Invoke(nameof(GenerateWoodResults), 3f);
    }

    // Generates wood findings using RNG with the back up mechanic.
    void GenerateWoodResults()
    {
        transitionPanel.SetActive(false);
        resultsPanel.SetActive(true);

        int roll = Random.Range(0, 100);
        // Prevents the player from getting 3 failures in a row, improving pacing.
        if (failedWoodAttempts >= 2)
            roll = 80; // force win

        // roll outcomes
        if (roll < 20) // 20% chance
        {
            failedWoodAttempts++;
            resultsBackground.sprite = nothingSprite; // nothing image
            sigh?.Play(); // sigh sound effect
            WoodresultsText.text = "You found nothing...";
        }
        else if (roll < 70) // 50% chance 
        {
            failedWoodAttempts = 0; // since player got a wood type set the fail to 0
            resultsBackground.sprite = stickSprite; //stick image 
            firewhoosh?.Play(); // play fire sound effect
            WoodresultsText.text = "You found a stick (+15 fire)";
            AddWood(15); // add 15 to fire meter
        }
        else // 30% chance
        {
            failedWoodAttempts = 0; // since player got a wood type set the fail to 0
            resultsBackground.sprite = logSprite; // log image
            firewhoosh?.Play(); // play fire sound effect
            WoodresultsText.text = "You found a log (+30 fire)";
            AddWood(30); // add 30 to fire meter
        }
    }

    public void SearchForFood()
    {
        ApplyExploreCosts(); // Deduct sanity, hunger, and time before result

        explorePanel.SetActive(false); 
        foodTransitionPanel.SetActive(true); 

        string path = System.IO.Path.Combine(Application.streamingAssetsPath, exploreVideoName); // Loading video from StreamingAssets folder.
        foodTransitionVideo.source = VideoSource.Url; 
        foodTransitionVideo.url = path; // Set the URL into the video player.
        foodTransitionVideo.Play(); // Begin transition vid.

        StartCoroutine(FoodSearchSequence()); // Start timed sequence that lets the results start after delay.
    }

    private System.Collections.IEnumerator FoodSearchSequence()
    {
        yield return new WaitForSeconds(3f); // Delay as player searches through the forest.
        foodTransitionPanel.SetActive(false); // Remove the transition screen.

        // Predator chance increases the closer it gets to day.
        // Lerp again smoothly scales the risk as sunrise gets closer.
        float predatorChance = Mathf.Lerp(0f, 35f, currentTime / sunriseTime);

        int roll = Random.Range(0, 100); // Roll RNG 

        // 30% nothing
        if (roll < 18)
        {
            ShowFoodResults("You found nothing...", nothingSprite);
            sigh?.Play();
        }
        // 35% berries
        else if (roll < 58)
        {
            ShowFoodResults("You found berries (+25 hunger)", berriesSprite);
            food?.Play();
            hunger += 25;
        }
        // 25% rabbit
        else if (roll < 88)
        {
            ShowFoodResults("You caught a rabbit (+40 hunger)", rabbitSprite);
            food?.Play();
            hunger += 40;
        }
        // 10% predator (will probably change later)
        else if (roll < 88 + predatorChance) // grows as time passes
        {
            predatorPanel.SetActive(true);
            heart?.Play();
            GeneratePredatorCosts();
        }
    }

    private void ShowFoodResults(string text, Sprite sprite)
    {
        foodResultsPanel.SetActive(true); // Open results UI.
        foodResultsBackground.sprite = sprite; // image for food outcome.
        foodResultsText.text = text; // Text for results/rewards.
    }


    public void FightPredator()
    {
        // apply fight costs
        sanity -= fightSanityCost;
        hunger -= fightHungerCost;

        sanity = Mathf.Clamp(sanity, 0, 100); // Stops stats from going below 0.
        hunger = Mathf.Clamp(hunger, 0, 100); // Same clamping logic for hunger.

        predatorPanel.SetActive(false);

        // Win chance is weighted more by sanity than hunger (done since there is a better chance the players sanity is higher).
        // Player with better survival stats is more likely to win
        float winChance = Mathf.Clamp((sanity * 0.6f + hunger * 0.4f), 0, 100);

        float roll = Random.Range(0, 100f); // Roll for success.

        if (roll <= winChance)
        {
            // WIN = large animal
            foodResultsPanel.SetActive(true);
            foodResultsText.text = "Predator killed (+80 hunger)";
            food?.Play();
            foodResultsBackground.sprite = largeAnimalSprite;
            hunger += 80;
        }
        else
        {
            // LOSE = nothing
            foodResultsPanel.SetActive(true);
            foodResultsText.text = "Predator won (+0 hunger)";
            sigh?.Play();
            foodResultsBackground.sprite = nothingSprite;
        }
    }

    public void ReturnToCamp()
    {
        currentTime += runTimeCost; // returning takes time.
        sanity -= runSanityCost; // returning takes sanity.

        // close any open panels
        resultsPanel.SetActive(false);
        foodResultsPanel.SetActive(false);
        predatorPanel.SetActive(false);

        isExploring = false; // Exploration ends and player returns to campsite.
    }
}