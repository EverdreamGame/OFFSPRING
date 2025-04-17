using UnityEngine;
using UnityEditor;
using System.IO;

public class SVGImporterWindow : EditorWindow
{
    private TextAsset svgFile;
    private float extrudeAmount = 0f;

    [MenuItem("Tools/SVG Importer")]
    public static void ShowWindow()
    {
        GetWindow<SVGImporterWindow>("SVG Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("SVG File Import", EditorStyles.boldLabel);
        svgFile = (TextAsset)EditorGUILayout.ObjectField("SVG File", svgFile, typeof(TextAsset), false);

        extrudeAmount = EditorGUILayout.FloatField("Extrude Amount", extrudeAmount);

        if (GUILayout.Button("Generate Mesh"))
        {
            if (svgFile != null)
            {
                GenerateMeshFromSVG();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select an SVG file first", "OK");
            }
        }
    }

    private void GenerateMeshFromSVG()
    {
        string fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(svgFile));
        SVGReader svgReader = new SVGReader(svgFile);
        svgReader.extrudeAmount = extrudeAmount;
        svgReader.export(fileName);
    }
}