using UnityEngine;

/// <summary>
/// Locks the Y (and optionally Z) world position of the butterfly after
/// the Animator has run each frame, preventing the built-in
/// animation from drifting it above or below its spawn height.
/// </summary>
public class LockAnimatorPosition : MonoBehaviour
{
    [Tooltip("Lock the Y position to the spawn point.")]
    public bool lockY = true;
    [Tooltip("Lock the Z position to the spawn point.")]
    public bool lockZ = false;

    private float lockedY;
    private float lockedZ;

    void Start()
    {
        lockedY = transform.position.y;
        lockedZ = transform.position.z;
    }

    // LateUpdate runs AFTER the Animator, so this overrides animation offsets
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (lockY) pos.y = lockedY;
        if (lockZ) pos.z = lockedZ;
        transform.position = pos;
    }
}
