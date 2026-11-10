using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using TssUtils.Code.Classes;
using TssUtils.Code.Classes.Results;
using TssUtils.Code.Extensions;
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
    public string[] BlacklistedAssemblyNames { get; set; } = [];

    public override bool Execute()
    {
        (AssemblyName Name, string FullPath)[] outputAssemblyNames = [];
        (AssemblyName Name, string FullPath)[] referenceAssemblyNames = [];

        try
        {
            BlacklistedAssemblyNames =
            [
                ..
                BlacklistedAssemblyNames
                    .Select(static str => str.Trim())
            ];

            outputAssemblyNames =
            [
                .. Directory.GetFiles(OutputPath, "*.dll")
                    .Select((AssemblyName, string)? (path) =>
                        {
                            if (
                                !TryGet<AssemblyName>
                                    .RunSafe(() =>
                                        AssemblyName.GetAssemblyName(path)
                                    )
                                    .HasErrored(out _, out var value)
                            )
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
                            if (
                                !TryGet<AssemblyName>
                                    .RunSafe(() =>
                                        AssemblyName.GetAssemblyName(path)
                                    )
                                    .HasErrored(out _, out var value)
                            )
                                return (value, path);

                            Log.LogWarning(
                                $"Failed the load of assembly name for [reference] {Path.GetFileName(path)}");
                            return null;
                        }
                    )
                    .RemoveNulls()
            ];

            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            if (!File.Exists(AssemblyPath))
            {
                Log.LogError("Target DLL not found: " + AssemblyPath);
                return false;
            }

            var assemblyToCheck = Assembly.LoadFrom(AssemblyPath);

            LogMsg($"\n{nameof(assemblyToCheck)} is now {assemblyToCheck.FullName}\n");

            var methods = outputAssemblyNames
                .Select(tuple =>
                    {
                        if (BlacklistedAssemblyNames.Any(name => name == tuple.Name.Name))
                        {
                            LogMsg($"Assembly {tuple.Name.Name} is blacklisted. Skipping.");
                            return null;
                        }

                        if (
                            AppDomain.CurrentDomain
                                .GetAssemblies()
                                .FirstOrDefault(loaded =>
                                    loaded.GetName().Name == tuple.Name.Name
                                )
                            is { } foundAssembly
                        )
                            return foundAssembly;

                        if (
                            !TryGet<Assembly>
                                .RunSafe(() =>
                                    Assembly.LoadFrom(tuple.FullPath)
                                )
                                .HasErrored(out _, out var value)
                        )
                            return value;

                        Log.LogWarning($"Failed the loading of assembly {tuple.Name}");
                        return null;
                    }
                )
                .RemoveNulls()
                .AppendIfAll(assemblyToCheck, loaded => loaded != assemblyToCheck)
                .SelectMany(loaded =>
                    {
                        if (
                            !TryGet<Type[]>
                                .RunSafe(loaded.GetTypes)
                                .HasErrored(out _, out var types)
                        )
                            return types;

                        Log.LogWarning($"Failed the loading of types for assembly {loaded.FullName}");
                        return [];
                    }
                )
                .SelectMany(static t =>
                    t.GetMethods(
                        BindingFlags.Static
                        | BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.DeclaredOnly
                    )
                )
                .Where(method =>
                    {
                        if (
                            !TryGet<bool>
                                .RunSafe(() =>
                                    method.IsDefined(typeof(VerifyMethodAttribute), false)
                                )
                                .HasErrored(out _, out var value)
                        )
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

                if (
                    method.IsDefined(
                        typeof(AsyncStateMachineAttribute),
                        false
                    )
                )
                    throw new InvalidOperationException(
                        $"Method cannot be async\n\n{longName}\n"
                    );

                if (method.ContainsGenericParameters)
                    throw new InvalidOperationException(
                        $"Method cannot have generic parameters\n\n{longName}\n"
                    );

                if (
                    method.GetParameters().Length is not 1
                    || method.GetParameters()[0].ParameterType != typeof(Assembly)
                    || method.ReturnType != typeof(void)
                )
                    throw new FormatException(
                        $"Method does not have a void (Assembly) signature\n\n{longName}\n"
                    );

                LogMsg($"Checked {ShortMethodName(method)}");
            }

            LogMsg("\nRUNNING VERIFIERS\n");

            foreach (var method in methods)
            {
                method.Invoke(null, [assemblyToCheck]);
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

                if (
                    AppDomain.CurrentDomain
                        .GetAssemblies()
                        .FirstOrDefault(assembly =>
                            assembly.GetName().Name == nameObj.Name
                        )
                    is { } foundLoaded
                )
                    return foundLoaded;

                if (
                    outputAssemblyNames
                        .FirstOrNullable(tuple =>
                            tuple.Name.Name == nameObj.Name
                        )
                    is { } foundOutputTuple
                )
                    return Assembly.LoadFrom(foundOutputTuple.FullPath);

                if (
                    referenceAssemblyNames
                        .FirstOrNullable(tuple =>
                            tuple.Name.Name == nameObj.Name
                        )
                    is { } foundReferenceTuple
                )
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
            Log.LogMessage(MessageImportance.High, message);
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
