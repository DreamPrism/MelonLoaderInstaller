using MelonLoaderInstaller.Core;

namespace MelonLoaderInstaller.ConsolePatcher;

internal class ConsoleLogger:IPatchLogger
{
    public void Log(string message) => Console.WriteLine(message);
}