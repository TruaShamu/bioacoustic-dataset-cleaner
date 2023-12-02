public class Program
{
    public static string destDirectory = @"D:\Sofia\Wrapup\bioacoustic-dataset-cleaner\TestCase\Done"; // Directory to store cleaned files
    async static Task Main(String[] args)
    {
        string sourceDirectory = @"D:\Sofia\Wrapup\bioacoustic-dataset-cleaner\TestCase"; // Directory to be cleaned
        Console.WriteLine(sourceDirectory);
        await ProcessSubdirectories(sourceDirectory);
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

            foreach (string origAudio in audioFiles)
            {
                Console.WriteLine(origAudio);
                
                // The new filepath to store the wav conversion of origAudio
                string guidString = Guid.NewGuid().ToString();
                string wavPath = directory + @"\"+ guidString + ".wav";

                // Convert the origAudio in source directory to wav format
                string wavConvert = DataCleaning.buildWavConversionCommand(origAudio, wavPath);
                FFmpegProcessor.FFmpegExecRunner(wavConvert);

                // Delete the original audio file since we have the wav version now
                DeleteFile(origAudio);

                // Get the list of timestamps for speech in the wav file
                List<(double startTicks, double endTicks)> speechTimeStamp = await DataCleaning.ListSpeechTimeStamp(wavPath);


                // The new filepath to store the cleaned audio
                guidString = Guid.NewGuid().ToString();
                string finalDest = destDirectory + @"\"+ guidString + ".wav";

                // Clean the audio and write it to destination
                if (speechTimeStamp.Count != 0)
                {
                    string removeSpeech = DataCleaning.buildRemovalFFmpegCommand(wavPath, finalDest, speechTimeStamp);
                    FFmpegProcessor.FFmpegExecRunner(removeSpeech);
                } else {
                    File.Copy(wavPath, finalDest);
                }
                
                
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
