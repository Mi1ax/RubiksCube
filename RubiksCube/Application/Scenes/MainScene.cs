using System.Numerics;
using ImGuiNET;
using RubiksCube.Core;
using RubiksCube.Core.Graphics;
using RubiksCube.Core.Managers;

namespace RubiksCube.Application.Scenes;

public class MainScene : Scene
{
    private readonly Camera _camera;
    private readonly RubiksCube _rubiksCube;

    public MainScene() : base(nameof(MainScene))
    {
        ResourceManager.LoadTexture("CubeTexture", "Assets/Textures/Cube.png");
        ResourceManager.LoadModel("CubeModel", "Assets/Models/cube.obj");
        ResourceManager.SetModelTexture("CubeModel", "CubeTexture");
        
        _camera = new Camera();
        _rubiksCube = new RubiksCube(Vector3.Zero);
    }
    
    public override void Update()
    {
        _camera.Update();
        if (IsKeyPressed(KeyboardKey.KEY_R))
            _camera.FocalPoint = Vector3.Zero;

        if (ImGui.GetIO().WantCaptureMouse)
            _camera.AllowMovement = false;
        
        if (_rubiksCube.Collision.hit)
            _camera.AllowMovement = false;
        else if (_rubiksCube.Collision.hit && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            _camera.AllowMovement = false;
        else if (!_rubiksCube.Collision.hit && IsMouseButtonUp(MouseButton.MOUSE_BUTTON_LEFT))
            _camera.AllowMovement = true;

        _rubiksCube.Update();

        Renderer.Begin(_camera);
        ClearBackground(Color.RAYWHITE);
        {
            _rubiksCube.Draw(Renderer.GetMouseRay(_camera));
        }
        Renderer.End();
    }

    public override void UpdateImGui()
    {
        _camera.ImGuiSettings();
        _rubiksCube.ImGuiUpdate();
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