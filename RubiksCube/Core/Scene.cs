namespace RubiksCube.Core;

public abstract class Scene
{
    protected readonly Application Application;

    public readonly string Name;
    
    protected Scene(string name)
    {
        Application = Application.GetInstance();
        Name = name;
    }
    
    public abstract void OnUpdate();
    public abstract void OnExit();
    public abstract void Unload();
}