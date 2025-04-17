using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public class TextManager : EditorWindow
{
    private Vector2 scrollPos;

    private List<TextMeshPro> textElements = new();
    private List<TextMeshProUGUI> uiTextElements = new();

    [MenuItem("Tools/Text Manager")]
    public static void ShowWindow()
    {
        GetWindow<TextManager>("Text Manager");
    }

    void OnEnable()
    {
        RefreshTextElements();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Refrescar textos"))
        {
            RefreshTextElements();
        }

        GUILayout.Space(10);
        GUILayout.Label("Textos en escena", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (textElements.Count == 0 && uiTextElements.Count == 0)
        {
            GUILayout.Label("No se encontraron textos en la escena.");
        }

        DrawTextList(textElements);
        DrawTextList(uiTextElements);

        EditorGUILayout.EndScrollView();
    }

    void DrawTextList<T>(List<T> texts) where T : TMP_Text
    {
        foreach (var text in texts)
        {
            if (text == null) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("GameObject:", text.gameObject.name);
            if (GUILayout.Button("Ver en escena", GUILayout.Width(90)))
            {
                Selection.activeGameObject = text.gameObject;
                EditorGUIUtility.PingObject(text.gameObject);
                SceneView.lastActiveSceneView.FrameSelected();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            string newText = EditorGUILayout.TextField("Texto:", text.text);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(text, "Editar texto");
                text.text = newText;
                EditorUtility.SetDirty(text);
            }

            EditorGUILayout.EndVertical();
        }
    }

    void RefreshTextElements()
    {
        textElements = new List<TextMeshPro>(FindObjectsByType<TextMeshPro>(FindObjectsSortMode.InstanceID));
        uiTextElements = new List<TextMeshProUGUI>(FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.InstanceID));
    }
}
