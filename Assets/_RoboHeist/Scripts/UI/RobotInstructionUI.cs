using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

public class RobotInstructionUI : MonoBehaviour
{
	[Header("References")]
	public VisualTreeAsset SingleInstructionUI;
	public Sprite ForwardSprite;
	public Sprite LeftSprite;
    public Sprite RightSprite;
    public Light selectionSpotlight;

    [Header("Runtime")]
    public RobotEntityBehaviour VisualizingRobot;

    private UIDocument[] allUIDocuments;
	private UIDocument uiDocument;
	private VisualElement ui;
	private VisualElement instructionListContainer;
	private List<Instruction> visualizedInstructions;
    private Label headerUI;

	// Drag n drop
	private bool draggingInstructionPointer = false;
	private RobotState lastRobotState;
    private bool draggingInstruction = false;
    private Instruction draggedInstruction = null;

	void Start()
	{
		uiDocument = GetComponent<UIDocument>();
		ui = uiDocument.rootVisualElement;
		instructionListContainer = ui.Q("InstructionList");
        headerUI = ui.Q<Label>("Header");

        allUIDocuments = FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"{allUIDocuments.Length}");
    }

    private Sprite GetIconForInstruction(Instruction instruction)
	{
		if (instruction == null) return null;

		switch (instruction)
		{
			case MoveForward: return ForwardSprite;
			case TurnLeft: return LeftSprite;
			case TurnRight: return RightSprite;
			default: return null;
		}
	}

	private void OnInstructionPointerMouseDown(PointerDownEvent e, int instructionIndex)
	{
		// On left mouse button
		if (draggingInstruction || e.button != (int)MouseButton.LeftMouse)
			return;

		// Start dragging
		draggingInstructionPointer = true;
		instructionListContainer[instructionIndex].Q("InstructionPointer").CapturePointer(e.pointerId);

		// Pause robot execution
        lastRobotState = VisualizingRobot.CurrentState;
		VisualizingRobot.CurrentState = RobotState.Idle;

        // We processed this event
        e.StopPropagation();
    }

	private int InstructionUnderMouse(Vector2 screenPos)
	{
		// Get all ui elements under the mouse
        List<VisualElement> visualElements = new List<VisualElement>();
        uiDocument.runtimePanel.PickAll(screenPos, visualElements);

        // Find one representing a robot instruction
        foreach (VisualElement visualElement in visualElements)
        {
            if (visualElement.dataSource is Instruction)
            {
                int hoveredInstructionIndex = instructionListContainer.IndexOf(visualElement);
                return hoveredInstructionIndex;
            }
        }

		return -1;
    }

    private void OnInstructionPointerMouseMove(PointerMoveEvent e, int originalInstructionIndex)
	{
        // Only handle left mouse buttons dragging
        if (!draggingInstructionPointer)
			return;

        // Get the element under the mouse
        // Set the instruction index to visually indicate which instruction is going to be executed
        int hoveredInstructionIndex = InstructionUnderMouse(e.position);
		if (hoveredInstructionIndex != -1)
            VisualizingRobot.instructionQueue.SetInstructionPointer(hoveredInstructionIndex+1);
    }

    private void OnInstructionPointerMouseUp(PointerUpEvent e, int originalInstructionIndex)
    {
        // Only handle dragging left mouse button up
        if (!draggingInstructionPointer) 
			return;

		// Stop dragging
		draggingInstructionPointer = false;
		instructionListContainer[originalInstructionIndex].Q("InstructionPointer").ReleasePointer(e.pointerId);

        // Get the element under the mouse
        // Set the instruction index to the instruction tha is going to be executed
        int hoveredInstructionIndex = InstructionUnderMouse(e.position);
        if (hoveredInstructionIndex != -1)
            VisualizingRobot.instructionQueue.SetInstructionPointer(hoveredInstructionIndex);

		// Continue running, will directly execute the instruction at the instruction pointer
        VisualizingRobot.CurrentState = lastRobotState;

        // Handled event
        e.StopPropagation();
    }

    private void OnInstructionMouseDown(PointerDownEvent e, int instructionIndex)
    {
        // On left mouse button
        if (draggingInstructionPointer || e.button != (int)MouseButton.LeftMouse)
            return;

        // Start dragging
        draggingInstruction = true;
        instructionListContainer[instructionIndex].CapturePointer(e.pointerId);
        draggedInstruction = visualizedInstructions[instructionIndex];

        // Pause robot execution
        lastRobotState = VisualizingRobot.CurrentState;
        VisualizingRobot.CurrentState = RobotState.Idle;

        // We processed this event
        e.StopPropagation();
    }

    private void OnInstructionMouseMove(PointerMoveEvent e)
    {
        // Only handle left mouse buttons dragging
        if (!draggingInstruction)
            return;

        // Get the element under the mouse
        // move the instruction
        int hoveredInstructionIndex = InstructionUnderMouse(e.position);
        if (hoveredInstructionIndex != -1)
        {
            int dragged_instruction_index = visualizedInstructions.IndexOf(draggedInstruction);
            //if (hoveredInstructionIndex > dragged_instruction_index)
            //    hoveredInstructionIndex--;

            visualizedInstructions.Remove(draggedInstruction);
            visualizedInstructions.Insert(hoveredInstructionIndex, draggedInstruction);
            VisualizingRobot.instructionQueue.SetInstructions(visualizedInstructions.ToArray());
        }
    }

    private void OnInstructionMouseUp(PointerUpEvent e)
    {
        // Only handle dragging left mouse button up
        if (!draggingInstruction)
            return;

        // Stop dragging
        draggingInstruction = false;
        int dragged_instruction_index = visualizedInstructions.IndexOf(draggedInstruction);
        foreach (var entryUI in instructionListContainer.Children())
            entryUI.ReleasePointer(e.pointerId);

        // Get the element under the mouse
        // Move the instruction for the final time
        int hoveredInstructionIndex = InstructionUnderMouse(e.position);
        if (hoveredInstructionIndex != -1)
        {
            //if (hoveredInstructionIndex > dragged_instruction_index)
            //    hoveredInstructionIndex--;

            visualizedInstructions.Remove(draggedInstruction);
            visualizedInstructions.Insert(hoveredInstructionIndex, draggedInstruction);
            VisualizingRobot.instructionQueue.SetInstructions(visualizedInstructions.ToArray());
        }

        draggedInstruction = null;

        // Continue running, will directly execute the instruction at the instruction pointer
        VisualizingRobot.CurrentState = lastRobotState;

        // Handled event
        e.StopPropagation();
    }

    void Update()
	{
        RaycastHit hit;
        if ( Input.GetMouseButtonDown((int)MouseButton.LeftMouse) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, float.MaxValue);
 
            RobotEntityBehaviour robot = hit.collider?.GetComponent<RobotEntityBehaviour>();
            Debug.Log($"Changing Instruction UI to {robot}");
            VisualizingRobot = robot;
        }

		if (VisualizingRobot == null)
		{
			// Reset instruction list ui and make invisible
			instructionListContainer.Clear();
            headerUI.text = "No robot selected";
            selectionSpotlight.enabled = false;
            visualizedInstructions = null;
            //uiDocument.enabled = false;
            return;
		}

        //uiDocument.enabled = true;
        selectionSpotlight.enabled = true;
        selectionSpotlight.transform.position = VisualizingRobot.transform.position + new Vector3(0.0f, 5.1f, 0.0f);

        // Repopulate the ui to be in sync with the instruction list
        Instruction[] all_instructions = VisualizingRobot.instructionQueue?.GetAllInstructions().ToArray();
		if (all_instructions == null)
		{
            // Destroy old ui
            instructionListContainer.Clear();
			visualizedInstructions = new();
			return;
		}
		if (visualizedInstructions == null || !visualizedInstructions.SequenceEqual(all_instructions))
		{
			// Destroy old ui
            instructionListContainer.Clear();
            visualizedInstructions = new(all_instructions);

			for (int i = 0; i < all_instructions.Length; i++)
			{
				Instruction instruction = all_instructions[i];

				// Create UI
				SingleInstructionUI.CloneTree(instructionListContainer, out var entryIndex, out var _);
				VisualElement entryUI = instructionListContainer[i];
				entryUI.dataSource = instruction;

				// Connect events
				int i_copy_so_it_gets_copied_by_value_in_the_lambda = i;
				entryUI.Q("InstructionPointer").RegisterCallback<PointerDownEvent>((e) => OnInstructionPointerMouseDown(e, i_copy_so_it_gets_copied_by_value_in_the_lambda));
                entryUI.Q("InstructionPointer").RegisterCallback<PointerMoveEvent>((e) => OnInstructionPointerMouseMove(e, i_copy_so_it_gets_copied_by_value_in_the_lambda));
                entryUI.Q("InstructionPointer").RegisterCallback<PointerUpEvent>((e) => OnInstructionPointerMouseUp(e, i_copy_so_it_gets_copied_by_value_in_the_lambda));
                entryUI.RegisterCallback<PointerDownEvent>((e) => OnInstructionMouseDown(e, i_copy_so_it_gets_copied_by_value_in_the_lambda));
                entryUI.RegisterCallback<PointerMoveEvent>((e) => OnInstructionMouseMove(e));
                entryUI.RegisterCallback<PointerUpEvent>((e) => OnInstructionMouseUp(e));
			}
        }

		// Update values
        if (VisualizingRobot.instructionQueue != null)
		{
            headerUI.text = VisualizingRobot.robotEntityData.robotConfig.DisplayName;

            for (int i = 0; i < all_instructions.Length; i++)
			{
				Instruction instruction = all_instructions[i];
				VisualElement entryUI = instructionListContainer[i];
                entryUI.dataSource = instruction;

                // Update instruction pointer indicator
                bool is_executing_instruction = (i == VisualizingRobot.instructionQueue.GetInstructionPointer()-1);
				entryUI.Q("InstructionPointer").visible = is_executing_instruction;

				// Update label and icon
				entryUI.Q<Label>("Label").text = instruction?.CommandShortForm ?? "NUL";
                entryUI.Q("Icon").style.backgroundImage = new StyleBackground(GetIconForInstruction(instruction));
                entryUI.Q<Label>("Address").text = $"0╳ {VisualizingRobot.basePointer + i * 4:X}";
            }
        }
    }
}
