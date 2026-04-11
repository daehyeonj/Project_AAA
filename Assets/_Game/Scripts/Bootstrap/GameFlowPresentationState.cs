using UnityEngine;

public sealed class GameFlowPresentationState
{
    public GameStateId CurrentStateId = GameStateId.Boot;
    public Color BackgroundColor = Color.black;
    public bool ShowWorldSim;
    public bool ShowDungeonRun;
    public bool EnableWorldCamera;
    public bool RequiresDungeonCameraConfigure;
}
