using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class TriggerMemoryVaultByCollision : MonoBehaviour
{
    // SO del coleccionable en concreto
    [SerializeField] private SO_Collectable data;

    // Sistema de partículas de cuando pillas el coleccionable
    [HideInInspector][SerializeField] public GameObject collectedEffectPrefab;

    // Animacion
    private float shrinkAnimationDuration = 1f;
    private static readonly AnimationCurve shrinkCurve = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, 4f),
        new Keyframe(0.3f, 1.2f),
        new Keyframe(1f, 0f, -4f, 0f)
    );

    // Pop Up
    [HideInInspector] public GameObject canvasPrefab;
    private RawImage modelPreviewRawImage;
    private static Vector3 previewPosition = new Vector3(1000, 0, 0);
    private static Vector3 cameraOffset = new Vector3(0, 0, 2);
    private static Vector3 rotationOffset = new Vector3(0, -90, 0);
    private static float rotationSpeed = 30f;
    private static float displayDuration = 5f;
    private GameObject rotatingAnchor;
    private Camera previewCamera;
    private RenderTexture previewTexture;

    [Space]

    [Tooltip("Eventos adicionales que se ejecutan al recoger el objeto")]
    public UnityEvent onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ShrinkCoroutine(transform.localScale));

            if (data == null)
            {
                Debug.LogError("Scriptable object del coleccionable no asignado en el inspector");
            }
            else
            {
                MemoriesCollectedManager.collectables.Add(data);
                Debug.Log(data.name + " has been collected!");
            }

            GetComponent<Collider>().enabled = false;

            onTriggerEnter.Invoke();
        }
    }

    IEnumerator ShrinkCoroutine(Vector3 originalScale)
    {
        float elapsed = 0f;

        while (elapsed < shrinkAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shrinkAnimationDuration);

            // Usa la curva para calcular el factor de escala
            float scaleFactor = shrinkCurve.Evaluate(t);

            transform.localScale = originalScale * scaleFactor;

            yield return null;
        }

        transform.localScale = Vector3.zero;

        ParticleBurstAnimation();

        StartCoroutine(ShowCollectableCoroutine());
    }

    private void ParticleBurstAnimation()
    {
        ParticleSystem auraPS = GetComponentInChildren<ParticleSystem>();

        auraPS.Stop();

        Instantiate(collectedEffectPrefab, auraPS.transform.position, auraPS.transform.rotation);
    }

    private IEnumerator ShowCollectableCoroutine()
    {
        if (data.mesh_3D == null)
        {
            Debug.LogError("Mesh no asignada en el scriptable object del coleccionable");
            yield break;
        }

        // Instanciar modelo
        GameObject model = Instantiate(data.mesh_3D, previewPosition, Quaternion.Euler(rotationOffset));

        // Crear anchor rotatorio
        rotatingAnchor = new GameObject("ModelPreviewRotator");
        rotatingAnchor.transform.position = previewPosition;
        model.transform.SetParent(rotatingAnchor.transform);

        // Asignar capa "Preview"
        SetLayerRecursively(rotatingAnchor, LayerMask.NameToLayer("Preview"));

        // Crear cámara
        GameObject camObj = new GameObject("PreviewCamera");
        previewCamera = camObj.AddComponent<Camera>();
        camObj.transform.position = previewPosition + cameraOffset;
        camObj.transform.LookAt(previewPosition);
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.cullingMask = LayerMask.GetMask("Preview");

        // Instanciar canvas
        GameObject canvas = Instantiate(canvasPrefab);

        // Crear RenderTexture
        previewTexture = new RenderTexture(512, 512, 16);
        previewCamera.targetTexture = previewTexture;

        // Asignar textura a una RawImage
        modelPreviewRawImage = FindRawImageRecursively(canvas);
        if (modelPreviewRawImage == null)
        {
            Debug.LogError("Raw image no encontrada en el prefab del canvas");
        }
        else
        {
            modelPreviewRawImage.texture = previewTexture;
        }


        // Mostrar durante X segundos
        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            rotatingAnchor.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Limpiar
        Destroy(canvas);
        Destroy(rotatingAnchor);
        Destroy(previewCamera.gameObject);
        modelPreviewRawImage.texture = null;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private RawImage FindRawImageRecursively(GameObject obj)
    {
        // Comprobar si el propio objeto tiene RawImage
        RawImage rawImage = obj.GetComponent<RawImage>();
        if (rawImage != null)
            return rawImage;

        // Recorrer hijos
        foreach (Transform child in obj.transform)
        {
            RawImage found = FindRawImageRecursively(child.gameObject);
            if (found != null)
                return found;
        }

        // No se encontró
        return null;
    }
}
