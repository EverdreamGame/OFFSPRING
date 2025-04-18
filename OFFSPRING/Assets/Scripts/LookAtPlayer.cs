using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = Player.Instance.KinematicCharacterController.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player.position);
    }
}
