using System.Numerics;
using RubiksCube.Core.Graphics;
using RubiksCube.Core.Managers;

namespace RubiksCube.Application;

public class Cube
{
    private readonly CenterAxis _centerAxis;
    
    private Model _model;

    public Vector3 GridPosition { get; set; } = Vector3.Zero;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;
    
    public Matrix4x4 Transform { get; private set; }

    public bool IsSelected { get; set; }
    
    public Cube(CenterAxis centerAxis)
    {
        _centerAxis = centerAxis;
        _model = ResourceManager.GetModel("CubeModel");
    }

    public unsafe RayCollision? GetCollision(Ray mouseRay)
    {
        var collision = GetRayCollisionMesh(mouseRay, _model.meshes[0], Transform);
        return collision.hit ? collision : null;
    }
    
    public void Draw()
    {
        _model.transform = Matrix4x4.CreateFromQuaternion(Rotation);

        var parentRotationMatrix = IsSelected 
            ? MatrixRotateXYZ(_centerAxis.Rotation * DEG2RAD) 
            : Matrix4x4.Identity;

        var scaleMatrix = MatrixScale(Scale.X, Scale.Y, Scale.Z);
        var translationMatrix = MatrixTranslate(Position.X, Position.Y, Position.Z);

        var transformMatrix = parentRotationMatrix * scaleMatrix * translationMatrix;
            
        Matrix4x4.Invert(transformMatrix, out var invertedTransformMatrix);
        var position = Vector3.Transform(Position, invertedTransformMatrix);

        GridPosition = new Vector3(
            MathF.Round(position.X),
            MathF.Round(position.Y),
            MathF.Round(position.Z)
        );
        
        Transform = Renderer.DrawModel(
            _model,
            _centerAxis.Position + position,
            IsSelected ? _centerAxis.Rotation : Vector3.Zero, 
            Scale,
            //IsSelected ? Color.LIGHTGRAY : Color.WHITE
            Color.WHITE
        );
    }
}