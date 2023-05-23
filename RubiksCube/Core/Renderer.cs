namespace RubiksCube.Core;

using static Rlgl;

public class Renderer
{
    private static readonly Application _application = Application.GetInstance();
    
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