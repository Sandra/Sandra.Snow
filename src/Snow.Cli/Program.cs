using Spectre.Console;

namespace Snow.Cli;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            return await SimpleProgram.RunAsync(args);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Operation cancelled by user[/]");
            return 130; // Standard cancellation exit code
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShowLinks);
            return 1; // General error exit code
        }
        finally
        {
            // Ensure all console output is flushed
            Console.Out.Flush();
            Console.Error.Flush();
        }
    }
}
