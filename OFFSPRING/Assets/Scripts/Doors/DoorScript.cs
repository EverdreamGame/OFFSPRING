using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public ProceduralCylinder doorMesh;
    [Space]
    public float totalLength;
    public float animationTime;
    public bool isClosed = true;

    public void OpenDoor()
    {
        StartCoroutine(AnimateDoor(totalLength, 1f));
    }

    private IEnumerator AnimateDoor(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < animationTime)
        {
            float t = elapsed / animationTime;
            float value = Mathf.Lerp(from, to, t);
            doorMesh.UpdateMeshIfNeeded(value, totalLength);

            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    public void CloseDoor()
    {
        StartCoroutine(AnimateDoor(1f, totalLength));
    }
}
