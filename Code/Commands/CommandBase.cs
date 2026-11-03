using System.Text;
using CommandSystem;
using TssUtils.Code.Misc.Classes.Results;
using TssUtils.Code.Misc.Extensions;

namespace TssUtils.Code.Commands;

public abstract class CommandBase : ICommand, IHelpProvider
{
    private static readonly char[] StringStarters = ['"', '\''];
    public abstract ConsoleType ConsoleType { get; }
    public static char[] StringArgumentStarters => [.. StringStarters];

    public bool Execute(
        ArraySegment<string> arguments,
        ICommandSender sender,
        out string response)
    {
        if (
            TryGet<string[]>
            .RunSafe(() =>
                ParseArguments(arguments)
            )
            .HasErrored(
                out var argEx,
                out var args
            )
        )
        {
            response = argEx.GetFancyMessage();
            return false;
        }

        try
        {
            if (
                !Execute(args, sender)
                    .HasErrored(
                        out var errMsg,
                        out response
                    )
            )
                return true;

            response = errMsg.GetFancyMessage();
        }
        catch (Exception ex)
        {
            response = ex.GetFancyMessage();
        }

        return false;
    }

    public abstract string Command { get; }
    public abstract string[] Aliases { get; }
    public abstract string Description { get; }

    public string GetHelp(ArraySegment<string> arguments) =>
        TryGet<string[]>
            .RunSafe(() =>
                ParseArguments(arguments)
            )
            .HasErrored(
                out var argErrMsg,
                out var args
            )
            ? argErrMsg.GetFancyMessage()
            : GetHelp(args);

    protected virtual string GetHelp(string[] args) => Description;

    protected abstract StringTryGet Execute(string[] args, ICommandSender sender);

    // the core idea and code done by me; polishing + comments made by Claude
    private static string[] ParseArguments(ArraySegment<string> args)
    {
        var result = new List<string>();
        char? quoteChar = null; // Current quote character being read, null if not in a string
        var stringBuffer = new StringBuilder();

        foreach (var arg in args)
        {
            // todo translations
            var contentAfterStrErr = $"Nieoczekiwana zawartość po zamknięciu cytatu: {arg} <- TUTAJ";
            var quoteInMiddleErr = $"Nieoczekiwany znak cytatu w środku ciągu znaków: {arg} <- TUTAJ";

            // Case 1: We're currently inside a quoted string
            if (quoteChar.HasValue)
            {
                var closingIndex = arg.IndexOf(quoteChar.Value);

                if (closingIndex >= 0)
                {
                    // Found closing quote
                    stringBuffer.Append(' ').Append(arg[..closingIndex]);
                    result.Add(stringBuffer.ToString());

                    // Check for content after closing quote
                    if (closingIndex < arg.Length - 1 && !string.IsNullOrWhiteSpace(arg[(closingIndex + 1)..]))
                        throw new FormatException(contentAfterStrErr);

                    stringBuffer.Clear();
                    quoteChar = null;
                }
                else if (arg.Contains(quoteChar.Value))
                {
                    // Quote appears mid-string (not at start or end)
                    throw new FormatException(quoteInMiddleErr);
                }
                else
                {
                    // Continue accumulating
                    stringBuffer.Append(' ').Append(arg);
                }

                continue;
            }

            // Case 2: Starting a new potential quoted string
            if (arg.Length >= 2 && IsQuoteCharacter(arg[0]))
            {
                if (arg[^1] == arg[0])
                {
                    // Complete quoted string in single argument
                    result.Add(arg.Length == 2 ? string.Empty : arg[1..^1]);
                }
                else if (arg[1..].Contains(arg[0]))
                {
                    // Quote appears in middle (not just open/close)
                    throw new FormatException(quoteInMiddleErr);
                }
                else
                {
                    // Start of multi-argument string
                    quoteChar = arg[0];
                    stringBuffer.Append(arg[1..]);
                }

                continue;
            }

            // Case 3: Single character quote starter
            if (IsQuoteCharacter(arg[0]))
            {
                quoteChar = arg[0];
                stringBuffer.Clear();
                continue;
            }

            // Case 4: Regular unquoted argument
            result.Add(arg);
        }

        return quoteChar.HasValue
            ? throw new InvalidOperationException(
                $"Niedomknięty ciąg znaków. Oczekiwano {quoteChar} na końcu argumentów."
            )
            : [.. result];
    }

    private static bool IsQuoteCharacter(char c) => StringStarters.Contains(c);
}

[Flags]
public enum ConsoleType : byte
{
    None = 0,
    Player = 1,
    Server = 1 << 1,
    RemoteAdmin = 1 << 2
}
