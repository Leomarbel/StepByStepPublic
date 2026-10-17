using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private RoomSection startingRoom;
    [SerializeField] private RoomSection[] roomPrefabs;
    [SerializeField, Min(1)] private int roomsAhead = 5;

    [SerializeField, Min(0)] private int minRoomsBetweenCorners = 3;

    public event System.Action EntrancePassed;
    private RoomSection currentRoom;


    private void Start()
    {
        if (startingRoom == null ||
            roomPrefabs == null ||
            roomPrefabs.Length == 0)
        {
            Debug.LogError("Assign a starting room and room prefabs.", this);
            enabled = false;
            return;
        }

        currentRoom = startingRoom;
        currentRoom.Initialize(this);

        GenerateAhead(currentRoom, roomsAhead);

    }
    

    public void OnRoomEntered(RoomSection room)
    {
        if (!enabled || currentRoom == null || room == null) return;
        if (room.Parent != currentRoom) return;

        RoomBranchHandler.KeepChosenBranch(currentRoom, room);

        EntrancePassed?.Invoke();

        currentRoom.PlayerLeft();

        currentRoom = room;


        GenerateAhead(currentRoom, roomsAhead);

    }

    private void GenerateAhead(RoomSection room, int depth)
    {
        if (depth <= 0) return;

        if (!room.Expanded)
        {
            GenerateChildren(room);
        }

        foreach (RoomSection child in room.Children)
        {
            GenerateAhead(child, depth - 1);
        }
    }

    private void GenerateChildren(RoomSection room)
    {
        room.Expanded = true;

        if (room.Exits == null) return;

        foreach (Transform exit in room.Exits)
        {
            if (exit == null)
            {
                Debug.LogError(
                    "A room has an unassigned exit.", room);

                continue;
            }

            RoomSection child = SpawnRoom(exit, room);

            if (child == null) continue;

            child.Initialize(this, room);
            room.Children.Add(child);
        }
    }

    private RoomSection SpawnRoom(Transform exit, RoomSection parent)
    {
        List<RoomSection> allowedPrefabs = new();

        foreach (RoomSection prefab in roomPrefabs)
        {
            if (prefab == null || prefab.Entrance == null)
                continue;

            bool cornerAllowed =
                parent.RoomsSinceCorner >= minRoomsBetweenCorners;

            if (!prefab.IsCorner || cornerAllowed)
                allowedPrefabs.Add(prefab);
        }

        if (allowedPrefabs.Count == 0)
        {
            Debug.LogError(
                "No eligible room prefab. Assign a non-corner prefab.",
                this);

            return null;
        }

        RoomSection selected =
            allowedPrefabs[Random.Range(0, allowedPrefabs.Count)];

        RoomSection room = Instantiate(selected);

        // Align the entrance direction with the exit direction.
        Quaternion rotationDifference =
            exit.rotation *
            Quaternion.Inverse(room.Entrance.rotation);

        room.transform.rotation =
            rotationDifference * room.transform.rotation;

        // Align their positions.
        room.transform.position +=
            exit.position - room.Entrance.position;

        room.RoomsSinceCorner = room.IsCorner
            ? 0
            : parent.RoomsSinceCorner + 1;

        return room;
    }
    
}
