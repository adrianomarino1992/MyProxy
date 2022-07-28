using System.Reflection;
using System.Reflection.Emit;
using MyProxy.Objects.Delegates;
using MyProxy.Objects.Interfaces;
using static MyProxy.Objects.MethodBinder;

namespace MyProxy.Objects
{
    
    public class TypeGenerator
    {

        public BeforeMethodCall? BeforeMethodCallHandler;
        public ReplaceMethodCall? ReplaceMethodCallHandler;
        public AfterMethodCall? AfterMethodCallHandler;


        private FieldBuilder _beforeMethodCall;
        private FieldBuilder _replaceMethodCall;
        private FieldBuilder _afterMethodCall;        
        private FieldBuilder _refBinder;
        private FieldBuilder _refCurrMethod;

#pragma warning disable
        public TypeGenerator(BeforeMethodCall? before, AfterMethodCall? after, ReplaceMethodCall? replace = null)
        {
#pragma warning restore

            BeforeMethodCallHandler = before;
            AfterMethodCallHandler = after;
            ReplaceMethodCallHandler = replace;
        }

        public Type? GenerateTypeFrom(Type context, Type? toCopy = null)
        {
            bool inject = toCopy == null;

            toCopy = toCopy ?? context;


            Type? tp = ProxyContainer.Container.GetType(toCopy);

            if (tp != null)
            {
                return tp;
            }

            Type[] interfaces = toCopy.GetInterfaces().Concat(new Type[] { typeof(IProxyType) }).ToArray();

            if (toCopy.IsInterface)
                interfaces = interfaces.Concat(new Type[] { toCopy }).ToArray();

            TypeBuilder tb = ProxyContainer.Container.ModuleBuilder.DefineType($"RunTimeCast_{context.Name}_{toCopy.Name}",
                TypeAttributes.Class | TypeAttributes.Public, context, interfaces);

            SetProxy(tb);

            List<MethodInfo> mthImplementeds = new List<MethodInfo>();

            GenerateFields(tb, context);

            mthImplementeds.AddRange(GenerateProperties(tb, context));

            foreach (Type @interface in interfaces)
            {
                mthImplementeds.AddRange(GenerateMethodsFrom(tb, context, mthImplementeds, @interface, true));
            }   
            
            GenerateMethodsFrom(tb, context, mthImplementeds);

            GenerateConstructors(tb, context);

            tp = tb.CreateType();

            ProxyContainer.Container.AddType(toCopy, tp);

            return tp;
        }

        public void SetProxy(TypeBuilder typeBuilder)
        {
            _beforeMethodCall = typeBuilder.DefineField(FieldsNames.BEFORE_CALL_METHOD_FIELD_NAME, typeof(BeforeMethodCall), FieldAttributes.Private);

            _replaceMethodCall = typeBuilder.DefineField(FieldsNames.REPLACE_CALL_METHOD_FIELD_NAME, typeof(ReplaceMethodCall), FieldAttributes.Private);

            _afterMethodCall = typeBuilder.DefineField(FieldsNames.AFTER_CALL_METHOD_FIELD_NAME, typeof(AfterMethodCall), FieldAttributes.Private);

            _refCurrMethod = typeBuilder.DefineField(FieldsNames.CURRENT_METHOD_RUNNING_NAME, typeof(string), FieldAttributes.Private);

            _refBinder = typeBuilder.DefineField(FieldsNames.METHODBINDERS_FIELD_NAME, typeof(List<MethodBinder>), FieldAttributes.Private);
        }

        public void GenerateConstructors(TypeBuilder typeBuilder, Type context)
        {            
            foreach(ConstructorInfo c in context.GetConstructors())
            {
                ConstructorBuilder ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, c.GetParameters().Select(s => s.ParameterType).ToArray());

                ILGenerator il = ctor.GetILGenerator();

                for(int i = 1; i <= c.GetParameters().Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i);
                }

                il.Emit(OpCodes.Newobj, c);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ret);

            }

        }

        public List<MethodInfo> GenerateProperties(TypeBuilder typeBuilder, Type context, bool @override = false)
        {
            List<MethodInfo> getSet = new List<MethodInfo>();

            foreach (PropertyInfo p in context.GetProperties())
            {
                PropertyBuilder prop = typeBuilder.DefineProperty(p.Name, p.Attributes, p.PropertyType, null);

                MethodAttributes mtAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;

                if (!@override)
                {
                    mtAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Final;
                }
                                
                mtAttr |= MethodAttributes.SpecialName;

                MethodBuilder get = typeBuilder.DefineMethod($"get_{p.Name}", mtAttr, CallingConventions.HasThis, p.PropertyType, null);

                ILGenerator il = get.GetILGenerator();

                MethodInfo? getter = context.GetMethod($"get_{p.Name}");

                il.Emit(OpCodes.Ldarg_0);

                if(!@override)
                    il.Emit(OpCodes.Call, getter!);
                else
                    il.Emit(OpCodes.Callvirt, getter!);

                il.Emit(OpCodes.Ret);

                prop.SetGetMethod(get);

                MethodBuilder set = typeBuilder.DefineMethod($"get_{p.Name}", mtAttr, CallingConventions.HasThis, p.PropertyType, null);

                il = set.GetILGenerator();

                MethodInfo? setter = context.GetMethod($"set_{p.Name}");

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);

                if (!@override)
                    il.Emit(OpCodes.Call, setter!);
                else
                    il.Emit(OpCodes.Callvirt, setter!);

                il.Emit(OpCodes.Ret);

                prop.SetSetMethod(set);

                getSet.Add(getter);
                getSet.Add(setter);

                
            }

            return getSet;
        }

        public void GenerateFields(TypeBuilder typeBuilder, Type context)
        {
            foreach (FieldInfo f in context.GetFields())
            {
                FieldBuilder field = typeBuilder.DefineField(f.Name, f.FieldType, f.Attributes);
            }

        }

        public List<MethodInfo> GenerateMethodsFrom(TypeBuilder typeBuilder, Type context, List<MethodInfo> mthIgnore, Type? toCopy = null, bool @override = false)
        {
            toCopy = toCopy ?? context;

            List<MethodInfo> @news = new List<MethodInfo>();

            foreach (MethodInfo info in toCopy.GetMethods())
            {
                if (mthIgnore.Exists(a =>
                {
                    bool sameName = a.Name == info.Name;
                    List<Type> aTypes = a.GetParameters().Select(s => s.ParameterType).ToList();
                    List<Type> infoTypes = info.GetParameters().Select(s => s.ParameterType).ToList();

                    bool sameTypes = aTypes.All(s => infoTypes.Contains(s)) && aTypes.Count == infoTypes.Count;

                    return sameName && sameTypes;

                }))
                {
                    continue;
                }

                news.Add(info);

                MethodAttributes mtAttr = ((info.Attributes & MethodAttributes.Public) > 0 ? MethodAttributes.Public : MethodAttributes.Private) | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;

                if (!@override)
                {
                    mtAttr = ((info.Attributes & MethodAttributes.Public) > 0 ? MethodAttributes.Public : MethodAttributes.Private) | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Final;
                }

             

                MethodBuilder mb = typeBuilder.DefineMethod(info.Name, mtAttr, CallingConventions.HasThis, info.ReturnType, info.GetParameters().Select(s => s.ParameterType).ToArray());
                               

                ILGenerator il = mb.GetILGenerator();

                Label exitCode = il.DefineLabel();
                Label doLabel = il.DefineLabel();
                Label beforeLabel = il.DefineLabel();
                Label afterLabel = il.DefineLabel();
                Label codeLabel = il.DefineLabel();

                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, $"{info.ReturnType.Name}<>{info.Name}{{{String.Join(',', info.GetParameters().Select(s => s.ParameterType.Name))}}}");
                il.Emit(OpCodes.Stfld, _refCurrMethod);

                int numArgs = info.GetParameters().Count();


                il.MarkLabel(beforeLabel);

                if (BeforeMethodCallHandler != null)
                {
                    LocalBuilder arr = m_CreateArrayOfArgs(il, numArgs, info);

                    LocalBuilder bfArg = il.DeclareLocal(typeof(BeforeMethodCallArgs));

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, info.Name);
                    il.Emit(OpCodes.Ldloc, arr);
                    il.Emit(OpCodes.Newobj, typeof(BeforeMethodCallArgs).GetConstructor(new Type[] { typeof(object), typeof(string), typeof(object[]) })!);

                    il.Emit(OpCodes.Stloc, bfArg);

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _beforeMethodCall);


                    MethodInfo? beforeCall = typeof(BeforeMethodCall).GetMethod("Invoke");
                    if (beforeCall == null)
                        goto METHOD;


                    il.Emit(OpCodes.Ldloc, bfArg);

                    il.Emit(OpCodes.Callvirt, beforeCall);
                }

            METHOD:
                {
                    MethodInfo chDo = typeof(MethodBinderManager).GetMethod(nameof(MethodBinderManager.HasDoProxy))!;

                    il.Emit(OpCodes.Ldstr, info.Name);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _refBinder);

                    il.Emit(OpCodes.Call, chDo);
                    
                    il.Emit(OpCodes.Brtrue, doLabel);

                    il.MarkLabel(codeLabel);

                    if (ReplaceMethodCallHandler != null)
                    {
                        LocalBuilder arr = m_CreateArrayOfArgs(il, numArgs, info);


                        LocalBuilder bfArg = il.DeclareLocal(typeof(ReplaceMethodCall));

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldstr, info.Name);
                        il.Emit(OpCodes.Ldloc, arr);
                        il.Emit(OpCodes.Newobj, typeof(ReplaceMethodCallArgs).GetConstructor(new Type[] { typeof(object), typeof(string), typeof(object[]) })!);

                        il.Emit(OpCodes.Stloc, bfArg);

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, _replaceMethodCall);


                        MethodInfo? replaceCall = typeof(ReplaceMethodCall).GetMethod("Invoke");
                        if (replaceCall == null)
                            goto METHOD;


                        il.Emit(OpCodes.Ldloc, bfArg);

                        il.Emit(OpCodes.Callvirt, replaceCall);

                        if (info.ReturnType == typeof(void))
                            il.Emit(OpCodes.Pop);
                    }
                    else
                    {
                        bool ld_0 = true;

                        if (numArgs > 0)
                        {
                            ld_0 = false;

                            for (int i = 0; i <= numArgs; i++)
                            {
                                il.Emit(OpCodes.Ldarg_S, i);
                            }
                        }

                        MethodInfo? md = context.GetMethod(info.Name, info.GetParameters().Select(s => s.ParameterType).ToArray());

                        if (md == null || info.ReturnType != md.ReturnType)
                        {
                            throw new Exceptions.MethodNotFoundException(context, info.Name, info.GetParameters().Select(s => s.ParameterType).ToArray(), info.ReturnType);

                        }

                        if (ld_0)
                            il.Emit(OpCodes.Ldarg_0);

                        if(@override)
                            il.Emit(OpCodes.Callvirt, md);
                        else
                            il.Emit(OpCodes.Call, md);
                    }

                    il.Emit(OpCodes.Br, afterLabel);

                    il.MarkLabel(doLabel);

                    if (info.ReturnType != typeof(void))
                        il.Emit(OpCodes.Ldnull);

                    MethodInfo doM = typeof(MethodBinderManager).GetMethod(nameof(MethodBinderManager.ExecuteDOMethod))!;

                    m_CallExtern(il, numArgs, info, doM, info);
                    
                }


                il.MarkLabel(afterLabel);

                MethodInfo checkB = typeof(MethodBinderManager).GetMethod(nameof(MethodBinderManager.CheckBinder))!;

                m_CallExtern(il, numArgs, info, checkB, info);

                if (AfterMethodCallHandler != null)
                {

                    LocalBuilder reArg = il.DeclareLocal(typeof(object));

                    if (info.ReturnType == typeof(void))
                    {
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stloc, reArg);
                    }
                    else
                        il.Emit(OpCodes.Stloc, reArg);

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, info.Name);
                    il.Emit(OpCodes.Ldloc, reArg);

                    LocalBuilder afArg = il.DeclareLocal(typeof(AfterMethodCallArgs));

                    il.Emit(OpCodes.Newobj, typeof(AfterMethodCallArgs).GetConstructor(new Type[] { typeof(object), typeof(string), typeof(object) })!);

                    il.Emit(OpCodes.Stloc, afArg);


                    MethodInfo? afterCall = typeof(AfterMethodCall).GetMethod("Invoke");

                    if (afterCall == null)
                        goto RETURN;

                    il.Emit(OpCodes.Ldarg_0);

                    il.Emit(OpCodes.Ldfld, _afterMethodCall);

                    il.Emit(OpCodes.Ldloc, afArg);

                    il.Emit(OpCodes.Callvirt, afterCall);

                    if (info.ReturnType == typeof(void))
                        il.Emit(OpCodes.Pop);

                }

            RETURN:
                {
                    il.MarkLabel(exitCode);
                    il.Emit(OpCodes.Ret);
                }

                int p = 0;

                foreach (var item in info.GetParameters())
                {
                    ParameterBuilder pb = mb.DefineParameter
                        (
                            p + 1,
                            item.Attributes,
                            item.Name
                        );
                }

                if (@override)
                    typeBuilder.DefineMethodOverride(mb, info);

            }

            return news;

        }

        private LocalBuilder m_CreateArrayOfArgs(ILGenerator il, int numArgs, MethodInfo info)
        {
            LocalBuilder arr = il.DeclareLocal(typeof(object[]));
            il.Emit(OpCodes.Ldc_I4, numArgs);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, arr);


            if (numArgs > 0)
            {
                for (int i = 0; i < numArgs; i++)
                {
                    il.Emit(OpCodes.Ldloc, arr);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    if (info.GetParameters()[i].ParameterType.IsValueType)
                        il.Emit(OpCodes.Box, info.GetParameters()[i].ParameterType);
                    il.Emit(OpCodes.Stelem, typeof(object));

                }

            }

            return arr;
        }

        private void m_CallExtern(ILGenerator il, int numArgs, MethodInfo info, MethodInfo checkB, MethodInfo methodInfo)
        {           
            ConstructorInfo ctor = typeof(MethodBinderManager).GetConstructor(new Type[] { typeof(object), typeof(string), typeof(object[]), typeof(object) })!;

            LocalBuilder currR = il.DeclareLocal(typeof(object));

            Type cast = info.ReturnType;

            if (info.ReturnType != typeof(void))
            {
                if (cast != null && !cast.Equals(typeof(void)) && cast.IsValueType)
                    il.Emit(OpCodes.Box, cast);

                il.Emit(OpCodes.Stloc, currR);
            }

            LocalBuilder array = m_CreateArrayOfArgs(il, numArgs, info);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, info.Name);
            il.Emit(OpCodes.Ldloc, array);

            if (info.ReturnType != typeof(void))
                il.Emit(OpCodes.Ldloc, currR);
            else
                il.Emit(OpCodes.Ldnull);

            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _refBinder);
            il.Emit(OpCodes.Call, checkB);

            if (info.ReturnType == typeof(void))
                il.Emit(OpCodes.Pop);
                
            if (!cast!.Equals(typeof(void)) && cast!.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, info.ReturnType);
            }
        }

    }

    internal static class FieldsNames
    {
        internal const string BEFORE_CALL_METHOD_FIELD_NAME = "_delegateBeforeCall";
        internal const string REPLACE_CALL_METHOD_FIELD_NAME = "_delegateReplaceCall";
        internal const string AFTER_CALL_METHOD_FIELD_NAME = "_delegateAfterCall";
        internal const string METHODBINDERS_FIELD_NAME = "_refBinder";
        internal const string CURRENT_METHOD_RUNNING_NAME = "_refCurrMethod";
    }
}
