// LorQBShuffler.cs  (Blender 5.1.1 reference build)
// Digital shuffling device — Rule 1 of the original design spec.
// Generates a random color order at the start of each turn.
// The player must route the ball through the cubes in this exact order to win.
// Place in: Assets/LorQB/Scripts/

using System.Collections.Generic;
using UnityEngine;

namespace LorQB
{
    public class LorQBShuffler : MonoBehaviour
    {
        // ── Current shuffled sequence ─────────────────────────────────────────
        public CubeId[] Sequence { get; private set; }

        // Current step the player is on (0 = waiting to drop into first cube)
        public int CurrentStep { get; private set; } = 0;

        // ── Events ─────────────────────────────────────────────────────────────
        public System.Action<CubeId[]> OnShuffled;      // fires when new sequence generated
        public System.Action<int>      OnStepAdvanced;  // fires when ball lands in next cube

        // ── Shuffle ────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a new random permutation of all 4 cubes.
        /// Call at the start of each turn.
        /// </summary>
        public void Shuffle()
        {
            Sequence    = RandomPermutation();
            CurrentStep = 0;
            OnShuffled?.Invoke(Sequence);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Which cube must the ball go to next?</summary>
        public CubeId NextTarget()
        {
            if (CurrentStep >= Sequence.Length) return CubeId.Blue; // fallback
            return Sequence[CurrentStep];
        }

        /// <summary>Called by GameManager each time ball arrives in a cube.</summary>
        public bool AdvanceStep(CubeId arrivedAt)
        {
            if (CurrentStep >= Sequence.Length) return false;
            if (Sequence[CurrentStep] != arrivedAt) return false;

            CurrentStep++;
            OnStepAdvanced?.Invoke(CurrentStep);
            return true;
        }

        public bool IsComplete() => CurrentStep >= Sequence.Length;

        public bool IsFirstStep() => CurrentStep == 0;

        // ── Fisher-Yates shuffle ───────────────────────────────────────────────
        static CubeId[] RandomPermutation()
        {
            var arr = new CubeId[] { CubeId.Blue, CubeId.Red, CubeId.Green, CubeId.Yellow };
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr;
        }
    }
}
