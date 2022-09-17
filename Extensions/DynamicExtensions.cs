using MyProxy.Exceptions;
using MyProxy.Objects;
using MyProxy.Objects.Delegates;
using MyRefs.Extensions;
using System.Reflection;

namespace MyProxy
{
    public static class DynamicExtensions
    {

        public static I AppendEventListener<I>(this object @object, params object[] ctorArgs) where I : class
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(null, null, null, ctorArgs);
#pragma warning restore IDE004
        }

        public static I AppendEventListener<T, I>(this T @object, params object[] ctorArgs) where T : class, I
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(null, null, null, ctorArgs);
#pragma warning restore IDE004
        }


        private static bool _checkIfTypeCanInjectCodeBlocks(Type type)
        {
            return type.GetInterfaces().Contains(typeof(MyProxy.Objects.Interfaces.IProxyType));
        }

        private static void _checkIfCanInjectCodeBlocks(object @object)
        {
              if(!_checkIfTypeCanInjectCodeBlocks(@object.GetType()))
                {
                throw new MyProxy.Exceptions.InvalidTypeException(@object.GetType(),
                 $"The object does not have the {typeof(MyProxy.Objects.Interfaces.IProxyType).Name} interface implemented to be able to inject code");
                }
        }


        public static void InjectCodeBeforeMethodCall(this object @object, BeforeMethodCall? before)
        {

            _checkIfCanInjectCodeBlocks(@object);

#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            @object._InjectCode(before, null, null, new object[]{});
#pragma warning restore IDE004
        }

        public static void InjectCodeToReplaceMethodCall(this object @object, ReplaceMethodCall? replace)
        {

            _checkIfCanInjectCodeBlocks(@object);

#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            @object._InjectCode(null, null, replace, new object[]{});
#pragma warning restore IDE004
        }

        public static void InjectCodeAfterMethodCall(this object @object, AfterMethodCall? after)
        {

            _checkIfCanInjectCodeBlocks(@object);

#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            @object._InjectCode(null, after, null, new object[]{});
#pragma warning restore IDE004
        }




        public static I InjectCode<I>(this object @object, BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs) where I : class
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(before, after, replace, ctorArgs);
#pragma warning restore IDE004
        }


        public static I InjectCode<T, I>(this T @object, BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs) where T : class, I
        {
#pragma warning disable IDE004 // keep the cast to use correct implementations of interface methods
            return (I)@object._InjectCode(before, after, replace, ctorArgs);
#pragma warning restore IDE004
        }


        private static object _InjectCode(this object @object, BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs)
        {
            Type bType = @object.GetType();

            object o = @object;

            bool needsCreateNewObject = !bType.GetInterfaces().Contains(typeof(MyProxy.Objects.Interfaces.IProxyType));
            
            if (needsCreateNewObject)
            {

                ConstructorInfo? ctor = MyRefs.Extensions.ReflectionExtension.GetCtorByParamsType(bType, ctorArgs.Select(s => s.GetType()).ToArray());
                if (ctor == null)
                    throw new MyRefs.Exceptions.ContructorNotFoundException(bType, $"The type {bType.Name} do not have a constructor like {bType.Name}({String.Join(",", ctorArgs.Select(s => s.GetType().Name).ToArray())})");

#pragma warning disable


                try
                {
                    o = Activator.CreateInstance(new TypeGenerator(before, after, replace).GenerateTypeFrom(bType), ctorArgs);
                }
                catch (Exception ex)
                {
                    throw new FailCastException(bType, bType, ex.Message);
                }

#pragma warning restore;

            }

            if(before != null)
                o.GetType().GetField(FieldsNames.BEFORE_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(o, before);

            if(after != null)
                o.GetType().GetField(FieldsNames.AFTER_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(o, after);

            if(replace != null)
                o.GetType().GetField(FieldsNames.REPLACE_CALL_METHOD_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(o, replace);

            if(needsCreateNewObject)
                @object.m_Copy(o);

            return o;
        }

        public static T CreateListener<T>() where T : class
        {
            return CreateListener<T>(null, null, null, new object[] { });
        }

        public static T CreateListener<T>(BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs) where T : class
        {
            return (T)CreateListenerFromObject(typeof(T), before, after, replace, ctorArgs);
        }

        public static object CreateListenerFromObject(Type bType, BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs)
        {
            ConstructorInfo? ctor = MyRefs.Extensions.ReflectionExtension.GetCtorByParamsType(bType, ctorArgs.Select(s => s.GetType()).ToArray());
            if (ctor == null)
                throw new MyRefs.Exceptions.ContructorNotFoundException(bType, $"The type {bType.Name} do not have a constructor like {bType.Name}({String.Join(",", ctorArgs.Select(s => s.GetType().Name).ToArray())})");

#pragma warning disable

            object o = null;

            try
            {
                Type pType = new TypeGenerator(before, after, replace).GenerateTypeFrom(bType);

                ctor = MyRefs.Extensions.ReflectionExtension.GetCtorByParamsType(pType, ctorArgs.Select(s => s.GetType()).ToArray());

                o = ctor.Invoke(ctorArgs);
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

            return o;
        }


        public static I CreateObjectLike<I>()
        {
            return (I)CreateObjectLike(typeof(I), new Type[] { }, new object[] { });
        }


        public static I CreateObjectLike<I>(Type[] cTorArgsTypes, params object[] cTorArgs)
        {
            return (I)CreateObjectLike(typeof(I), cTorArgsTypes, cTorArgs);
        }

        public static object CreateObjectLike(Type type, Type[] cTorArgsTypes, params object[] cTorArgs)
        {
            if (type.IsAbstract || type.IsInterface)
                throw new InvalidTypeException(type, $"The type {type.Name} must be a concrete type");

            ConstructorInfo? cTor = type.GetConstructor(cTorArgsTypes);

            if (cTor is null)
            {
                throw new MyRefs.Exceptions.ContructorNotFoundException(type, $"The type {type.Name} do not have any constructor that matchs the list of arguments");
            }

            try
            {
                object o;

                Type pType = new TypeGenerator(null, null, null).GenerateTypeFrom(type)!;

                cTor = pType.GetConstructor(cTorArgsTypes);

                o = cTor.Invoke(cTorArgs);

                return o;
            }
            catch (Exception ex)
            {
                throw new FailCastException(type, type, ex.Message);
            }

        }

        public static I CreateObjectWithProxy<T, I>(BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace, params object[] ctorArgs) where T : class, I
        {
            if (!typeof(I).IsInterface)
                throw new InvalidTypeException(typeof(I));


            ConstructorInfo? ctor = MyRefs.Extensions.ReflectionExtension.GetCtorByParamsType<T>(ctorArgs.Select(s => s.GetType()).ToArray());
            if (ctor == null)
                throw new MyRefs.Exceptions.ContructorNotFoundException(typeof(T), $"The type {typeof(T).Name} do not have a constructor like {typeof(T).Name}({String.Join(",", ctorArgs.Select(s => s.GetType().Name).ToArray())})");

#pragma warning disable
            I o = default(I);

            try
            {
                o = (I)Activator.CreateInstance(new TypeGenerator(before, after, replace).GenerateTypeFrom(typeof(T), typeof(I)), ctorArgs);
            }
            catch (Exception ex)
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

        public static I AddProxy<T, I>(this T @object, BeforeMethodCall before, AfterMethodCall after, ReplaceMethodCall replace, params object[] ctorArgs) where T : class, I
        {
            I o = CreateObjectWithProxy<T, I>(before, after, replace, ctorArgs);

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
