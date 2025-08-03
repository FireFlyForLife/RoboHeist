using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineUIController : MonoBehaviour
{
	private LevelBuilder levelBuilder;

	private VisualElement ui;
	private SliderInt timeSlider;
	private ToggleButtonGroup playButtonGroup;
	private Label pauseNotification;
	private bool isDraggingTimeSlider = false;

    public static float TheTime;
    public const float TickDelay = 0.75f;
	private float maxTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
	{
		levelBuilder = FindAnyObjectByType<LevelBuilder>();

		ui = GetComponent<UIDocument>().rootVisualElement;
		timeSlider = ui.Q<SliderInt>("TimeSlider");
		playButtonGroup = ui.Q<ToggleButtonGroup>("PlayButtonGroup");
		pauseNotification = ui.Q<Label>("PauseNotification");

        timeSlider.RegisterValueChangedCallback(OnTimeValueChanged);
		timeSlider.RegisterCallback<MouseCaptureEvent>(OnTimeValueStartedDragging);
		timeSlider.RegisterCallback<MouseCaptureOutEvent>(OnTimeValueStoppedDragging);
		playButtonGroup.RegisterValueChangedCallback(OnPlayStateChanged);
		playButtonGroup.value = new ToggleButtonGroupState(0b100, 3);

        TheTime = Time.time;
	}

    private void OnTimeValueStartedDragging(MouseCaptureEvent e)
    {
		isDraggingTimeSlider = true;
		Debug.Log("Started time dragging");
		Time.timeScale = 0.0f;
    }

    private void OnTimeValueStoppedDragging(MouseCaptureOutEvent e)
    {
		isDraggingTimeSlider = false;
        Debug.Log("Stopped time dragging");

        RobotEntityBehaviour[] robots = RobotEntityBehaviour.allRobots.ToArray(); //FindObjectsByType<RobotEntityBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var robot in robots)
        {
            // Find the last entry we want to keep
            // This will never be -1, since the initial entry we add is set with time=-1, meaning it is always there
            int index = robot.transformHistory.FindLastIndex((historicalTransform) => historicalTransform.time <= TheTime);

            // Set position back
            robot.robotEntityData.position = robot.transformHistory[index].pos;
            robot.robotEntityData.direction = robot.transformHistory[index].direction;
            robot.instructionQueue.SetInstructions(robot.transformHistory[index].instructionQueueState.GetAllInstructions().ToArray());
			robot.instructionQueue.SetInstructionPointer(robot.transformHistory[index].instructionQueueState.GetInstructionPointer());

            // Clear rolled back entries
            if (!isDraggingTimeSlider)
            {
                maxTime = TheTime;
                robot.transformHistory.RemoveRange(index + 1, robot.transformHistory.Count - (index + 1));
            }
        }

		Time.timeScale = 1.0f;
    }

    private void OnTimeValueChanged(ChangeEvent<int> e)
	{
		// Update time value
        TheTime = Mathf.Min(e.newValue * TickDelay, maxTime);
        timeSlider.SetValueWithoutNotify(Mathf.FloorToInt(TheTime / TickDelay)); // In case we clamped the time, reflect that clamp in the UI slider pos

        Debug.Log("Time value dragged!");

        // Rewind robot positions
        RobotEntityBehaviour[] robots = RobotEntityBehaviour.allRobots.ToArray(); //FindObjectsByType<RobotEntityBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (var robot in robots)
		{
			// Find the last entry we want to keep
			// This will never be -1, since the initial entry we add is set with time=-1, meaning it is always there
			int index = robot.transformHistory.FindLastIndex((historicalTransform) => historicalTransform.time <= TheTime);
			
			// Set position back
			robot.robotEntityData.position = robot.transformHistory[index].pos;
			robot.robotEntityData.direction = robot.transformHistory[index].direction;
            robot.instructionQueue.SetInstructions(robot.transformHistory[index].instructionQueueState.GetAllInstructions().ToArray());
            robot.instructionQueue.SetInstructionPointer(robot.transformHistory[index].instructionQueueState.GetInstructionPointer());

            // Clear rolled back entries
            if (!isDraggingTimeSlider)
			{
				maxTime = TheTime;
                robot.transformHistory.RemoveRange(index + 1, robot.transformHistory.Count - (index + 1));
			}
        }
    }

	private void OnPlayStateChanged(ChangeEvent<ToggleButtonGroupState> e)
	{
		int[] indices = Enumerable.Range(0, 3).ToArray();
		e.newValue.GetActiveOptions(indices);
		int new_index = indices[0];

		switch (new_index)
		{
			case 0: Time.timeScale = 1.0f; pauseNotification.visible = false; break;
			case 1: Time.timeScale = 0.0f; pauseNotification.visible = true; break;
			case 2: Time.timeScale = 0.0f; levelBuilder.InstantiateLevel(TheGrid.Instance.levelData.LevelGrid); pauseNotification.visible = true; break;
		}
	}

	// Update is called once per frame
	void Update()
	{
		// Update time value
		TheTime += Time.deltaTime;
		maxTime = Mathf.Max(maxTime, TheTime);

        // Update slider UI
        int amount_of_ticks = Mathf.FloorToInt(TheTime / TickDelay);
		if (amount_of_ticks > timeSlider.highValue)
			timeSlider.highValue = amount_of_ticks;
		timeSlider.SetValueWithoutNotify(amount_of_ticks);
	}
}
