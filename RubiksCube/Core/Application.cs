using System.Drawing;
using ImGuiNET;
using RubiksCube.Scenes;
using Color = Raylib_cs.Color;

namespace RubiksCube.Core;

public struct WindowSpec
{
    public string Title { get; set; }
    public Size Size { get; set; }
}

public class Application
{
    private static Application? _instance;
    public static Application GetInstance() => _instance ?? throw new ArgumentNullException(nameof(_instance));

    private WindowSpec _windowSpecification;
    private bool _isRunning;

    private SceneManager _sceneManager;
    
    public Application(WindowSpec windowSpecification)
    {
        if (_instance != null)
            throw new Exception("Only one application instance is allowed");
        _instance = this;

        _windowSpecification = windowSpecification;
        
        InitWindow(
            WindowSize.Width, 
            WindowSize.Height, 
            Title
        );
        RayImGui.Setup();

        _sceneManager = new SceneManager();
        
        _sceneManager.AddScene(new MainScene());
        _sceneManager.AddScene(new SecondScene());
    }

    public Size WindowSize
    {
        get => _windowSpecification.Size;
        set
        {
            _windowSpecification.Size = value;
            SetWindowSize(value.Width, value.Height);
        }
    }

    public string Title
    {
        get => _windowSpecification.Title;
        set
        {
            _windowSpecification.Title = value;
            SetWindowTitle(value);
        }
    }

    public void Run()
    {
        _isRunning = true;
        while (!WindowShouldClose() && _isRunning)
        {
            BeginDrawing();
            ClearBackground(Color.BLACK);
            
            _sceneManager.OnUpdate();
            
            RayImGui.Begin();
            {
                _sceneManager.ImGuiSceneSelection();
            }
            RayImGui.End();
            
            EndDrawing();
        }
        RayImGui.Shutdown();
        CloseWindow();
    }
}