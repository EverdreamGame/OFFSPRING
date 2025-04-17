using UnityEngine;

[System.Serializable]
public class BezierSegment
{
    public Vector3 anchor1;
    public Vector3 control1;
    public Vector3 control2;
    public Vector3 anchor2;

    public BezierSegment(Vector3 anchor1, Vector3 control1, Vector3 control2, Vector3 anchor2)
    {
        this.anchor1 = anchor1;
        this.control1 = control1;
        this.control2 = control2;
        this.anchor2 = anchor2;
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
}
