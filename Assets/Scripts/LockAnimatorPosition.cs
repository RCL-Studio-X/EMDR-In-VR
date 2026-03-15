using UnityEngine;

/// <summary>
/// Locks the LOCAL Y (and optionally Z) of this GameObject after the Animator runs.
/// Add this to SM_Single_Big (the child mesh), NOT the root butterfly prefab.
/// This prevents the built-in fly animation from drifting the body up/down
/// while still allowing the EMDRController to move the root in world space.
/// </summary>
public class LockAnimatorPosition : MonoBehaviour
{
    [Tooltip("Lock the local Y position.")]
    public bool lockY = true;
    [Tooltip("Lock the local Z position.")]
    public bool lockZ = false;

    private float lockedLocalY;
    private float lockedLocalZ;

    void Start()
    {
        lockedLocalY = transform.localPosition.y;
        lockedLocalZ = transform.localPosition.z;
    }

    // LateUpdate runs AFTER the Animator, overriding its local position offsets
    void LateUpdate()
    {
        Vector3 localPos = transform.localPosition;
        if (lockY) localPos.y = lockedLocalY;
        if (lockZ) localPos.z = lockedLocalZ;
        transform.localPosition = localPos;
    }
}
