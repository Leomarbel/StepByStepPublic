using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private RoomSection startingRoom;
    [SerializeField] private RoomSection[] roomPrefabs;
    [SerializeField, Min(1)] private int roomsToAdd = 5;

    private void Start()
    {
        if (startingRoom == null ||
            roomPrefabs == null ||
            roomPrefabs.Length == 0)
        {
            Debug.LogError("Assign a starting room and room prefabs.", this);
            return;
        }

        RoomSection currentRoom = startingRoom;

        for (int i = 0; i < roomsToAdd; i++)
        {
            if (currentRoom.Exits == null ||
                currentRoom.Exits.Length == 0)
                break;

            Transform exit = currentRoom.Exits[
                Random.Range(0, currentRoom.Exits.Length)
            ];

            RoomSection prefab = roomPrefabs[
                Random.Range(0, roomPrefabs.Length)
            ];

            if (exit == null || prefab == null ||
                prefab.Entrance == null)
            {
                Debug.LogError("A room connection is missing.", this);
                break;
            }

            RoomSection nextRoom = Instantiate(prefab);

            // Rotate the whole room to align its entrance direction.
            Quaternion rotationDifference =
                exit.rotation *
                Quaternion.Inverse(nextRoom.Entrance.rotation);

            nextRoom.transform.rotation =
                rotationDifference * nextRoom.transform.rotation;

            // Move the whole room to align its entrance position.
            nextRoom.transform.position +=
                exit.position - nextRoom.Entrance.position;

            currentRoom = nextRoom;
        }
    }
}
