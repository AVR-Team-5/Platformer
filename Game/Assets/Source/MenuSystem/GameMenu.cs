using UnityEngine;

namespace Source.MenuSystem
{
    [RequireComponent(typeof(MenuManager))]
    public class GameMenu : MonoBehaviour, IInputHandler
    {
        private MenuManager _menuManager;
        private PlayerController.PlayerController _playerController;

        void Start() {
            _menuManager = GetComponent<MenuManager>();
            _playerController = FindObjectOfType<PlayerController.PlayerController>();
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
            if (inputEvent.key == KeyCode.Escape && inputEvent.type == EventType.KeyDown) 
                _menuManager.PushMenu(MenuID.Main);
            else 
                _playerController.HandleInput(inputEvent);
        }
    }
}
