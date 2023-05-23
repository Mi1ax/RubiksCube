using System.Numerics;
using RubiksCube.Core;
using RubiksCube.Core.Managers;

namespace RubiksCube.Scenes;

public class MainScene : Scene
{
    private readonly Model _cube;
    private readonly Camera _camera;
    
    public MainScene() : base(nameof(MainScene))
    {
        ResourceManager.LoadTexture("CubeTexture", "Assets/Textures/Cube.png");
        _cube = ResourceManager.LoadModel("CubeModel", "Assets/Models/cube.obj");
        ResourceManager.SetModelTexture("CubeModel", "CubeTexture");
        _camera = new Camera();
    }
    
    public override void OnUpdate()
    {
        _camera.Update();
        
        Renderer.Begin(_camera);
        DrawModel(_cube, Vector3.Zero, 1f, Color.WHITE);
        DrawGrid(10, 1f);
        Renderer.End();
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