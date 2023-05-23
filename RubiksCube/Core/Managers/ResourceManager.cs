namespace RubiksCube.Core.Managers;

public static class ResourceManager
{
    private static readonly Dictionary<string, Texture2D> _textures;
    private static readonly Dictionary<string, Model> _models;

    static ResourceManager()
    {
        _textures = new Dictionary<string, Texture2D>();
        _models = new Dictionary<string, Model>();
    }

    #region Texture

    public static Texture2D LoadTexture(string name, string path)
    {
        TraceLog(TraceLogLevel.LOG_INFO, $"Resource Manager: Texture [{name}] loaded");
        _textures.Add(name, Raylib.LoadTexture(path));
        return GetTexture(name);
    }

    public static Texture2D GetTexture(string name) => _textures[name];

    private static void UnloadTextures()
    {
        foreach (var (name, texture) in _textures)
        {
            TraceLog(TraceLogLevel.LOG_INFO, $"Resource Manager: Texture [{name}] unloaded");
            UnloadTexture(texture);
        }
    }

    #endregion

    #region Model

    public static Model LoadModel(string name, string path)
    {
        TraceLog(TraceLogLevel.LOG_INFO, $"Resource Manager: Model [{name}] loaded");
        _models.Add(name, Raylib.LoadModel(path));
        return GetModel(name);
    }

    public static void SetModelTexture(string modelName, string textureName)
    {
        var model = GetModel(modelName);
        var texture = GetTexture(textureName);
        SetMaterialTexture(ref model, 0, MaterialMapIndex.MATERIAL_MAP_DIFFUSE, ref texture);
    }

    public static Model GetModel(string name) => _models[name];

    private static void UnloadModels()
    {
        foreach (var (name, model) in _models)
        {
            TraceLog(TraceLogLevel.LOG_INFO, $"Resource Manager: Model [{name}] unloaded");
            UnloadModel(model);
        }
    }

    #endregion

    public static void Shutdown()
    {
        UnloadTextures();
        UnloadModels();
    }
}