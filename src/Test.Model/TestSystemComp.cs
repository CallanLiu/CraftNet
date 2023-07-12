using System.Diagnostics;
using CraftNet;

namespace Test;

public class TestSystemComp
{
    public Stopwatch               Stopwatch               = Stopwatch.StartNew();
    public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
}