using System.Numerics;
using RubiksCube.Core;

namespace RubiksCube.Scenes;

public class SecondScene : Scene
{
    public SecondScene() : base(nameof(SecondScene)) { }

    public override void OnUpdate()
    {
        DrawTextEx(
            GetFontDefault(),
            nameof(SecondScene),
            new Vector2(Application.WindowSize.Width / 2f, Application.WindowSize.Height / 2f),
            24f,
            2.5f,
            Color.WHITE
        );
    }

    public override void OnExit()
    {
        TraceLog(TraceLogLevel.LOG_INFO, $"SCENE: [{Name}] Exit");
    }
}