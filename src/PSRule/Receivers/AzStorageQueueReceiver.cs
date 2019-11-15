using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using PSRule.Pipeline;
using System;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace PSRule.Receivers
{
    /// <summary>
    /// A pipeline receiver that accepts messages from an Azure Storage Queue.
    /// </summary>
    internal sealed class AzStorageQueueReceiver : IPipelineReceiver
    {
        private readonly CloudQueueClient _QueueClient;
        private readonly JsonSerializerSettings _SerializerSettings;
        private readonly string _Uri;
        private readonly string _QueueName;
        private readonly QueueRetryPolicy _RetryPolicy;

        private bool _Disposed = false;
        private CloudQueue _Queue;

        // The number of messages to get at a time
        private int _BatchSize = 5;

        public AzStorageQueueReceiver(string uri, string queueName, SecureString sasToken)
        {
            _Uri = uri;
            _QueueName = queueName;
            
            // Configure back off policy for accessing queue
            _RetryPolicy = new QueueRetryPolicy();

            // Configure client to access queue
            _QueueClient = new CloudQueueClient(new Uri(uri), new StorageCredentials(sasToken: new System.Net.NetworkCredential(string.Empty, sasToken).Password));

            // Configure settings for deserialization
            _SerializerSettings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024, Culture = CultureInfo.InvariantCulture };
            _SerializerSettings.Converters.Insert(0, new PSObjectJsonConverter());
        }

        private sealed class QueueRetryPolicy
        {
            private readonly int[] _WaitTime;
            private int _Index;
            private int _Length;

            public QueueRetryPolicy()
            {
                _Index = 0;
                _WaitTime = new int[] { 500, 1000, 2000, 4000, 8000 };
                _Length = _WaitTime.Length;
            }

            public int WaitTime
            {
                get
                {
                    var index = Interlocked.Increment(ref _Index);

                    if (index > _Length)
                    {
                        index = _Length;
                        Interlocked.Exchange(ref _Index, _Length);
                    }

                    var result = _WaitTime[index - 1];

                    return result;
                }
            }

            public void Reset()
            {
                Interlocked.Exchange(ref _Index, 0);
            }
        }

        public void Process(ReceiveTargetObject callback, CancellationToken cancelToken)
        {
            var queue = GetQueue();

            // Wait for a message

            while (true)
            {
                var task = queue.GetMessagesAsync(_BatchSize);
                task.Wait(cancelToken);

                var messages = task.Result.ToArray();

                if (messages == null || messages.Length == 0)
                {
                    Task.Delay(_RetryPolicy.WaitTime).Wait(cancelToken);
                }
                else
                {
                    ProcessTargetObject(messages: messages, callback: callback, cancelToken: cancelToken);
                    _RetryPolicy.Reset();
                }

                // Check for stop
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private void ProcessTargetObject(CloudQueueMessage[] messages, ReceiveTargetObject callback, CancellationToken cancelToken)
        {
            try
            {
                // Deserialize the target object
                for (var i = 0; i < messages.Length; i++)
                {
                    var targetObject = JsonConvert.DeserializeObject<PSObject>(messages[i].AsString, _SerializerSettings);
                    var task = callback(targetObject, null);
                    task.Wait();

                    if (!task.IsFaulted && task.IsCompleted)
                    {
                        GetQueue().DeleteMessage(messages[i]);
                    }

                    if (cancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch
            {

            }
            finally
            {

            }
        }

        private void ProcessTargetObject(IAsyncResult asyncResult)
        {
            // Get the context for the current request
            var requestContext = GetQueue().EndGetMessages(asyncResult);

            // TODO: Check type

            var callback = asyncResult.AsyncState as ReceiveTargetObject;

            try
            {
                bool shouldReset = false;

                // Deserialize the target object
                foreach (var message in requestContext)
                {
                    shouldReset = true;

                    var targetObject = JsonConvert.DeserializeObject<PSObject>(message.AsString, _SerializerSettings);
                    var task = callback(targetObject, null);
                    task.Wait();

                    if (!task.IsFaulted && task.IsCompleted)
                    {
                        GetQueue().DeleteMessage(message);
                    }
                }

                if (shouldReset)
                {
                    _RetryPolicy.Reset();
                }
            }
            catch
            {
                
            }
            finally
            {
                
            }
        }

        private CloudQueue GetQueue()
        {
            if (_Queue == null)
            {
                _Queue = _QueueClient.GetQueueReference(queueName: _QueueName);
            }

            return _Queue;
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    
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
