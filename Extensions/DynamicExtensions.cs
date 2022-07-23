﻿using MyProxy.Exceptions;
using MyProxy.Objects;
using MyProxy.Objects.Delegates;
using MyRefs.Extensions;
using System.Reflection;

namespace MyProxy
{
    public static class DynamicExtensions
    {

        public static I ActLike<I>(this object @object, params object[] ctorArgs) where I : class
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(null, null, null, ctorArgs);
#pragma warning restore IDE004
        }

        public static I ActLike<T, I>(this T @object, params object[] ctorArgs) where T : class, I
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(null,null,null, ctorArgs);
#pragma warning restore IDE004
        }

        public static I InjectCode<I>(this object @object, BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs) where I : class
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(before, after, replace, ctorArgs);
#pragma warning restore IDE004
        }


        public static I InjectCode<T, I>(this T @object, BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace,  params object[] ctorArgs) where T : class , I 
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(before, after, replace, ctorArgs);
#pragma warning restore IDE004
        }

        private static object _InjectCode(this object @object, BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs) 
        {
            Type bType = @object.GetType();

            ConstructorInfo? ctor = MyRefs.Extensions.ReflectionExtension.GetCtorByParamsType(bType, ctorArgs.Select(s => s.GetType()).ToArray());
            if (ctor == null)
                throw new MyRefs.Exceptions.ContructorNotFoundException(bType, $"The type {bType.Name} do not have a constructor like {bType.Name}({String.Join(",", ctorArgs.Select(s => s.GetType().Name).ToArray())})");

#pragma warning disable

            object o = null;

            try
            {
                o = Activator.CreateInstance(new TypeGenerator(before, after, replace).GenerateTypeFrom(bType), ctorArgs);
            }
            catch (Exception ex)
            {
                throw new FailCastException(bType, bType, ex.Message);
            }

            o.GetType().GetField(FieldsNames.BEFORE_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
             .SetValue(o, before);

            o.GetType().GetField(FieldsNames.AFTER_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(o, after);

            o.GetType().GetField(FieldsNames.REPLACE_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(o, replace);

            @object.m_Copy(o);

#pragma warning restore;

            return o;
        }

        public static I CreateObjectWithProxy<T,I>(BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs) where T : class, I 
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
                o = (I)Activator.CreateInstance(new TypeGenerator(before, after, replace).GenerateTypeFrom(typeof(T), typeof(I)), ctorArgs);
            }
            catch(Exception ex)
            {
                throw new FailCastException(typeof(T), typeof(I), ex.Message);
            }

            o.GetType().GetField(FieldsNames.BEFORE_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
             .SetValue(o, before);

            o.GetType().GetField(FieldsNames.AFTER_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(o, after);

            o.GetType().GetField(FieldsNames.REPLACE_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
           .SetValue(o, replace);

#pragma warning restore
            return o;
        }

        public static I AddProxy<T, I>(this T @object, BeforeMethodCall before, AfterMethodCall after, ReplaceMethodCall replace,  params object[] ctorArgs) where T : class, I
        {
            I o = CreateObjectWithProxy<T,I>(before, after, replace,ctorArgs);

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
