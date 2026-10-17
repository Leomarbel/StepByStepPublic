using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private RoomSection startingRoom;
    [SerializeField] private RoomSection[] roomPrefabs;
    [SerializeField, Min(1)] private int roomsAhead = 5;
    [SerializeField, Min(0)] private int roomsBehind = 2;

    private readonly List<RoomSection> rooms = new();
    private int currentIndex;

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

        startingRoom.Initialize(this);
        rooms.Add(startingRoom);
        MaintainRoomsAhead();
    }

    public void OnRoomEntered(RoomSection room)
    {
        if (!enabled) return;

        int index = rooms.IndexOf(room);

        if (index <= currentIndex) return; //Ignore current and rooms behind player

        currentIndex = index;

        MaintainRoomsAhead();
        RemoveOldRooms();
    }

    private void MaintainRoomsAhead()
    {
        while (rooms.Count - currentIndex - 1 < roomsAhead)
        {
            if (!GenerateNextRoom())
                break;
        }
    }

    private bool GenerateNextRoom()
    {
        RoomSection lastRoom = rooms[rooms.Count - 1];

        if (lastRoom.Exits == null || lastRoom.Exits.Length == 0)
        {
            Debug.LogError("The last room has no exits.", lastRoom);
            return false;
        }
        Transform exit = lastRoom.Exits[
            Random.Range(0, lastRoom.Exits.Length)
        ];

        RoomSection prefab = roomPrefabs[
            Random.Range(0, roomPrefabs.Length)
        ];

        if (exit == null || prefab == null ||
            prefab.Entrance == null)
        {
            Debug.LogError("A room prefab or connection is missing.", this);
            return false;
        }

        RoomSection nextRoom = Instantiate(prefab);

        Quaternion rotationDifference =
            exit.rotation *
            Quaternion.Inverse(nextRoom.Entrance.rotation);

        nextRoom.transform.rotation =
            rotationDifference * nextRoom.transform.rotation;

        nextRoom.transform.position +=
            exit.position - nextRoom.Entrance.position;

        nextRoom.Initialize(this);
        rooms.Add(nextRoom);

        return true;
    }
    private void RemoveOldRooms()
    {
        while (currentIndex > roomsBehind)
        {
            RoomSection oldestRoom = rooms[0];

            rooms.RemoveAt(0);
            currentIndex--;

            Destroy(oldestRoom.gameObject);
        }
    }
}
