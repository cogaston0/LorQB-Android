// LorQBTypes.cs  (Blender 5.1.1 reference build)
// All enums, geometry constants, and full chapter table for LorQB V2.
// Includes all C-series (both directions) + T-series stubs for shuffle support.
// Place in: Assets/LorQB/Scripts/

using UnityEngine;

namespace LorQB
{
    // ── Cube identity ─────────────────────────────────────────────────────────
    public enum CubeId { Blue = 0, Red = 1, Green = 2, Yellow = 3 }

    // ── Top-level game state ──────────────────────────────────────────────────
    public enum GamePhase
    {
        WaitingForShuffle,  // Pre-turn: shuffle result not yet shown
        WaitingForBallDrop, // Ball is outside; player must drag it to first cube
        Idle,               // Ball seated; player picks next cube
        Transferring,       // Chapter in progress
        Win                 // Full shuffled sequence completed
    }

    // ── Transfer type ─────────────────────────────────────────────────────────
    public enum TransferType
    {
        Adjacent,   // C-series: shares a hinge directly
        Diagonal    // T-series: requires 2+ rotations
    }

    // ── Chapter definition ────────────────────────────────────────────────────
    public class ChapterDef
    {
        public CubeId       From;
        public CubeId       To;
        public TransferType Type;
        public int          HingeIndex;      // Primary hinge: 0=BR, 1=RG, 2=GY
        public int          SecondHinge;     // T-series second hinge (-1 if none)
        public float        RotSign;         // +1 or -1 — TODO: verify empirically (Rule 5)
        public float        RotDegrees;      // Hinge rotation for visual
        public bool         Verified;        // false = ROT_SIGN unconfirmed
    }

    // ── All static config in one place ────────────────────────────────────────
    public static class LorQBConfig
    {
        // World-unit cube geometry
        public const float CUBE_SIZE      = 2.2f;
        public const float HALF_CUBE      = CUBE_SIZE * 0.5f;
        public const float BALL_RADIUS    = 0.28f;
        public const float HINGE_RADIUS   = 0.07f;
        public const float HINGE_LENGTH   = 0.5f;

        // Ball start position (outside cubes, to the left of the cluster)
        public const float BALL_START_X   = -(HALF_CUBE * 2f + CUBE_SIZE * 1.4f);
        public static Vector3 BallStartPos => new(BALL_START_X, 0f, 0f);

        // Timing
        public const float ROTATION_DURATION = 0.65f;
        public const float TRANSFER_DURATION  = 0.55f;
        public const float SETTLE_DURATION    = 0.18f;

        // ── 2×2 grid layout ──────────────────────────────────────────────────
        public static readonly Vector3[] CubePositions =
        {
            new( HALF_CUBE,  HALF_CUBE, 0f),   // [0] Blue
            new( HALF_CUBE, -HALF_CUBE, 0f),   // [1] Red
            new(-HALF_CUBE, -HALF_CUBE, 0f),   // [2] Green
            new(-HALF_CUBE,  HALF_CUBE, 0f),   // [3] Yellow
        };

        // Hinge pivots: midpoint of shared face between adjacent cubes
        public static readonly Vector3[] HingePivots =
        {
            new( HALF_CUBE, 0f,         0f),   // [0] BR  — Blue ↔ Red
            new( 0f,       -HALF_CUBE,  0f),   // [1] RG  — Red ↔ Green
            new(-HALF_CUBE, 0f,         0f),   // [2] GY  — Green ↔ Yellow
        };

        // Hinge world-space rotation axes
        public static readonly Vector3[] HingeAxes =
        {
            Vector3.forward,   // BR
            Vector3.forward,   // RG
            Vector3.forward,   // GY
        };

        // ── Colors ───────────────────────────────────────────────────────────
        public static readonly Color[] CubeColors =
        {
            new(0.20f, 0.50f, 1.00f, 0.22f),   // Blue
            new(1.00f, 0.22f, 0.22f, 0.22f),   // Red
            new(0.20f, 0.90f, 0.22f, 0.22f),   // Green
            new(1.00f, 0.82f, 0.00f, 0.22f),   // Yellow
        };

        public static readonly Color[] EmissiveColors =
        {
            new(0.08f, 0.30f, 0.90f),
            new(0.90f, 0.08f, 0.08f),
            new(0.08f, 0.75f, 0.08f),
            new(0.90f, 0.72f, 0.00f),
        };

        public static readonly string[] CubeNames = { "Blue", "Red", "Green", "Yellow" };

        // ── Ball seat ─────────────────────────────────────────────────────────
        public static Vector3 SeatOffset => new(0f, -(HALF_CUBE - BALL_RADIUS - 0.08f), 0f);

        // ── Full chapter table ────────────────────────────────────────────────
        // ROT_SIGN marked Verified=false until tested empirically (Rule 5).
        // T-series entries are stubs — HingeIndex/RotSign need empirical verification.
        public static readonly ChapterDef[] Chapters =
        {
            // ── C-series forward ──────────────────────────────────────────────
            new() { From=CubeId.Blue,   To=CubeId.Red,    Type=TransferType.Adjacent, HingeIndex=0, SecondHinge=-1, RotSign= 1f, RotDegrees=90f, Verified=false },  // C12
            new() { From=CubeId.Red,    To=CubeId.Green,  Type=TransferType.Adjacent, HingeIndex=1, SecondHinge=-1, RotSign= 1f, RotDegrees=90f, Verified=false },  // C13
            new() { From=CubeId.Green,  To=CubeId.Yellow, Type=TransferType.Adjacent, HingeIndex=2, SecondHinge=-1, RotSign=-1f, RotDegrees=90f, Verified=false },  // C14
            new() { From=CubeId.Yellow, To=CubeId.Blue,   Type=TransferType.Adjacent, HingeIndex=0, SecondHinge=-1, RotSign=-1f, RotDegrees=90f, Verified=false },  // C15

            // ── C-series reverse ──────────────────────────────────────────────
            new() { From=CubeId.Red,    To=CubeId.Blue,   Type=TransferType.Adjacent, HingeIndex=0, SecondHinge=-1, RotSign=-1f, RotDegrees=90f, Verified=false },  // C12r
            new() { From=CubeId.Green,  To=CubeId.Red,    Type=TransferType.Adjacent, HingeIndex=1, SecondHinge=-1, RotSign=-1f, RotDegrees=90f, Verified=false },  // C13r
            new() { From=CubeId.Yellow, To=CubeId.Green,  Type=TransferType.Adjacent, HingeIndex=2, SecondHinge=-1, RotSign= 1f, RotDegrees=90f, Verified=false },  // C14r
            new() { From=CubeId.Blue,   To=CubeId.Yellow, Type=TransferType.Adjacent, HingeIndex=0, SecondHinge=-1, RotSign= 1f, RotDegrees=90f, Verified=false },  // C15r

            // ── T-series diagonal (stubs — ROT_SIGN and HingeIndex unverified) ─
            new() { From=CubeId.Red,    To=CubeId.Yellow, Type=TransferType.Diagonal,  HingeIndex=1, SecondHinge=2, RotSign= 1f, RotDegrees=90f, Verified=true  },  // T03 (confirmed working)
            new() { From=CubeId.Yellow, To=CubeId.Red,    Type=TransferType.Diagonal,  HingeIndex=2, SecondHinge=1, RotSign=-1f, RotDegrees=90f, Verified=false },  // T03r
            new() { From=CubeId.Blue,   To=CubeId.Green,  Type=TransferType.Diagonal,  HingeIndex=0, SecondHinge=1, RotSign= 1f, RotDegrees=90f, Verified=false },  // T-BG
            new() { From=CubeId.Green,  To=CubeId.Blue,   Type=TransferType.Diagonal,  HingeIndex=1, SecondHinge=0, RotSign=-1f, RotDegrees=90f, Verified=false },  // T-GB
        };

        // Lookup — returns null if no chapter defined for this pair
        public static ChapterDef GetChapter(CubeId from, CubeId to)
        {
            foreach (var ch in Chapters)
                if (ch.From == from && ch.To == to) return ch;
            return null;
        }

        // All cubes that can be reached from a given cube
        public static CubeId[] ReachableFrom(CubeId from)
        {
            var result = new System.Collections.Generic.List<CubeId>();
            foreach (var ch in Chapters)
                if (ch.From == from) result.Add(ch.To);
            return result.ToArray();
        }
    }
}
