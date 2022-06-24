using MyProxy.Exceptions;
using MyProxy.Objects;
using MyRefs.Extensions;
using System.Reflection;

namespace MyProxy
{
    public static class DynamicExtensions
    {
        

        public static I AddProxy<T, I>(this T @object, BeforeMethodCall before, AfterMethodCall after)
        {
#pragma warning disable

            if (!typeof(I).IsInterface)
                throw new InvalidTypeException(typeof(T));

            I o = default(I);

            try
            {
                o = (I)Activator.CreateInstance(new TypeGenerator(before, after).GenerateTypeFrom(typeof(T), typeof(I)));
            }
            catch
            {
                throw new FailCastException(typeof(T), typeof(I));
            }            

            o.GetType().GetField("_delegate_before_call", BindingFlags.Public | BindingFlags.Instance)
             .SetValue(o, before);

            o.GetType().GetField("_delegate_after_call", BindingFlags.Public | BindingFlags.Instance)
            .SetValue(o, after);

            @object.m_Copy(o);

            return o;
#pragma warning restore

        }

        private static void m_Copy<T, I>(this T source, I target)
        {
#pragma warning disable
            source.GetPublicProperties(s => true).ToList().ForEach(s =>
            {
                object v = s.GetValue(source);

                try
                {
                    s.SetValue(target, v);
                }
                catch { }
            });

            source.GetPublicFields(s => true).ToList().ForEach(s =>
            {
                object v = s.GetValue(source);

                try
                {
                    s.SetValue(target, v);
                }
                catch { }
            });
#pragma warning restore

        }
    }
}
