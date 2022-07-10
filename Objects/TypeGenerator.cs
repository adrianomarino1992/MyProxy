﻿using System.Reflection;
using System.Reflection.Emit;
using MyProxy.Objects.Delegates;

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
        private FieldBuilder _refResult;

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


            Type[] interfaces = toCopy.GetInterfaces();

            if (toCopy.IsInterface)
                interfaces = interfaces.Concat(new Type[] { toCopy }).ToArray();

            TypeBuilder tb = ProxyContainer.Container.ModuleBuilder.DefineType($"RunTimeCast_{context.Name}_{toCopy.Name}",
                TypeAttributes.Class | TypeAttributes.Public, context, interfaces);

            SetProxy(tb);

            List<MethodInfo> mthImplementeds = new List<MethodInfo>();

            if (inject)
                mthImplementeds.AddRange(GenerateMethodsFrom(tb, context, mthImplementeds));

            foreach (Type @interface in interfaces)
            {
                mthImplementeds.AddRange(GenerateMethodsFrom(tb, context, new List<MethodInfo>(), @interface, true));
            }

            if (!inject)
                GenerateMethodsFrom(tb, context, mthImplementeds);

            tp = tb.CreateType();

            ProxyContainer.Container.AddType(toCopy, tp);

            return tp;
        }

        public void SetProxy(TypeBuilder typeBuilder)
        {
            _beforeMethodCall = typeBuilder.DefineField($"_delegate_before_call", typeof(BeforeMethodCall), FieldAttributes.Public);

            _replaceMethodCall = typeBuilder.DefineField($"_delegate_replace_call", typeof(ReplaceMethodCall), FieldAttributes.Public);

            _afterMethodCall = typeBuilder.DefineField("_delegate_after_call", typeof(AfterMethodCall), FieldAttributes.Public);

            _refResult = typeBuilder.DefineField("_refResult", typeof(object), FieldAttributes.Public);
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

                int numArgs = info.GetParameters().Count();


                if (BeforeMethodCallHandler != null)
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

                    if (ReplaceMethodCallHandler != null)
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

                        il.Emit(OpCodes.Call, md);
                    }
                }

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

    }
}