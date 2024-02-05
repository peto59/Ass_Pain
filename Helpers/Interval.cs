﻿using System;
using System.Threading.Tasks;
using System.Threading;

namespace MWP
{
    internal static class Interval
    {
        public static async Task SetIntervalAsync(Action action, int delay, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(delay, token);
                    action();
                }
            }
            catch (TaskCanceledException) { }
        }

    }
}