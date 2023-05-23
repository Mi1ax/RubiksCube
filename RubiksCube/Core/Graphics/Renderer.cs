using System.Numerics;
using RubiksCube.Application;

namespace RubiksCube.Core.Graphics;

using static Rlgl;

public partial class Renderer
{
    private static readonly Application _application = Application.GetInstance();
    
    public static Ray GetMouseRay(Camera camera)
    {
        var mouse = GetMousePosition();
        var ray = new Ray();

        var x = 2.0f * mouse.X / _application.WindowSize.Width - 1.0f;
        var y = 1.0f - 2.0f * mouse.Y / _application.WindowSize.Height;

        var deviceCoords = new Vector3(x, y, 1.0f);

        var matView = Raymath.MatrixLookAt(camera.Position, camera.FocalPoint, Vector3.UnitY);

        var matProj =
            Raymath.MatrixPerspective(
                camera.Fov * Raylib.DEG2RAD, 
                (double)_application.WindowSize.Width / _application.WindowSize.Height, 
                RL_CULL_DISTANCE_NEAR, RL_CULL_DISTANCE_FAR
            );

        var nearPoint = Raymath.Vector3Unproject(deviceCoords with { Z = 0.0f }, matProj, matView);
        var farPoint = Raymath.Vector3Unproject(deviceCoords with { Z = 1.0f }, matProj, matView);
        var direction = Raymath.Vector3Normalize(Raymath.Vector3Subtract(farPoint, nearPoint));

        ray.position = camera.Position;
        ray.direction = direction;

        return ray;
    }
    
    public static MouseMoveDirection GetMouseMovingDirection()
    {
        var mouseDelta = Vector2.Normalize(GetMouseDelta());
        mouseDelta.X = MathF.Round(mouseDelta.X);
        mouseDelta.Y = MathF.Round(mouseDelta.Y);
        switch (mouseDelta.X)
        {
            case < 0 when mouseDelta.X < mouseDelta.Y:
                return MouseMoveDirection.Left;
            case > 0 when mouseDelta.X > mouseDelta.Y:
                return MouseMoveDirection.Right;
            default:
                switch (mouseDelta.Y)
                {
                    case > 0 when mouseDelta.Y > mouseDelta.X:
                        return MouseMoveDirection.Down;
                    case < 0 when mouseDelta.Y < mouseDelta.X:
                        return MouseMoveDirection.Up;
                }
                break;
        }

        return MouseMoveDirection.None;
    }
    
    public static void Begin(Camera camera)
    {
        rlDrawRenderBatchActive();

        rlMatrixMode(MatrixMode.PROJECTION);
        rlPushMatrix();
        rlLoadIdentity();

        var aspect = (float)_application.WindowSize.Width / _application.WindowSize.Height;

        var top = RL_CULL_DISTANCE_NEAR * MathF.Tan(camera.Fov * 0.5f * DEG2RAD);
        var right = top * aspect;
        rlFrustum(-right, right, -top, top, RL_CULL_DISTANCE_NEAR, RL_CULL_DISTANCE_FAR);

        rlMatrixMode(MatrixMode.MODELVIEW);
        rlLoadIdentity();
        
        camera.UpdateView();
        rlMultMatrixf(camera.View);

        rlEnableDepthTest();
    }

    public static void End()
    {
        rlDrawRenderBatchActive();

        rlMatrixMode(MatrixMode.PROJECTION);
        rlPopMatrix();

        rlMatrixMode(MatrixMode.MODELVIEW);
        rlLoadIdentity();

        rlDisableDepthTest();
    }
}