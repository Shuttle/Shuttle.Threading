using Microsoft.Extensions.Options;

namespace Shuttle.Core.Threading;

public class ProcessorIdleOptionsValidator : IValidateOptions<ProcessorIdleOptions>
{
    public ValidateOptionsResult Validate(string? name, ProcessorIdleOptions options)
    {
        if (options.Durations.Count == 0)
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.ProcessorIdleOptionsDurationException, name ?? "unknown"));
        }

        return ValidateOptionsResult.Success;
    }
}