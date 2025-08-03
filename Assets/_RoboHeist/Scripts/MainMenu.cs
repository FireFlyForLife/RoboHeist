using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private VisualElement ui;
    private VisualElement creditsPanel;

    void Start()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        creditsPanel = ui.Q("CreditsPanel");

        creditsPanel.visible = false;

        ui.Q<Button>("PlayGameButton").clicked += PlayGameButton_Clicked;
        ui.Q<Button>("CreditsButton").clicked += CreditsButton_Clicked;
        ui.Q<Button>("ExitGameButton").clicked += ExitGameButton_Clicked;
        ui.Q<Button>("CloseCreditsButton").clicked += CloseCreditsButton_Clicked;
    }

    private void PlayGameButton_Clicked()
    {
        SceneManager.LoadScene(1);
    }
    private void CreditsButton_Clicked()
    {
        creditsPanel.visible = (!creditsPanel.visible);
    }
    private void ExitGameButton_Clicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
    }
    private void CloseCreditsButton_Clicked()
    {
        creditsPanel.visible = false;
    }
}
