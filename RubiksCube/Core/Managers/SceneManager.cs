using ImGuiNET;

namespace RubiksCube.Core.Managers;

public static class SceneManager
{
    private static Scene? _selectedScene;
    private static int _selectedSceneIndex = -1;
    private static readonly Dictionary<string, Scene> _scenes;
    
    static SceneManager()
    {
        _scenes = new Dictionary<string, Scene>();
    }

    public static void AddScene(Scene scene, bool rewriteExisting = false)
    {
        if (!rewriteExisting && _scenes.ContainsKey(scene.Name))
            throw new Exception($"Scene with name: {scene.Name} already exist");
        _scenes[scene.Name] = scene;
    }

    public static void SelectScene(int index)
    {
        _selectedSceneIndex = index;
        _selectedScene?.OnExit();
        _selectedScene = _scenes.ElementAt(index).Value;
    }

    public static void OnUpdate()
    {
        _selectedScene?.OnUpdate();
    }

    public static void Shutdown()
    {
        foreach (var scene in _scenes)
        {
            scene.Value.Unload();
        }
    }

    public static void ImGuiSceneSelection()
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