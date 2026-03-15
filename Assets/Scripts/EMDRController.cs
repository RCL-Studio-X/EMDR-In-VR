using System.Collections;
using UnityEngine;

/// <summary>
/// Manages which butterfly is shown and how it moves per EMDR condition.
/// Attach this to an empty "ButterflyManager" parent GameObject.
///
/// Condition 1 — Control:              All butterflies hidden.
/// Condition 2 — Bilateral Stimulation: Butterfly_fly_single_big, smooth L→R tracking.
/// Condition 3 — Pro-Saccadic:         Butterfly_idle_single_big, flashes L/R 500ms.
/// Condition 4 — Distraction:          Butterfly_fly_group, random wander.
/// </summary>
public class EMDRController : MonoBehaviour
{
    [Header("Butterfly GameObjects (assign in Inspector)")]
    public GameObject butterfly_C2; // Butterfly_fly_single_big
    public GameObject butterfly_C3; // Butterfly_idle_single_big
    public GameObject butterfly_C4; // Butterfly_fly_group

    [Header("Anchor Points")]
    public Transform leftAnchor;
    public Transform rightAnchor;

    [Header("Timing")]
    public float minSweepTime = 1.3f;
    public float maxSweepTime = 1.7f;
    public float flashDuration = 0.5f;

    [Header("Condition 4 – Distraction")]
    public float wanderRangeX = 2.5f;
    public float wanderRangeY = 0.2f;
    public float minWanderInterval = 2.0f;
    public float maxWanderInterval = 4.0f;
    public float wanderSpeed = 0.6f;

    // Active butterfly this session
    private GameObject activeButterfly;

    void Start()
    {
        // Hide all first
        if (butterfly_C2) butterfly_C2.SetActive(false);
        if (butterfly_C3) butterfly_C3.SetActive(false);
        if (butterfly_C4) butterfly_C4.SetActive(false);

        int condition = 4; //UIManager.EMDRCondition;

        switch (condition)
        {
            case 1:
                // Nothing visible — control condition
                break;

            case 2:
                activeButterfly = butterfly_C2;
                if (activeButterfly) activeButterfly.SetActive(true);
                StartCoroutine(BilateralTracking());
                break;

            case 3:
                activeButterfly = butterfly_C3;
                // Starts hidden — flashes handle visibility
                StartCoroutine(ProSaccadic());
                break;

            case 4:
                activeButterfly = butterfly_C4;
                if (activeButterfly) activeButterfly.SetActive(true);
                StartCoroutine(DistractionMovement());
                break;
        }
    }

    // -----------------------------------------------------------------------
    // Condition 2 — smooth L/R tracking
    IEnumerator BilateralTracking()
    {
        if (activeButterfly == null) yield break;
        bool goingRight = true;

        while (true)
        {
            Vector3 from = goingRight ? leftAnchor.position : rightAnchor.position;
            Vector3 to   = goingRight ? rightAnchor.position : leftAnchor.position;
            float sweepTime = Random.Range(minSweepTime, maxSweepTime);
            float elapsed = 0f;

            while (elapsed < sweepTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / sweepTime);
                activeButterfly.transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }

            activeButterfly.transform.position = to;
            goingRight = !goingRight;
        }
    }

    // -----------------------------------------------------------------------
    // Condition 3 — pro-saccadic flash
    IEnumerator ProSaccadic()
    {
        if (activeButterfly == null) yield break;
        bool showLeft = true;

        while (true)
        {
            activeButterfly.transform.position = showLeft ? leftAnchor.position : rightAnchor.position;
            activeButterfly.SetActive(true);

            yield return new WaitForSeconds(flashDuration);

            activeButterfly.SetActive(false);

            float waitTime = Mathf.Max(0f, Random.Range(minSweepTime, maxSweepTime) - flashDuration);
            yield return new WaitForSeconds(waitTime);

            showLeft = !showLeft;
        }
    }

    // -----------------------------------------------------------------------
    // Condition 4 — random distraction wander
    IEnumerator DistractionMovement()
    {
        if (activeButterfly == null) yield break;
        Vector3 centre = activeButterfly.transform.position;

        while (true)
        {
            Vector3 target = centre + new Vector3(
                Random.Range(-wanderRangeX, wanderRangeX),
                Random.Range(-wanderRangeY, wanderRangeY),
                0f
            );

            while (Vector3.Distance(activeButterfly.transform.position, target) > 0.05f)
            {
                activeButterfly.transform.position = Vector3.MoveTowards(
                    activeButterfly.transform.position, target, wanderSpeed * Time.deltaTime);
                yield return null;
            }

            activeButterfly.transform.position = target;
            yield return new WaitForSeconds(Random.Range(minWanderInterval, maxWanderInterval));
        }
    }
}
