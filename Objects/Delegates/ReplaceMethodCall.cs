using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyProxy.Objects.Delegates
{
    public delegate object ReplaceMethodCall(ReplaceMethodCallArgs args);

    public class ReplaceMethodCallArgs
    {
        public object? Sender { get; }
        public MethodInfo? Method { get; }
        public object?[]? Arguments { get; }

        public ReplaceMethodCallArgs(object sender, string name, object[] args)
        {
            Sender = sender;
            Method = DelegatesHelpers.GetCurrentMethod(sender, name);
            Arguments = args;
        }
    }
}
