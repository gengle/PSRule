using PSRule.Configuration;
using PSRule.Receivers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A pipeline that receives PSObjects and processes them.
    /// </summary>
    public sealed class ReceivePipeline : IDisposable
    {
        private readonly IPipelineReceiver[] _Recievers;
        private readonly ConcurrentQueue<QueueItem> _Queue;
        private readonly CancellationTokenSource _PipelineTokenSource;
        private readonly Task[] _Tasks;
        private readonly InvokeRulePipeline _InvokePipeline;
        private readonly int _Limit;

        private bool _Disposed = false;

        internal ReceivePipeline(IPipelineReceiver[] recievers, InvokeRulePipeline invokePipeline, PSRuleOption option)
        {
            _Recievers = recievers;
            _Queue = new ConcurrentQueue<QueueItem>();
            _PipelineTokenSource = new CancellationTokenSource();
            _Tasks = new Task[_Recievers.Length];
            _InvokePipeline = invokePipeline;
            _Limit = option.Execution.Limit.Value;
        }

        internal sealed class QueueItem
        {
            public readonly Task<InvokeResult> Task;
            public readonly PSObject TargetObject;

            public QueueItem(Task<InvokeResult> task, PSObject targetObject)
            {
                Task = task;
                TargetObject = targetObject;
            }
        }

        public bool IsStopping
        {
            get { return _PipelineTokenSource.IsCancellationRequested; }
        }

        /// <summary>
        /// Called by recievers to queue a target object for processing.
        /// </summary>
        /// <param name="targetObject">A target object to process.</param>
        private Task<InvokeResult> QueueTargetObject(PSObject targetObject, CompleteTargetObject complete)
        {
            var task = new Task<InvokeResult>(InvokeProcess, targetObject);

            _Queue.Enqueue(new QueueItem(task: task, targetObject: targetObject));

            return task;
        }

        private InvokeResult InvokeProcess(object targetObject)
        {
            var result = _InvokePipeline.Process(targetObject as PSObject);

            return result;
        }

        public IEnumerable<PSObject> Process()
        {
            for (var i = 0; i < _Tasks.Length; i++)
            {
                if (_Tasks[i].IsFaulted)
                {
                    throw new Exception("Receive failed.", _Tasks[i].Exception);
                }
            }

            while (!_Queue.IsEmpty)
            {
                _Queue.TryDequeue(out QueueItem item);

                item.Task.RunSynchronously();

                yield return item.TargetObject;

                if (_Limit > 0 && _Limit <= _InvokePipeline._Context.ObjectCounter + 1)
                {
                    Stop();

                    yield break;
                }
            }
        }

        public void Start()
        {
            if (_Recievers.Length == 0)
            {
                throw new Exception("No receivers.");
            }

            // Start each reciever in a background thread
            for (var i = 0; i < _Recievers.Length; i++)
            {
                var r = _Recievers[i];
                _Tasks[i] = Task.Run(() => r.Process(QueueTargetObject, _PipelineTokenSource.Token));
            }
        }

        public void Stop()
        {
            _PipelineTokenSource.Cancel();

            Task.WaitAll(tasks: _Tasks, timeout: new TimeSpan(0, 0, 30));
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    foreach (var r in _Recievers)
                    {
                        if (r is IDisposable)
                        {
                            (r as IDisposable).Dispose();
                        }
                    }

                    _PipelineTokenSource.Dispose();
                }

                _Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable
    }
}
