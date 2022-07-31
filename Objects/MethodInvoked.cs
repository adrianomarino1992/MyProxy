using MyRefs.Extensions;
using System.Reflection;

namespace MyProxy.Objects
{
    public class MethodInvoked
    {
        public string Signature { get; }
        public object[] Args { get; }
        public Type[] Parameters { get; }
        public object Sender { get; }
        public object Result { get; }

        public MethodInfo Method { get; }
       
        public MethodInvoked(string signature, object[] args, object sender, object result)
        {
            Signature = signature;
            Args = args;
            Method = Delegates.DelegatesHelpers.GetCurrentMethod(sender)!;
            Parameters = Method.GetParameters().Select(s => s.ParameterType).ToArray();
            Sender = sender;
            Result = result;
        }

        public static void AddInvocationEvent(MethodInvoked info, object sender)
        {
            List<MethodInvoked> calls = (List<MethodInvoked>)sender.GetFieldValue(FieldsNames.METHODS_INVOKED_LIST);
            bool needSet = calls == null;

            if(needSet)
            {
                calls = new List<MethodInvoked>();
            }

            calls!.Add(info);

            if (needSet)
                sender.SetFieldValue(FieldsNames.METHODS_INVOKED_LIST, calls);
        }

        public static string GenerateSignature(MethodInfo info)
        {
            return $"{info.ReturnType.Name}<>{info.Name}<>{{{String.Join(',', info.GetParameters().Select(s => s.ParameterType.Name))}}}";
        }

        public static MethodInfo? GetCurrentMethod(object sender)
        {
            string f = sender.GetFieldValue<string>(FieldsNames.CURRENT_METHOD_RUNNING_NAME);

            if (String.IsNullOrEmpty(f))
                return null;

            try
            {
                string @return = f.Split("<>")[0];
                string @methodName = f.Split("<>")[1].Trim();
                int i = f.IndexOf("{") + 1;
                int l = f.LastIndexOf("}");

                string @params = f.Substring(i, l - i);

                foreach (var m in sender.GetType().GetMethods().Where(s => s.Name == @methodName))
                {
                    if (m.ReturnType.Name.Trim().ToLower().Equals(@return.ToLower().Trim()))
                    {
                        string args = String.Join(',', m.GetParameters().Select(s => s.ParameterType.Name));
                        if (args.ToLower().Trim().Equals(@params.ToLower().Trim()))
                        {
                            return m;
                        }
                    }
                }

            }
            catch { }

            return null;

        }
    }


}
