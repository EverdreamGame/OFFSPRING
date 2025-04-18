using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;


public class FloatingTextManager : MonoBehaviour
{
    [System.Serializable]
    public class TextOption
    {
        public string text;
        public int weight = 1;
    }

    [Header("Text Options")]
    public List<TextOption> possibleTexts = new List<TextOption>();
    public GameObject floatingTextPrefab;
    public TMP_FontAsset font;
    public float fontSize;

    [Header("Rotation Settings")]
    public float minRadius = 1f;
    public float maxRadius = 3f;
    public float minArcAngle = 90f;
    public float maxArcAngle = 270f;

    [Header("Animation Settings")]
    public AnimationCurve fadeCurve;
    public float minLifetime = 1f;
    public float maxLifetime = 3f;
    public float minTimeBetweenSpawns = 0.5f;
    public float maxTimeBetweenSpawns = 2.5f;
    public float floatHeight = 0.5f;

    private void Start()
    {
        StartCoroutine(StartFloatingTextSequence());
    }

    public void SpawnText()
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextPrefab is not assigned!");
            return;
        }

        string selected = GetWeightedRandomText();
        GameObject instance = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);

        var curvedText = instance.GetComponent<CurvedText>();
        var floatingText = instance.GetComponent<FloatingText>();
        var tmp = instance.GetComponent<TextMeshPro>();

        if (tmp == null || curvedText == null || floatingText == null)
        {
            Debug.LogError("Missing required components on prefab!");
            Destroy(instance);
            return;
        }

        // Configure text
        tmp.text = selected;
        if (font != null) tmp.font = font;
        tmp.fontSize = fontSize * Random.Range(.8f, 1.2f);

        // Configure curve
        curvedText.radius = Random.Range(minRadius, maxRadius);
        curvedText.arcAngle = Random.Range(minArcAngle, maxArcAngle);
        curvedText.faceOutward = true;

        // Initialize floating text
        floatingText.Initialize(
            transform,
            fadeCurve,
            Random.Range(minLifetime, maxLifetime),
            floatHeight
        );
    }

    private IEnumerator StartFloatingTextSequence()
    {
        SpawnText();
        yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns));
        StartCoroutine(StartFloatingTextSequence());
    }

    private string GetWeightedRandomText()
    {
        // Calculate total weight (higher weight = more likely)
        int totalWeight = 0;
        foreach (var option in possibleTexts)
            totalWeight += Mathf.Max(1, option.weight);

        int rand = Random.Range(0, totalWeight);
        int sum = 0;

        for (int i = 0; i < possibleTexts.Count; i++)
        {
            sum += possibleTexts[i].weight;
            if (rand < sum)
            {
                // Decrease weight slightly when selected to vary the output
                possibleTexts[i].weight = Mathf.Max(1, possibleTexts[i].weight - 1);
                return possibleTexts[i].text;
            }
        }

        return possibleTexts[0].text; // fallback
    }
}