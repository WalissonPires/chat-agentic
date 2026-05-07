using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.AI.Usage;

internal static class AIUsageTokenMapper
{
    public static (long Input, long Output) FromUsageDetails(UsageDetails? usage)
    {
        if (usage is null)
            return (0, 0);

        var input = usage.InputTokenCount.GetValueOrDefault();
        var output = usage.OutputTokenCount.GetValueOrDefault();
        return (input, output);
    }

    public static (long Input, long Output) FromOpenAiSdkUsageObject(object? usage)
    {
        if (usage is null)
            return (0, 0);

        var t = usage.GetType();
        long input = 0, output = 0;

        var inputProp = t.GetProperty("InputTokenCount");
        var outputProp = t.GetProperty("OutputTokenCount");
        if (inputProp?.GetValue(usage) is int i32in)
            input = i32in;
        else if (inputProp?.GetValue(usage) is long li)
            input = li;

        if (outputProp?.GetValue(usage) is int i32out)
            output = i32out;
        else if (outputProp?.GetValue(usage) is long lo)
            output = lo;

        return (input, output);
    }
}
