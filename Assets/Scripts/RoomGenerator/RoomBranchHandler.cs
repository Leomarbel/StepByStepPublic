using UnityEngine;

public static class RoomBranchHandler
{
    public static void KeepChosenBranch(
        RoomSection junction,
        RoomSection chosenRoom)
    {
        foreach (RoomSection child in junction.Children)
        {
            if (child != chosenRoom)
                DestroyBranch(child);
        }

        junction.Children.Clear();
        junction.Children.Add(chosenRoom);
    }

    private static void DestroyBranch(RoomSection room)
    {
        foreach (RoomSection child in room.Children)
        {
            DestroyBranch(child);
        }

        room.Children.Clear();
        Object.Destroy(room.gameObject);
    }
}