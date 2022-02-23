using System.Collections.Generic;
using UnityEngine;

namespace Source.MenuSystem
{
    public enum MenuID {
        Game, 
        Main
    }

    [RequireComponent(typeof(GameMenu))]
    [RequireComponent(typeof(MainMenu))]
    public class MenuManager : MonoBehaviour
    {
        private readonly Stack<IInputHandler> _menuStack = new Stack<IInputHandler>();
        private readonly Dictionary<MenuID, IInputHandler> _menuList = new Dictionary<MenuID, IInputHandler>();
    
        public void CatchInput(InputEvent inputEvent) {
            // redirect to current active menu
            _menuStack.Peek().HandleInput(inputEvent);
        }

        // probably don't send the actual menus, just an id
        public void PushMenu(IInputHandler menu) {
            _menuStack.Peek()?.Deactivate();
            _menuStack.Push(menu);
            _menuStack.Peek().Activate();
        }

        public void PushMenu(MenuID menuID) {
            _menuStack.Peek()?.Deactivate();
            _menuStack.Push(_menuList[menuID]);
            _menuStack.Peek().Activate();
        }

        public void PopMenu() {
            _menuStack.Peek().Deactivate();
            _menuStack.Pop();
            _menuStack.Peek().Activate();
        }

        private void Start() {
            _menuList.Add(MenuID.Game, GetComponent<GameMenu>());
            _menuList.Add(MenuID.Main, GetComponent<MainMenu>());
        
            _menuStack.Push(_menuList[MenuID.Game]);
        }
    }
}