using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Events;

public class VRUIBuilder : EditorWindow
{
    [MenuItem("Tools/Generate VR UI")]
    public static void GenerateVRUI()
    {
        // 1. Create VR UI Canvas
        GameObject canvasObj = new GameObject("VR-UI-Canvas");
        canvasObj.transform.position = new Vector3(0, 1.5f, 2f); // Eye-level, 2m in front
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800, 600);
        canvasRect.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Add UIManager script
        UIManager uiManager = canvasObj.AddComponent<UIManager>();
        SerializedObject so = new SerializedObject(uiManager);

        // 3. Create Main Menu Panel
        GameObject mainMenu = CreatePanel(canvasObj.transform, "MainMenuPanel", new Color(0.1f, 0.1f, 0.1f));
        CreateTMPText(mainMenu.transform, "Main Menu", 48, new Vector2(0, 200));
        CreateButton(mainMenu.transform, "Start", new Vector2(0, 50), uiManager, "OnStartPressed");
        CreateButton(mainMenu.transform, "Learn More", new Vector2(0, -50), uiManager, "OnLearnMorePressed");
        CreateButton(mainMenu.transform, "Exit", new Vector2(0, -150), uiManager, "OnExitPressed");
        so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenu;

        // 4. Create Condition Selection Panel
        GameObject conditionPanel = CreatePanel(canvasObj.transform, "ConditionSelectionPanel", new Color(0.15f, 0.15f, 0.15f));
        CreateTMPText(conditionPanel.transform, "Select Condition", 36, new Vector2(0, 220));
        CreateConditionButton(conditionPanel.transform, "A", new Vector2(-150, 0), uiManager, "A");
        CreateConditionButton(conditionPanel.transform, "B", new Vector2(-50, 0), uiManager, "B");
        CreateConditionButton(conditionPanel.transform, "C", new Vector2(50, 0), uiManager, "C");
        CreateConditionButton(conditionPanel.transform, "D", new Vector2(150, 0), uiManager, "D");
        so.FindProperty("conditionSelectionPanel").objectReferenceValue = conditionPanel;

        // 5. Create Stress Task Instruction UI
        GameObject instructionPanel = CreatePanel(canvasObj.transform, "InstructionPanel", new Color(0.1f, 0.2f, 0.1f));
        CreateTMPText(instructionPanel.transform, "Reverse Arithmetic Task\n\u2022 Start from 100\n\u2022 Subtract 7 each time\n\u2022 Say the answer out loud", 32, new Vector2(0, 100));
        CreateTMPText(instructionPanel.transform, "Example: 100 \u2192 93 \u2192 86 \u2192 79", 28, new Vector2(0, -100));
        CreateButton(instructionPanel.transform, "Begin", new Vector2(0, -220), uiManager, "OnBeginTaskPressed");
        so.FindProperty("instructionsPanel").objectReferenceValue = instructionPanel;

        // 6. Create Stress Task UI
        GameObject taskPanel = CreatePanel(canvasObj.transform, "StressTaskPanel", new Color(0.2f, 0.1f, 0.1f));
        TextMeshProUGUI progText = CreateTMPText(taskPanel.transform, "100 - 7 \u2192 93 - 7", 48, new Vector2(0, 80));
        TextMeshProUGUI instrText = CreateTMPText(taskPanel.transform, "Say the next number out loud.", 32, new Vector2(0, -80));
        so.FindProperty("stressTaskPanel").objectReferenceValue = taskPanel;
        so.FindProperty("progressionText").objectReferenceValue = progText;
        so.FindProperty("taskInstructionText").objectReferenceValue = instrText;

        // 7. Create Transition UI
        GameObject transitionPanel = CreatePanel(canvasObj.transform, "TransitionPanel", new Color(0.1f, 0.1f, 0.3f));
        CreateTMPText(transitionPanel.transform, "Good job.\n\nNow we will move to a calming exercise.", 42, Vector2.zero);
        so.FindProperty("transitionPanel").objectReferenceValue = transitionPanel;

        // Apply SerializedObject changes and register Undo
        so.ApplyModifiedProperties();
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create VR UI");

        // Set initial state visibility (Main Menu visible, all others hidden)
        mainMenu.SetActive(true);
        conditionPanel.SetActive(false);
        instructionPanel.SetActive(false);
        taskPanel.SetActive(false);
        transitionPanel.SetActive(false);

        Debug.Log("VR UI Hierarchy created successfully! Hierarchy is visible in the scene.");
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(color.r, color.g, color.b, 0.9f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(750, 550);
        return panel;
    }

    private static TextMeshProUGUI CreateTMPText(Transform parent, string text, float fontSize, Vector2 anchoredPosition)
    {
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = tmpText.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700, 300);
        rect.anchoredPosition = anchoredPosition;
        return tmpText;
    }

    private static void CreateButton(Transform parent, string label, Vector2 position, MonoBehaviour listener, string methodName)
    {
        GameObject bObj = new GameObject(label + " Button");
        bObj.transform.SetParent(parent, false);
        Image img = bObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        Button btn = bObj.AddComponent<Button>();

        // Add Persistent Listener in Editor
        UnityEventTools.AddVoidPersistentListener(btn.onClick, (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), listener, methodName));

        RectTransform rect = bObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 60);
        rect.anchoredPosition = position;

        GameObject btObj = new GameObject("Label");
        btObj.transform.SetParent(bObj.transform, false);
        TextMeshProUGUI tmpText = btObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = label;
        tmpText.fontSize = 24;
        tmpText.alignment = TextAlignmentOptions.Center;
        
        RectTransform btnTextRect = tmpText.GetComponent<RectTransform>();
        btnTextRect.sizeDelta = new Vector2(180, 40);
    }

    private static void CreateConditionButton(Transform parent, string condition, Vector2 position, UIManager listener, string paramValue)
    {
        GameObject bObj = new GameObject("Condition " + condition + " Button");
        bObj.transform.SetParent(parent, false);
        Image img = bObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.4f, 0.5f, 1f);
        Button btn = bObj.AddComponent<Button>();

        // Use UnityEventTools for string parameters
        System.Reflection.MethodInfo method = listener.GetType().GetMethod("SelectCondition");
        UnityEventTools.AddStringPersistentListener(btn.onClick, (UnityEngine.Events.UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<string>), listener, method), paramValue);

        RectTransform rect = bObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80);
        rect.anchoredPosition = position;

        GameObject btObj = new GameObject("Label");
        btObj.transform.SetParent(bObj.transform, false);
        TextMeshProUGUI tmpText = btObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = condition;
        tmpText.fontSize = 32;
        tmpText.alignment = TextAlignmentOptions.Center;
    }
}

