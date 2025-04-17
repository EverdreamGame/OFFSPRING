using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierPath))]
public class BezierPathEditor : Editor
{
    private BezierPath path;

    private void OnEnable()
    {
        path = (BezierPath)target;
    }

    private void OnSceneGUI()
    {
        for (int i = 0; i < path.segments.Count; i++)
        {
            var seg = path.segments[i];

            // Editable handles
            EditorGUI.BeginChangeCheck();

            Vector3 a1 = Handles.PositionHandle(seg.anchor1, Quaternion.identity);
            Vector3 c1 = Handles.PositionHandle(seg.control1, Quaternion.identity);
            Vector3 c2 = Handles.PositionHandle(seg.control2, Quaternion.identity);
            Vector3 a2 = Handles.PositionHandle(seg.anchor2, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(path, "Move Bezier Handle");
                seg.anchor1 = a1;
                seg.control1 = c1;
                seg.control2 = c2;
                seg.anchor2 = a2;
            }

            // Draw curve + handles
            Handles.color = Color.green;
            Handles.DrawLine(a1, c1);
            Handles.DrawLine(a2, c2);

            Handles.color = Color.white;
            Handles.DrawBezier(a1, a2, c1, c2, Color.white, null, 2f);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Add Segment"))
        {
            Vector3 newPoint = path.transform.position + Vector3.forward * (path.segments.Count + 1) * 2f;
            path.AddSegment(newPoint);
        }
    }
}