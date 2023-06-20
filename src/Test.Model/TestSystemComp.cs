﻿using System.Diagnostics;
using XGFramework;

namespace Test;

public class TestSystemComp : IComp
{
    public Stopwatch               Stopwatch               = Stopwatch.StartNew();
    public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
}