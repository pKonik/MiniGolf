using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string levelScene = "Assets/Scenes/hoyo01.unity";
    private bool loading;
    private Button playButton;
    private Text buttonLabel;
    private Font font;

    void Awake()
    {
        foreach (var ball in FindObjectsByType<GolfBallController>())
        {
            ball.enabled = false;
            var body = ball.GetComponent<Rigidbody>();
            if (body != null) body.isKinematic = true;
        }
        foreach (var hole in FindObjectsByType<HoleTrigger>()) hole.enabled = false;
        foreach (var follow in FindObjectsByType<GolfCameraFollow>()) follow.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var canvasObject = new GameObject("Pantalla de inicio", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        var shade = Box("Fondo oscuro", canvasObject.transform, new Color(0.025f, 0.09f, 0.07f, 0.70f));
        Stretch(shade.rectTransform);
        var panel = Box("Tarjeta", canvasObject.transform, new Color(0.055f, 0.15f, 0.12f, 0.97f));
        Center(panel.rectTransform, new Vector2(560, 420), Vector2.zero);
        var accent = Box("Acento", panel.transform, new Color(0.72f, 0.92f, 0.37f));
        Center(accent.rectTransform, new Vector2(64, 5), new Vector2(0, 168));
        Label("MINIGOLF", panel.transform, 62, FontStyle.Bold, new Vector2(500, 90), new Vector2(0, 100), Color.white);
        var buttonImage = Box("Jugar", panel.transform, new Color(0.72f, 0.92f, 0.37f));
        Center(buttonImage.rectTransform, new Vector2(320, 74), new Vector2(0, -65));
        playButton = buttonImage.gameObject.AddComponent<Button>();
        playButton.targetGraphic = buttonImage;
        var colors = playButton.colors;
        colors.highlightedColor = new Color(0.88f, 1f, 0.72f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.65f, 0.80f, 0.50f);
        playButton.colors = colors;
        buttonLabel = Label("JUGAR", buttonImage.transform, 28, FontStyle.Bold, new Vector2(300, 65), Vector2.zero, new Color(0.08f, 0.18f, 0.10f));
        playButton.onClick.AddListener(Play);
        if (EventSystem.current == null)
        {
            var events = new GameObject("Menu EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
        }
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }

    public void Play()
    {
        if (loading) return;
        if (!Application.CanStreamedLevelBeLoaded(levelScene))
        { Debug.LogError("No se puede cargar el nivel: " + levelScene); return; }
        loading = true;
        playButton.interactable = false;
        buttonLabel.text = "CARGANDO...";
        StartCoroutine(LoadLevel());
    }
    IEnumerator LoadLevel()
    {
        yield return null;
        SceneManager.LoadSceneAsync(levelScene, LoadSceneMode.Single);
    }
    Image Box(string name, Transform parent, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var img = obj.GetComponent<Image>(); img.color = color; return img;
    }
    Text Label(string value, Transform parent, int size, FontStyle style, Vector2 dimensions, Vector2 position, Color color)
    {
        var obj = new GameObject(value, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        var label = obj.GetComponent<Text>();
        label.font = font; label.text = value; label.fontSize = size; label.fontStyle = style;
        label.color = color; label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false;
        Center(label.rectTransform, dimensions, position); return label;
    }
    static void Center(RectTransform rect, Vector2 size, Vector2 position)
    { rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = size; rect.anchoredPosition = position; }
    static void Stretch(RectTransform rect)
    { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
}


