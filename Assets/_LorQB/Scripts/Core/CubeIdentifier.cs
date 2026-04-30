using UnityEngine;

namespace LorQB.Core
{
    /// <summary>
    /// Component that identifies a cube GameObject and exposes its seat transform
    /// (the canonical world-space position the ball occupies while inside this cube).
    /// </summary>
    public class CubeIdentifier : MonoBehaviour
    {
        [SerializeField] private CubeColor cubeColor;

        /// <summary>
        /// The seat is the world position the ball snaps to when it enters this cube.
        /// Assign the matching Seat_[Color] Transform in the Inspector.
        /// </summary>
        [SerializeField] private Transform seatTransform;

        /// <summary>The color/identity of this cube.</summary>
        public CubeColor Color => cubeColor;

        /// <summary>
        /// World position the ball occupies while seated in this cube.
        /// Falls back to this cube's own position when no seat Transform is assigned.
        /// </summary>
        public Vector3 SeatPosition => seatTransform != null ? seatTransform.position : transform.position;

        /// <summary>The seat Transform for external reference (e.g. from BallTransferController).</summary>
        public Transform SeatTransform => seatTransform;
    }
}
