using PSRule.Configuration;
using PSRule.Receivers;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public sealed class ReceivePipelineBuilder
    {
        private PSRuleOption _Option;
        private List<IPipelineReceiver> _Receivers;
        private bool _LogError;
        private bool _LogWarning;
        private bool _LogVerbose;
        private bool _LogInformation;

        public ReceivePipelineBuilder()
        {
            _Option = new PSRuleOption();
            _Receivers = new List<IPipelineReceiver>();
        }

        public void Receiver(IPipelineReceiver[] receiver)
        {
            _Receivers.AddRange(receiver);
        }

        public void UseLoggingPreferences(ActionPreference error, ActionPreference warning, ActionPreference verbose, ActionPreference information)
        {
            _LogError = !(error == ActionPreference.Ignore);
            _LogWarning = !(warning == ActionPreference.Ignore);
            _LogVerbose = !(verbose == ActionPreference.Ignore || verbose == ActionPreference.SilentlyContinue);
            _LogInformation = !(information == ActionPreference.Ignore || information == ActionPreference.SilentlyContinue);
        }

        public ReceivePipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
            {
                return this;
            }

            _Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            _Option.Execution.InconclusiveWarning = option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning;
            _Option.Execution.NotProcessedWarning = option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning;
            _Option.Execution.Limit = option.Execution.Limit ?? ExecutionOption.Default.Limit;

            return this;
        }

        public ReceivePipeline Build(InvokeRulePipeline invokePipeline)
        {
            //var context = PipelineContext.New(logger: _Logger, option: _Option, bindTargetName: _BindTargetNameHook, logError: _LogError, logWarning: _LogWarning, logVerbose: _LogVerbose, logInformation: _LogInformation);
            return new ReceivePipeline(recievers: _Receivers.ToArray(), invokePipeline: invokePipeline, option: _Option);
        }
    }
}
