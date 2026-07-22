using UnityEngine;

namespace Hanseithon.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class BunnyLocalKeyInventory : MonoBehaviour
    {
        public int KeyCount { get; private set; }

        public void AddKey()
        {
            KeyCount++;
        }

        public bool TryConsumeKey()
        {
            if (KeyCount <= 0)
            {
                return false;
            }

            KeyCount--;
            return true;
        }
    }
}
