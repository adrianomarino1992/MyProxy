using MyProxy.Exceptions;
using MyProxy.Objects;
using MyRefs.Extensions;
using System.Reflection;

namespace MyProxy
{
    public static class DynamicExtensions
    {

        public static bool Call<T>(this Task<T> r)
        {
            return true;
        }

        public static I CreateObjectWithProxy<T,I>(BeforeMethodCall before, AfterMethodCall after,params object[] ctorArgs) where T : I
        {
            if (!typeof(I).IsInterface)
                throw new InvalidTypeException(typeof(I));


            ConstructorInfo? ctor = MyRefs.Extensions.ReflectionExtension.GetCtorByParamsType<T>(ctorArgs.Select(s => s.GetType()).ToArray());
            if (ctor == null)
                throw new MyRefs.Exceptions.ContructorNotFoundException(typeof(T),$"The type {typeof(T).Name} do not have a constructor like {typeof(T).Name}({String.Join(",", ctorArgs.Select(s => s.GetType().Name).ToArray())})");

#pragma warning disable
            I o = default(I);

            try
            {
                o = (I)Activator.CreateInstance(new TypeGenerator(before, after).GenerateTypeFrom(typeof(T), typeof(I)), ctorArgs);
            }
            catch
            {
                throw new FailCastException(typeof(T), typeof(I));
            }

            o.GetType().GetField("_delegate_before_call", BindingFlags.Public | BindingFlags.Instance)
             .SetValue(o, before);

            o.GetType().GetField("_delegate_after_call", BindingFlags.Public | BindingFlags.Instance)
            .SetValue(o, after);

#pragma warning restore
            return o;
        }

        public static I AddProxy<T, I>(this T @object, BeforeMethodCall before, AfterMethodCall after) where T : I
        {
            I o = CreateObjectWithProxy<T,I>(before, after);

            @object.m_Copy(o);

            return o;

        }

        private static void m_Copy<T, I>(this T source, I target)
        {
#pragma warning disable
            source.GetAllProperties(s => true).ToList().ForEach(s =>
            {
                object v = s.GetValue(source);

                try
                {

                    s.SetValue(target, v);
                }
                catch { }
            });

            source.GetAllFields(s => true).ToList().ForEach(s =>
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
