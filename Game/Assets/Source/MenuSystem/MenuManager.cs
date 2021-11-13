using UnityEngine;
using System.Collections.Generic;
public enum Menu {
    Game, 
    Main
}

[RequireComponent(typeof(GameMenu))]
[RequireComponent(typeof(MainMenu))]
public class MenuManager : MonoBehaviour
{
    Stack<IInputHandler> menuStack = new Stack<IInputHandler>();
    Dictionary<Menu, IInputHandler> menuList = new Dictionary<Menu, IInputHandler>();
    
    public void CatchInput(InputEvent inputEvent) {
        // redirect to current active menu
        menuStack.Peek().HandleInput(inputEvent);
    }

    // probably don't send the actual menus, just an id
    public void PushMenu(IInputHandler menu) {
        menuStack.Peek()?.Deactivate();
        menuStack.Push(menu);
        menuStack.Peek().Activate();
    }

    public void PushMenu(Menu menu) {
        menuStack.Peek()?.Deactivate();
        menuStack.Push(menuList[menu]);
        menuStack.Peek().Activate();
    }

    public void PopMenu() {
        menuStack.Peek().Deactivate();
        menuStack.Pop();
        menuStack.Peek().Activate();
    }

    void Start() {
        menuList.Add(Menu.Game, GetComponent<GameMenu>());
        menuList.Add(Menu.Main, GetComponent<MainMenu>());
        
        menuStack.Push(menuList[Menu.Game]);
    }
}
