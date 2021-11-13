using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

[RequireComponent(typeof(MenuManager))]
public class GameMenu : MonoBehaviour, IInputHandler
{
    private MenuManager menuManager;

    void Start() {
        menuManager = GetComponent<MenuManager>();
    }

    public void Activate()
    {
        print("Activated game menu");
    }

    public void Deactivate()
    {
        print("Deactivated game menu");
    }

    public void HandleInput(InputEvent inputEvent)
    {
        if (inputEvent.key == KeyCode.Escape && inputEvent.type == EventType.KeyDown) {
            menuManager.PushMenu(Menu.Main);
        }
    }
}
