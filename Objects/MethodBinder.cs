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
        public bool IsAsync { get; internal set; }
        public object[]? Args { get; private set; }
        public ProxyMethodType ProxyType { get; private set; }
        public MethodInfo Method { get; private set; }

        public Type[] GenericArguments {get; internal set;}

        public MyProxy.Objects.Delegates.WhenConditionCall? Action { get; private set; }

        public MethodBinder(Type type, MethodInfo method, WhenConditionCall? action, object sender, object[]? args,  ProxyMethodType proxyType)
        {
            Type = type;
            Method = method;
            Action = action;
            Sender = sender;
            Args = args;
            ProxyType = proxyType;
            GenericArguments = new Type[]{};
        }

        public DoPromisse Do(WhenConditionCall call)
        {
            this.Action = call;
            this.ProxyType = ProxyMethodType.DO;

            return new DoPromisse(this);
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

            public static bool HasDoProxy(string method, List<MethodBinder> binders, object[] args)
            {
                if (binders == null)
                    return false;

                List<MethodBinder>? methods = binders.Where(s => s.Method?.Name == method && s.ProxyType == ProxyMethodType.DO).ToList();

                if (methods == null || methods.Count == 0)
                    return false;

                MethodBinder? binder = methods.Where(s => s.Args != null)
                    .Where(s => s.Args!.All(a => args.Any(p => p.Equals(a)))).FirstOrDefault();

                if (binder == null)
                {
                    binder = methods.FirstOrDefault(s => s.Args is null);
                }

                if (binder == null)
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

            public static Type CheckType(object o)
            {
                return o.GetType();
            }

            private static object m_Run(MethodBinderManager args, List<MethodBinder> binders, ProxyMethodType typeCall = ProxyMethodType.REPLACE)
            {
                if (binders == null)
                    return args.Result;

                List<MethodBinder>? methods = binders.Where(s => s.Method?.Name == args.Name && s.ProxyType == typeCall).ToList();

                if (methods == null || methods.Count == 0)
                    return args.Result;

                MethodBinder? binder = methods.Where(s => s.Args != null)
                    .Where(s => s.Args!.All(a => args.Args.Any(p => p.Equals(a)))).FirstOrDefault();

                if(binder == null)
                {
                    binder = methods.FirstOrDefault(s => s.Args is null);
                }

                if (binder == null)
                    return args.Result;

                if (binder.IsAsync)
                {
                    var o = binder!.Action!.Invoke(new WhenConditionCallArgs(args.Sender, args.Name, args.Args, args.Result, binder.GenericArguments){}); 

                    if (o is null)
                        return Task.CompletedTask;

                    if (o.GetType().IsAssignableTo(typeof(Task)))
                        return o;
#pragma warning disable
                    MethodInfo? createTaskGeneric = typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(o.GetType());

                    if (createTaskGeneric == null)
                        return o;

                    return createTaskGeneric!.Invoke(null, new object[] { o });
#pragma warning restore;

                }
                else
                    return binder!.Action!.Invoke(new WhenConditionCallArgs(args.Sender, args.Name, args.Args, args.Result, binder.GenericArguments));


            }
        }


        public class DoPromisse 
        {
            public MethodBinder MethodBinder { get; }

            internal DoPromisse(MethodBinder method)
            {
                MethodBinder = method;
            }

            public void AsAsyncTask()
            {
                MethodBinder.IsAsync = true;
            }
        }
    }

}
