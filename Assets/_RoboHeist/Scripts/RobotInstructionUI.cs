using UnityEngine;
using UnityEngine.UIElements;

public class RobotInstructionUI : MonoBehaviour
{
	[Header("References")]
	public VisualTreeAsset SingleInstructionUI;
	public Sprite BackupSprite;

    [Header("Runtime")]
    public Robot VisualizingRobot;

	private UIDocument uiDocument;
	private VisualElement ui;
	private VisualElement instructionListContainer;

	void Start()
	{
		uiDocument = GetComponent<UIDocument>();
		ui = uiDocument.rootVisualElement;
		instructionListContainer = ui.Q("InstructionList");
    }

	void Update()
	{
		if (VisualizingRobot == null)
		{
            // Reset instruction list ui and make invisible
            instructionListContainer.Clear();
            uiDocument.enabled = false;
			return;
		}

		uiDocument.enabled = true;

        // Repopulate the ui to be in sync with the instruction list
        instructionListContainer.Clear();
		foreach (var instruction in VisualizingRobot.instructionList)
		{
			SingleInstructionUI.CloneTree(instructionListContainer, out var entryIndex, out var _);
			VisualElement entryUI = instructionListContainer[entryIndex];

            entryUI.Q("Icon").style.backgroundImage = new StyleBackground(instruction.Sprite ?? BackupSprite);
            entryUI.Q<Label>("Label").text = instruction.Label;
        }
    }
}
