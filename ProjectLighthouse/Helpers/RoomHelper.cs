#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Match;
using LBPUnion.ProjectLighthouse.Types.Profiles;

namespace LBPUnion.ProjectLighthouse.Helpers;

public class RoomHelper
{
    public static readonly List<Room> Rooms = new();

    public static readonly RoomSlot PodSlot = new()
    {
        SlotType = SlotType.Pod,
        SlotId = 0,
    };

    private static int roomIdIncrement;

    public static void StartCleanupThread()
    {
        // ReSharper disable once FunctionNeverReturns
        Task.Factory.StartNew
        (
            async () =>
            {
                while (true)
                {
                    CleanupRooms();
                    await Task.Delay(10000);
                }
            }
        );
    }

    internal static int RoomIdIncrement => roomIdIncrement++;

    public static FindBestRoomResponse? FindBestRoom(User? user, GameVersion roomVersion, RoomSlot? slot, Platform? platform, string? location)
    {
        if (roomVersion == GameVersion.LittleBigPlanet1 || roomVersion == GameVersion.LittleBigPlanetPSP)
        {
            Logger.LogError($"Returning null for FindBestRoom, game ({roomVersion}) does not support dive in (should never happen?)", LogArea.Match);
            return null;
        }

        bool anyRoomsLookingForPlayers;
        List<Room> rooms;

        lock(Rooms)
        {
            anyRoomsLookingForPlayers = Rooms.Any(r => r.IsLookingForPlayers);
            rooms = anyRoomsLookingForPlayers ? Rooms.Where(r => anyRoomsLookingForPlayers && r.IsLookingForPlayers).ToList() : Rooms;
        }

        rooms = rooms.Where(r => r.RoomVersion == roomVersion).ToList();
        if (platform != null) rooms = rooms.Where(r => r.RoomPlatform == platform).ToList();

        // If the user is in the pod while trying to look for a room, then they're diving in.
        // Otherwise they're looking for people to play with in a particular level.
        // We handle that here:
        if (slot != null && slot.SlotType != SlotType.Pod && slot.SlotId != 0)
        {
            rooms = rooms.Where(r => r.Slot.SlotType == slot.SlotType && r.Slot.SlotId == slot.SlotId).ToList();
        }

        // Don't attempt to dive into the current room the player is in.
        if (user != null)
        {
            rooms = rooms.Where(r => !r.Players.Contains(user)).ToList();
        }

        foreach (Room room in rooms)
            // Look for rooms looking for players before moving on to rooms that are idle.
        {
            if (user != null && MatchHelper.DidUserRecentlyDiveInWith(user.UserId, room.Host.UserId)) continue;

            Dictionary<int, string> relevantUserLocations = new();

            // Determine if all players in a room have UserLocations stored, also store the relevant userlocations while we're at it
            bool allPlayersHaveLocations = room.Players.All
            (
                p =>
                {
                    bool gotValue = MatchHelper.UserLocations.TryGetValue(p.UserId, out string? value);

                    if (gotValue && value != null) relevantUserLocations.Add(p.UserId, value);
                    return gotValue;
                }
            );

            // If we don't have all locations then the game won't know how to communicate. Thus, it's not a valid room.
            if (!allPlayersHaveLocations) continue;

            // If we got here then it should be a valid room.

            FindBestRoomResponse response = new();
            response.RoomId = room.RoomId;

            response.Players = new List<Player>();
            response.Locations = new List<string>();
            foreach (User player in room.Players)
            {
                response.Players.Add
                (
                    new Player
                    {
                        MatchingRes = 0,
                        User = player,
                    }
                );

                response.Locations.Add(relevantUserLocations.GetValueOrDefault(player.UserId)); // Already validated to exist
            }

            if (user != null)
                response.Players.Add
                (
                    new Player
                    {
                        MatchingRes = 1,
                        User = user,
                    }
                );

            if (location == null) response.Locations.Add(location);

            response.Slots = new List<List<int>>
            {
                new()
                {
                    (int)room.Slot.SlotType,
                    room.Slot.SlotId,
                },
            };

            Logger.LogSuccess($"Found a room (id: {room.RoomId}) for user {user?.Username ?? "null"} (id: {user?.UserId ?? -1})", LogArea.Match);

            return response;
        }

        return null;
    }

    public static Room CreateRoom(User user, GameVersion roomVersion, Platform roomPlatform, RoomSlot? slot = null)
        => CreateRoom
        (
            new List<User>
            {
                user,
            },
            roomVersion,
            roomPlatform,
            slot
        );
    public static Room CreateRoom(List<User> users, GameVersion roomVersion, Platform roomPlatform, RoomSlot? slot = null)
    {
        Room room = new()
        {
            RoomId = RoomIdIncrement,
            Players = users,
            State = RoomState.Idle,
            Slot = slot ?? PodSlot,
            RoomVersion = roomVersion,
            RoomPlatform = roomPlatform,
        };

        CleanupRooms(room.Host, room);
        lock(Rooms) Rooms.Add(room);
        Logger.LogInfo($"Created room (id: {room.RoomId}) for host {room.Host.Username} (id: {room.Host.UserId})", LogArea.Match);

        return room;
    }

    public static Room? FindRoomByUser(User user, GameVersion roomVersion, Platform roomPlatform, bool createIfDoesNotExist = false)
    {
        lock(Rooms)
            foreach (Room room in Rooms.Where(room => room.Players.Any(player => user == player)))
                return room;

        return createIfDoesNotExist ? CreateRoom(user, roomVersion, roomPlatform) : null;
    }

    public static Room? FindRoomByUserId(int userId)
    {
        lock(Rooms)
            foreach (Room room in Rooms.Where(room => room.Players.Any(player => player.UserId == userId)))
                return room;

        return null;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public static void CleanupRooms(User? host = null, Room? newRoom = null)
    {
        lock(Rooms)
        {
            int roomCountBeforeCleanup = Rooms.Count;

            // Remove offline players from rooms
            foreach (Room room in Rooms)
            {
                // do not shorten, this prevents collection modified errors
                List<User> playersToRemove = room.Players.Where(player => player.Status.StatusType == StatusType.Offline).ToList();
                foreach (User user in playersToRemove) room.Players.Remove(user);
            }

            // Delete old rooms based on host
            if (host != null)
                try
                {
                    Rooms.RemoveAll(r => r.Host == host);
                }
                catch
                {
                    // TODO: detect the room that failed and remove it
                }

            // Remove players in this new room from other rooms
            if (newRoom != null)
                foreach (Room room in Rooms)
                {
                    if (room == newRoom) continue;

                    foreach (User newRoomPlayer in newRoom.Players) room.Players.RemoveAll(p => p == newRoomPlayer);
                }

            Rooms.RemoveAll(r => r.Players.Count == 0); // Remove empty rooms
            Rooms.RemoveAll(r => r.Players.Count > 4); // Remove obviously bogus rooms

            int roomCountAfterCleanup = Rooms.Count;

            if (roomCountBeforeCleanup != roomCountAfterCleanup)
            {
                Logger.LogDebug($"Cleaned up {roomCountBeforeCleanup - roomCountAfterCleanup} rooms.", LogArea.Match);
            }
        }
    }
}