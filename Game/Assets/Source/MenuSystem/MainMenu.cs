using UnityEngine;

namespace Source.MenuSystem
{
    [RequireComponent(typeof(MenuManager))]
    public class MainMenu : MonoBehaviour, IInputHandler
    {
        private MenuManager menuManager;

        void Start() {
            menuManager = GetComponent<MenuManager>();
        }

        public void Activate()
        {
            print("Activated main menu");
        }

        public void Deactivate()
        {
            print("Deactivated main menu");
        }

        public void HandleInput(InputEvent inputEvent)
        {
            if (inputEvent.key == KeyCode.Escape && inputEvent.type == EventType.KeyDown) {
                menuManager.PopMenu();
            }
        }
    }
}
