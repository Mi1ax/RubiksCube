using System.Numerics;
using RubiksCube.Core;

namespace RubiksCube.Application.Scenes;

public class TestScene : Scene
{
    public TestScene() : base(nameof(TestScene)) { }

    public override void Update()
    {
        DrawTextEx(
            GetFontDefault(),
            nameof(TestScene),
            new Vector2(Application.WindowSize.Width / 2f, Application.WindowSize.Height / 2f),
            24f,
            2.5f,
            Color.WHITE
        );
    }

    public override void UpdateImGui()
    {
        
    }

    public override void Exit()
    {
        TraceLog(TraceLogLevel.LOG_INFO, $"SCENE: [{Name}] Exited");
    }
    
    public override void Unload()
    {
        TraceLog(TraceLogLevel.LOG_INFO, $"SCENE: [{Name}] Unloaded");
    }
}