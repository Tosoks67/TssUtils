using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using TssUtils.Code.Misc.Classes;
using TssUtils.Code.Misc.Classes.Results;
using TssUtils.Code.Misc.Extensions;
using Task = Microsoft.Build.Utilities.Task;

namespace TssUtils.Code;

[UsedImplicitly]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class BuildTask : Task
{
    [Required] public string AssemblyPath { get; set; } = string.Empty;
    [Required] public string OutputPath { get; set; } = string.Empty;
    [Required] public string ReferencePath { get; set; } = string.Empty;
    public bool RunOnSelf { get; set; } = false;

    public override bool Execute()
    {
        (AssemblyName Name, string FullPath)[] outputAssemblyNames = [];
        (AssemblyName Name, string FullPath)[] referenceAssemblyNames = [];

        try
        {
            outputAssemblyNames =
            [
                .. Directory.GetFiles(OutputPath, "*.dll")
                    .Select((AssemblyName, string)? (path) =>
                        {
                            if (!TryGet<AssemblyName>.RunSafe(() =>
                                        AssemblyName.GetAssemblyName(path)
                                    )
                                    .HasErrored(out _, out var value))
                                return (value, path);

                            Log.LogWarning($"Failed the load of assembly name for [output] {Path.GetFileName(path)}");
                            return null;
                        }
                    )
                    .RemoveNulls()
            ];
            referenceAssemblyNames =
            [
                .. Directory.GetFiles(ReferencePath, "*.dll")
                    .Select((AssemblyName, string)? (path) =>
                        {
                            if (!TryGet<AssemblyName>.RunSafe(() =>
                                        AssemblyName.GetAssemblyName(path)
                                    )
                                    .HasErrored(out _, out var value))
                                return (value, path);

                            Log.LogWarning(
                                $"Failed the load of assembly name for [reference] {Path.GetFileName(path)}");
                            return null;
                        }
                    )
                    .RemoveNulls()
            ];

            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            if (!RunOnSelf && !File.Exists(AssemblyPath))
            {
                Log.LogError("Target DLL not found: " + AssemblyPath);
                return false;
            }

            var assembly = RunOnSelf
                ? typeof(BuildTask).Assembly
                : Assembly.LoadFrom(AssemblyPath);

            LogMsg($"{nameof(assembly)} is now {assembly.FullName}");

            var methods = outputAssemblyNames
                .Tap(tuple => LogMsg($"Output assembly: {tuple.Name}"))
                .Where(static tuple =>
                    AppDomain.CurrentDomain
                        .GetAssemblies()
                        .All(assembly =>
                            assembly.GetName().Name != tuple.Name.Name
                        )
                )
                .Tap(tuple => LogMsg($"Non-loaded assembly: {tuple.Name}"))
                .Select(tuple =>
                    {
                        if (!TryGet<Assembly>.RunSafe(() =>
                                    Assembly.LoadFrom(tuple.FullPath)
                                )
                                .HasErrored(out _, out var value))
                            return value;

                        Log.LogWarning($"Failed the loading of assembly {tuple.Name}");
                        return null;
                    }
                )
                .RemoveNulls()
                .Append(assembly)
                .Tap(loaded => LogMsg($"Assembly: {loaded.FullName}"))
                .SelectMany(loaded =>
                    {
                        if (!TryGet<Type[]>.RunSafe(loaded.GetTypes)
                                .HasErrored(out _, out var types))
                            return types;

                        Log.LogWarning($"Failed the loading of types for assembly {loaded.FullName}");
                        return [];
                    }
                )
                .Tap(type => LogMsg($"Type to check: {type.DeclaringType?.Name ?? "<global>"}::{type.Format()}"))
                .SelectMany(static t =>
                    t.GetMethods(
                        BindingFlags.Static
                        | BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.DeclaredOnly
                    )
                )
                .Tap(method => LogMsg($"Method to check: {ShortMethodName(method)}"))
                .Where(method =>
                    {
                        if (!TryGet<bool>.RunSafe(() =>
                                    method.IsDefined(typeof(VerifyMethodAttribute), false)
                                )
                                .HasErrored(out _, out var value))
                            return value;

                        Log.LogWarning($"Failed the attribute check for method {ShortMethodName(method)}");
                        return false;
                    }
                )
                .ToArray();

            LogMsg("\nCHECKING VERIFYING METHODS\n");

            foreach (var method in methods)
            {
                var longName = LongMethodName(method);

                if (method.IsDefined(
                        typeof(AsyncStateMachineAttribute),
                        false
                    ))
                    throw new InvalidOperationException(
                        "Method cannot be async\n\n" +
                        longName + "\n"
                    );

                if (method.ContainsGenericParameters)
                    throw new InvalidOperationException(
                        "Method cannot have generic parameters\n\n" +
                        longName + "\n"
                    );

                if (method.GetParameters().Length is not 1
                    || method.GetParameters()[0].ParameterType != typeof(Assembly)
                    || method.ReturnType != typeof(void))
                    throw new FormatException(
                        "Method does not have a void (Assembly) signature\n\n" +
                        longName + "\n"
                    );

                LogMsg($"Checked {ShortMethodName(method)}");
            }

            LogMsg("\nRUNNING VERIFIERS\n");

            foreach (var method in methods)
            {
                method.Invoke(null, [assembly]);
                LogMsg($"Successfully ran {ShortMethodName(method)}");
            }

            LogMsg("\nRAN ALL VERIFIERS\n");
        }
        catch (ReflectionTypeLoadException e)
        {
            foreach (var loaderEx in e.LoaderExceptions)
                Log.LogError("Loader Error: " + loaderEx);

            return false;
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            Log.LogError(e.InnerException.ToString());
            return false;
        }
        catch (Exception e)
        {
            Log.LogError(e.ToString());
            return false;
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolver;
        }

        return true;

        Assembly? Resolver(object _, ResolveEventArgs args)
        {
            try
            {
                var nameObj = new AssemblyName(args.Name);

                if (outputAssemblyNames
                        .FirstOrNullable(tuple => tuple.Name.Name == nameObj.Name)
                    is { } foundOutputTuple)
                    return Assembly.LoadFrom(foundOutputTuple.FullPath);

                if (referenceAssemblyNames
                        .FirstOrNullable(tuple => tuple.Name.Name == nameObj.Name)
                    is { } foundReferenceTuple)
                    return Assembly.LoadFrom(foundReferenceTuple.FullPath);
            }
            catch (Exception e)
            {
                Log.LogError("Resolver Error: " + e);
            }

            return null;
        }

        void LogMsg(string message)
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.High, message);
        }

        static string LongMethodName(MethodInfo method)
        {
            return $"{method.DeclaringType?.FullName ?? "<global>"}::{method.Name}()";
        }

        static string ShortMethodName(MethodInfo method)
        {
            return (
                    method.DeclaringType is { } declaring
                        ? $"[{declaring.Assembly.GetName().Name}] {declaring.Name}"
                        : "<global>"
                ) + $"::{method.Name}()";
        }
    }
}
