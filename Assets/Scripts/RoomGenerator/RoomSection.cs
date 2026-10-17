using UnityEngine;

public class RoomSection : MonoBehaviour
{
    [SerializeField] private Transform entrance;
    [SerializeField] private Transform[] exits;
    public Transform Entrance => entrance;
    public Transform[] Exits => exits;
    private RoomGenerator generator;
    private bool entered;

    public void Initialize(RoomGenerator roomGenerator)
    {
        generator = roomGenerator;
        entered = false;
    }

    public void PlayerEntered()
    {
        if (entered || generator == null) return;

        entered = true;
        generator.OnRoomEntered(this);
    }
}
