using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyProxy.Objects.Delegates
{
    public delegate object WhenConditionCall(WhenConditionCallArgs args);

    public class WhenConditionCallArgs
    {
        public object? Sender { get; }
        public MethodInfo? Method { get; }
        public object?[]? Arguments { get; }
        public object? Result { get; }

        public WhenConditionCallArgs(object sender, string name, object[] args, object result)
        {
            Sender = sender;
            Method = sender.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, args.Select(s => s.GetType()).ToArray());
            Arguments = args;
            Result = result;
        }
    }
}
