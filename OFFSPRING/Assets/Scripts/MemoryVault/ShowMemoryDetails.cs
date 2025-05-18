using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowMemoryDetails : MonoBehaviour
{
    public Image renderBackgroundImage;
    public RawImage renderRawImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private GameObject mesh_3D;

    [HideInInspector] public SO_Collectable memorySelected;

    private Vector3 previewPosition = new Vector3(1000, 0, 0);
    private Vector3 cameraOffset = new Vector3(0, 0, 2);
    private Vector3 rotationOffset = new Vector3(0, -90, 0);
    private Quaternion currentRotation;
    private static float rotationSpeed = 30f;

    private GameObject rotatingAnchor;
    private Camera previewCamera;
    private RenderTexture previewTexture;

    private void OnEnable()
    {
        // Crear anchor rotatorio
        rotatingAnchor = new GameObject("ModelViewRotator");
        rotatingAnchor.transform.position = previewPosition;

        // Crear cámara
        GameObject camObj = new GameObject("ModelViewCamera");
        previewCamera = camObj.AddComponent<Camera>();
        camObj.transform.position = previewPosition + cameraOffset;
        camObj.transform.LookAt(previewPosition);
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.cullingMask = LayerMask.GetMask("ViewMemory");

        // Crear RenderTexture
        previewTexture = new RenderTexture(512, 512, 16);
        previewCamera.targetTexture = previewTexture;

        // Asignar textura a la Raw Image
        renderRawImage.texture = previewTexture;

        currentRotation = Quaternion.Euler(rotationOffset);
    }

    private void Update()
    {
        rotatingAnchor.transform.Rotate(Vector3.up, rotationSpeed * Time.unscaledDeltaTime);
    }

    public void SetMemoryDetails()
    {
        if (memorySelected != null)
        {
            if (mesh_3D != null) Destroy(mesh_3D);

            // Instanciar modelo sin reiniciar la rotación
            currentRotation = rotatingAnchor.transform.rotation;
            mesh_3D = Instantiate(memorySelected.mesh_3D, previewPosition, currentRotation, rotatingAnchor.transform);

            // Asignar capa "ViewMemory"
            SetLayerRecursively(rotatingAnchor, LayerMask.NameToLayer("ViewMemory"));

            // Asignar textura a la Raw
            renderBackgroundImage.color = Color.white; // Para forzar opacidad al 100%
            renderRawImage.color = Color.white; // Para forzar opacidad al 100%
            renderRawImage.texture = previewTexture;

            // Asignar nombre y descripcion
            nameText.text = memorySelected.memoryName;
            descriptionText.text = memorySelected.description;
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
