using System;
using System.Collections.Generic;
using LLama.Abstractions;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;

internal static class IInferanceParamsExtensions
{
    public static ISamplingPipeline Create(this IInferenceParams @params, ref ISamplingPipeline? pipeline)
    {
        // This method exists to adapt the old style of inference params to the newer sampling pipeline system. It's touching a lot
        // of obsolete things which we don't really care about, disable the warning.
        #pragma warning disable CS0618 // Type or member is obsolete

        if (@params.Mirostat == MirostatType.Mirostat)
        {
            if (pipeline is not MirostatSamplingPipeline)
                pipeline = new MirostatSamplingPipeline();

            var m = (MirostatSamplingPipeline)pipeline;
            m.Eta = @params.MirostatEta;
            m.Tau = @params.MirostatTau;
            return m;
        }

        if (@params.Mirostat == MirostatType.Mirostat2)
        {
            if (pipeline is not Mirostat2SamplingPipeline)
                pipeline = new Mirostat2SamplingPipeline();

            var m = (Mirostat2SamplingPipeline)pipeline;
            m.Eta = @params.MirostatEta;
            m.Tau = @params.MirostatTau;
            return m;
        }

        if (pipeline is not DefaultSamplingPipeline)
            pipeline = new DefaultSamplingPipeline();

        var d = (DefaultSamplingPipeline)pipeline;
        d.AlphaPresence = @params.PresencePenalty;
        d.MinP = @params.MinP;
        d.PenalizeNewline = @params.PenalizeNL;
        d.RepeatPenalty = @params.RepeatPenalty;
        d.TailFreeZ = @params.TfsZ;
        d.Temperature = @params.Temperature;
        d.TopK = @params.TopK;
        d.TopP = @params.TopP;
        d.AlphaFrequency = @params.FrequencyPenalty;
        d.TypicalP = @params.TypicalP;
        d.Grammar = @params.Grammar;

        d.LogitBias.Clear();

        return d;

        #pragma warning restore CS0618 // Type or member is obsolete
    }
}