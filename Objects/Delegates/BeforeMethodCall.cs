using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyProxy.Objects.Delegates
{
    public delegate void BeforeMethodCall(BeforeMethodCallArgs args);

    public class BeforeMethodCallArgs
    {
        public object? Sender { get; }
        public MethodInfo? Method { get; }

        public object?[]? Arguments { get; }

        public BeforeMethodCallArgs(object sender, string name, object[] args)
        {
            Sender = sender;
            Method = DelegatesHelpers.GetCurrentMethod(sender);
            Arguments = args;
        }

    }
}
