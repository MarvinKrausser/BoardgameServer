namespace ConsoleApp1.Gameplay.Utility;

public class PausableTimeout
{
    private Timer timer;
    private int remainingTime;
    private bool isPaused;
    public AutoResetEvent sem = new(false);
    public AutoResetEvent semPaused = new(false);

    public void StartTimeout(int milliseconds)
    {
        remainingTime = milliseconds;
        isPaused = false;
        Console.WriteLine($"Timeout started: {remainingTime}");

        timer = new Timer(OnTimeout, null, milliseconds, Timeout.Infinite);
    }

    public void PauseTimeout()
    {
        if (!isPaused)
        {
            Console.WriteLine($"Timeout paused: {remainingTime}");
            isPaused = true;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public void StopTimeout()
    {
        Console.WriteLine($"Timeout stopped: {remainingTime}");
        timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void ResumeTimeout()
    {
        if (isPaused)
        {
            Console.WriteLine($"Timeout resumed: {remainingTime}");
            isPaused = false;
            timer.Change(remainingTime, Timeout.Infinite);
            semPaused.Set();
        }
    }

    private void OnTimeout(object state)
    {
        // Timeout logic here
        Console.WriteLine("Timeout occurred");
        sem.Set();
    }
}