using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProxy.Objects.Delegates
{
    public delegate object AfterMethodCall(AfterMethodCallArgs args);

    public class AfterMethodCallArgs
    {
        public object Sender { get; }

        public string MethodName { get; }

        public object Result { get; }


        public AfterMethodCallArgs(object sender, string methodName, object result)
        {
            Sender = sender;
            MethodName = methodName;
            Result = result;
        }
    }
}
