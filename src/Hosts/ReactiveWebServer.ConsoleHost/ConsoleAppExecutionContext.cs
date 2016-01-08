using System;
using System.Threading;
using System.Threading.Tasks;
using AnywayAnyday.ReactiveWebServer.Contract;

namespace AnywayAnyday.ReactiveWebServer.ConsoleHost
{
    /// <summary>
    /// A helper to control console application.
    /// </summary>
    public sealed class ConsoleAppExecutionContext : IExecutionContext, IProgress<ProgressInfo>
    {
        readonly CancellationTokenSource _cancelScr;

        public ConsoleAppExecutionContext()
        {
            _cancelScr = new CancellationTokenSource();
        }

        public IProgress<ProgressInfo> ProgressCallback => this;

        public CancellationTokenSource CancelSource => _cancelScr;

        public void Report(ProgressInfo value)
        {
            if (!string.IsNullOrEmpty(value.CustomMessage))
                System.Console.WriteLine(value.CustomMessage);
            else
            {
                long percent =
                    (long)(value.TotalIterations > 0 ?
                            100M / (decimal)value.TotalIterations * (decimal)value.Iteration : 100M);
                System.Console.WriteLine($"{percent}%: {value.Iteration} of {value.TotalIterations}");
            }
        }

        public bool CancelledByUser { get; private set; }

        public void StartReading()
        {
            var cancelWatcher = new ConsoleCancelEventHandler((sender, args) => {
                _cancelScr.Cancel();
                args.Cancel = true;
            });
            System.Console.CancelKeyPress += cancelWatcher;

            Task.Factory.StartNew(() =>
            {
                while (!_cancelScr.Token.IsCancellationRequested)
                {
                    if (System.Console.KeyAvailable && System.Console.ReadKey(true).Key == ConsoleKey.Q)
                    {
                        CancelledByUser = true;
                        _cancelScr.Cancel();
                    }
                    else
                        Task.Delay(200, _cancelScr.Token).Wait();
                }


            }).ContinueWith(delegate { System.Console.CancelKeyPress -= cancelWatcher; });
        }
    }
}
