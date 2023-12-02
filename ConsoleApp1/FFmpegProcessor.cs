using System.Diagnostics;


// Run ffmpeg command. Need to update ffmpeg to local path.
public class FFmpegProcessor {
    public static string ffmpegPath = @"C:\ProgramData\chocolatey\bin\ffmpeg.exe";
    public static void FFmpegExecRunner(string ffmpegArgs) {
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = ffmpegArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true // Redirect the standard input
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data); // Handle standard output
            process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);  // Handle standard error

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("FFmpeg command executed successfully.");
            }
            else
            {
                Console.WriteLine("FFmpeg command execution failed.");
            }
        }
    }
}
