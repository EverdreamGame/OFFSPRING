using UnityEngine;
using TMPro;

[ExecuteAlways] // Para ver como queda sin necesidad de darle al play, Auronplay
[RequireComponent(typeof(TextMeshPro))]
public class TextMeshSurfaceProjector : MonoBehaviour
{
    public LayerMask surfaceLayer;           // Capa donde est�n las superficies
    public Vector3 rayDirection = Vector3.forward; // Direcci�n del raycast desde cada v�rtice
    public float maxRayDistance = 10f;       // M�ximo alcance del raycast
    public float surfaceOffset = 0.01f;      // Separaci�n respecto a la superficie

    private TMP_Text tmpText;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void LateUpdate()
    {
        if (tmpText == null || tmpText.textInfo == null)
            return;

        tmpText.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                Vector3 worldPos = tmpText.transform.TransformPoint(vertices[vertexIndex + j]);

                // Disparar raycast desde el v�rtice hacia la direcci�n deseada
                Ray ray = new Ray(worldPos, rayDirection.normalized);
                if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, surfaceLayer))
                {
                    // Ajustar el v�rtice a la posici�n del impacto m�s un peque�o offset
                    Vector3 projected = hit.point + hit.normal * surfaceOffset;
                    vertices[vertexIndex + j] = tmpText.transform.InverseTransformPoint(projected);
                }
            }
        }

        // Aplicar los cambios a la malla
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            tmpText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
