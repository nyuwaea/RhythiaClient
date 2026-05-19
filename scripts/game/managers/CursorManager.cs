using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Class containing all gameplay related logic regarding the cursor.
/// </summary>
public partial class CursorManager : Node
{
    [Export] private Runner runner;
    [Export] private PlayerInputController playerInputController;
    [Export] private ReplayManager replayManager;
    [Export] private MeshInstance3D cursorMesh;
    [Export] private Camera3D camera;

    private SettingsProfile settings;
    private float sensitivity;
    private List<MeshInstance3D> cursors;

    [Signal]
    public delegate void OnCursorUpdatedEventHandler(
        Vector2 position
    );

    public override void _Ready()
    {
        cursorMesh ??= GetNode<MeshInstance3D>("Cursor");
        playerInputController ??= GetNode<PlayerInputController>("/PlayerInputController");
        replayManager ??= GetNode<ReplayManager>("ReplayManager");
    }

    public override void _EnterTree()
    {
        settings = GameScene.Attempt.IsReplay ? GameScene.Attempt.Replays[0].Settings : GameScene.Attempt.Settings;
        cursorMesh.Position = Vector3.Zero;
        cursorMesh.Rotation = Vector3.Zero;
        cursors = [ cursorMesh ];

        var parent = cursorMesh.GetParent();

        if (GameScene.Attempt.IsReplay)
        {
            for (int i = 1; i < GameScene.Attempt.Replays.Length; i++)
            {
                cursors.Add(cursorMesh.Duplicate() as MeshInstance3D);
                parent.AddChild(cursors[i]);
            }
        }
    }

    public override void _ExitTree()
    {
        if (cursors.Count > 1)
        {
            for (int i = 1; i < cursors.Count; i++)
            {
                cursors[i].QueueFree();
            }

            cursors.RemoveRange(1, cursors.Count - 1);
        }
    }

    public override void _Process(double delta)
    {
        if (!runner.Playing) return;

        updateCursorRotation(delta);
    }

    public void ShowCursor(int cursorIndex = 0, bool instant = true)
    {
        if (instant)
        {
            cursors[cursorIndex].Transparency = 1 - (float)settings.CursorOpacity / 100;
        }
        else
        {
            CreateTween().TweenProperty(cursors[cursorIndex], "transparency", 1, 0.5);
        }
    }

    public void HideCursor(int cursorIndex = 0, bool instant = true)
    {
        ShowCursor(cursorIndex, instant);
    }

    public void UpdateCursor(Vector2 inputDelta, int cursorIndex = 0)
    {
        EmitSignalOnCursorUpdated(inputDelta);

        sensitivity = (float)settings.Sensitivity;

        if (settings.AbsoluteInput && !runner.Attempt.IsReplay)
        {
            sensitivity = (float)settings.AbsoluteSensitivity;
        }

        sensitivity *= (float)settings.FoV / 70f;

        if (settings.AbsoluteInput || runner.Attempt.IsReplay)
            repositionAbsolute();
        
        Vector3 cursorPos;

        if (runner.SpinCamera)
            cursorPos = updateSpinState(inputDelta);
        else
            cursorPos = updateLockedState(inputDelta);

        cursors[cursorIndex].Position = cursorPos;
    }

    private Vector3 updateSpinState(Vector2 inputDelta)
    {
        var attempt = runner.Attempt;

        if (!attempt.IsReplay)
        {
            camera.Rotation += new Vector3(-inputDelta.Y / 120 * sensitivity / (float)Math.PI, -inputDelta.X / 120 * sensitivity / (float)Math.PI, 0);
        }
        else
        {
            camera.Rotation += new Vector3(inputDelta.Y / (float)Math.PI, -inputDelta.X / (float)Math.PI, 0);
        }
        camera.Rotation = new Vector3((float)Math.Clamp(camera.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90)), camera.Rotation.Y, camera.Rotation.Z);

        var origin = new Vector3(0, 0, 3.5f);
        var cursorLock = new Vector3(attempt.CursorPosition.X, attempt.CursorPosition.Y, 0);
        // The pivot is to mimic ROBLOX's orbital camera
        var pivot = camera.Basis.Z / 4f;

        // Proper Parallax Support
        camera.Position = origin + cursorLock * (float)settings.CameraParallax + pivot;

        var lookVector = camera.Basis.Z;
        var cameraVector2 = new Vector2(camera.Position.X, camera.Position.Y);
        var lookVector2 = new Vector2(lookVector.X, lookVector.Y);

        // Project Cursor from Camera's "ray cast"
        attempt.RawCursorPosition = cameraVector2 - lookVector2 * Mathf.Abs(camera.Position.Z / lookVector.Z);
        attempt.CursorPosition = attempt.RawCursorPosition.Clamp(-Constants.BOUNDS, Constants.BOUNDS);

        return new(attempt.CursorPosition.X, attempt.CursorPosition.Y, 0);
    }

    private Vector3 updateLockedState(Vector2 inputDelta)
    {
        var attempt = runner.Attempt;
        var delta = new Vector2(1, -1) * (inputDelta * sensitivity / 120f);

        if (settings.CursorDrift)
        {
            attempt.CursorPosition = attempt.IsReplay
                ? replayManager.CursorPosition.Clamp(-Constants.BOUNDS, Constants.BOUNDS)
                : (attempt.CursorPosition + delta).Clamp(-Constants.BOUNDS, Constants.BOUNDS);
        }
        else
        {
            attempt.RawCursorPosition = attempt.IsReplay
                ? replayManager.CursorPosition
                : attempt.RawCursorPosition + delta;
            attempt.CursorPosition = attempt.RawCursorPosition.Clamp(-Constants.BOUNDS, Constants.BOUNDS);
        }

        var origin = new Vector3(0, 0, 3.75f);
        float parallax = (float)settings.CameraParallax;

        // camera should manage parallax on its own
        camera.Position = origin + (attempt.IsReplay && attempt.Replays.Length > 1
            ? Vector3.Zero
            : new Vector3(attempt.CursorPosition.X, attempt.CursorPosition.Y, 0) * parallax);
        camera.Rotation = Vector3.Zero;

        return new(attempt.CursorPosition.X, attempt.CursorPosition.Y, 0);
    }

    // Reset everything to zero so it doesn't have infinite sensitivity
    private void repositionAbsolute()
    {
        camera.Rotation = Vector3.Zero;
        runner.Attempt.RawCursorPosition = Vector2.Zero;
        runner.Attempt.CursorPosition = Vector2.Zero;
    }
    private void updateCursorRotation(double delta) => cursorMesh.RotationDegrees += Vector3.Back * (float)settings.CursorRotation * (float)delta;
}
