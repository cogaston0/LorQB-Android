using LorQB.Core;

namespace LorQB.UI
{
    /// <summary>
    /// Generates a randomised shuffle order for Level 1.
    /// Rules:
    ///   - Blue is always index 0 (ball starts in Blue cube).
    ///   - All four colours appear exactly once.
    ///   - Remaining three slots are shuffled using Fisher-Yates.
    /// </summary>
    public static class ShuffleOrderGenerator
    {
        private static readonly System.Random _rng = new System.Random();

        /// <summary>Returns a 4-element array of CubeColor, Blue always first.</summary>
        public static CubeColor[] GenerateOrder()
        {
            // Start with the three non-Blue cubes
            CubeColor[] remaining = { CubeColor.Red, CubeColor.Green, CubeColor.Yellow };

            // Fisher-Yates shuffle on the remaining three
            for (int i = remaining.Length - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (remaining[i], remaining[j]) = (remaining[j], remaining[i]);
            }

            return new CubeColor[]
            {
                CubeColor.Blue,
                remaining[0],
                remaining[1],
                remaining[2]
            };
        }
    }
}
