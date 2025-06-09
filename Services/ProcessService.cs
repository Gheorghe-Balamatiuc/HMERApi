using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HMERApi.Services;

public interface IProcessService
{
    Task<(string, string)> RunAsync(string createdImageName);
}

public partial class ProcessService(ILogger<ProcessService> logger) : IProcessService
{
    [GeneratedRegex(@"(\d)\s+(\d)")]
    private static partial Regex DigitSpaceDigitRegex();

    private async Task<string> RunPythonScriptAsync(string createdImageName)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = "Services/HMER/inference.py --config Services/HMER/14.yaml --image_path uploads/" + createdImageName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            logger.LogError("Failed to start Python process");
            throw new InvalidOperationException("Failed to start Python process");
        }

        string imagePrediction = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            logger.LogError("Python process failed with exit code {ExitCode}: {Stderr}", process.ExitCode, stderr);
            throw new InvalidOperationException($"Python process failed with exit code {process.ExitCode}: {stderr}");
        }

        logger.LogInformation("Python process completed successfully: {Stdout}", imagePrediction);

        return imagePrediction.Trim();
    }
    
    private async Task<string> ConvertToMathMLAsync(string imagePrediction)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add("C:/Python312/Lib/site-packages/latex2mathml/converter.py");
        psi.ArgumentList.Add("-t");
        psi.ArgumentList.Add(imagePrediction);

        using var processMathML = Process.Start(psi);

        if (processMathML == null)
        {
            logger.LogError("Failed to start MathML conversion process");
            throw new InvalidOperationException("Failed to start MathML conversion process");
        }

        var mathML = await processMathML.StandardOutput.ReadToEndAsync();
        string stderr = await processMathML.StandardError.ReadToEndAsync();
        processMathML.WaitForExit();

        if (processMathML.ExitCode != 0)
        {
            logger.LogError("Latex to MathML conversion failed with exit code {ExitCode}: {Stderr}", processMathML.ExitCode, stderr);
            throw new InvalidOperationException($"Latex to MathML conversion failed with exit code {processMathML.ExitCode}: {stderr}");
        }

        logger.LogInformation("Latex to MathML conversion completed successfully: {MathML}", mathML);

        return mathML;
    }

    private async Task<string> ConvertToSpeechAsync(string mathML)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "C:/Program Files/nodejs/npx.cmd",
            Arguments = "sre -d clearspeak",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var processSpeech = Process.Start(psi);
        if (processSpeech == null)
        {
            logger.LogError("Failed to start speech conversion process");
            throw new InvalidOperationException("Failed to start speech conversion process");
        }

        await processSpeech.StandardInput.WriteAsync(mathML);
        processSpeech.StandardInput.Close();

        string speechOutput = await processSpeech.StandardOutput.ReadToEndAsync();
        string stderr = await processSpeech.StandardError.ReadToEndAsync();
        processSpeech.WaitForExit();

        if (processSpeech.ExitCode != 0)
        {
            logger.LogError("Speech conversion failed with exit code {ExitCode}: {Stderr}", processSpeech.ExitCode, stderr);
            throw new InvalidOperationException($"Speech conversion failed with exit code {processSpeech.ExitCode}: {stderr}");
        }

        logger.LogInformation("Speech conversion completed successfully: {SpeechOutput}", speechOutput);

        return speechOutput.Trim();
    }

    public async Task<(string, string)> RunAsync(string createdImgeName)
    {
        var imagePrediction = await RunPythonScriptAsync(createdImgeName);
        
        imagePrediction = DigitSpaceDigitRegex().Replace(
            DigitSpaceDigitRegex().Replace(imagePrediction, "$1$2"),
            "$1$2"
        );

        var mathML = await ConvertToMathMLAsync(imagePrediction);
        var speechOutput = await ConvertToSpeechAsync(mathML);

        return (imagePrediction, speechOutput);
    }
}