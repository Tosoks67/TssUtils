using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TssUtils.Code.Misc.Extensions;

public static class UnityExtensions
{
    extension(Vector3 vector)
    {
        public bool IsInSelection(Vector3 point1, Vector3 point2) =>
            vector.x.IsInRange(point1.x, point2.x)
            && vector.y.IsInRange(point1.y, point2.y)
            && vector.z.IsInRange(point1.z, point2.z);
    }

    extension(Random)
    {
        public static int RangeInclusive(int min, int max) => Random.Range(min, max + 1);
    }

    extension(GameObject obj)
    {
        public NetworkIdentity? NetworkIdentity =>
            obj.TryGetComponent(out NetworkIdentity networkIdentity)
                ? networkIdentity
                : null;
    }

    extension(ReferenceHub hub)
    {
        public Player ToLabApiPlayer() => Player.Get(hub);
    }
}
