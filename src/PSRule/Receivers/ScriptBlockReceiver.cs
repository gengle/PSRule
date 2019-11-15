using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Receivers
{
    internal sealed class ScriptBlockReceiver : IPipelineReceiver
    {
        private readonly ScriptBlock _ScriptBlock;

        public ScriptBlockReceiver(ScriptBlock scriptBlock)
        {
            _ScriptBlock = scriptBlock;
        }

        public void Process(ReceiveTargetObject callback, CancellationToken cancelToken)
        {
            var result = _ScriptBlock.Invoke().ToArray();

            for (var i = 0; i < result.Length; i++)
            {
                callback(targetObject: result[i], complete: null);
            }
        }
    }
}
