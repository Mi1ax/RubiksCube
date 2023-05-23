using System.Numerics;
using RubiksCube.Core;
using RubiksCube.Core.Managers;

namespace RubiksCube.Scenes;

public class MainScene : Scene
{
    private readonly Model _cube;
    private readonly Camera3D _camera3D;
    
    public MainScene() : base(nameof(MainScene))
    {
        ResourceManager.LoadTexture("CubeTexture", "Assets/Textures/Cube.png");
        _cube = ResourceManager.LoadModel("CubeModel", "Assets/Models/cube.obj");
        ResourceManager.SetModelTexture("CubeModel", "CubeTexture");
        _camera3D = new Camera3D
        {
            position = new Vector3(0.0f, 10.0f, 10.0f),
            target = Vector3.Zero,
            up = Vector3.UnitY,
            fovy = 45.0f,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
    }
    
    public override void OnUpdate()
    {
        BeginMode3D(_camera3D);
        DrawModel(_cube, Vector3.Zero, 1f, Color.WHITE);
        DrawGrid(10, 1f);
        EndMode3D();
    }

    public override void OnExit()
    {
        TraceLog(TraceLogLevel.LOG_INFO, $"SCENE: [{Name}] Exited");
    }
    
    public override void Unload()
    {
        TraceLog(TraceLogLevel.LOG_INFO, $"SCENE: [{Name}] Unloaded");
    }
}