using UnityEngine;
using System.Collections.Generic;
public class RoomSection : MonoBehaviour
{
    [SerializeField] private Transform entrance;
    [SerializeField] private Transform[] exits;
    [SerializeField, Min(1)] private int entrancesBeforeDestroy = 2;

    //Corner 
    [SerializeField] private bool isCorner;
    public bool IsCorner => isCorner;
    public int RoomsSinceCorner { get; set; }

    public Transform Entrance => entrance;
    public Transform[] Exits => exits;

    public RoomSection Parent { get; private set; }
    public List<RoomSection> Children { get; } = new();
    public bool Expanded { get; set; }

    private RoomGenerator generator;
    private int entrancesPassed;
    private bool entered;

    public void Initialize(
        RoomGenerator roomGenerator,
        RoomSection parent = null)
    {
        generator = roomGenerator;
        Parent = parent;
    }

    public void PlayerEntered()
    {
        if (generator != null)
            generator.OnRoomEntered(this);
    }

    public void PlayerLeft()
    {
        if (entered || generator == null) return;

        entered = true;
        entrancesPassed = 0;

        generator.EntrancePassed += CountEntrance;
    }

    private void CountEntrance()
    {
        entrancesPassed++;

        if (entrancesPassed >= entrancesBeforeDestroy)
        {
            generator.EntrancePassed -= CountEntrance;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (generator != null)
            generator.EntrancePassed -= CountEntrance;
    }

}
