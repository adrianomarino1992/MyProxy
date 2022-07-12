using MyProxy.Objects.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MyRefs.Extensions;

namespace MyProxy.Objects
{
    public class MethodBinder
    {
        public Type Type { get; private set; }
        public object Sender { get; private set; }
               
        public ProxyMethodType ProxyType { get; private set; }
        public MethodInfo Method { get; private set; }

        public MyProxy.Objects.Delegates.WhenConditionCall? Action { get; private set; }

        public MethodBinder(Type type, MethodInfo method, WhenConditionCall? action, object sender, ProxyMethodType proxyType)
        {
            Type = type;
            Method = method;
            Action = action;
            Sender = sender;
            ProxyType = proxyType;
        }

        public void Do(WhenConditionCall call)
        {
            this.Action = call;
            this.ProxyType = ProxyMethodType.DO;
        }

        public enum ProxyMethodType 
        {
            DO,
            BEFORE,
            REPLACE, 
            AFTER,
            RETURN
        }

        public class MethodBinderManager 
        {
            public object Sender { get; }
            public string Name { get; }
            public object[] Args { get; }

            public object Result { get; }

            public MethodBinderManager(object sender, string name, object[] args, object result)
            {
                Sender = sender;
                Name = name;
                Args = args;
                Result = result;
            }

            public static bool HasDoProxy(string method, List<MethodBinder> binders)
            {
                if (binders == null)
                    return false;

                MethodBinder? binder = binders.FirstOrDefault(s => s.Method?.Name == method && s.ProxyType == ProxyMethodType.DO);

                if (binder == null || binder.Action == null)
                    return false;

                return true;
            }

            public static object ExecuteDOMethod(MethodBinderManager args, List<MethodBinder> binders)
            {
                return m_Run(args, binders, ProxyMethodType.DO);
            }

            public static object CheckBinder(MethodBinderManager args, List<MethodBinder> binders)
            {
                return m_Run(args, binders);
            }

            private static object m_Run(MethodBinderManager args, List<MethodBinder> binders, ProxyMethodType typeCall = ProxyMethodType.REPLACE)
            {
                if (binders == null)
                    return args.Result;

                MethodBinder? binder = binders.FirstOrDefault(s => s.Method?.Name == args.Name && s.ProxyType == typeCall);

                if (binder == null || binder.Action == null)
                    return args.Result;

                return binder.Action.Invoke(new WhenConditionCallArgs(args.Sender, args.Name, args.Args, args.Result));
            }
        }

        
                
    }

}
