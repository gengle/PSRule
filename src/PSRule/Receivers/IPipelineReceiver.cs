using PSRule.Pipeline;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSRule.Receivers
{
    public delegate Task<InvokeResult> ReceiveTargetObject(PSObject targetObject, CompleteTargetObject complete);

    public delegate void CompleteTargetObject(PSObject targetObject, InvokeResult result);

    public interface IPipelineReceiver
    {
        void Process(ReceiveTargetObject callback, CancellationToken cancelToken);
    }
}
