using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public enum UIState
    {
        MainMenu,
        ConditionSelection,
        Instructions,
        StressTask,
        Transition
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject conditionSelectionPanel;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject stressTaskPanel;
    [SerializeField] private GameObject transitionPanel;

    [Header("Stress Task UI Elements")]
    [SerializeField] private TextMeshProUGUI progressionText;
    [SerializeField] private TextMeshProUGUI taskInstructionText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Stress Task Settings")]
    [SerializeField] private float stepInterval = 2.0f;
    [SerializeField] private int targetNumber = 50;

    private UIState currentState;
    private string selectedCondition;
    private int currentNumber = 100;
    private float taskTimer = 60f; // Total time limit if needed
    private bool isTaskActive = false;
    private Coroutine taskCoroutine;

    private void Start()
    {
        SetUIState(UIState.MainMenu);
    }

    private void Update()
    {
        if (isTaskActive)
        {
            taskTimer -= Time.deltaTime;
            UpdateTimerUI();
            if (taskTimer <= 0)
            {
                CompleteTask();
            }
        }
    }

    public void SetUIState(UIState newState)
    {
        currentState = newState;

        // Stop any running task logic if we exit the stress task state
        if (taskCoroutine != null)
        {
            StopCoroutine(taskCoroutine);
            taskCoroutine = null;
        }

        mainMenuPanel.SetActive(newState == UIState.MainMenu);
        conditionSelectionPanel.SetActive(newState == UIState.ConditionSelection);
        instructionsPanel.SetActive(newState == UIState.Instructions);
        stressTaskPanel.SetActive(newState == UIState.StressTask);
        transitionPanel.SetActive(newState == UIState.Transition);

        if (newState == UIState.StressTask)
        {
            StartStressTask();
        }
        else
        {
            isTaskActive = false;
        }
    }

    #region Main Menu Methods
    public void OnStartPressed() => SetUIState(UIState.ConditionSelection);
    public void OnLearnMorePressed() => Debug.Log("Learn More");
    public void OnExitPressed() => Application.Quit();
    #endregion

    #region Condition Selection Methods
    public void SelectCondition(string condition)
    {
        selectedCondition = condition;
        SetUIState(UIState.Instructions);
    }
    #endregion

    #region Instruction Methods
    public void OnBeginTaskPressed() => SetUIState(UIState.StressTask);
    #endregion

    #region Stress Task Methods
    private void StartStressTask()
    {
        currentNumber = 100;
        taskTimer = 60f;
        isTaskActive = true;
        UpdateTaskUI();
        UpdateTimerUI();

        // Start automatic progression
        taskCoroutine = StartCoroutine(TaskSequence());
    }

    private IEnumerator TaskSequence()
    {
        while (isTaskActive && currentNumber > targetNumber)
        {
            yield return new WaitForSeconds(stepInterval);
            AdvanceTask();
        }
        
        // Once we reach the target, wait one last interval and complete
        if (currentNumber <= targetNumber)
        {
            yield return new WaitForSeconds(stepInterval);
            CompleteTask();
        }
    }

    private void UpdateTaskUI()
    {
        int nextNumber = currentNumber - 7;
        progressionText.text = $"{currentNumber} - 7 \u2192 <color=#FF5555>{nextNumber}</color>";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(taskTimer)}s";
    }

    public void AdvanceTask()
    {
        currentNumber -= 7;
        UpdateTaskUI();
        
        if (currentNumber <= targetNumber)
        {
            // The Coroutine will handle final transition
        }
    }

    public void CompleteTask()
    {
        isTaskActive = false;
        if (taskCoroutine != null)
        {
            StopCoroutine(taskCoroutine);
            taskCoroutine = null;
        }
        SetUIState(UIState.Transition);
    }
    #endregion

    #region Transition Methods

    public void OnTransitionComplete()
    {
        SceneManager.LoadScene("Calm Scene");
    }

    #endregion
}
