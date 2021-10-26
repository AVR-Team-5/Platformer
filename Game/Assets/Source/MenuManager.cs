using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    IInputHandler activeMenu;
    Stack<IInputHandler> menuStack;

    public void CatchInput(InputEvent inputEvent) {
        // redirect to current active menu
        menuStack.Peek().HandleInput(inputEvent);
    }

    // probably don't send the actual menus, just an id
    public void PushMenu(IInputHandler menu) {
        menuStack.Peek().Deactivate();
        menuStack.Push(menu);
        menuStack.Peek().Activate();
    }

    public void PopMenu() {
        menuStack.Peek().Deactivate();
        menuStack.Pop();
        menuStack.Peek().Activate();
    }
}
