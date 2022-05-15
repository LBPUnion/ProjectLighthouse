#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.StorableLists;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;

namespace LBPUnion.ProjectLighthouse.Match.Rooms;

public class RoomHelper
{
    public static readonly StorableList<Room> Rooms = RoomStore.GetRooms();

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
    
    private static int roomIdIncrement;
    internal static int RoomIdIncrement => roomIdIncrement++;

    public static FindBestRoomResponse? FindBestRoom(User? user, GameVersion roomVersion, RoomSlot? slot, Platform? platform, string? location)
    {
        if (roomVersion == GameVersion.LittleBigPlanet1 || roomVersion == GameVersion.LittleBigPlanetPSP)
        {
            Logger.Error($"Returning null for FindBestRoom, game ({roomVersion}) does not support dive in (should never happen?)", LogArea.Match);
            return null;
        }

        IEnumerable<Room> rooms = Rooms;

        rooms = rooms.OrderBy(r => r.IsLookingForPlayers);

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
            rooms = rooms.Where(r => !r.PlayerIds.Contains(user.UserId)).ToList();
        }

        foreach (Room room in rooms)
            // Look for rooms looking for players before moving on to rooms that are idle.
        {
            if (user != null && MatchHelper.DidUserRecentlyDiveInWith(user.UserId, room.HostId)) continue;

            Dictionary<int, string> relevantUserLocations = new();

            // Determine if all players in a room have UserLocations stored, also store the relevant userlocations while we're at it
            bool allPlayersHaveLocations = room.PlayerIds.All
            (
                p =>
                {
                    bool gotValue = MatchHelper.UserLocations.TryGetValue(p, out string? value);

                    if (gotValue && value != null) relevantUserLocations.Add(p, value);
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
            foreach (User player in room.GetPlayers(new Database()))
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

            Logger.Success($"Found a room (id: {room.RoomId}) for user {user?.Username ?? "null"} (id: {user?.UserId ?? -1})", LogArea.Match);

            return response;
        }

        return null;
    }

    public static Room CreateRoom(int userId, GameVersion roomVersion, Platform roomPlatform, RoomSlot? slot = null)
        => CreateRoom
        (
            new List<int>
            {
                userId,
            },
            roomVersion,
            roomPlatform,
            slot
        );
    public static Room CreateRoom(List<int> users, GameVersion roomVersion, Platform roomPlatform, RoomSlot? slot = null)
    {
        Room room = new()
        {
            RoomId = RoomIdIncrement,
            PlayerIds = users,
            State = RoomState.Idle,
            Slot = slot ?? RoomSlot.PodSlot,
            RoomVersion = roomVersion,
            RoomPlatform = roomPlatform,
        };

        CleanupRooms(room.HostId, room);
        lock(Rooms) Rooms.Add(room);
        Logger.Info($"Created room (id: {room.RoomId}) for host {room.HostId}", LogArea.Match);

        return room;
    }

    public static Room? FindRoomByUser(int userId, GameVersion roomVersion, Platform roomPlatform, bool createIfDoesNotExist = false)
    {
        foreach (Room room in Rooms.Where(room => room.PlayerIds.Any(player => userId == player)))
            return room;

        return createIfDoesNotExist ? CreateRoom(userId, roomVersion, roomPlatform) : null;
    }

    public static Room? FindRoomByUserId(int userId)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (Room room in Rooms)
        {
            if (room.PlayerIds.Any(p => p == userId))
            {
                return room;
            }
        }

        return null;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public static void CleanupRooms(int? hostId = null, Room? newRoom = null, Database? database = null)
    {
        lock(Rooms)
        {
            int roomCountBeforeCleanup = Rooms.Count();

            // Remove offline players from rooms
            foreach (Room room in Rooms)
            {
                List<User> players = room.GetPlayers(database ?? new Database());

                List<int> playersToRemove = players.Where(player => player.Status.StatusType == StatusType.Offline).Select(player => player.UserId).ToList();

                foreach (int player in playersToRemove) room.PlayerIds.Remove(player);
            }

            // Delete old rooms based on host
            if (hostId != null)
                try
                {
                    Rooms.RemoveAll(r => r.HostId == hostId);
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

                    foreach (int newRoomPlayer in newRoom.PlayerIds) room.PlayerIds.RemoveAll(p => p == newRoomPlayer);
                }

            Rooms.RemoveAll(r => r.PlayerIds.Count == 0); // Remove empty rooms
            Rooms.RemoveAll(r => r.PlayerIds.Count > 4); // Remove obviously bogus rooms

            int roomCountAfterCleanup = Rooms.Count();

            if (roomCountBeforeCleanup != roomCountAfterCleanup)
            {
                Logger.Debug($"Cleaned up {roomCountBeforeCleanup - roomCountAfterCleanup} rooms.",
                    LogArea.Match);
            }
        }
    }
}