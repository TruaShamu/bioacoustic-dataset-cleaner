using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class DataCleaning {

    // Azure Speech Resource Secrets (need to store with keyvault later, but lower priority for now)
    public static string speechServiceKey = "b54b9731878a4f4083c6b41dc774a92c";
    public static string speechRegion = "eastus";

    // Return a list of speech timestamp (start, end) in seconds
    public static async Task<List<(double startTicks, double endTicks)>> ListSpeechTimeStamp(string inputFilePath) {
        var config = SpeechConfig.FromSubscription(speechServiceKey, speechRegion);

        // Language auto-detection (English, Spanish, Portuguese)        
        var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "es-ES", "pt-BR" });
        var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Speech timestamps
        List<(double startTicks, double endTicks)> speechTimestamps = new List<(double, double)>();

        using (var audioInput = AudioConfig.FromWavFileInput(inputFilePath))
        {
            using (var recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, audioInput))
            {
                recognizer.Recognized += (s, e) =>
                {
                    // Speech recognized
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        var autoDetectSourceLanguageResult = AutoDetectSourceLanguageResult.FromResult(e.Result);
                        long startTicks = e.Result.OffsetInTicks;
                        long endTicks = startTicks + e.Result.Duration.Ticks;
                        string recognizedText = e.Result.Text;
                        speechTimestamps.Add(((double)startTicks / (double)10000000, (double)endTicks / (double)10000000));
                        Console.WriteLine($"RECOGNIZED: Text={recognizedText}");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");
                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                    stopRecognition.TrySetResult(0);
                };

                recognizer.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("\n    Session started event.");
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    Console.WriteLine("\nStop recognition.");
                    stopRecognition.TrySetResult(0);
                };

                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
                Task.WaitAny(new[] { stopRecognition.Task });
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }
        }
        return speechTimestamps;
    }

    // Build the ffmpeg command for removing all the timestamps from the audio!
    public static string buildRemovalFFmpegCommand(string inputFilePath, string outputFilePath, List<(double startTicks, double endTicks)> speechTimestamps) {
        string range = "";
        for (int i = 0; i < speechTimestamps.Count; i++)
        {
            range += "between(t," + speechTimestamps[i].startTicks + "," + speechTimestamps[i].endTicks + ")";
            if (i != speechTimestamps.Count - 1)
            {
                range += "+";
            }
        }

        // Wrap the input and output file paths in double quotes
        string inputPath = $"\"{inputFilePath}\"";
        string outputPath = $"\"{outputFilePath}\"";

        // Construct the FFmpeg command
        return $"-i {inputPath} -af \"aselect='not({range})'\" {outputPath}";
    }


    // FFmpeg Build command for conversion mp3 to wav
    public static string buildWavConversionCommand(string inputFilePath, string outputFilePath) {
        return $"-i \"{inputFilePath}\" \"{outputFilePath}\"";
    }
}
