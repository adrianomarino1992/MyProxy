using System.Reflection;

namespace MyProxy.Objects.Delegates
{
    internal static class DelegatesHelpers
    {
        internal static MethodInfo? GetCurrentMethod(object sender)
        {
            return MethodInvoked.GetCurrentMethod(sender);
        }

        internal static string GenerateSignature(MethodInfo info)
        {
            return MethodInvoked.GenerateSignature(info);
        }

    }
}
