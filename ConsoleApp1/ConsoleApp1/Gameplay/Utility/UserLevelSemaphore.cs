namespace ConsoleApp1.Gameplay.Utility;

public class UserLevelSemaphore
{
    private int availableCount;

    public UserLevelSemaphore(int initialCount)
    {
        if (initialCount < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCount), "Initial count cannot be negative.");

        availableCount = initialCount;
    }

    public void WaitOne()
    {
        lock (this)
        {
            while (availableCount == 0)
            {
                Monitor.Wait(this);
            }

            availableCount--;
        }
    }

    public void Release()
    {
        lock (this)
        {
            availableCount++;
            Monitor.Pulse(this);
        }
    }
}