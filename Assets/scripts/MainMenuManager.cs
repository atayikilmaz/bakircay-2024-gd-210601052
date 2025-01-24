using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void Start()
    {
        // Create simple UI if not set in inspector
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create panel for buttons
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.MiddleCenter;

        // Position panel
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(200, 150);
        panelRect.anchoredPosition = Vector2.zero;

        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Fruit Match";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        RectTransform titleRect = titleText.rectTransform;
        titleRect.sizeDelta = new Vector2(200, 50);

        // Create play button
        Button playButton = CreateButton(panel, "Play", Color.green);
        playButton.onClick.AddListener(StartGame);

        // Create quit button
        Button quitButton = CreateButton(panel, "Quit", Color.red);
        quitButton.onClick.AddListener(QuitGame);
    }

    private Button CreateButton(GameObject parent, string text, Color color)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent.transform, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        RectTransform rectTransform = buttonImage.rectTransform;
        rectTransform.sizeDelta = new Vector2(160, 40);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textTransform = tmp.rectTransform;
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.sizeDelta = Vector2.zero;

        return button;
    }
}