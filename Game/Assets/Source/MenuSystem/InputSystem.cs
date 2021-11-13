using System.Diagnostics;
using System.Linq;
using UnityEngine;

// TODO: add documentations for the whole idea
// Docs: https://docs.unity3d.com/ScriptReference/Event.html
// Docs: https://docs.unity3d.com/ScriptReference/EventType.html
// Docs: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch.elapsedticks?view=net-5.0#System_Diagnostics_Stopwatch_ElapsedTicks
// Reference: https://github.com/PunkyIANG/physics-stuff/blob/custom-input-system-test/Assets/Scripts/InputSystem.cs

public readonly struct InputEvent
{
    public readonly KeyCode key;
    public readonly EventType type;
    public readonly long timestamp;

    // for now we're restricting this to only keyboard presses
    // will be extended to mouse/gamepad input later on
    public InputEvent(KeyCode key, EventType type, long timestamp)
    {
        this.key = key;
        this.type = type;
        this.timestamp = timestamp;
    }
}
[RequireComponent(typeof(MenuManager))]
public class InputSystem : MonoBehaviour
{
    private MenuManager _menuManager;
    private readonly EventType[] _acceptedEvents = new EventType[] {
        EventType.KeyDown,
        EventType.KeyUp
    };
    private Stopwatch _stopwatch = new Stopwatch();


    void Start() {
        _stopwatch.Start();
        _menuManager = GetComponent<MenuManager>();
    }

    void OnGUI()
    {
        if (!_acceptedEvents.Contains(Event.current.type))
            return;
        
        // switch (Event.current.type) {
        // }

        _menuManager.CatchInput(new InputEvent(
            Event.current.keyCode, 
            Event.current.type, 
            _stopwatch.ElapsedTicks
            ));
    }
}
