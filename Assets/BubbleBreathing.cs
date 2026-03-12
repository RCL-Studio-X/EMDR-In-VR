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
    public AnimationCurve breathingCurve = AnimationCurve.InOut(0, 0, 1, 1);

    public float inhaleTime = 4f;
    public float holdAfterInhaleTime = 4f; // Separated holds for flexibility
    public float exhaleTime = 4f;
    public float holdAfterExhaleTime = 4f; 

    Vector2 originalBubbleSize;
    Vector2 originalRingSize;
    Vector3 originalTextScale;

    // Cached objects to avoid Garbage Collection (GC Alloc) in the infinite loop
    WaitForSeconds holdAfterInhaleWait;
    WaitForSeconds holdAfterExhaleWait;
    Coroutine breathingCoroutine;

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
            // INHALE
            breathingText.text = "Breathe In";
            StartCoroutine(AnimateText(1.1f, inhaleTime));

            yield return AnimateSizes(
                originalBubbleSize * inhaleScale,
                originalRingSize * ringScale,
                inhaleTime
            );

            // HOLD
            breathingText.text = "Hold";
            yield return holdAfterInhaleWait;

            // EXHALE
            breathingText.text = "Breathe Out";
            StartCoroutine(AnimateText(0.9f, exhaleTime));

            yield return AnimateSizes(
                originalBubbleSize * exhaleScale,
                originalRingSize, // Ring returns to its normal size
                exhaleTime
            );

            // HOLD
            breathingText.text = "Hold";
            yield return holdAfterExhaleWait;
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
            float t = breathingCurve.Evaluate(time / duration);

            bubbleRect.sizeDelta = Vector2.LerpUnclamped(bubbleStart, bubbleTarget, t);
            outerRing.sizeDelta = Vector2.LerpUnclamped(ringStart, ringTarget, t);

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