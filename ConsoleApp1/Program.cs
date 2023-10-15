public class Program
{

    async static Task Main(String[] args)
    {
        string rootDirectory = @"C:\Users\trua\TestCase";
        await ProcessSubdirectories(rootDirectory);



    }

    static async Task ProcessSubdirectories(string rootDirectory)
    {
        string[] subdirectories = Directory.GetDirectories(rootDirectory);

        foreach (string subdirectory in subdirectories)
        {
            await ProcessFilesInDirectory(subdirectory);
        }
    }

    static async Task ProcessFilesInDirectory(string directory)
    {
        try
        {
            string[] audioFiles = Directory.GetFiles(directory);

            foreach (string audioFile in audioFiles)
            {
                Console.WriteLine("Original File: " + audioFile);
                Guid newGuid = Guid.NewGuid();
                string guidString = newGuid.ToString();
                string outputFilePath = directory + @"\"+ guidString + ".wav";
                Console.WriteLine("Converted Wav: " + outputFilePath);
                string wavConvert = DataCleaning.buildWavConversionCommand(audioFile, outputFilePath);
                FFmpegProcessor.FFmpegExecRunner(wavConvert);
                //DeleteFile(audioFile);
                List<(double startTicks, double endTicks)> speechTimeStamp = await DataCleaning.ListSpeechTimeStamp(audioFile);
                newGuid = Guid.NewGuid();
                guidString = newGuid.ToString();
                string finalDest = directory + @"\"+ guidString + ".wav";
                Console.WriteLine(finalDest);
                string removeSpeech = DataCleaning.buildRemovalFFmpegCommand(outputFilePath, finalDest, speechTimeStamp);
                FFmpegProcessor.FFmpegExecRunner(removeSpeech);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing directory {directory}: {ex.Message}");
        }
    }

    static bool DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            else
            {
                Console.WriteLine($"File {filePath} does not exist.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting the file {filePath}: {ex.Message}");
            return false;
        }
    }

    
}
