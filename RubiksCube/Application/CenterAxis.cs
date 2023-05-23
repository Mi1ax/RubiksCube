using System.Numerics;

namespace RubiksCube.Application;

public class CenterAxis
{
    public Vector3 Position;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;

    private readonly Vector3 _centerPosition;

    public CenterAxis(Vector3 position)
    {
        Position = _centerPosition = position;
    }
    
    public void Reset()
    {
        Position = _centerPosition;
        Rotation = Vector3.Zero;
        Scale = Vector3.One; 
    }
}