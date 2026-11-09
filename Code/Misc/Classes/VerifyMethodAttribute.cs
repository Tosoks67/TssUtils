using JetBrains.Annotations;

namespace TssUtils.Code.Misc.Classes;

/// <summary>
///     Marks a method as a verifying method
///     (will be run during <see cref="BuildTask" /> if loaded)
/// </summary>
/// <remarks>
///     the method needs to have a
///     <c>void (Assembly)</c> signature,
///     has to be static and cannot be async
///     nor generic
/// </remarks>
/// <example>
///     <code>
///         [VerifyMethod]
///         private static void Verify(Assembly assemblyToVerify)
///         {
///         }
///     </code>
/// </example>
[MeansImplicitUse]
[UsedImplicitly]
[AttributeUsage(AttributeTargets.Method)]
public class VerifyMethodAttribute : Attribute;
