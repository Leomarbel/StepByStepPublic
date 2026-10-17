using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    private RoomSection room;

    private void Awake()
    {
        room = GetComponentInParent<RoomSection>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        room.PlayerEntered();
    }
}