using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    private static ScreenFader _instance;
    public static ScreenFader Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ScreenFader");
                _instance = go.AddComponent<ScreenFader>();
                DontDestroyOnLoad(go);
                _instance.Initialize();
            }
            return _instance;
        }
    }
    private Canvas _canvas;
    private Image  _overlay;
    private bool   _initialized = false;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        var overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(_canvas.transform, false);

        _overlay = overlayGo.AddComponent<Image>();
        _overlay.color = new Color(0, 0, 0, 0);

        var rect = _overlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin  = Vector2.zero;
        rect.offsetMax  = Vector2.zero;
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(0f, 1f, duration);
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(1f, 0f, duration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        _overlay.gameObject.SetActive(true);

        float elapsed = 0f;
        Color c = _overlay.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            _overlay.color = c;
            yield return null;
        }
        c.a = to;
        _overlay.color = c;

        if (to <= 0f)
            _overlay.gameObject.SetActive(false);
    }
}
