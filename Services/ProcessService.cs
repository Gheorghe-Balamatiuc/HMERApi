using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HMERApi.Services;

/// <summary>
/// Interface for processing handwritten mathematical expressions
/// </summary>
public interface IProcessService
{
    /// <summary>
    /// Processes an image and returns the LaTeX prediction and speech output
    /// </summary>
    /// <param name="createdImageName">Name of the image file to process</param>
    /// <returns>Tuple containing (LaTeX prediction, speech description)</returns>
    Task<(string, string)> RunAsync(string createdImageName);
}

/// <summary>
/// Service responsible for processing handwritten math expressions through external Python scripts
/// and converting the results to MathML and speech output
/// </summary>
public partial class ProcessService(ILogger<ProcessService> logger) : IProcessService
{
    [GeneratedRegex(@"(\d)\s+(\d)")]
    private static partial Regex DigitSpaceDigitRegex();

    /// <summary>
    /// Runs the Python inference script to predict the LaTeX representation of a handwritten math expression
    /// </summary>
    /// <param name="createdImageName">Name of the image file to process</param>
    /// <returns>LaTeX string representation of the math expression</returns>
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
    
    /// <summary>
    /// Converts LaTeX markup to MathML using Python latex2mathml package
    /// </summary>
    /// <param name="imagePrediction">LaTeX markup string</param>
    /// <returns>MathML representation of the input LaTeX</returns>
    private async Task<string> ConvertToMathMLAsync(string imagePrediction)
    {
        // Create a process to run the converter.py script
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

    /// <summary>
    /// Converts MathML to speech text using Speech Rule Engine (SRE)
    /// </summary>
    /// <param name="mathML">MathML markup string</param>
    /// <returns>Speech text describing the math expression</returns>
    private async Task<string> ConvertToSpeechAsync(string mathML)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "C:/Program Files/nodejs/npx.cmd",
            Arguments = "sre -d clearspeak", // Using clearspeak for more natural speech output
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

    /// <summary>
    /// Main processing method that orchestrates the complete workflow:
    /// 1. Run the Python model to extract LaTeX from the image
    /// 2. Clean up the LaTeX output
    /// 3. Convert LaTeX to MathML
    /// 4. Convert MathML to speech text
    /// </summary>
    /// <param name="createdImgeName">Name of the image file to process</param>
    /// <returns>Tuple with (LaTeX prediction, speech description)</returns>
    public async Task<(string, string)> RunAsync(string createdImgeName)
    {
        var imagePrediction = await RunPythonScriptAsync(createdImgeName);
        
        // Clean up the LaTeX by removing spaces between digits
        // Apply the regex twice to catch multi-digit numbers separated by spaces
        imagePrediction = DigitSpaceDigitRegex().Replace(
            DigitSpaceDigitRegex().Replace(imagePrediction, "$1$2"),
            "$1$2"
        );

        var mathML = await ConvertToMathMLAsync(imagePrediction);
        var speechOutput = await ConvertToSpeechAsync(mathML);

        return (imagePrediction, speechOutput);
    }
}