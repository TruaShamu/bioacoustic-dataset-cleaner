public class Program
{

    async static Task Main(String[] args)
    {
        string inputFilePath = @"C:\Users\trua\jello\TestCase\original-with-human.wav";
        List<(double startTicks, double endTicks)> speechTimeStamp = await DataCleaning.ListSpeechTimeStamp(inputFilePath);

        foreach (var entry in speechTimeStamp) {
            Console.WriteLine(entry.startTicks + " " + entry.endTicks);
        }

        string ffmpegCommand = DataCleaning.buildRemovalFFmpegCommand(inputFilePath, "jello.wav", speechTimeStamp);
        FFmpegProcessor.FFmpegExecRunner(ffmpegCommand);

    }
}
