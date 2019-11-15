using System.Management.Automation;
using System.Security;

namespace PSRule.Receivers
{
    public static class ReceiverBuilder
    {
        public static IPipelineReceiver AzStorageQueueReceiver(string uri, string queueName, SecureString sasToken)
        {
            return new AzStorageQueueReceiver(uri: uri, queueName: queueName, sasToken: sasToken);
        }

        public static IPipelineReceiver HttpListenerReceiver(string[] prefixes)
        {
            return new HttpListenerReceiver(prefixes: prefixes);
        }

        public static IPipelineReceiver ScriptBlockReceiver(ScriptBlock scriptBlock)
        {
            return new ScriptBlockReceiver(scriptBlock: scriptBlock);
        }
    }
}
