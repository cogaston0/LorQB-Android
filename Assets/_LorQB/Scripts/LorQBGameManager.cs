// LorQBGameManager.cs  (Blender 5.1.1 reference build)
// Central state machine — shuffle-aware.
// Valid move = next cube in shuffled sequence.
// Ball starts outside cubes. Timer starts on first-cube drop.
// Place in: Assets/LorQB/Scripts/

using System.Collections;
using UnityEngine;

namespace LorQB
{
    public class LorQBGameManager : MonoBehaviour
    {
        // ── Wired by LorQBSceneBuilder ────────────────────────────────────────
        [HideInInspector] public LorQBCubeNode[]      Cubes;
        [HideInInspector] public GameObject[]          Hinges;
        [HideInInspector] public LorQBBallController   Ball;
        [HideInInspector] public LorQBShuffler         Shuffler;

        // ── State ─────────────────────────────────────────────────────────────
        public GamePhase Phase      { get; private set; } = GamePhase.WaitingForShuffle;
        public CubeId    BallOwner  { get; private set; } = CubeId.Blue;

        // Timer — starts when ball drops into first cube
        private float _elapsed  = 0f;
        private bool  _timing   = false;
        public  float Elapsed   => _elapsed;

        // ── Singleton ─────────────────────────────────────────────────────────
        public static LorQBGameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            Shuffler.OnShuffled      += OnSequenceShuffled;
            Shuffler.OnStepAdvanced  += OnStepAdvanced;
            Shuffler.Shuffle();          // Generate first sequence immediately
        }

        void Update()
        {
            if (_timing) _elapsed += Time.deltaTime;
        }

        // ── Shuffle callbacks ─────────────────────────────────────────────────

        void OnSequenceShuffled(CubeId[] seq)
        {
            Phase   = GamePhase.WaitingForBallDrop;
            _timing = false;
            _elapsed = 0f;

            // Ball resets outside cubes
            Ball.ResetToOutside();
            RefreshCubeHighlights();
        }

        void OnStepAdvanced(int step)
        {
            RefreshCubeHighlights();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Called by InputHandler when the player drops the ball on a cube
        /// (drag-to-first-cube gesture, WaitingForBallDrop phase only).
        /// </summary>
        public void OnBallDroppedOnCube(CubeId target)
        {
            if (Phase != GamePhase.WaitingForBallDrop) return;

            CubeId required = Shuffler.NextTarget();
            if (target != required)
            {
                StartCoroutine(ShakeCube(Cubes[(int)target]));
                return;
            }

            // Valid — ball enters first cube, timer starts
            BallOwner = target;
            Shuffler.AdvanceStep(target);
            _timing = true;

            Vector3 seat = LorQBConfig.CubePositions[(int)target] + LorQBConfig.SeatOffset;
            StartCoroutine(Ball.ArcToSeat(seat, LorQBConfig.EmissiveColors[(int)target], LorQBConfig.TRANSFER_DURATION));

            Phase = GamePhase.Idle;
            RefreshCubeHighlights();
        }

        /// <summary>
        /// Called by InputHandler when player taps a cube during Idle phase.
        /// </summary>
        public void OnCubeTapped(CubeId tapped)
        {
            if (Phase != GamePhase.Idle) return;
            if (tapped == BallOwner) return;

            // Must match the next step in the shuffled sequence
            CubeId required = Shuffler.NextTarget();
            if (tapped != required)
            {
                StartCoroutine(ShakeCube(Cubes[(int)tapped]));
                return;
            }

            var chapter = LorQBConfig.GetChapter(BallOwner, tapped);
            if (chapter == null)
            {
                // No chapter defined for this pair — show error
                StartCoroutine(ShakeCube(Cubes[(int)tapped]));
                Debug.LogWarning($"[LorQB] No chapter defined for {BallOwner}→{tapped}");
                return;
            }

            StartCoroutine(RunChapter(chapter));
        }

        // ── Chapter execution ─────────────────────────────────────────────────

        IEnumerator RunChapter(ChapterDef ch)
        {
            Phase = GamePhase.Transferring;
            Cubes[(int)ch.From].SetState(CubeState.Transferring);

            // 1. Rotate primary hinge
            yield return StartCoroutine(
                RotateHinge(ch.HingeIndex, ch.RotSign, ch.RotDegrees, LorQBConfig.ROTATION_DURATION));

            // 2. Rotate secondary hinge (T-series)
            if (ch.SecondHinge >= 0)
                yield return StartCoroutine(
                    RotateHinge(ch.SecondHinge, ch.RotSign, ch.RotDegrees, LorQBConfig.ROTATION_DURATION));

            // 3. Arc ball to destination seat
            Vector3 destSeat = LorQBConfig.CubePositions[(int)ch.To] + LorQBConfig.SeatOffset;
            yield return StartCoroutine(
                Ball.ArcToSeat(destSeat, LorQBConfig.EmissiveColors[(int)ch.To], LorQBConfig.TRANSFER_DURATION));

            // 4. Commit ownership and advance sequence
            BallOwner = ch.To;
            Ball.SetOwnerColor(LorQBConfig.EmissiveColors[(int)ch.To]);
            Shuffler.AdvanceStep(ch.To);

            // 5. Return hinge to neutral
            yield return StartCoroutine(
                RotateHinge(ch.HingeIndex, -ch.RotSign, ch.RotDegrees, LorQBConfig.ROTATION_DURATION * 0.5f));

            if (ch.SecondHinge >= 0)
                yield return StartCoroutine(
                    RotateHinge(ch.SecondHinge, -ch.RotSign, ch.RotDegrees, LorQBConfig.ROTATION_DURATION * 0.5f));

            // 6. Refresh visuals
            RefreshCubeHighlights();

            // 7. Check win
            if (Shuffler.IsComplete())
            {
                _timing = false;
                Phase   = GamePhase.Win;
                StartCoroutine(WinSequence());
            }
            else
            {
                Phase = GamePhase.Idle;
            }
        }

        // ── Hinge rotation ────────────────────────────────────────────────────

        IEnumerator RotateHinge(int hingeIndex, float rotSign, float degrees, float duration)
        {
            var hinge = Hinges[hingeIndex];
            if (hinge == null) yield break;

            Vector3 pivot  = LorQBConfig.HingePivots[hingeIndex];
            Vector3 axis   = LorQBConfig.HingeAxes[hingeIndex];
            float   target = degrees * rotSign;
            float   elapsed = 0f, rotated = 0f;

            while (elapsed < duration)
            {
                float dt  = Time.deltaTime;
                elapsed  += dt;
                float t   = Mathf.Clamp01(elapsed / duration);
                float newR = EaseInOut(t) * target;
                hinge.transform.RotateAround(pivot, axis, newR - rotated);
                rotated   = newR;
                yield return null;
            }
            hinge.transform.RotateAround(pivot, axis, target - rotated);
        }

        // ── Highlight: show player which cube to go to next ───────────────────

        void RefreshCubeHighlights()
        {
            CubeId? next = Shuffler.IsComplete() ? (CubeId?)null : Shuffler.NextTarget();

            for (int i = 0; i < Cubes.Length; i++)
            {
                var id = (CubeId)i;
                if (id == BallOwner && Phase != GamePhase.WaitingForBallDrop)
                    Cubes[i].SetState(CubeState.BallInside);
                else if (id == next)
                    Cubes[i].SetState(CubeState.NextTarget);
                else
                    Cubes[i].SetState(CubeState.Idle);
            }
        }

        // ── Shake feedback ────────────────────────────────────────────────────

        IEnumerator ShakeCube(LorQBCubeNode node)
        {
            var origin = node.transform.localPosition;
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float x = Mathf.Sin(t * 55f) * 0.06f * (1f - t / 0.35f);
                node.transform.localPosition = origin + new Vector3(x, 0, 0);
                yield return null;
            }
            node.transform.localPosition = origin;
        }

        // ── Win ───────────────────────────────────────────────────────────────

        IEnumerator WinSequence()
        {
            for (int pulse = 0; pulse < 3; pulse++)
            {
                foreach (var c in Cubes) c.SetState(CubeState.Win);
                yield return new WaitForSeconds(0.35f);
                foreach (var c in Cubes) c.SetState(CubeState.Idle);
                yield return new WaitForSeconds(0.25f);
            }
            foreach (var c in Cubes) c.SetState(CubeState.Win);

            // Auto-shuffle for next turn after 2s
            yield return new WaitForSeconds(2f);
            Shuffler.Shuffle();
        }

        // ── HUD ───────────────────────────────────────────────────────────────

        GUIStyle _seqStyle, _winStyle, _timerStyle, _hintStyle;

        void OnGUI()
        {
            if (_seqStyle == null) InitStyles();

            int sw = Screen.width, sh = Screen.height;

            // ── Order-of-moves bar (bottom) ───────────────────────────────────
            if (Shuffler?.Sequence != null)
            {
                float cellW = sw / 4f;
                float barY  = sh - 72f;

                for (int i = 0; i < Shuffler.Sequence.Length; i++)
                {
                    CubeId id  = Shuffler.Sequence[i];
                    bool done  = i < Shuffler.CurrentStep;
                    bool isCur = i == Shuffler.CurrentStep;

                    Color c = LorQBConfig.EmissiveColors[(int)id];
                    GUI.color = isCur ? c : (done ? c * 0.45f : c * 0.25f);

                    string label = (done ? "✓ " : (isCur ? "► " : "  ")) + LorQBConfig.CubeNames[(int)id];
                    GUI.Box(new Rect(cellW * i + 6, barY, cellW - 12, 54), label, _seqStyle);
                }
                GUI.color = Color.white;
            }

            // ── Timer ──────────────────────────────────────────────────────────
            if (_timing || Phase == GamePhase.Win)
            {
                string t = $"{_elapsed:F2}s";
                GUI.Label(new Rect(sw - 110, 16, 100, 36), t, _timerStyle);
            }

            // ── Phase hint ────────────────────────────────────────────────────
            string hint = Phase switch
            {
                GamePhase.WaitingForBallDrop =>
                    $"Drag ball → {(Shuffler?.Sequence != null ? LorQBConfig.CubeNames[(int)Shuffler.Sequence[0]] : "")} cube",
                GamePhase.Idle =>
                    Shuffler != null && !Shuffler.IsComplete()
                        ? $"Tap {LorQBConfig.CubeNames[(int)Shuffler.NextTarget()]} cube"
                        : "",
                GamePhase.Transferring => "...",
                GamePhase.Win          => "COMPLETE!",
                _ => ""
            };
            GUI.color = new Color(1, 1, 1, 0.55f);
            GUI.Label(new Rect(sw * 0.5f - 120, 16, 240, 32), hint, _hintStyle);
            GUI.color = Color.white;

            // ── Win banner ────────────────────────────────────────────────────
            if (Phase == GamePhase.Win)
            {
                GUI.color = new Color(1f, 0.85f, 0.2f, 1f);
                GUI.Label(new Rect(sw * 0.5f - 160, sh * 0.35f, 320, 80), $"YOU WIN!\n{_elapsed:F2}s", _winStyle);
                GUI.color = Color.white;
            }
        }

        void InitStyles()
        {
            _seqStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 13, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _seqStyle.normal.textColor = Color.white;

            _winStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 44, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _winStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

            _timerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            _timerStyle.normal.textColor = Color.white;

            _hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _hintStyle.normal.textColor = Color.white;
        }

        static float EaseInOut(float t) =>
            t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }
}
