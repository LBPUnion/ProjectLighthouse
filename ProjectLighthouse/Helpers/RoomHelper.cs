#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.StorableLists;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Matchmaking;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Helpers;

public class RoomHelper
{
    public static readonly object RoomLock = new();
    public static StorableList<Room> Rooms => RoomStore.GetRooms();

    private static int roomIdIncrement;
    internal static int RoomIdIncrement => roomIdIncrement++;

    public static FindBestRoomResponse? FindBestRoom(User? user, GameVersion roomVersion, RoomSlot? slot, Platform? platform, string? location)
    {
        if (roomVersion == GameVersion.LittleBigPlanet1 || roomVersion == GameVersion.LittleBigPlanetPSP)
        {
            Logger.Error($"Returning null for FindBestRoom, game ({roomVersion}) does not support dive in (should never happen?)", LogArea.Match);
            return null;
        }

        Random random = new();
        IEnumerable<Room> rooms = Rooms.OrderBy(_ => random.Next());

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

                    if (gotValue && value != null) relevantUserLocations.TryAdd(p, value);
                    return gotValue;
                }
            );

            // If we don't have all locations then the game won't know how to communicate. Thus, it's not a valid room.
            if (!allPlayersHaveLocations) continue;

            // If we got here then it should be a valid room.

            FindBestRoomResponse response = new()
            {
                RoomId = room.RoomId,
                Players = new List<Player>(),
                Locations = new List<string>(),
            };

            foreach (User player in room.GetPlayers(new DatabaseContext()))
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

        if (user == null) return null;

        MatchHelper.ClearUserRecentDiveIns(user.UserId);
        Logger.Info($"Cleared {user.Username} (id: {user.UserId})'s recent dive-ins", LogArea.Match);

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
        lock(RoomLock) Rooms.Add(room);
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
    public static Task CleanupRooms(int? hostId = null, Room? newRoom = null, DatabaseContext? database = null)
    {
        #if DEBUG
        Stopwatch stopwatch = new();
        stopwatch.Start();
        #endif
        lock(RoomLock)
        {
            StorableList<Room> rooms = Rooms; // cache rooms so we dont gen a new one every time
            List<Room> roomsToUpdate = new();
            
            #if DEBUG
            Logger.Debug($"Cleaning up rooms... (took {stopwatch.ElapsedMilliseconds}ms to get lock on {nameof(RoomLock)})", LogArea.Match);
            #endif
            int roomCountBeforeCleanup = rooms.Count();

            // Remove offline players from rooms
            foreach (Room room in rooms)
            {
                List<User> players = room.GetPlayers(database ?? new DatabaseContext());
                List<int> playersToRemove = players.Where(player => player.Status.StatusType == StatusType.Offline).Select(player => player.UserId).ToList();

                foreach (int player in playersToRemove) room.PlayerIds.Remove(player);
                
                roomsToUpdate.Add(room);
            }

            // DO NOT REMOVE ROOMS BEFORE THIS POINT!
            // this will cause the room to be added back to the database
            foreach (Room room in roomsToUpdate)
            {
                rooms.Update(room);
            }

            // Delete old rooms based on host
            if (hostId != null)
            {
                try
                {
                    rooms.RemoveAll(r => r.PlayerIds.Contains((int)hostId));
                }
                catch
                {
                    // TODO: detect the room that failed and remove it
                }
            }

            // Remove rooms containing players in this new room
            if (newRoom != null)
            {
                foreach (Room room in rooms.Where(room => room != newRoom))
                {
                    foreach (int newRoomPlayer in newRoom.PlayerIds)
                    {
                        if (room.PlayerIds.Contains(newRoomPlayer)) rooms.Remove(room);
                    }
                }
            }

            rooms.RemoveAll(r => r.PlayerIds.Count == 0); // Remove empty rooms
            rooms.RemoveAll(r => r.HostId == -1); // Remove rooms with broken hosts
            rooms.RemoveAll(r => r.PlayerIds.Count > 4); // Remove obviously bogus rooms

            int roomCountAfterCleanup = rooms.Count();

            // Log the amount of rooms cleaned up.
            // If we didnt clean any rooms, it's not useful to log in a 
            // production environment but it's still quite useful for debugging.
            //
            // So, we handle that case here:
            int roomsCleanedUp = roomCountBeforeCleanup - roomCountAfterCleanup;
            string logText = $"Cleaned up {roomsCleanedUp} rooms.";

            if (roomsCleanedUp == 0)
            {
                Logger.Debug(logText, LogArea.Match);
            }
            else
            {
                Logger.Info(logText, LogArea.Match);
            }

            logText = $"Updated {roomsToUpdate.Count} rooms.";
            if (roomsToUpdate.Count == 0)
            {
                Logger.Debug(logText, LogArea.Match);
            }
            else
            {
                Logger.Info(logText, LogArea.Match);
            }
        }

        return Task.FromResult(0);
    }
}