using System.Collections.Generic;
using UnityEngine;

namespace LorQB.Core
{
    /// <summary>
    /// Manages the four-color shuffle sequence and tracks progression through it.
    /// Plain class — no MonoBehaviour, no UI, no physics.
    /// The Validation system advances the index; this class never drives GameState.
    /// </summary>
    public class SequenceManager
    {
        // ── Constants ────────────────────────────────────────────────────────────
        private const int MaxIndex = 3;

        // ── Data ─────────────────────────────────────────────────────────────────
        private List<CubeColor> sequence = new List<CubeColor>();
        private int sequenceIndex = 0;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a random permutation of [Blue, Red, Green, Yellow] and resets
        /// the sequence index to 0.
        /// </summary>
        public void GenerateSequence()
        {
            sequence = new List<CubeColor>
            {
                CubeColor.Blue,
                CubeColor.Red,
                CubeColor.Green,
                CubeColor.Yellow
            };

            // Fisher-Yates shuffle
            for (int i = sequence.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                CubeColor temp = sequence[i];
                sequence[i] = sequence[j];
                sequence[j] = temp;
            }

            sequenceIndex = 0;

            Debug.Log(
                $"[SequenceManager] Sequence generated: " +
                $"[{sequence[0]}, {sequence[1]}, {sequence[2]}, {sequence[3]}]");
        }

        /// <summary>
        /// Returns the color at the current sequence index.
        /// </summary>
        public CubeColor GetCurrentColor()
        {
            return sequence[sequenceIndex];
        }

        /// <summary>
        /// Returns true if there is a next color (sequenceIndex is not yet at the final index).
        /// </summary>
        public bool HasNextColor()
        {
            return sequenceIndex < MaxIndex;
        }

        /// <summary>
        /// Returns the color at the next sequence index.
        /// Returns the final color and logs a warning if already at the final index.
        /// </summary>
        public CubeColor GetNextColor()
        {
            if (!HasNextColor())
            {
                Debug.LogWarning("[SequenceManager] No next color (at final index)");
                return sequence[MaxIndex];
            }

            return sequence[sequenceIndex + 1];
        }

        /// <summary>
        /// Advances sequenceIndex by 1. Does nothing if already at the maximum (3).
        /// </summary>
        public void AdvanceSequence()
        {
            if (sequenceIndex >= MaxIndex)
            {
                Debug.LogWarning("[SequenceManager] Already at final index");
                return;
            }

            sequenceIndex++;
            Debug.Log($"[SequenceManager] Advanced to index {sequenceIndex}");
        }

        /// <summary>
        /// Resets sequenceIndex to 0 without regenerating the sequence.
        /// </summary>
        public void ResetSequence()
        {
            sequenceIndex = 0;
            Debug.Log("[SequenceManager] Sequence index reset to 0.");
        }
    }
}
