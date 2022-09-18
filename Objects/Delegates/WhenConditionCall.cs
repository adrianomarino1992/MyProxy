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
        public Type[] GenericArguments { get; internal set; }


        public WhenConditionCallArgs(object sender, string name, object[] args, object result, Type[] genericArguments)
        {
            Sender = sender;

            GenericArguments = genericArguments;


            MethodInfo[] methods = sender.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Type[] argsTypes = args.Select(s => s.GetType()).ToArray();

            foreach (var mt in methods)
            {
                try { var _ = mt.CallingConvention; } catch { continue; }

                if (mt.Name == name)
                {
                    var parameters = mt.GetParameters().Select(s => s.ParameterType).ToArray();

                    if (
                        (args.All(s => s != null) && parameters.All(t => argsTypes.Any(u => u.Equals(t) || t.IsAssignableFrom(u)))) || 
                        args.Count() == parameters.Count()
                        )
                    {
                        if (GenericArguments != null)
                        {
                            if (mt.IsGenericMethod && mt.GetGenericArguments().Length == GenericArguments.Count())
                            {
                                Method = mt.MakeGenericMethod(GenericArguments);
                            }
                            else
                            {
                                Method = mt;
                            }
                        }
                        else
                        {
                            Method = mt;
                        }
                    }
                }

            }


            Arguments = args;
            Result = result;
        }
    }
}
