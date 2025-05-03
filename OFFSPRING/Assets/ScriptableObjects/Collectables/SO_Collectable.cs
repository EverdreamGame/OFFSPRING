using UnityEngine;

[CreateAssetMenu(fileName = "SO_Collectable", menuName = "Scriptable Objects/SO_Collectable")]
public class SO_Collectable : ScriptableObject
{
    public string itemName;
    [TextArea]
    public string description;
    public GameObject mesh_3D;
    public Sprite render_2D;
}
