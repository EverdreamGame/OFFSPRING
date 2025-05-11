using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GlowPulse : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color baseColor = Color.white;   // Color HDR

    [Min(0)] public float minIntensity = 1000f;     // Brillo mínimo
    [Min(0)] public float pulseAmplitude = 9000f;    // Incremento sobre el mínimo
    [Min(1)] public float bpm = 60f;    // Latidos por minuto

    static readonly int EmissID = Shader.PropertyToID("_EmissionColor");

    Renderer _rend;
    MaterialPropertyBlock _mpb;
    float _phase;   // fase acumulada

    void Awake()
    {
        _rend = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        _phase += Time.deltaTime * bpm / 60f;

        float k = (Mathf.Sin(_phase * Mathf.PI * 2f) + 1f) * 0.5f;

        float intensity = minIntensity + k * pulseAmplitude;

        _rend.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissID, baseColor * intensity);
        _rend.SetPropertyBlock(_mpb);
    }
}