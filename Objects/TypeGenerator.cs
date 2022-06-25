using System.Reflection;
using System.Reflection.Emit;

namespace MyProxy.Objects
{
    public delegate void BeforeMethodCall(object sender, string name);

    public delegate object AfterMethodCall(object sender, object methodResult);
    public class TypeGenerator
    {
        

        public BeforeMethodCall? BeforeMethodCallHandler;

        public AfterMethodCall? AfterMethodCallHandler;


        private FieldBuilder _fieldBuilder;
        private FieldBuilder _afterfieldBuilder;
        private FieldBuilder _refResult;

#pragma warning disable
        public TypeGenerator(BeforeMethodCall? before, AfterMethodCall? after)
        {
#pragma warning restore

            BeforeMethodCallHandler = before;
            AfterMethodCallHandler = after;
        }

        public Type? GenerateTypeFrom(Type context, Type? toCopy = null)
        {
            bool inject = toCopy == null;

            toCopy = toCopy ?? context;


            Type ? tp = ProxyContainer.Container.GetType(toCopy);

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

            if(inject)
                mthImplementeds.AddRange(GenerateMethodsFrom(tb, context, mthImplementeds));

            foreach (Type @interface in interfaces)
            {
                mthImplementeds.AddRange(GenerateMethodsFrom(tb, context, new List<MethodInfo>(), @interface, true));
            }

            if(!inject)
                GenerateMethodsFrom(tb, context, mthImplementeds);

            tp = tb.CreateType();

            ProxyContainer.Container.AddType(toCopy, tp);

            return tp;
        }

        public void SetProxy(TypeBuilder typeBuilder)
        {
            _fieldBuilder = typeBuilder.DefineField($"_delegate_before_call", typeof(BeforeMethodCall), FieldAttributes.Public);

            _afterfieldBuilder = typeBuilder.DefineField("_delegate_after_call", typeof(AfterMethodCall), FieldAttributes.Public);

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

                if(!@override)
                {
                    mtAttr = ((info.Attributes & MethodAttributes.Public) > 0 ? MethodAttributes.Public : MethodAttributes.Private) | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Final;
                }

                MethodBuilder mb = typeBuilder.DefineMethod(info.Name, mtAttr, CallingConventions.HasThis, info.ReturnType, info.GetParameters().Select(s => s.ParameterType).ToArray());

                ILGenerator il = mb.GetILGenerator();

                int numArgs = info.GetParameters().Count();


                if (BeforeMethodCallHandler != null)
                {
                    il.Emit(OpCodes.Ldarg_0);

                    il.Emit(OpCodes.Ldfld, _fieldBuilder);

                    MethodInfo? beforeCall = typeof(BeforeMethodCall).GetMethod("Invoke");
                    if (beforeCall == null)
                        goto METHOD;

                    var parameters = beforeCall.GetParameters();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, info.Name);

                    il.Emit(OpCodes.Callvirt, beforeCall);
                }

            METHOD:
                {
                    if (numArgs > 0)
                    {
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

                    il.Emit(OpCodes.Call, md);
                }

                if (AfterMethodCallHandler != null)
                {

                    il.Emit(OpCodes.Starg, 1);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarga, 1);
                    il.Emit(OpCodes.Stfld, _refResult);

                    MethodInfo? afterCall = typeof(AfterMethodCall).GetMethod("Invoke");

                    if (afterCall == null)
                        goto RETURN;

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _afterfieldBuilder);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _refResult);
                    il.Emit(OpCodes.Ldobj, info.ReturnType);
                    il.Emit(OpCodes.Callvirt, afterCall);

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

                if(@override)
                    typeBuilder.DefineMethodOverride(mb, info);

            }

            return news;

        }

    }
}
