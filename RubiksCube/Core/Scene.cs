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
    
    public abstract void Update();
    public abstract void UpdateImGui();
    public abstract void Exit();
    public abstract void Unload();
}