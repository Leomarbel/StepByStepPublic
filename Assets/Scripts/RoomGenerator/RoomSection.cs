using UnityEngine;

public class RoomSection : MonoBehaviour
{
    [SerializeField] private Transform entrance;
    [SerializeField] private Transform[] exits;

    public Transform Entrance => entrance;
    public Transform[] Exits => exits;
}
