using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public PlacementPlatform platform;
    
    [Header("Button Prefabs")]
    public Color multiplierButtonColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    public Color reducerButtonColor = new Color(1f, 0.2f, 0.2f, 1f);      // Red
    public Color rearrangeButtonColor = new Color(0.2f, 1f, 0.2f, 1f);    // Green
    public Color destroyButtonColor = new Color(1f, 0.2f, 1f, 1f);        // Magenta
    
    void Start()
    {
        CreateUIElements();
    }

    void CreateUIElements()
    {
        // Create Canvas if it doesn't exist
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create buttons container
        GameObject buttonsPanel = new GameObject("ButtonsPanel");
        buttonsPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform buttonsPanelRect = buttonsPanel.AddComponent<RectTransform>();
        buttonsPanelRect.anchorMin = new Vector2(0.5f, 0);
        buttonsPanelRect.anchorMax = new Vector2(0.5f, 0);
        buttonsPanelRect.pivot = new Vector2(0.5f, 0);
        buttonsPanelRect.anchoredPosition = new Vector2(0, 20); // Slightly higher from bottom
        buttonsPanelRect.sizeDelta = new Vector2(500, 80);

        // Add background to make buttons more visible
        Image panelImage = buttonsPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);

        HorizontalLayoutGroup layout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = false;
        layout.childForceExpandWidth = false;

        // Create all buttons in a row
        Button resetButton = CreateSkillButton(buttonsPanel, "Reset", Color.gray, platform.ResetGame);
        platform.multiplierButton = CreateSkillButton(buttonsPanel, "4X", multiplierButtonColor, platform.ActivateScoreMultiplier);
        platform.reducerButton = CreateSkillButton(buttonsPanel, "0.5X", reducerButtonColor, platform.ActivateScoreReducer);
        platform.rearrangeButton = CreateSkillButton(buttonsPanel, "Mix", rearrangeButtonColor, platform.ActivateRearrangement);
        platform.destroyButton = CreateSkillButton(buttonsPanel, "50%", destroyButtonColor, platform.ActivateDestroy);

        // Create cooldown images and text
        platform.multiplierCooldownImage = CreateCooldownImage(platform.multiplierButton.gameObject);
        platform.reducerCooldownImage = CreateCooldownImage(platform.reducerButton.gameObject);
        platform.rearrangeCooldownImage = CreateCooldownImage(platform.rearrangeButton.gameObject);
        platform.destroyCooldownImage = CreateCooldownImage(platform.destroyButton.gameObject);

        platform.multiplierCooldownText = CreateCooldownText(platform.multiplierButton.gameObject);
        platform.reducerCooldownText = CreateCooldownText(platform.reducerButton.gameObject);
        platform.rearrangeCooldownText = CreateCooldownText(platform.rearrangeButton.gameObject);
        platform.destroyCooldownText = CreateCooldownText(platform.destroyButton.gameObject);
    }

    Button CreateSkillButton(GameObject parent, string text, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent.transform, false);

        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(90, 60);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(action);

        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = Color.white;
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textTransform = textObj.GetComponent<RectTransform>();
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.sizeDelta = Vector2.zero;

        return button;
    }

    TextMeshProUGUI CreateCooldownText(GameObject buttonObj)
    {
        GameObject textObj = new GameObject("CooldownText");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textTransform = textObj.AddComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.5f, 0.5f);
        textTransform.anchorMax = new Vector2(0.5f, 0.5f);
        textTransform.pivot = new Vector2(0.5f, 0.5f);
        textTransform.sizeDelta = new Vector2(40, 40);
        textTransform.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.gameObject.SetActive(false);

        return tmp;
    }

    Image CreateCooldownImage(GameObject buttonObj)
    {
        GameObject cooldownObj = new GameObject("CooldownOverlay");
        cooldownObj.transform.SetParent(buttonObj.transform, false);

        RectTransform rectTransform = cooldownObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        Image cooldownImage = cooldownObj.AddComponent<Image>();
        cooldownImage.color = new Color(0, 0, 0, 0.8f);
        cooldownImage.raycastTarget = false; // Prevent blocking button clicks
        cooldownImage.fillMethod = Image.FillMethod.Radial360;
        cooldownImage.fillOrigin = 2; // Top
        cooldownImage.fillAmount = 0;

        return cooldownImage;
    }
}