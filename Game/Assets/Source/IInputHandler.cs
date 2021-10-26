public interface IInputHandler {
    void HandleInput(InputEvent inputEvent); 
    void Activate();
    void Deactivate();
}