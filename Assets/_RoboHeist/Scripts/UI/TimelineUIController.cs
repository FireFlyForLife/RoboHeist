using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineUIController : MonoBehaviour
{
	private LevelBuilder levelBuilder;

	private VisualElement ui;
	private SliderInt timeSlider;
	private ToggleButtonGroup playButtonGroup;
	private float theTime;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		levelBuilder = FindAnyObjectByType<LevelBuilder>();

		ui = GetComponent<UIDocument>().rootVisualElement;
		timeSlider = ui.Q<SliderInt>("TimeSlider");
		playButtonGroup = ui.Q<ToggleButtonGroup>("PlayButtonGroup");

		timeSlider.RegisterValueChangedCallback(OnTimeValueChanged);
		playButtonGroup.RegisterValueChangedCallback(OnPlayStateChanged);

		theTime = Time.time;
	}

	private void OnTimeValueChanged(ChangeEvent<int> e)
	{
		// TODO: actually rewind
		timeSlider.SetValueWithoutNotify(e.previousValue);
	}

	private void OnPlayStateChanged(ChangeEvent<ToggleButtonGroupState> e)
	{
		int[] indices = Enumerable.Range(0, 3).ToArray();
		e.newValue.GetActiveOptions(indices);
		int new_index = indices[0];

		switch (new_index)
		{
			case 0: Time.timeScale = 1.0f; break;
			case 1: Time.timeScale = 0.0f; break;
			case 2: Time.timeScale = 0.0f; levelBuilder.InstantiateLevel(TheGrid.Instance.levelData.LevelGrid); break;
		}
	}

	// Update is called once per frame
	void Update()
	{
		// Update time value
		theTime += Time.deltaTime;

		// Update slider UI
		const float instructionDelay = 1.0f;
		int amount_of_ticks = Mathf.FloorToInt(theTime / instructionDelay);
		if (amount_of_ticks > timeSlider.highValue)
			timeSlider.highValue = amount_of_ticks;
		timeSlider.SetValueWithoutNotify(amount_of_ticks);
	}
}
