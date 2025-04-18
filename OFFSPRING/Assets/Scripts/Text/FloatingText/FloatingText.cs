using TMPro;
using UnityEngine;

[RequireComponent(typeof(CurvedText))]
public class FloatingText : MonoBehaviour
{
    private Transform center;
    private float lifetime;
    private float elapsed;
    private AnimationCurve fadeCurve;
    private Color baseColor;
    private CurvedText curvedText;
    private TextMeshPro textMesh;
    private float floatHeight;
    private float originalFontSize;
    private Vector3 startPosition;
    private Transform cameraTransform;

    private Vector3 direction;

    public void Initialize(Transform origin, AnimationCurve curve, float duration, float floatHeight)
    {
        center = origin;
        fadeCurve = curve;
        lifetime = duration;

        this.floatHeight = floatHeight;

        curvedText = GetComponent<CurvedText>();
        textMesh = GetComponent<TextMeshPro>();
        baseColor = textMesh.color;
        originalFontSize = textMesh.fontSize;
        cameraTransform = Camera.main.transform;

        direction = Quaternion.AngleAxis(Random.Range(-45f, 45f), Vector3.right) * Vector3.up;

        // Start small and grow
        textMesh.fontSize = 0.1f;
        startPosition = center.position + Random.insideUnitSphere * 0.1f;

        transform.position = startPosition;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        // Grow text size
        textMesh.fontSize = Mathf.Lerp(0.1f, originalFontSize, t * 2f);

        // Move upward
        transform.position = startPosition + direction * t * floatHeight;

        // Face the camera while maintaining upward orientation
        if (cameraTransform != null)
        {
            Vector3 lookDirection = transform.position - cameraTransform.position;
            lookDirection.y = 0; // Keep the text upright
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Fade effect
        float alpha = fadeCurve.Evaluate(t);
        textMesh.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        // Update curve (this will maintain the curved shape)
        curvedText.UpdateTextCurve();

        if (t >= 1f)
            Destroy(gameObject);
    }
}