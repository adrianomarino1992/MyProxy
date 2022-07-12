using MyProxy.Objects;
using MyProxy.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyRefs.Extensions;
using MyProxy.Exceptions;
using System.Reflection;

namespace MyProxy.Extensions
{
    public static class ProxyTypeExtensions
    {

        public static MethodBinder When<T>(this T proxy, string name) where T : class
        {
            if (!proxy.GetType().GetInterfaces().Contains(typeof(IProxyType)))
            {
                throw new InvalidTypeException(typeof(T), $"The type {proxy.GetType().Name} must have the interface {typeof(IProxyType).Name} to works correctly");
            }
            List<MethodBinder>? binders = (proxy.GetFieldValue(FieldsNames.METHODBINDERS_FIELD_NAME) as List<MethodBinder>);

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

            MethodBinder newbinder = new MethodBinder(t, m!, null, proxy, MethodBinder.ProxyMethodType.REPLACE);

            if (binder != null)
                binders!.Remove(binder);

            binders!.Add(newbinder);

            if (needSet)
            {
                proxy.SetPropertyValue(FieldsNames.METHODBINDERS_FIELD_NAME, binders);
            }

            return newbinder;
        }

        public static MethodBinder When<T>(this T proxy, string name, MyProxy.Objects.Delegates.WhenConditionCall @do) where T : class
        {
            if(!proxy.GetType().GetInterfaces().Contains(typeof(IProxyType)))
            {
                throw new InvalidTypeException(typeof(T), $"The type {proxy.GetType().Name} must have the interface {typeof(IProxyType).Name} to works correctly");
            }

            List<MethodBinder>? binders = (proxy.GetFieldValue(FieldsNames.METHODBINDERS_FIELD_NAME) as List<MethodBinder>);

            bool needSet = binders == null;

            if (needSet)
                binders = new List<MethodBinder>();

            MethodBinder? binder = binders!.FirstOrDefault(s => s.Method?.Name == name);

            Type t = proxy.GetType();
            MethodInfo? m = t.GetMethods().FirstOrDefault(s => s.Name == name);

            if(m == null)
            {
                throw new MethodNotFoundException(t, name);
            }

            MethodBinder newbinder = new MethodBinder(t, m!, @do, proxy, MethodBinder.ProxyMethodType.REPLACE);

            if (binder != null)
                binders!.Remove(binder);

            binders!.Add(newbinder);

            if(needSet)
            {
                proxy.SetPropertyValue(FieldsNames.METHODBINDERS_FIELD_NAME, binders);
            }

            return newbinder;
        }
    }
}
