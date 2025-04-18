using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class CurvedText : MonoBehaviour
{
    [Header("Curve Settings")]
    public float radius = 2f;
    [Range(10, 360)]
    public float arcAngle = 180f;
    public float verticalOffset = 0f;
    public bool faceOutward = true;

    private TextMeshPro tmp;
    private float textWidth;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        UpdateTextCurve();
    }

    public void UpdateTextCurve()
    {
        if (tmp == null) return;

        tmp.ForceMeshUpdate();
        textWidth = tmp.preferredWidth;

        if (textWidth <= 0 || tmp.textInfo.characterCount == 0) return;

        // Calculate the total angle each character should cover
        float anglePerChar = (arcAngle * Mathf.Deg2Rad) / Mathf.Max(1, tmp.textInfo.characterCount - 1);

        for (int i = 0; i < tmp.textInfo.characterCount; i++)
        {
            var charInfo = tmp.textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // Calculate position along arc
            float angle = -arcAngle * 0.5f * Mathf.Deg2Rad + (i * anglePerChar);

            // Position in XY plane (vertical arc)
            Vector3 charOffset = new Vector3(
                Mathf.Sin(angle) * radius,
                Mathf.Cos(angle) * radius + verticalOffset,
                0
            );

            // Apply to each vertex of the character
            for (int j = 0; j < 4; j++)
            {
                int vertexIndex = charInfo.vertexIndex + j;
                Vector3 vertexOffset = tmp.textInfo.meshInfo[0].vertices[vertexIndex] - charInfo.bottomLeft;

                if (faceOutward)
                {
                    // Rotate offset to face outward from arc
                    vertexOffset = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg, Vector3.forward) * vertexOffset;
                }

                tmp.textInfo.meshInfo[0].vertices[vertexIndex] = charOffset + vertexOffset;
            }
        }

        tmp.UpdateVertexData();
    }
}