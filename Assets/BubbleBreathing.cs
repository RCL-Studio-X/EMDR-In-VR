using UnityEngine;
using TMPro;
using System.Collections;

public class BubbleBreathing : MonoBehaviour
{
    public TextMeshProUGUI breathingText;
    public RectTransform bubbleRect;
    public RectTransform outerRing;

    Vector2 originalBubbleSize;
    Vector2 originalRingSize;
    Vector3 originalTextScale;

    public float inhaleScale = 1.3f;
    public float exhaleScale = 1.0f;
    public float ringScale = 1.035f;

    public float inhaleTime = 4f;
    public float holdTime = 4f;
    public float exhaleTime = 4f;

    void Start()
    {
        originalBubbleSize = bubbleRect.sizeDelta;
        originalRingSize = outerRing.sizeDelta;
        originalTextScale = breathingText.transform.localScale;

        StartCoroutine(BreathingLoop());
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
            yield return new WaitForSeconds(holdTime);

            // EXHALE
            breathingText.text = "Breathe Out";
            StartCoroutine(AnimateText(0.9f, exhaleTime));

            yield return AnimateSizes(
                originalBubbleSize * exhaleScale,
                originalRingSize,
                exhaleTime
            );

            // HOLD
            breathingText.text = "Hold";
            yield return new WaitForSeconds(holdTime);
        }
    }

    IEnumerator AnimateSizes(Vector2 bubbleTarget, Vector2 ringTarget, float duration)
    {
        Vector2 bubbleStart = bubbleRect.sizeDelta;
        Vector2 ringStart = outerRing.sizeDelta;

        float time = 0;

        while (time < duration)
        {
            bubbleRect.sizeDelta = Vector2.Lerp(bubbleStart, bubbleTarget, time / duration);
            outerRing.sizeDelta = Vector2.Lerp(ringStart, ringTarget, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

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
            breathingText.transform.localScale =
                Vector3.Lerp(startScale, targetScale, time / duration);

            time += Time.deltaTime;
            yield return null;
        }
    }
}