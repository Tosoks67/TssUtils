using System.Reflection;
using LabApi.Features.Console;
using RemoteAdmin;
using TssUtils.Code.Misc.Classes;
using TssUtils.Code.Misc.Extensions;
using Console = GameCore.Console;

namespace TssUtils.Code.Commands;

public static class CommandsIndex
{
    // todo checking already initialized
    private static readonly ConcurrentList<CommandBase> Commands = [];

    public static void AddAll(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var loadedCommandTypes = Commands
            .Select(static cmdBase =>
                cmdBase.GetType()
            )
            .ToArray();

        var validCommands = GetCommands(assembly)
            .Where(command =>
                !loadedCommandTypes.Contains(command.GetType())
            )
            .ToArray();

        Commands.AddRange(validCommands);

        foreach (var cmd in validCommands)
        foreach (var cType in cmd.ConsoleType.GetFlags())
            try
            {
                switch (cType)
                {
                    case ConsoleType.Player:
                    {
                        QueryProcessor.DotCommandHandler.RegisterCommand(cmd);
                        break;
                    }
                    case ConsoleType.Server:
                    {
                        Console.ConsoleCommandHandler.RegisterCommand(cmd);
                        break;
                    }
                    case ConsoleType.RemoteAdmin:
                    {
                        CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(cmd);
                        break;
                    }
                    case ConsoleType.None:
                    default:
                    {
                        throw new ArgumentException($"Unknown command type: {cType}");
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                var formattedEx = ex.ToString();
#else
                var formattedEx = ex.GetFancyMessage();
#endif
                Logger.Error($"Exception during initialization of {cmd.GetType().Format()}: {formattedEx}");
            }
    }

    [VerifyMethod]
    private static void Verify(Assembly assembly)
    {
        GetCommands(assembly)
            .Where(static cmd => !cmd.GetType().Name.EndsWith("Command"))
            .ForEach(static cmd =>
                throw new FormatException(
                    $"Command \"{cmd.GetType().Format()}\" doesn't end with 'Command'"
                )
            );
    }

    private static IEnumerable<CommandBase> GetCommands(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(static t =>
                t is { IsClass: true, IsAbstract: false, IsGenericType: false }
                && typeof(CommandBase).IsAssignableFrom(t)
            )
            .Select(static t => Activator.CreateInstance(t) as CommandBase)
            .OfType<CommandBase>();
    }

    public static void RemoveAll(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var commandsToRemove = Commands
            .Where(command =>
                command.GetType().Assembly == assembly
            )
            // materialization to prevent a throw on _commands.Remove()
            .ToArray();

        foreach (var cmd in commandsToRemove)
        {
            Commands.Remove(cmd);
            foreach (var cType in cmd.ConsoleType.GetFlags())
                switch (cType)
                {
                    case ConsoleType.Player:
                    {
                        QueryProcessor.DotCommandHandler.UnregisterCommand(cmd);
                        break;
                    }
                    case ConsoleType.Server:
                    {
                        Console.ConsoleCommandHandler.UnregisterCommand(cmd);
                        break;
                    }
                    case ConsoleType.RemoteAdmin:
                    {
                        CommandProcessor.RemoteAdminCommandHandler.UnregisterCommand(cmd);
                        break;
                    }
                    case ConsoleType.None:
                    default:
                    {
                        Logger.Error($"Unknown command type: {cType} [{cmd.GetType().Format()}]");
                        break;
                    }
                }
        }
    }
}
