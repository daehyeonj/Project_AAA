using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class BootstrapInputGate : MonoBehaviour
{
    private PrototypeDebugHUD _debugHud;
    private PrototypePresentationShell _presentationShell;

    private void Awake()
    {
        EnsureInputSurfaces();
    }

    public bool AreKeyboardShortcutsBlocked()
    {
        EnsureDebugHUD();
        return _debugHud != null && _debugHud.IsSearchFieldFocused;
    }

    public bool AreDungeonKeyboardInputsBlocked()
    {
        EnsureDebugHUD();
        return AreKeyboardShortcutsBlocked() || (_debugHud != null && _debugHud.ShouldBlockDungeonInput);
    }

    public bool IsWorldSelectionBlocked(Vector2 screenPosition)
    {
        return IsPresentationShellBlockingPointer(screenPosition);
    }

    public Keyboard ResolveDungeonKeyboardInput(Keyboard keyboard)
    {
        return AreDungeonKeyboardInputsBlocked() ? null : keyboard;
    }

    public Mouse ResolveDungeonPointerInput()
    {
        Mouse dungeonMouse = Mouse.current;
        if (dungeonMouse != null && IsPresentationShellBlockingPointer(dungeonMouse.position.ReadValue()))
        {
            return null;
        }

        return dungeonMouse;
    }

    private void EnsureInputSurfaces()
    {
        EnsureDebugHUD();
        EnsurePresentationShell();
    }

    private void EnsureDebugHUD()
    {
        _debugHud = GetComponent<PrototypeDebugHUD>();
        if (_debugHud == null)
        {
            _debugHud = gameObject.AddComponent<PrototypeDebugHUD>();
        }
    }

    private void EnsurePresentationShell()
    {
        _presentationShell = GetComponent<PrototypePresentationShell>();
        if (_presentationShell == null)
        {
            _presentationShell = gameObject.AddComponent<PrototypePresentationShell>();
        }
    }

    private bool IsPresentationShellBlockingPointer(Vector2 screenPosition)
    {
        EnsurePresentationShell();
        return _presentationShell != null && _presentationShell.IsPointerOverBlockingUi(screenPosition);
    }
}
