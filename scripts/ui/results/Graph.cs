using Godot;

public partial class Graph : ColorRect
{
    public override void _Draw()
    {
        Color hitColor = Color.FromHtml("00ff00ff");
        Color missColor = Color.FromHtml("ff000044");

        float[] hitsInfo = GameScene.Attempt.IsReplay ? GameScene.Attempt.Replays[0].Notes : GameScene.Attempt.HitsInfo;

        for (ulong i = (ulong)hitsInfo.GetLowerBound(0); i < (ulong)hitsInfo.Length; i++)
        {
            float offset = hitsInfo[i];

            if (offset < 0)
            {
                int position = (int)(Size.X * GameScene.Attempt.Map.Notes[i].Millisecond / GameScene.Attempt.Map.Length);
                DrawLine(Vector2.Right * position, new(position, Size.Y), missColor, 1);
            }
            else
            {
                DrawRect(new(Size.X * (GameScene.Attempt.Map.Notes[i].Millisecond / (float)GameScene.Attempt.Map.Length), Size.Y * (offset / 55), Vector2.One), hitColor);
            }
        }

        if (GameScene.Attempt.DeathTime >= 0)
        {
            int position = (int)(Size.X * GameScene.Attempt.DeathTime / GameScene.Attempt.Map.Length);
            DrawLine(Vector2.Right * position, new(position, Size.Y), Color.Color8(255, 255, 0), 3);
        }
    }
}
