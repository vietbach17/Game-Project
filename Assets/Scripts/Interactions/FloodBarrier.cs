using UnityEngine;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Component placed on Sandbags and Flood Boards.
    /// Used by SoilCells to detect protection from flood water.
    /// </summary>
    public class FloodBarrier : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The protection radius of this barrier.")]
        public float protectionRadius = 2.2f;

        [Tooltip("Type of barrier: True = Sandbag, False = Flood Board.")]
        public bool isSandbag = true;
    }
}
