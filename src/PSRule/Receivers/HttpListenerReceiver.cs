using Newtonsoft.Json;
using PSRule.Pipeline;
using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Threading;

namespace PSRule.Receivers
{
    /// <summary>
    /// A pipeline receiver that accepts objects from HTTP requests.
    /// </summary>
    internal sealed class HttpListenerReceiver : IPipelineReceiver, IDisposable
    {
        private readonly HttpListener _HttpListener;
        private readonly JsonSerializerSettings _SerializerSettings;

        private bool _Disposed = false;

        public HttpListenerReceiver(string[] prefixes)
        {
            _HttpListener = new HttpListener();

            // Configure settings for deserialization
            _SerializerSettings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024, Culture = CultureInfo.InvariantCulture };
            _SerializerSettings.Converters.Insert(0, new PSObjectJsonConverter());

            foreach (var prefix in prefixes)
            {
                _HttpListener.Prefixes.Add(prefix);
            }
        }

        public void Process(ReceiveTargetObject callback, CancellationToken cancelToken)
        {
            _HttpListener.Start();

            // Wait for a request
            var result = _HttpListener.BeginGetContext(ProcessTargetObject, callback);

            while (true)
            {
                result.AsyncWaitHandle.WaitOne(500, false);

                // Check for stop
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }
                else
                {
                    // Wait for next request
                    result = _HttpListener.BeginGetContext(ProcessTargetObject, callback);
                }
            }

            _HttpListener.Stop();
        }

        /// <summary>
        /// Process a HTTP request.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ProcessTargetObject(IAsyncResult asyncResult)
        {
            // Check that the listener isn't in the process of stopping
            if (!_HttpListener.IsListening)
            {
                return;
            }

            // Get the context for the current request
            var requestContext = _HttpListener.EndGetContext(asyncResult);

            // TODO: Check type

            var callback = asyncResult.AsyncState as ReceiveTargetObject;

            try
            {
                // Deserialize the target object
                using (var reader = new StreamReader(requestContext.Request.InputStream))
                {
                    var targetObject = JsonConvert.DeserializeObject<PSObject>(reader.ReadToEnd(), _SerializerSettings);
                    var task = callback(targetObject, null);
                    task.Wait();

                    if (!task.IsFaulted && task.IsCompleted)
                    {

                    }
                }

                // Send OK
                requestContext.Response.StatusCode = 200;
            }
            catch
            {
                // Send error
                requestContext.Response.StatusCode = 500;
            }
            finally
            {
                requestContext.Response.OutputStream.Close();
                requestContext.Response.Close();
            }
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _HttpListener.Close();
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
