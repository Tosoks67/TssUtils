using Footprinting;
using LabApi.Features.Wrappers;
using MapGeneration;
using Mirror;

namespace TssUtils.Code.Misc.Extensions;

public static class LabApiExtensions
{
    extension(AdminToy toy)
    {
        public void ActivateAndSpawn()
        {
            toy.GameObject.SetActive(true);
            if (!NetworkServer.spawned.ContainsValue(toy.GameObject.NetworkIdentity))
                toy.Spawn();
        }
    }

    extension(Room _)
    {
        public static Room? GetByName(RoomName name) => Room.Get(name).FirstOrDefault();
    }

    public static void SetLastOwner(this Pickup pickup, Player? player)
    {
        pickup.Base.PreviousOwner = new Footprint(player?.ReferenceHub);
    }
}
