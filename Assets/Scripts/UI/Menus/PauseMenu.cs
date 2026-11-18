using UnityEngine;

public class PauseMenu : Menu
{
    public const float PauseTimeScale = 0.0f;

    float timeScaleBeforePause = 1.0f;

    public bool IsPaused => Time.timeScale == PauseTimeScale;

    public override void Close()
    {
        base.Close();
        Utility.SetCursorState(true);
        ResumeTime();
    }
    public override void Open()
    {
        base.Open();
        PauseTime();
        Utility.SetCursorState(false);
    }

    void ResumeTime()
    {
        Time.timeScale = timeScaleBeforePause;
        timeScaleBeforePause = 1.0f;
    }
    void PauseTime()
    {
        timeScaleBeforePause = Time.timeScale;
        Time.timeScale = PauseTimeScale;
    }

    public void Toggle()
    {
        if (IsPaused) Close();
        else Open();
    }
}
