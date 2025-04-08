using UnityEngine;

public enum TextType
{
    STANDARD,
    UNFOLDING,
    REACTIVE,
}

[CreateAssetMenu(fileName = "SO_Texts", menuName = "Scriptable Objects/SO_Texts")]
public class SO_TextData : ScriptableObject
{
    public TextType textType;
    public string content;

    private bool _enabled = true;
    private bool _finished = false;
}
