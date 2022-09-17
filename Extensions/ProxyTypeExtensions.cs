using MyProxy.Exceptions;
using MyProxy.Objects;
using MyProxy.Objects.Interfaces;
using MyRefs.Extensions;
using System.Reflection;

namespace MyProxy.Extensions
{
    public static class ProxyTypeExtensions
    {
        public static MethodBinder WhenGenericMethod<T>(this T proxy, string name, Type[] genericArguments, params object[]? args) where T : class
        {
           _validateType(proxy);

            List<MethodBinder>? binders = _ExtractBinders(proxy);
            
            bool needSet = binders == null;

            if (needSet)
                binders = new List<MethodBinder>();

            MethodBinder? binder = binders!.FirstOrDefault(s => s.Method?.Name == name && (genericArguments == null || s.GenericArguments.All(a => genericArguments.Contains(a))));

            Type t = proxy.GetType();

            MethodInfo? m = t.GetMethods().FirstOrDefault(s => s.Name == name && (genericArguments == null || s.GetGenericArguments().All(a => genericArguments.Contains(a))));

            if (m == null)
            {
                throw new MethodNotFoundException(t, name);
            }

            MethodBinder newbinder = new MethodBinder(t, m!, null, proxy, args, MethodBinder.ProxyMethodType.DO);
            newbinder.GenericArguments = genericArguments;

            binders!.Add(newbinder);

            if (needSet)
            {
                proxy.SetPropertyValue(FieldsNames.METHODBINDERS_FIELD_NAME, binders);
            }

            return newbinder;
        }

        public static MethodBinder When<T>(this T proxy, string name) where T : class
        {
            return proxy.When(name, args: null);
        }

        public static MethodBinder When<T>(this T proxy, string name, params object[]? args) where T : class
        {
            _validateType(proxy);

            List<MethodBinder>? binders = _ExtractBinders(proxy);

            bool needSet = binders == null;

            if (needSet)
                binders = new List<MethodBinder>();

            MethodBinder? binder = binders!.FirstOrDefault(s => s.Method?.Name == name);

            Type t = proxy.GetType();
            MethodInfo? m = t.GetMethods().FirstOrDefault(s => s.Name == name);

            if (m == null)
            {
                throw new MethodNotFoundException(t, name);
            }

            MethodBinder newbinder = new MethodBinder(t, m!, null, proxy, args, MethodBinder.ProxyMethodType.DO);


            binders!.Add(newbinder);

            if (needSet)
            {
                proxy.SetPropertyValue(FieldsNames.METHODBINDERS_FIELD_NAME, binders);
            }

            return newbinder;
        }

        public static MethodBinder When<T>(this T proxy, string name, MyProxy.Objects.Delegates.WhenConditionCall @do) where T : class
        {

            _validateType(proxy);

            List<MethodBinder>? binders = _ExtractBinders(proxy);

            bool needSet = binders == null;

            if (needSet)
                binders = new List<MethodBinder>();

            MethodBinder? binder = binders!.FirstOrDefault(s => s.Method?.Name == name);

            Type t = proxy.GetType();
            MethodInfo? m = t.GetMethods().FirstOrDefault(s => s.Name == name);

            if (m == null)
            {
                throw new MethodNotFoundException(t, name);
            }

            MethodBinder newbinder = new MethodBinder(t, m!, @do, proxy, null, MethodBinder.ProxyMethodType.DO);

            binders!.Add(newbinder);

            if (needSet)
            {
                proxy.SetPropertyValue(FieldsNames.METHODBINDERS_FIELD_NAME, binders);
            }

            return newbinder;
        }

        private static void _validateType(object proxy)
        {
            if (!proxy.GetType().GetInterfaces().Contains(typeof(IProxyType)))
            {
                throw new InvalidTypeException(proxy.GetType(), $"The type {proxy.GetType().Name} must have the interface {typeof(IProxyType).Name} to works correctly");
            }
        }

        private static List<MethodBinder>? _ExtractBinders(object proxy)
        {
            return (proxy.GetFieldValue(FieldsNames.METHODBINDERS_FIELD_NAME) as List<MethodBinder>);
        }

        public static MethodsInvokedCollection GetMethodInvokeds<T>(this T proxy) where T : class
        {
            List<MethodInvoked>? calls = proxy.GetFieldValue(FieldsNames.METHODS_INVOKED_LIST) as List<MethodInvoked>;

            if (calls == null)
                calls = new List<MethodInvoked>();

            return new MethodsInvokedCollection(calls);
        }

        public static bool HasExecuted<T>(this T proxy, string method, Type[]? argsTypes = null, object[]? args = null) where T : class
        {
            var calls = proxy.GetMethodInvokeds();

            var methods = calls.Where(s => s.Method.Name == method);

            if (methods.Count() == 0)
                return false;

            if (argsTypes != null && !methods.Any(s =>
            {
                return s.Method.Name == method && s.Parameters.All(p => argsTypes!.Contains(p));

            }))
            {
                return false;
            }

            if (args != null && !methods.Any(s =>
            {
                return s.Method.Name == method && s.Args.All(p => args!.Any(a => a.Equals(p)));

            }))
            {
                return false;
            }

            return true;
        }
    }
}
