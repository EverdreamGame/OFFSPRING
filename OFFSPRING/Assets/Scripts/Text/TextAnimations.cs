using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TextMeshPro))]
public class TextAnimations : MonoBehaviour
{
    public float delayBetweenLetters = 0.05f;
    private TextMeshPro tmp;
    private string fullText;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        fullText = tmp.text;
        tmp.ForceMeshUpdate();
        tmp.maxVisibleCharacters = 0;
    }

    //public void ShowTextLetterByLetter()
    //{
    //    tmp.text = fullText; // En caso de que haya sido modificado
    //    tmp.ForceMeshUpdate();

    //    tmp.maxVisibleCharacters = 0;
    //    StartCoroutine(RevealLetters());
    //}

    public void ShowTextLetterByLetter(string text)
    {
        tmp.text = text; // En caso de que haya sido modificado
        tmp.ForceMeshUpdate();

        tmp.maxVisibleCharacters = 0;
        StartCoroutine(RevealLetters());
    }

    private System.Collections.IEnumerator RevealLetters()
    {
        int totalChars = tmp.textInfo.characterCount;

        for (int i = 0; i < totalChars; i++)
        {
            tmp.maxVisibleCharacters = i + 1;

            // Extra: fade in con DOTween por carácter
            int materialIndex = tmp.textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = tmp.textInfo.characterInfo[i].vertexIndex;

            if (!tmp.textInfo.characterInfo[i].isVisible)
            {
                yield return new WaitForSeconds(delayBetweenLetters);
                continue;
            }

            Color32[] newVertexColors = tmp.textInfo.meshInfo[materialIndex].colors32;
            Color32 c = newVertexColors[vertexIndex];

            // Set alpha a 0 al principio
            c.a = 0;
            for (int j = 0; j < 4; j++)
                newVertexColors[vertexIndex + j] = c;

            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // Tween al alpha 255
            DOTween.ToAlpha(() => c, x =>
            {
                for (int j = 0; j < 4; j++)
                    newVertexColors[vertexIndex + j] = x;
                tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }, 1f, delayBetweenLetters * 2f).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(delayBetweenLetters);
        }
    }
}
