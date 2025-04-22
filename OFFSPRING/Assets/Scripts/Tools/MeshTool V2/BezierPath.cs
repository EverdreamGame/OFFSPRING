using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
public class BezierPath : MonoBehaviour
{
    public List<BezierSegment> segments = new List<BezierSegment>();
    public event Action OnPathChanged;

    public void AddSegment(Vector3 newAnchor)
    {
        if (segments.Count == 0)
        {
            Vector3 start = newAnchor;
            segments.Add(new BezierSegment(start, start + Vector3.right, start + Vector3.right * 2, newAnchor));
        }
        else
        {
            var last = segments[segments.Count - 1];
            Vector3 start = last.anchor2;
            Vector3 control1 = start + (last.anchor2 - last.control2);
            Vector3 control2 = newAnchor - Vector3.right;
            segments.Add(new BezierSegment(start, control1, control2, newAnchor));
        }
        OnPathChanged?.Invoke();
    }

    private void OnValidate()
    {
        OnPathChanged?.Invoke();
    }

    private void Update()
    {
        // If user moves handles manually in Scene View
        OnPathChanged?.Invoke();
    }
}
#endif