using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace TssUtils.Code.Misc.Extensions;

public static class PlayerExtensions
{
    extension(Player player)
    {
        public Vector3 RelativePosition => player.Room is null
            ? new Vector3(0, 0, 0)
            : player.Room.Transform.InverseTransformPoint(player.Position) - new Vector3(0, player.Scale.y + 0.01f, 0);

        public string StringRep => $"{player.Nickname} ({player.PlayerId})";

        public void TeleportToRoom(RoomName roomName, Vector3 relativePosition)
        {
            var room = Room.Get(roomName).FirstOrDefault();

            if (room is null)
                return;

            player.Position = room.Transform.TransformPoint(relativePosition)
                              + new Vector3(0, player.Scale.y + 0.01f, 0);
        }
    }
}
