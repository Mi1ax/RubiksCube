using System.Numerics;
using System.Reflection;
using ImGuiNET;
using RubiksCube.Core.Graphics;

namespace RubiksCube.Application;

public enum MouseMoveDirection
{
    None,
    Left, Right,
    Up, Down
}

public enum CubeSide
{
    None = 0,
    Left, Right, 
    Up, Bottom,
    Back, Front,
    
    CenterVerticalFrontBack,
    CenterVerticalLeftRight,
    CenterHorizontal
}

public struct CubeNormalDirection
{
    public static readonly Vector3 Up = new(0, 1, 0);
    public static readonly Vector3 Bottom = new(0, -1, 0);
    public static readonly Vector3 Left = new(-1, 0, 0);
    public static readonly Vector3 Right = new(1, 0, 0);
    public static readonly Vector3 Front = new(0, 0, 1);
    public static readonly Vector3 Back = new(0, 0, -1);

    public static readonly CubeSide[] AllCubeSides = Enum.GetValues<CubeSide>();
    
    public static readonly Dictionary<string, Vector3> DirectionNames = typeof(CubeNormalDirection)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(Vector3))
        .ToDictionary(f => f.Name, f => (Vector3) f.GetValue(null)!);

    public static CubeSide GetSide(Vector3 normal)
    {
        if (normal == Vector3.Zero) return CubeSide.None;
        var sideName = GetSideName(normal);
        return AllCubeSides.FirstOrDefault(cubeSide => 
            string.Equals(cubeSide.ToString(), sideName, StringComparison.CurrentCultureIgnoreCase)
        );
    }
    
    public static string GetSideName(Vector3 normal)
    {
        var minDistance = float.MaxValue;
        var direction = string.Empty;
        foreach (var (key, value) in DirectionNames)
        {
            var distance = Vector3.Distance(normal, value);
            if (!(minDistance > distance)) continue;

            direction = key;
            minDistance = distance;
        }

        return direction;
    }
}

public class RubiksCube
{
    private const float RotationSpeed = 270f;

    private readonly CenterAxis _centerAxis;
    private readonly Cube?[,,] _sideCubes;
    private readonly Dictionary<CubeSide, Vector3[]> _sideGridPositions = new();

    private CubeSide _hoveredSide;

    private List<CubeSide> _availableSides = new();
    private CubeSide _neededSide = CubeSide.None;

    private RayCollision _collision;
    private MouseMoveDirection _mouseMoveDirection;
    private bool _isAlreadyMoved;
    private float _collisionDistance = 2048f;

    private bool _isAnimPlaying;
    private Vector3 _savedRotation = Vector3.Zero;
    private int _rotationDirection;
    
    public RayCollision Collision => _collision;
    public Cube? SelectedCube { get; private set; }
    public CubeSide SelectedSide { get; private set; } = CubeSide.None;

    public RubiksCube(Vector3 centerPosition)
    {
        _centerAxis = new CenterAxis(centerPosition);
        _sideCubes = new Cube[3, 3, 3];
        
        for (var x = -1; x <= 1; x++)
        for (var y = -1; y <= 1; y++)
        for (var z = -1; z <= 1; z++)
        {
            var position = new Vector3(x, y, z);
            _sideCubes[
                (int) (position.X + 1),
                (int) (position.Y + 1),
                (int) (position.Z + 1)
            ] = new Cube(_centerAxis);
        }

        GenerateSides();
    }
    
    private void GenerateSides()
    {
        _centerAxis.Reset();
        foreach (var side in Enum.GetValues<CubeSide>())
            GenerateSide(side);
    }
    
    private void GenerateSide(CubeSide side)
    {
        var sidePositions = new Vector3[9];
        var index = 0;
        for (var i = -1; i <= 1; i++)
        for (var j = -1; j <= 1; j++)
        {
            var position = side switch
            {
                CubeSide.Left or CubeSide.Right => new Vector3(side == CubeSide.Left ? -1 : 1, i, j),
                CubeSide.CenterVerticalFrontBack => new Vector3(0, i, j),
                
                CubeSide.Up or CubeSide.Bottom => new Vector3(i, side == CubeSide.Bottom ? -1 : 1, j),
                CubeSide.CenterHorizontal => new Vector3(i, 0, j),
                
                CubeSide.Front or CubeSide.Back => new Vector3(i, j, side == CubeSide.Back ? -1 : 1),
                CubeSide.CenterVerticalLeftRight => new Vector3(i, j, 0),
                _ => Vector3.Zero
            };

            var sideCube = _sideCubes[(int) (position.X + 1), (int) (position.Y + 1), (int) (position.Z + 1)];
            if (sideCube is not null)
            {
                sideCube.Position = position;
                sideCube.Rotation = Quaternion.Identity;
            }
            sidePositions[index++] = position;
        }

        _sideGridPositions[side] = sidePositions;
    }

    private readonly List<CubeSide> _selectedSides = new();

    public List<CubeSide> GetSelectedSides()
    {
        _selectedSides.Clear();
        if (SelectedCube is null)
        {
            _selectedSides.Add(CubeSide.None);
            return _selectedSides;
        }

        var gridPosition = SelectedCube!.GridPosition;
        foreach (var (side, positions) in _sideGridPositions)
        {
            if (positions.Contains(gridPosition))
                _selectedSides.Add(side);
        }

        return _selectedSides;
    }
    
    public void SelectSide(CubeSide side)
    {
        if (_isAnimPlaying) return;
        
        ResetSelection();
        SelectedSide = side;
        if (SelectedSide == CubeSide.None) return;
        foreach (var sideCube in _sideCubes)
        {
            if (sideCube is null) continue;
            if (_sideGridPositions[SelectedSide].Contains(sideCube.GridPosition))
                sideCube.IsSelected = true;
        }
    }
    
    private void ResetSelection()
    {
        foreach (var sideCube in _sideCubes)
        {
            if (sideCube is null) continue;
            if (sideCube.IsSelected == false) continue;
            
            var parentRotation =
                Quaternion.CreateFromRotationMatrix(
                    MatrixRotateXYZ(_centerAxis.Rotation * DEG2RAD));
            sideCube.Rotation *= parentRotation;
            sideCube.Position = sideCube.GridPosition;
            sideCube.IsSelected = false;
        }
        _centerAxis.Rotation = Vector3.Zero;
    }

    public CubeSide NeededSide(MouseMoveDirection moveDirection, List<CubeSide> movingSides)
    {
        var neededSide = CubeSide.None;
        switch (moveDirection)
        {
            case MouseMoveDirection.Up or MouseMoveDirection.Down:
                if (_hoveredSide is not CubeSide.Up and not CubeSide.Bottom)
                {
                    if (movingSides.Contains(CubeSide.Left))
                        neededSide = CubeSide.Left;
                    else if (movingSides.Contains(CubeSide.Right))
                        neededSide = CubeSide.Right;
                    else if (movingSides.Contains(CubeSide.Back))
                        neededSide = CubeSide.Back;
                    else if (movingSides.Contains(CubeSide.Front))
                        neededSide = CubeSide.Front;
                
                    else if (movingSides.Contains(CubeSide.CenterVerticalFrontBack))
                        neededSide = CubeSide.CenterVerticalFrontBack;
                    else if (movingSides.Contains(CubeSide.CenterVerticalLeftRight))
                        neededSide = CubeSide.CenterVerticalLeftRight;
                }
                break;
            case MouseMoveDirection.Left or MouseMoveDirection.Right:
                if (_hoveredSide is not CubeSide.Up and not CubeSide.Bottom)
                {
                    if (movingSides.Contains(CubeSide.Up))
                        neededSide = CubeSide.Up;
                    else if (movingSides.Contains(CubeSide.Bottom))
                        neededSide = CubeSide.Bottom;

                    else if (movingSides.Contains(CubeSide.CenterHorizontal))
                        neededSide = CubeSide.CenterHorizontal;
                }
                break;
        }

        return neededSide;
    }

    public void StartAnimation(CubeSide sideToRotate, MouseMoveDirection moveDirection)
    {
        if (_isAnimPlaying) return;
        var hoverSide = CubeNormalDirection.GetSide(_collision.normal);
        if (hoverSide == CubeSide.None) return;
        
        _rotationDirection = moveDirection is MouseMoveDirection.Down or MouseMoveDirection.Right ? 1 : -1;
        SelectSide(sideToRotate);

        if (hoverSide == CubeSide.Right && SelectedSide == CubeSide.Front
            || hoverSide == CubeSide.Right && SelectedSide == CubeSide.Back
            || hoverSide == CubeSide.Right && SelectedSide == CubeSide.CenterVerticalLeftRight

            || hoverSide == CubeSide.Back && SelectedSide == CubeSide.Left
            || hoverSide == CubeSide.Back && SelectedSide == CubeSide.Right
            || hoverSide == CubeSide.Back && SelectedSide == CubeSide.CenterVerticalFrontBack)
        {
            _rotationDirection *= -1;
        }
        
        _isAnimPlaying = true;
        _savedRotation = _centerAxis.Rotation;
    }

    private bool RotateAxis(Vector3 axis, ref float axisValue)
    {
        var savedRotation = 0f;
        var centerAxisRotation = 0f;
        if (axis == Vector3.UnitX)
        {
            savedRotation = _savedRotation.X;
            centerAxisRotation = _centerAxis.Rotation.X;
        }
        else if (axis == Vector3.UnitY)
        {
            savedRotation = _savedRotation.Y;
            centerAxisRotation = _centerAxis.Rotation.Y;
        }
        else if (axis == Vector3.UnitZ)
        {
            savedRotation = _savedRotation.Z;
            centerAxisRotation = _centerAxis.Rotation.Z;
        }

        axisValue += RotationSpeed * _rotationDirection * GetFrameTime();

        switch (_rotationDirection)
        {
            case 1:
                if (!(savedRotation + 90f - centerAxisRotation <= 0)) return true;
                break;
            case -1:
                if (!(savedRotation - 90f + -centerAxisRotation >= 0)) return true;
                break;
        }

        axisValue = savedRotation + 90f * _rotationDirection;
        return false;
    }
    
    private void AnimationUpdate()
    {
        if (!_isAnimPlaying) return;

        switch (SelectedSide)
        {
            case CubeSide.None:
                break;
            case CubeSide.CenterVerticalFrontBack:
            case CubeSide.Right:
            case CubeSide.Left:
                if (RotateAxis(Vector3.UnitX, ref _centerAxis.Rotation.X)) return;
                break;
            case CubeSide.CenterHorizontal:
            case CubeSide.Up:
            case CubeSide.Bottom:
                if (RotateAxis(Vector3.UnitY, ref _centerAxis.Rotation.Y)) return;
                break;
            case CubeSide.CenterVerticalLeftRight:
            case CubeSide.Back:
            case CubeSide.Front:
                if (RotateAxis(Vector3.UnitZ, ref _centerAxis.Rotation.Z)) return;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _rotationDirection = 0;
        _isAnimPlaying = false;
        _savedRotation = Vector3.Zero;
    }

    public void ImGuiUpdate()
    {
        ImGui.Begin("Settings");
        {
            if (ImGui.Button("Reset"))
                GenerateSides();
        }
    }

    public void Update()
    {
        _mouseMoveDirection = Renderer.GetMouseMovingDirection();
        _hoveredSide = CubeNormalDirection.GetSide(_collision.normal);

        _availableSides = GetSelectedSides();
        if (_availableSides.Contains(_hoveredSide))
            _availableSides.Remove(_hoveredSide);

        _neededSide = NeededSide(_mouseMoveDirection, _availableSides);

        if (!_isAlreadyMoved &&
            GetMouseDelta() != Vector2.Zero &&
            IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) 
        {
            StartAnimation(_neededSide, _mouseMoveDirection);
            _isAlreadyMoved = true;
        }
        if (_isAlreadyMoved && IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            _isAlreadyMoved = false;
        
        AnimationUpdate();
        
        SelectedCube = null;
        _collisionDistance = 2048f;
        _collision = default;
    }

    public void Draw(Ray mouseRay)
    {
        foreach (var sideCube in _sideCubes)
        {
            if (sideCube is null) continue;
            
            sideCube.Draw();
            
            var collision = sideCube.GetCollision(mouseRay);
            if (collision is null) continue;
            
            if (!(_collisionDistance > collision.Value.distance)) continue;
            _collisionDistance = collision.Value.distance;
            _collision = collision.Value;
            SelectedCube = sideCube;
        }
    }
}