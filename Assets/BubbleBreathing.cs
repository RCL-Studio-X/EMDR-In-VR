using UnityEngine;
using TMPro;
using System.Collections;

public class BubbleBreathing : MonoBehaviour
{
    public TextMeshProUGUI breathingText;
    public RectTransform bubbleRect;
    public RectTransform outerRing;

    public float inhaleScale = 1.35f; // Increased bubble scale so it approaches the outer ring
    public float exhaleScale = 1.0f;
    public float ringScale = 1.01f; // Made much smaller so it scales significantly less than the bubble

    // An easing curve makes the breathing feel significantly more natural
    public AnimationCurve breathingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // A separate curve to decouple the ring's growth rate from the bubble
    public AnimationCurve ringCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float inhaleTime = 4f;
    public float holdAfterInhaleTime = 4f; // Separated holds for flexibility
    public float exhaleTime = 4f;
    public float holdAfterExhaleTime = 4f; 

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip breatheInClip;
    public AudioClip breatheOutClip;
    public AudioClip shortInClip;
    public AudioClip shortOutClip;
    public AudioClip thirdInClip;
    public AudioClip thirdOutClip;

    [Header("Cycle Settings")]
    [Tooltip("How many full breaths of 'breathe in/out' before switching to 'in/out'.")]
    public int longCycles = 2;
    [Tooltip("How many breaths of the short 'in/out' before reverting back.")]
    public int shortCycles = 1;
    [Tooltip("How many breaths of the third 'inhaling/exhaling' sound before reverting to the start.")]
    public int thirdCycles = 1;

    Vector2 originalBubbleSize;
    Vector2 originalRingSize;
    Vector3 originalTextScale;

    // Cached objects to avoid Garbage Collection (GC Alloc) in the infinite loop
    WaitForSeconds holdAfterInhaleWait;
    WaitForSeconds holdAfterExhaleWait;
    Coroutine breathingCoroutine;

    int currentCycle = 0;

    void Start()
    {
        originalBubbleSize = bubbleRect.sizeDelta;
        originalRingSize = outerRing.sizeDelta;
        originalTextScale = breathingText.transform.localScale;

        // Cache the waits once at start
        holdAfterInhaleWait = new WaitForSeconds(holdAfterInhaleTime);
        holdAfterExhaleWait = new WaitForSeconds(holdAfterExhaleTime);

        // Safely start the loop protecting against duplicates
        if (breathingCoroutine != null) StopCoroutine(breathingCoroutine);
        breathingCoroutine = StartCoroutine(BreathingLoop());
    }

    IEnumerator BreathingLoop()
    {
        while (true)
        {
            int totalCycleLength = longCycles + shortCycles + thirdCycles;
            int cycleIndex = currentCycle % totalCycleLength;
            
            int phase = 0; // 0 = long, 1 = short, 2 = third
            if (cycleIndex < longCycles) phase = 0;
            else if (cycleIndex < longCycles + shortCycles) phase = 1;
            else phase = 2;

            // INHALE
            if (phase == 0)
            {
                breathingText.text = "Breathe In";
                if (audioSource != null && breatheInClip != null) audioSource.PlayOneShot(breatheInClip);
            }
            else if (phase == 1)
            {
                breathingText.text = "In";
                if (audioSource != null && shortInClip != null) audioSource.PlayOneShot(shortInClip);
            }
            else
            {
                breathingText.text = "Inhaling";
                if (audioSource != null && thirdInClip != null) audioSource.PlayOneShot(thirdInClip);
            }
            
            StartCoroutine(AnimateText(1.1f, inhaleTime));

            yield return AnimateSizes(
                originalBubbleSize * inhaleScale,
                originalRingSize * ringScale, // Restored the ring target scale
                inhaleTime
            );

            // HOLD
            breathingText.text = "Hold";
            yield return holdAfterInhaleWait;

            // EXHALE
            if (phase == 0)
            {
                breathingText.text = "Breathe Out";
                if (audioSource != null && breatheOutClip != null) audioSource.PlayOneShot(breatheOutClip);
            }
            else if (phase == 1)
            {
                breathingText.text = "Out";
                if (audioSource != null && shortOutClip != null) audioSource.PlayOneShot(shortOutClip);
            }
            else
            {
                breathingText.text = "Exhaling";
                if (audioSource != null && thirdOutClip != null) audioSource.PlayOneShot(thirdOutClip);
            }
            
            StartCoroutine(AnimateText(0.9f, exhaleTime));

            yield return AnimateSizes(
                originalBubbleSize * exhaleScale,
                originalRingSize, // Ring returns to its normal size
                exhaleTime
            );

            // HOLD
            breathingText.text = "Hold";
            yield return holdAfterExhaleWait;

            currentCycle++;
        }
    }

    IEnumerator AnimateSizes(Vector2 bubbleTarget, Vector2 ringTarget, float duration)
    {
        Vector2 bubbleStart = bubbleRect.sizeDelta;
        Vector2 ringStart = outerRing.sizeDelta;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            
            // Evaluates the ease-in/ease-out curve based on percentage of completion (0.0 to 1.0)
            float bubbleT = breathingCurve.Evaluate(time / duration);
            
            // Decoupled evaluation using a linear curve (or any curve you set in inspector) for the ring
            float ringT = ringCurve.Evaluate(time / duration);

            bubbleRect.sizeDelta = Vector2.LerpUnclamped(bubbleStart, bubbleTarget, bubbleT);
            outerRing.sizeDelta = Vector2.LerpUnclamped(ringStart, ringTarget, ringT);

            yield return null;
        }

        // Lock to exact target sizes at the end
        bubbleRect.sizeDelta = bubbleTarget;
        outerRing.sizeDelta = ringTarget;
    }

    IEnumerator AnimateText(float scaleTarget, float duration)
    {
        Vector3 startScale = breathingText.transform.localScale;
        Vector3 targetScale = originalTextScale * scaleTarget;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = breathingCurve.Evaluate(time / duration);
            
            breathingText.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
            yield return null;
        }
        
        // Prevent floating point drift by locking exactly to target size
        breathingText.transform.localScale = targetScale;
    }
}