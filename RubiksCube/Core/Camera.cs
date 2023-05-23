using System.Numerics;
using ImGuiNET;

namespace RubiksCube.Core;

public class Camera
{
    private float _rotationSpeed;
    private float _zoomSpeed;
    private float _fov;

    private Matrix4x4 _view;
    private Vector3 _position;
    private Vector3 _focalPoint;
    private Vector2 _initialMousePosition;

    private float _distance;
    private float _pitch;
    private float _yaw;

    public Camera(float distance = 7.0f, float pitch = 0.342f, float yaw = 0.576f)
    {
        _fov = 45.0f;
        _rotationSpeed = 1.2f;
        _zoomSpeed = 5f;

        _position = Vector3.Zero;
        _focalPoint = Vector3.Zero;
        _initialMousePosition = Vector2.Zero;
        
        _distance = distance;
        _pitch = pitch;
        _yaw = yaw;
    }

    public ref float Fov => ref _fov;
    public ref float Distance => ref _distance;
    public ref float ZoomSpeed => ref _zoomSpeed;
    public ref float RotationSpeed => ref _rotationSpeed;
    public ref Vector3 FocalPoint => ref _focalPoint;
    public ref Vector3 Position => ref _position;
    public ref float Pitch => ref _pitch;
    public ref float Yaw => ref _yaw;
    public ref Matrix4x4 View => ref _view;

    public void ImGuiSettings()
    {
        ImGui.Begin("EditorCamera Settings");
        {
            ImGui.DragFloat("Fov", ref Fov);
            ImGui.DragFloat("Distance", ref Distance);
            ImGui.DragFloat("Rotation Speed", ref RotationSpeed);
            ImGui.DragFloat("Zoom Speed", ref ZoomSpeed);
            ImGui.Separator();
            ImGui.DragFloat3("Focal Point", ref FocalPoint);
            ImGui.DragFloat("Pitch", ref Pitch, 0.01f);
            ImGui.DragFloat("Yaw", ref Yaw, 0.01f);
        }
        ImGui.End();
    }

    public void UpdateView()
    {
        // _yaw = _pitch = 0.0f; // Lock the camera's rotation
        _position = CalculatePosition();

        var orientation = Raymath.QuaternionFromEuler(-_pitch, -_yaw, 0.0f);
        _view = Raymath.MatrixTranslate(_position.X, _position.Y, _position.Z) 
                * Raymath.QuaternionToMatrix(orientation);
        _view = Raymath.MatrixInvert(_view);
    }

    // TODO
    public bool AllowMovement = true;
    
    public void Update()
    {
        //if (_rl.IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
        if (AllowMovement)
        {
            var mousePosition = GetMousePosition();
            var delta = (mousePosition - _initialMousePosition) * 0.003f;
            _initialMousePosition = mousePosition;

            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                MousePan(delta);
            else if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                MouseRotate(delta);
            else if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE))
                MouseZoom(delta.Y);

            MouseZoom(GetMouseWheelMove() * 0.1f);
        }

        UpdateView();
    }

    public void MousePan(Vector2 delta)
    {
        var speed = PanSpeed();
        _focalPoint += -GetRightDirection() * delta.X * speed.X * _distance;
        _focalPoint += GetUpDirection() * delta.Y * speed.Y * _distance;
    }

    public void MouseRotate(Vector2 delta)
    {
        var yawSign = GetUpDirection().Y < 0 ? -1.0f : 1.0f;
        _yaw += yawSign * delta.X * _rotationSpeed;
        _pitch += delta.Y * _rotationSpeed;
    }

    public void MouseZoom(float delta)
    {
        _distance -= delta * _zoomSpeed;
        if (!(_distance < 1.0f)) return;
        
        _focalPoint += GetForwardDirection();
        _distance = 1.0f;
    }

    public Vector3 GetUpDirection()
    {
        return Raymath.Vector3RotateByQuaternion(
            Vector3.UnitY, 
            Raymath.QuaternionFromEuler(-_pitch, -_yaw, 0.0f)
        );
    }

    public Vector3 GetRightDirection()
    {
        return Raymath.Vector3RotateByQuaternion(
            Vector3.UnitX, 
            Raymath.QuaternionFromEuler(-_pitch, -_yaw, 0.0f)
        );
    }

    public Vector3 GetForwardDirection()
    {
        return Raymath.Vector3RotateByQuaternion(
            -Vector3.UnitZ, 
            Raymath.QuaternionFromEuler(-_pitch, -_yaw, 0.0f)
        );
    }

    public Vector3 CalculatePosition() => _focalPoint - GetForwardDirection() * _distance;

    private static Vector2 PanSpeed()
    {
        var x = Math.Min(GetScreenWidth() / 1000.0f, 2.4f);
        var xFactor = 0.0366f * (x * x) - 0.1778f * x + 0.3021f;

        var y = Math.Min(GetScreenHeight() / 1000.0f, 2.4f); // max = 2.4f
        var yFactor = 0.0366f * (y * y) - 0.1778f * y + 0.3021f;

        return new Vector2(xFactor, yFactor);
    }
}