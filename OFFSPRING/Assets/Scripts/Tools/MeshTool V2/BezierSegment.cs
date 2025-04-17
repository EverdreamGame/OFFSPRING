using UnityEngine;

[System.Serializable]
public class BezierSegment
{
    public Vector3 anchor1;
    public Vector3 control1;
    public Vector3 control2;
    public Vector3 anchor2;

    public float width1 = 0.5f;
    public float width2 = 0.5f;

    public BezierSegment(Vector3 a1, Vector3 c1, Vector3 c2, Vector3 a2)
    {
        anchor1 = a1;
        control1 = c1;
        control2 = c2;
        anchor2 = a2;

        width1 = 1f;
        width2 = 1f;
    }

    public Vector3 GetPoint(float t)
    {
        float u = 1 - t;
        return u * u * u * anchor1 +
               3 * u * u * t * control1 +
               3 * u * t * t * control2 +
               t * t * t * anchor2;
    }

    public Vector3 GetTangent(float t)
    {
        float u = 1 - t;
        return 3 * u * u * (control1 - anchor1) +
               6 * u * t * (control2 - control1) +
               3 * t * t * (anchor2 - control2);
    }

    public float GetWidth(float t)
    {
        return Mathf.Lerp(width1, width2, t);
    }
}
