using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyProxy.Objects.Delegates
{
    public delegate object AfterMethodCall(AfterMethodCallArgs args);

    public class AfterMethodCallArgs
    {
        public object Sender { get; }

        public MethodInfo? Method { get; }
        public string MethodName { get; }

        public object Result { get; }


        public AfterMethodCallArgs(object sender, string methodName, object result)
        {
            Sender = sender;
            MethodName = methodName;
            Method = DelegatesHelpers.GetCurrentMethod(sender, methodName);
            Result = result;
        }
    }
}
