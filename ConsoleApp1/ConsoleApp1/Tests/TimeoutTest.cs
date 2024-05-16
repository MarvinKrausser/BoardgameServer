using System.Diagnostics;
using ConsoleApp1.Gameplay.Utility;
using NUnit.Framework;

namespace ConsoleApp1.Tests;

[TestFixture]
public class GameManagerTest
{
    [Test]
    public void Test()
    {
        Stopwatch st = new Stopwatch();
        st.Start();
        PausableTimeout t = new PausableTimeout();
        TestContext.Progress.WriteLine("Starting");

        t.StartTimeout(5000);
        new Thread(() => PauseTimeout(t)).Start();
        t.sem.WaitOne();

        t.StopTimeout();

        TestContext.Progress.WriteLine("Ending");

        st.Stop();

        TimeSpan ts = st.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Console.WriteLine("RunTime " + elapsedTime);
    }

    public void PauseTimeout(PausableTimeout t)
    {
        Thread.Sleep(3000);

        t.PauseTimeout();

        TestContext.Progress.WriteLine("Pause");
        Thread.Sleep(3000);
        TestContext.Progress.WriteLine("Resume");

        t.ResumeTimeout();
    }
}