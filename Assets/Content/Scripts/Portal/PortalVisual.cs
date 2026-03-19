using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PortalVisual : MonoBehaviour
{
    [Header("Rotation")]
    public float innerRotationSpeed = 45f;
    public float outerRotationSpeed = -20f;

    [Header("Pulse")]
    public float pulseSpeed = 1.8f;
    public float pulseAmplitude = 0.06f;

    [Header("Color Shift")]
    [ColorUsage(true, true)]
    public Color colorA = new Color(0.1f, 0.4f, 1.0f, 1f);
    [ColorUsage(true, true)]
    public Color colorB = new Color(0.6f, 0.1f, 1.0f, 1f);

    [Header("References")]
    [Tooltip("Inner disc transform (child)")]
    public Transform innerDisc;
    [Tooltip("Outer ring transform (child)")]
    public Transform outerRing;

    private static readonly int PropColor1 = Shader.PropertyToID("_Color1");
    private static readonly int PropColor2 = Shader.PropertyToID("_Color2");
    private static readonly int PropTwirl = Shader.PropertyToID("_TwirlSpeed");
    private static readonly int PropEdgeGlow = Shader.PropertyToID("_EdgeGlow");

    private Material _mat;
    private Vector3 _baseScale;

    private void Start()
    {
        _mat = GetComponent<Renderer>().material;
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        float t = Time.time;

        if (innerDisc != null)
            innerDisc.Rotate(Vector3.up, innerRotationSpeed * Time.deltaTime, Space.Self);
        if (outerRing != null)
            outerRing.Rotate(Vector3.up, outerRotationSpeed * Time.deltaTime, Space.Self);

        float pulse = 1f + Mathf.Sin(t * pulseSpeed * Mathf.PI * 2f) * pulseAmplitude;
        transform.localScale = _baseScale * pulse;

        if (_mat != null)
        {
            float lerp = (Mathf.Sin(t * 0.5f) + 1f) * 0.5f;
            Color current = Color.Lerp(colorA, colorB, lerp);
            _mat.SetColor(PropColor1, current);
            _mat.SetColor(PropColor2, Color.Lerp(colorB, colorA, lerp));
        }
    }
}