#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EditablePlaneMeshForNavMesh))]
public class EditablePlaneMeshForNavMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (EditablePlaneMeshForNavMesh)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Add Vertex"))
        {
            Undo.RecordObject(script, "Add Vertex");
            script.vertices.Add(Vector3.zero);
            script.RecalculateMesh();
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Remove Vertex") && script.vertices.Count > 0)
        {
            Undo.RecordObject(script, "Remove Vertex");
            script.vertices.RemoveAt(script.vertices.Count - 1);
            script.RecalculateMesh();
            EditorUtility.SetDirty(script);
        }
    }

    private void OnSceneGUI()
    {
        var script = (EditablePlaneMeshForNavMesh)target;

        for (int i = 0; i < script.vertices.Count; i++)
        {
            Vector3 worldPos = script.transform.TransformPoint(script.vertices[i]);

            EditorGUI.BeginChangeCheck();

            Handles.color = Color.cyan;
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Move Vertex");
                script.vertices[i] = script.transform.InverseTransformPoint(newWorldPos);
                script.RecalculateMesh();
                EditorUtility.SetDirty(script);
            }
        }
    }
}
#endif