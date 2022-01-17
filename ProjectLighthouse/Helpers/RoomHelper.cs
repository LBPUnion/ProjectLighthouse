#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Match;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public class RoomHelper
    {
        public static readonly List<Room> Rooms = new();

        public static readonly RoomSlot PodSlot = new()
        {
            SlotType = SlotType.Pod,
            SlotId = 0,
        };

        private static int roomIdIncrement;

        internal static int RoomIdIncrement => roomIdIncrement++;

        public static FindBestRoomResponse? FindBestRoom(User? user, GameVersion roomVersion, string? location)
        {
            if (roomVersion == GameVersion.LittleBigPlanet1 || roomVersion == GameVersion.LittleBigPlanetPSP)
            {
                Logger.Log($"Returning null for FindBestRoom, game ({roomVersion}) does not support dive in", LoggerLevelMatch.Instance);
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
                {
                    response.Players.Add
                    (
                        new Player
                        {
                            MatchingRes = 1,
                            User = user,
                        }
                    );
                }

                if (location == null)
                {
                    response.Locations.Add(location);
                }

                response.Slots = new List<List<int>>
                {
                    new()
                    {
                        (int)room.Slot.SlotType,
                        room.Slot.SlotId,
                    },
                };

                Logger.Log($"Found a room (id: {room.RoomId}) for user {user?.Username ?? "null"} (id: {user?.UserId ?? -1})", LoggerLevelMatch.Instance);

                return response;
            }

            return null;
        }

        public static Room CreateRoom(User user, GameVersion roomVersion, RoomSlot? slot = null)
            => CreateRoom
            (
                new List<User>
                {
                    user,
                },
                roomVersion,
                slot
            );
        public static Room CreateRoom(List<User> users, GameVersion roomVersion, RoomSlot? slot = null)
        {
            Room room = new()
            {
                RoomId = RoomIdIncrement,
                Players = users,
                State = RoomState.Idle,
                Slot = slot ?? PodSlot,
                RoomVersion = roomVersion,
            };

            CleanupRooms(room.Host, room);
            lock(Rooms) Rooms.Add(room);
            Logger.Log($"Created room (id: {room.RoomId}) for host {room.Host.Username} (id: {room.Host.UserId})", LoggerLevelMatch.Instance);

            return room;
        }

        public static Room? FindRoomByUser(User user, GameVersion roomVersion, bool createIfDoesNotExist = false)
        {
            lock(Rooms)
            {
                foreach (Room room in Rooms.Where(room => room.Players.Any(player => user == player))) return room;
            }

            return createIfDoesNotExist ? CreateRoom(user, roomVersion) : null;
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        public static void CleanupRooms(User? host = null, Room? newRoom = null)
        {
            lock(Rooms)
            {
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
            }
        }
    }
}