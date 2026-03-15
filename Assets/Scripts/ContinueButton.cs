using UnityEngine;
using Michsky.MUIP;

/// <summary>
/// Attach this to the Continue button in the TransitionPanel.
/// It wires the ButtonManager's onClick event to UIManager.OnTransitionComplete()
/// without needing the custom editor UI to be visible.
/// </summary>
public class ContinueButton : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    void Start()
    {
        var btn = GetComponent<ButtonManager>();
        if (btn != null && uiManager != null)
        {
            btn.onClick.AddListener(uiManager.OnTransitionComplete);
        }
        else
        {
            if (btn == null) Debug.LogError("ContinueButton: No ButtonManager found on this GameObject.");
            if (uiManager == null) Debug.LogError("ContinueButton: UIManager reference is not set.");
        }
    }
}
