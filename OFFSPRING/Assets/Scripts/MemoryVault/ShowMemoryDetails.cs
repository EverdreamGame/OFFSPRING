using System;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShowMemoryDetails : MonoBehaviour
{
    public RawImage renderRawImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private GameObject mesh_3D;

    [ReadOnly] public SO_Collectable memorySelected;

    private Vector3 previewPosition = new Vector3(1000, 0, 0);
    private Vector3 cameraOffset = new Vector3(0, 0, 2);
    private Vector3 rotationOffset = new Vector3(0, -90, 0);
    private static float rotationSpeed = 30f;

    private GameObject rotatingAnchor;
    private Camera previewCamera;
    private RenderTexture previewTexture;

    private void OnEnable()
    {
        // Crear anchor rotatorio
        rotatingAnchor = new GameObject("ModelViewRotator");
        rotatingAnchor.transform.position = previewPosition;

        // Asignar capa "ViewMemory"
        SetLayerRecursively(rotatingAnchor, LayerMask.NameToLayer("ViewMemory"));

        // Crear cámara
        GameObject camObj = new GameObject("ModelViewCamera");
        previewCamera = camObj.AddComponent<Camera>();
        camObj.transform.position = previewPosition/* + cameraOffset*/;
        camObj.transform.LookAt(previewPosition);
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.cullingMask = LayerMask.GetMask("ViewMemory");

        // Crear RenderTexture
        previewTexture = new RenderTexture(512, 512, 16);
        previewCamera.targetTexture = previewTexture;
    }

    public void SetMemoryDetails()
    {
        if (memorySelected != null)
        {
            // Instanciar modelo
            mesh_3D = Instantiate(memorySelected.mesh_3D, previewPosition, Quaternion.Euler(rotationOffset), rotatingAnchor.transform);

            // Crear RenderTexture
            previewTexture = new RenderTexture(512, 512, 16);
            previewCamera.targetTexture = previewTexture;

            // Asignar textura a la Raw Image
            renderRawImage.texture = previewTexture;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
