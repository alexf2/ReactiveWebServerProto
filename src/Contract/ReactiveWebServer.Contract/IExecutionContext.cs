using System;
using System.Threading;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Describes application execution context, specific for console applications.
    /// </summary>
    public interface IExecutionContext
    {
        /// <summary>
        /// Return a callback to report task progress.
        /// </summary>
        IProgress<ProgressInfo> ProgressCallback { get; }
        /// <summary>
        /// Provides a facility to request task cancelling.
        /// </summary>
        CancellationTokenSource CancelSource { get; }
        /// <summary>
        /// Begins keyboard input reading.
        /// </summary>
        void StartReading();
    }
}
