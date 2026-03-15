using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the 3D butterfly's behaviour based on the EMDR condition passed
/// from the Stress Scene via UIManager.EMDRCondition.
///
/// Condition 1 — Control:              Butterfly hidden.
/// Condition 2 — Bilateral Stimulation: Smooth L→R→L tracking, 1.3–1.7 s per sweep.
/// Condition 3 — Pro-Saccadic:         Flashes at left OR right anchor for 500 ms,
///                                      alternating sides at the same pace.
/// Condition 4 — Distraction:          Moves to random positions in background.
/// </summary>
public class EMDRController : MonoBehaviour
{
    [Header("Anchor Points (set in Inspector)")]
    [Tooltip("World-space position of the left extent of butterfly travel.")]
    public Transform leftAnchor;
    [Tooltip("World-space position of the right extent of butterfly travel.")]
    public Transform rightAnchor;

    [Header("Timing")]
    [Tooltip("Minimum seconds for one side-to-side sweep (Conditions 2 & 3).")]
    public float minSweepTime = 1.3f;
    [Tooltip("Maximum seconds for one side-to-side sweep (Conditions 2 & 3).")]
    public float maxSweepTime = 1.7f;
    [Tooltip("How long the butterfly is visible per flash in Condition 3 (seconds).")]
    public float flashDuration = 0.5f;

    [Header("Condition 4 – Distraction")]
    [Tooltip("Half-width of the bounding box the butterfly can wander within (metres).")]
    public float wanderRangeX = 2.5f;
    [Tooltip("Half-height of the bounding box the butterfly can wander within (metres).")]
    public float wanderRangeY = 1.0f;
    [Tooltip("Minimum seconds before picking a new random position (Condition 4).")]
    public float minWanderInterval = 2.0f;
    [Tooltip("Maximum seconds before picking a new random position (Condition 4).")]
    public float maxWanderInterval = 4.0f;
    [Tooltip("Speed the butterfly travels toward each random point (metres/sec, Condition 4).")]
    public float wanderSpeed = 0.6f;

    // -----------------------------------------------------------------------
    private Renderer[] renderers;
    private Vector3 centrePosition; // position of the butterfly at scene start

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        centrePosition = transform.position;

        int condition = UIManager.EMDRCondition;

        switch (condition)
        {
            case 1:
                SetVisible(false);
                break;
            case 2:
                SetVisible(true);
                StartCoroutine(BilateralTracking());
                break;
            case 3:
                SetVisible(false); // starts hidden; flashes handle visibility
                StartCoroutine(ProSaccadic());
                break;
            case 4:
                SetVisible(true);
                StartCoroutine(DistractionMovement());
                break;
            default:
                SetVisible(false);
                break;
        }
    }

    // -----------------------------------------------------------------------
    // Condition 2 — smooth left/right tracking
    IEnumerator BilateralTracking()
    {
        bool goingRight = true;

        while (true)
        {
            Vector3 from = goingRight ? leftAnchor.position  : rightAnchor.position;
            Vector3 to   = goingRight ? rightAnchor.position : leftAnchor.position;
            float sweepTime = Random.Range(minSweepTime, maxSweepTime);

            float elapsed = 0f;
            while (elapsed < sweepTime)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, elapsed / sweepTime);
                yield return null;
            }
            transform.position = to;
            goingRight = !goingRight;
        }
    }

    // -----------------------------------------------------------------------
    // Condition 3 — pro-saccadic flash: appear 500 ms then disappear, alternate sides
    IEnumerator ProSaccadic()
    {
        bool showLeft = true;

        while (true)
        {
            // Jump to side
            transform.position = showLeft ? leftAnchor.position : rightAnchor.position;
            SetVisible(true);

            yield return new WaitForSeconds(flashDuration);

            SetVisible(false);

            // Wait for the remainder of the sweep interval before next flash
            float interval = Random.Range(minSweepTime, maxSweepTime);
            float waitTime = Mathf.Max(0f, interval - flashDuration);
            yield return new WaitForSeconds(waitTime);

            showLeft = !showLeft;
        }
    }

    // -----------------------------------------------------------------------
    // Condition 4 — distraction: wander randomly inside a bounding box
    IEnumerator DistractionMovement()
    {
        while (true)
        {
            // Pick a random target within the wander bounds centred on start position
            Vector3 target = centrePosition + new Vector3(
                Random.Range(-wanderRangeX, wanderRangeX),
                Random.Range(-wanderRangeY, wanderRangeY),
                0f
            );

            // Move toward target at wanderSpeed
            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, wanderSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;

            // Pause before choosing next position
            yield return new WaitForSeconds(Random.Range(minWanderInterval, maxWanderInterval));
        }
    }

    // -----------------------------------------------------------------------
    void SetVisible(bool visible)
    {
        foreach (var r in renderers)
            r.enabled = visible;
    }
}
