namespace Shuttle.Core.Threading;

internal sealed class ProcessorContextAccessor
{
    public IProcessorContext? Context { get; set; } = null;
}