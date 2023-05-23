using ImGuiNET;

namespace RubiksCube.Core;

public class SceneManager
{
    private static SceneManager? _instance;
    public static SceneManager Instance => _instance ?? throw new ArgumentNullException(nameof(_instance));
    
    private Scene? _selectedScene;
    private int _selectedSceneIndex = -1;
    private readonly Dictionary<string, Scene> _scenes;
    
    public SceneManager()
    {
        if (_instance != null)
            throw new Exception("Only one SceneManager instance is allowed");
        _instance = this;
        _scenes = new Dictionary<string, Scene>();
    }

    public void AddScene(Scene scene, bool rewriteExisting = false)
    {
        if (!rewriteExisting && _scenes.ContainsKey(scene.Name))
            throw new Exception($"Scene with name: {scene.Name} already exist");
        _scenes[scene.Name] = scene;
    }

    public void SelectScene(int index)
    {
        _selectedSceneIndex = index;
        _selectedScene?.OnExit();
        _selectedScene = _scenes.ElementAt(index).Value;
    }

    public void OnUpdate()
    {
        _selectedScene?.OnUpdate();
    }

    public void ImGuiSceneSelection()
    {
        ImGui.Begin("Scenes");
        {
            foreach (var x in _scenes.Select((keyValuePair, index) => new { keyValuePair, index }))
            {
                var i = x.index;
                var sceneName = x.keyValuePair.Key;
                if (!ImGui.Selectable(sceneName, i == _selectedSceneIndex)) continue;
                SelectScene(i);
            }
            ImGui.End();
        }
    }
}