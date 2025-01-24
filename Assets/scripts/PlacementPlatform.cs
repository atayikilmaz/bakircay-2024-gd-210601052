using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlacementPlatform : MonoBehaviour
{
    public MovableItem CurrentFruit;
    public int Score = 0;
    private float scoreMultiplier = 1f;
    public TextMeshProUGUI ScoreText;
    public Transform Fruits;
    public GameObject ComplatePanel;
    
    private UIManager uiManager;

    [Header("Particles & Skills")]
    public ParticleSystem matchParticleEffect;
    public ParticleSystem skillParticleEffect;
    
    [Header("Skill Settings")]
    public float multiplierDuration = 15f;
    public float reducerDuration = 10f;
    public float skillCooldown = 30f;
    
    // Skill cooldown tracking
    private float multiplierCooldown = 0f;
    private float reducerCooldown = 0f;
    private float rearrangeCooldown = 0f;
    private float destroyCooldown = 0f;
    
    // UI References
    public Button multiplierButton;
    public Button reducerButton;
    public Button rearrangeButton;
    public Button destroyButton;
    public Image multiplierCooldownImage;
    public Image reducerCooldownImage;
    public Image rearrangeCooldownImage;
    public Image destroyCooldownImage;

    // Cooldown text displays
    public TextMeshProUGUI multiplierCooldownText;
    public TextMeshProUGUI reducerCooldownText;
    public TextMeshProUGUI rearrangeCooldownText;
    public TextMeshProUGUI destroyCooldownText;

    private void Start()
    {
        SetupParticles();
        SetupUI();
    }

    private void SetupParticles()
    {
        if (skillParticleEffect == null)
        {
            GameObject particleObj = new GameObject("SkillParticles");
            particleObj.transform.SetParent(transform);
            
            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var mainModule = ps.main;
            mainModule.duration = 1f;
            mainModule.loop = false;
            mainModule.startLifetime = 1f;
            mainModule.startSpeed = 3f;
            mainModule.startSize = 0.3f;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            mainModule.maxParticles = 20;

            var emissionModule = ps.emission;
            emissionModule.enabled = true;
            emissionModule.rateOverTime = 0;
            emissionModule.SetBurst(0, new ParticleSystem.Burst(0f, 20));

            var shapeModule = ps.shape;
            shapeModule.enabled = true;
            shapeModule.shapeType = ParticleSystemShapeType.Sphere;
            shapeModule.radius = 0.1f;

            skillParticleEffect = ps;
        }
    }

    private void SetupUI()
    {
        // Get UIManager reference
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            GameObject uiManagerObj = new GameObject("UIManager");
            uiManager = uiManagerObj.AddComponent<UIManager>();
            uiManager.platform = this;
        }

        // Setup completion panel
        if (ComplatePanel == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            ComplatePanel = new GameObject("CompletePanel");
            ComplatePanel.transform.SetParent(canvas.transform, false);
            
            Image panelBg = ComplatePanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.9f);
            RectTransform panelRect = panelBg.rectTransform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Add container for content
            GameObject container = new GameObject("Container");
            container.transform.SetParent(ComplatePanel.transform, false);
            
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0.3f);
            containerRect.anchorMax = new Vector2(0.7f, 0.7f);
            containerRect.sizeDelta = Vector2.zero;

            // Add score text
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(container.transform, false);
            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.fontSize = 48;
            scoreText.text = "Level Complete!";
            scoreText.color = Color.white;
            scoreText.alignment = TextAlignmentOptions.Center;

            // Add menu button
            GameObject menuButtonObj = new GameObject("MenuButton");
            menuButtonObj.transform.SetParent(container.transform, false);
            
            Button menuButton = menuButtonObj.AddComponent<Button>();
            Image buttonImage = menuButtonObj.AddComponent<Image>();
            buttonImage.color = Color.blue;
            menuButton.targetGraphic = buttonImage;
            menuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

            RectTransform buttonRect = menuButtonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200, 50);

            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(menuButtonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Main Menu";
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = buttonText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        ComplatePanel.SetActive(false);
    }

    private void Update()
    {
        // Update cooldowns
        if (multiplierCooldown > 0) multiplierCooldown -= Time.deltaTime;
        if (reducerCooldown > 0) reducerCooldown -= Time.deltaTime;
        if (rearrangeCooldown > 0) rearrangeCooldown -= Time.deltaTime;
        if (destroyCooldown > 0) destroyCooldown -= Time.deltaTime;

        // Update UI
        UpdateSkillButtonStates();
    }

    private void UpdateCooldownDisplay(Image cooldownImage, TextMeshProUGUI cooldownText, float cooldown)
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount = cooldown / skillCooldown;

        if (cooldownText != null)
        {
            if (cooldown > 0)
            {
                cooldownText.text = Mathf.Ceil(cooldown).ToString();
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSkillButtonStates()
    {
        // Update button interactability
        multiplierButton.interactable = multiplierCooldown <= 0;
        reducerButton.interactable = reducerCooldown <= 0;
        rearrangeButton.interactable = rearrangeCooldown <= 0;
        destroyButton.interactable = destroyCooldown <= 0;

        // Update cooldown fill amounts and text
        UpdateCooldownDisplay(multiplierCooldownImage, multiplierCooldownText, multiplierCooldown);
        UpdateCooldownDisplay(reducerCooldownImage, reducerCooldownText, reducerCooldown);
        UpdateCooldownDisplay(rearrangeCooldownImage, rearrangeCooldownText, rearrangeCooldown);
        UpdateCooldownDisplay(destroyCooldownImage, destroyCooldownText, destroyCooldown);
    }

    public void ActivateScoreMultiplier()
    {
        if (multiplierCooldown > 0) return;

        StartCoroutine(ScoreMultiplierRoutine());
        multiplierCooldown = skillCooldown;

        if (skillParticleEffect != null)
        {
            var particle = Instantiate(skillParticleEffect, transform.position, Quaternion.identity);
            particle.startColor = Color.yellow;
        }
    }

    private IEnumerator ScoreMultiplierRoutine()
    {
        float originalMultiplier = scoreMultiplier;
        scoreMultiplier = 4f;
        yield return new WaitForSeconds(multiplierDuration);
        scoreMultiplier = originalMultiplier;
    }

    public void ActivateScoreReducer()
    {
        if (reducerCooldown > 0) return;

        StartCoroutine(ScoreReducerRoutine());
        reducerCooldown = skillCooldown;

        if (skillParticleEffect != null)
        {
            var particle = Instantiate(skillParticleEffect, transform.position, Quaternion.identity);
            particle.startColor = Color.red;
        }
    }

    private IEnumerator ScoreReducerRoutine()
    {
        float originalMultiplier = scoreMultiplier;
        scoreMultiplier = 0.5f;
        yield return new WaitForSeconds(reducerDuration);
        scoreMultiplier = originalMultiplier;
    }

    public void ActivateRearrangement()
    {
        if (rearrangeCooldown > 0) return;

        StartCoroutine(RearrangeFruits());
        rearrangeCooldown = skillCooldown;

        if (skillParticleEffect != null)
        {
            var particle = Instantiate(skillParticleEffect, transform.position, Quaternion.identity);
            particle.startColor = Color.green;
        }
    }

    private IEnumerator RearrangeFruits()
    {
        List<MovableItem> fruitsToRearrange = new List<MovableItem>();
        List<Vector3> originalPositions = new List<Vector3>();

        for (int i = 0; i < Fruits.childCount; i++)
        {
            var fruit = Fruits.GetChild(i).GetComponent<MovableItem>();
            if (fruit != null)
            {
                fruitsToRearrange.Add(fruit);
                originalPositions.Add(fruit.transform.position);
            }
        }

        // Shuffle positions
        System.Random rng = new System.Random();
        int n = originalPositions.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Vector3 temp = originalPositions[k];
            originalPositions[k] = originalPositions[n];
            originalPositions[n] = temp;
        }

        // Animate fruits to new positions
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);

            for (int i = 0; i < fruitsToRearrange.Count; i++)
            {
                if (fruitsToRearrange[i] != null)
                {
                    fruitsToRearrange[i].transform.position = Vector3.Lerp(
                        fruitsToRearrange[i].transform.position,
                        originalPositions[i],
                        smoothT
                    );
                }
            }

            yield return null;
        }
    }

    public void ActivateDestroy()
    {
        if (destroyCooldown > 0) return;

        StartCoroutine(DestroyHalfFruits());
        destroyCooldown = skillCooldown;

        if (skillParticleEffect != null)
        {
            var particle = Instantiate(skillParticleEffect, transform.position, Quaternion.identity);
            particle.startColor = Color.magenta;
        }
    }

    private IEnumerator DestroyHalfFruits()
    {
        List<MovableItem> fruits = new List<MovableItem>();
        for (int i = 0; i < Fruits.childCount; i++)
        {
            var fruit = Fruits.GetChild(i).GetComponent<MovableItem>();
            if (fruit != null)
                fruits.Add(fruit);
        }

        int countToDestroy = fruits.Count / 2;
        System.Random rng = new System.Random();
        List<MovableItem> fruitsToDestroy = new List<MovableItem>();

        while (fruits.Count > 0 && countToDestroy > 0)
        {
            int index = rng.Next(fruits.Count);
            MovableItem fruitToDestroy = fruits[index];
            fruits.RemoveAt(index);
            fruitsToDestroy.Add(fruitToDestroy);
            countToDestroy--;
        }

        // Start all animations
        foreach (var fruit in fruitsToDestroy)
        {
            StartCoroutine(fruit.PlayMatchAnimation(fruit.transform));
        }

        // Wait for animations
        yield return new WaitForSeconds(2f);

        // Destroy all selected fruits
        foreach (var fruit in fruitsToDestroy)
        {
            if (fruit != null && fruit.gameObject != null)
            {
                Destroy(fruit.gameObject);
            }
        }

        // Check for level completion
        if (Fruits.childCount <= 1)
        {
            ComplatePanel.SetActive(true);
        }
    }

    public void ResetGame()
    {
        Score = 0;
        scoreMultiplier = 1f;
        ScoreText.text = "Score: " + Score;
        
        // Reset all cooldowns
        multiplierCooldown = 0f;
        reducerCooldown = 0f;
        rearrangeCooldown = 0f;
        destroyCooldown = 0f;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddScore(int amount)
    {
        Score += Mathf.RoundToInt(amount * scoreMultiplier);
        ScoreText.text = $"Score: {Score}";
    }
}
