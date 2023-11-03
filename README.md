**bioacoustic-dataset-cleaner**
- refactoring needed
- and to check between repetitive transcription (bioacoustic) vs nonrepetitive transcription (noise)
- also see if possible to get mfcc directly
- and to store the keys in a KEY VAULT (the way it should be done!!)
- note this is resource intensive (1 api call / audio sample)

To use (build process has not been tested yet but should reasonably work)

1. In Program.cs replace rootDirectory with root directory to audio (folder structure should be "root directory" --> species subdirectory --> species audio files
2. In FFmpegProcessor.cs replace ffmpegPath with path to ffmpeg.exe
3. "dotnet restore" from terminal
4. "dotnet build" from terminal
5. "dotnet run" from terminal
