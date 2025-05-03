using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerMemoryVaultByCollision : MonoBehaviour
{
    [HideInInspector][SerializeField] public GameObject collectedEffectPrefab;

    private float shrinkAnimationDuration = 1f;

    private static readonly AnimationCurve shrinkCurve = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, 4f),
        new Keyframe(0.3f, 1.2f),
        new Keyframe(1f, 0f, -4f, 0f)
    );



    [Tooltip("Eventos adicionales que se ejecutan al recoger el objeto")]
    public UnityEvent onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ShrinkCoroutine(transform.localScale));

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

        Destroy(gameObject);
    }


    // *** Funcionará solo si la jerarquia del prefab del sistema de partículas es correcto!!
    private void ParticleBurstAnimation()
    {
        ParticleSystem auraPS = GetComponentInChildren<ParticleSystem>();

        auraPS.Stop();

        Instantiate(collectedEffectPrefab, auraPS.transform.position, auraPS.transform.rotation);
    }
}
