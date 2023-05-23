using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace RubiksCube.Core;

using static Rlgl;

public static class RayImGui
{
    private static nint _imGuiContext = nint.Zero;

    private static ImGuiMouseCursor _currentMouseCursor = ImGuiMouseCursor.COUNT;
    private static Dictionary<ImGuiMouseCursor, MouseCursor>? _mouseCursorMap;
    private static KeyboardKey[]? _keyEnumMap;

    private static Texture2D _fontTexture;

    public static void Setup()
    {
        _mouseCursorMap = new Dictionary<ImGuiMouseCursor, MouseCursor>();
        _keyEnumMap = Enum.GetValues(typeof(KeyboardKey)) as KeyboardKey[];

        _fontTexture.id = 0;

        _imGuiContext = ImGui.CreateContext();
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        ImGui.StyleColorsDark();
        
        SetDarkThemeColors();

        EndInitImGui();
    }
    
    private static void SetDarkThemeColors()
    {
        var colors = ImGui.GetStyle().Colors;
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1f, 0.105f, 0.11f, 1.0f);

        // Headers
        colors[(int)ImGuiCol.Header] = new Vector4( 0.2f, 0.205f, 0.21f, 1.0f );
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4( 0.3f, 0.305f, 0.31f, 1.0f );
        colors[(int)ImGuiCol.HeaderActive] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );

        // Buttons
        colors[(int)ImGuiCol.Button] = new Vector4( 0.2f, 0.205f, 0.21f, 1.0f );
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4( 0.3f, 0.305f, 0.31f, 1.0f );
        colors[(int)ImGuiCol.ButtonActive] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );

        // Frame BG
        colors[(int)ImGuiCol.FrameBg] = new Vector4( 0.2f, 0.205f, 0.21f, 1.0f );
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4( 0.3f, 0.305f, 0.31f, 1.0f );
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );

        // Tabs
        colors[(int)ImGuiCol.Tab] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );
        colors[(int)ImGuiCol.TabHovered] = new Vector4( 0.38f, 0.3805f, 0.381f, 1.0f );
        colors[(int)ImGuiCol.TabActive] = new Vector4( 0.28f, 0.2805f, 0.281f, 1.0f );
        colors[(int)ImGuiCol.TabUnfocused] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );
        colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4( 0.2f, 0.205f, 0.21f, 1.0f );

        // Title
        colors[(int)ImGuiCol.TitleBg] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4( 0.15f, 0.1505f, 0.151f, 1.0f );
    }

    private static void SetupMouseCursors()
    {
        if (_mouseCursorMap == null) return;
        _mouseCursorMap.Clear();
        _mouseCursorMap[ImGuiMouseCursor.Arrow] = MouseCursor.MOUSE_CURSOR_ARROW;
        _mouseCursorMap[ImGuiMouseCursor.TextInput] = MouseCursor.MOUSE_CURSOR_IBEAM;
        _mouseCursorMap[ImGuiMouseCursor.Hand] = MouseCursor.MOUSE_CURSOR_POINTING_HAND;
        _mouseCursorMap[ImGuiMouseCursor.ResizeAll] = MouseCursor.MOUSE_CURSOR_RESIZE_ALL;
        _mouseCursorMap[ImGuiMouseCursor.ResizeEW] = MouseCursor.MOUSE_CURSOR_RESIZE_EW;
        _mouseCursorMap[ImGuiMouseCursor.ResizeNESW] = MouseCursor.MOUSE_CURSOR_RESIZE_NESW;
        _mouseCursorMap[ImGuiMouseCursor.ResizeNS] = MouseCursor.MOUSE_CURSOR_RESIZE_NS;
        _mouseCursorMap[ImGuiMouseCursor.ResizeNWSE] = MouseCursor.MOUSE_CURSOR_RESIZE_NWSE;
        _mouseCursorMap[ImGuiMouseCursor.NotAllowed] = MouseCursor.MOUSE_CURSOR_NOT_ALLOWED;
    }

    private static unsafe void ReloadFonts()
    {
        ImGui.SetCurrentContext(_imGuiContext);
        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out var width, out var height, out _);

        var image = new Image
        {
            data = pixels,
            width = width,
            height = height,
            mipmaps = 1,
            format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8,
        };

        _fontTexture = LoadTextureFromImage(image);

        io.Fonts.SetTexID(new nint(_fontTexture.id));
    }

    private static void EndInitImGui()
    {
        SetupMouseCursors();

        ImGui.SetCurrentContext(_imGuiContext);
        var fonts = ImGui.GetIO().Fonts;
        fonts.AddFontDefault();

        var io = ImGui.GetIO();
        io.KeyMap[(int)ImGuiKey.Tab] = (int)KeyboardKey.KEY_TAB;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)KeyboardKey.KEY_LEFT;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)KeyboardKey.KEY_RIGHT;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)KeyboardKey.KEY_UP;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)KeyboardKey.KEY_DOWN;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)KeyboardKey.KEY_PAGE_UP;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)KeyboardKey.KEY_PAGE_DOWN;
        io.KeyMap[(int)ImGuiKey.Home] = (int)KeyboardKey.KEY_HOME;
        io.KeyMap[(int)ImGuiKey.End] = (int)KeyboardKey.KEY_END;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)KeyboardKey.KEY_DELETE;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)KeyboardKey.KEY_BACKSPACE;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)KeyboardKey.KEY_ENTER;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)KeyboardKey.KEY_ESCAPE;
        io.KeyMap[(int)ImGuiKey.Space] = (int)KeyboardKey.KEY_SPACE;
        io.KeyMap[(int)ImGuiKey.A] = (int)KeyboardKey.KEY_A;
        io.KeyMap[(int)ImGuiKey.C] = (int)KeyboardKey.KEY_C;
        io.KeyMap[(int)ImGuiKey.V] = (int)KeyboardKey.KEY_V;
        io.KeyMap[(int)ImGuiKey.X] = (int)KeyboardKey.KEY_X;
        io.KeyMap[(int)ImGuiKey.Y] = (int)KeyboardKey.KEY_Y;
        io.KeyMap[(int)ImGuiKey.Z] = (int)KeyboardKey.KEY_Z;

        ReloadFonts();
    }

    private static void NewFrame()
    {
        var io = ImGui.GetIO();

        if (IsWindowFullscreen())
        {
            var monitor = GetCurrentMonitor();
            io.DisplaySize = new Vector2(GetMonitorWidth(monitor), GetMonitorHeight(monitor));
        }
        else
        {
            io.DisplaySize = new Vector2(GetScreenWidth(), GetScreenHeight());
        }
            
        io.DisplayFramebufferScale = new Vector2(1, 1);
        io.DeltaTime = GetFrameTime();

        io.KeyCtrl = IsKeyDown(KeyboardKey.KEY_RIGHT_CONTROL) || IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        io.KeyShift = IsKeyDown(KeyboardKey.KEY_RIGHT_SHIFT) || IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        io.KeyAlt = IsKeyDown(KeyboardKey.KEY_RIGHT_ALT) || IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
        io.KeySuper = IsKeyDown(KeyboardKey.KEY_RIGHT_SUPER) || IsKeyDown(KeyboardKey.KEY_LEFT_SUPER);

        if (io.WantSetMousePos)
        {
            SetMousePosition((int)io.MousePos.X, (int)io.MousePos.Y);
        }
        else
        {
            io.MousePos = GetMousePosition();
        }

        io.MouseDown[0] = IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
        io.MouseDown[1] = IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT);
        io.MouseDown[2] = IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE);

        if (GetMouseWheelMove() > 0)
            io.MouseWheel += 1;
        else if (GetMouseWheelMove() < 0)
            io.MouseWheel -= 1;

        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0) return;
        
        var imGuiCursor = ImGui.GetMouseCursor();
        if (imGuiCursor == _currentMouseCursor && !io.MouseDrawCursor) return;
        _currentMouseCursor = imGuiCursor;
        if (io.MouseDrawCursor || imGuiCursor == ImGuiMouseCursor.None)
        {
            HideCursor();
        }
        else
        {
            ShowCursor();

            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0 || _mouseCursorMap == null) 
                return;
            SetMouseCursor(!_mouseCursorMap.ContainsKey(imGuiCursor)
                ? MouseCursor.MOUSE_CURSOR_DEFAULT
                : _mouseCursorMap[imGuiCursor]);
        }
    }


    private static void FrameEvents()
    {
        var io = ImGui.GetIO();

        if (_keyEnumMap != null)
            foreach (var key in _keyEnumMap)
            {
                io.KeysDown[(int) key] = IsKeyDown(key);
            }

        var pressed = (uint)GetCharPressed();
        while (pressed != 0)
        {
            io.AddInputCharacter(pressed);
            pressed = (uint)GetCharPressed();
        }
    }

    public static void Begin()
    {
        ImGui.SetCurrentContext(_imGuiContext);

        NewFrame();
        FrameEvents();
        ImGui.NewFrame();
    }
    
    private static void TriangleVert(ImDrawVertPtr idxVert)
    {
        var color = ImGui.ColorConvertU32ToFloat4(idxVert.col);

        rlColor4f(color.X, color.Y, color.Z, color.W);

        rlTexCoord2f(idxVert.uv.X, idxVert.uv.Y);
        rlVertex2f(idxVert.pos.X, idxVert.pos.Y);
    }

    private static void RenderTriangles(
        uint count,
        uint indexStart,
        ImVector<ushort> indexBuffer,
        ImPtrVector<ImDrawVertPtr> vertBuffer,
        nint texturePtr)
    {
        if (count < 3)
            return;

        uint textureId = 0;
        if (texturePtr != nint.Zero)
            textureId = (uint)texturePtr.ToInt32();

        rlBegin(DrawMode.TRIANGLES);
        rlSetTexture(textureId);

        for (var i = 0; i <= count - 3; i += 3)
        {
            if (rlCheckRenderBatchLimit(3))
            {
                rlBegin(DrawMode.TRIANGLES);
                rlSetTexture(textureId);
            }

            var indexA = indexBuffer[(int)indexStart + i];
            var indexB = indexBuffer[(int)indexStart + i + 1];
            var indexC = indexBuffer[(int)indexStart + i + 2];

            var vertexA = vertBuffer[indexA];
            var vertexB = vertBuffer[indexB];
            var vertexC = vertBuffer[indexC];

            TriangleVert(vertexA);
            TriangleVert(vertexB);
            TriangleVert(vertexC);
        }
        rlEnd();
    }

    private delegate void Callback(ImDrawListPtr list, ImDrawCmdPtr cmd);

    private static void RenderData()
    {
        rlDrawRenderBatchActive();
        rlDisableBackfaceCulling();

        var data = ImGui.GetDrawData();

        for (var l = 0; l < data.CmdListsCount; l++)
        {
            var commandList = data.CmdListsRange[l];

            for (var cmdIndex = 0; cmdIndex < commandList.CmdBuffer.Size; cmdIndex++)
            {
                var cmd = commandList.CmdBuffer[cmdIndex];

                rlEnableScissorTest();

                var clipOff = data.DisplayPos;
                var clipScale = Vector2.One;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    clipScale = new Vector2(2, 2);

                var fbHeight = data.DisplaySize.Y * clipScale.Y;

                var clipMin = new Vector2(
                    (cmd.ClipRect.X - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y
                    );
                
                var clipMax = new Vector2(
                    (cmd.ClipRect.Z - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.W - clipOff.Y) * clipScale.Y
                );

                rlScissor(
                    (int)clipMin.X, 
                    (int)(fbHeight - clipMax.Y), 
                    (int)(clipMax.X - clipMin.X), 
                    (int)(clipMax.Y - clipMin.Y)
                    );


                if (cmd.UserCallback != nint.Zero)
                {
                    var cb = Marshal.GetDelegateForFunctionPointer<Callback>(cmd.UserCallback);
                    cb(commandList, cmd);
                    continue;
                }

                RenderTriangles(cmd.ElemCount, cmd.IdxOffset, commandList.IdxBuffer, commandList.VtxBuffer, cmd.TextureId);

                rlDrawRenderBatchActive();
            }
        }
        rlSetTexture(0);
        rlDisableScissorTest();
        rlEnableBackfaceCulling();
    }

    public static void End()
    {
        ImGui.SetCurrentContext(_imGuiContext);
        ImGui.Render();
        RenderData();
    }

    public static void Shutdown()
    {
        UnloadTexture(_fontTexture);
    }
}