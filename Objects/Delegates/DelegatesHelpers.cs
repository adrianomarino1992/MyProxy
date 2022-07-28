using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MyRefs.Extensions;

namespace MyProxy.Objects.Delegates
{
    internal static class DelegatesHelpers
    {
        internal static MethodInfo? GetCurrentMethod(object sender, string methodName)
        {
            string f = sender.GetFieldValue<string>(FieldsNames.CURRENT_METHOD_RUNNING_NAME);

            if (String.IsNullOrEmpty(f))
                return null;

            try
            {
                string @return = f.Split("<>")[0];

                int i = f.IndexOf("{") + 1;
                int l = f.LastIndexOf("}");              

                string @params = f.Substring(i,l - i);

                foreach(var m in sender.GetType().GetMethods().Where(s => s.Name == methodName))
                {
                    if(m.ReturnType.Name.Trim().ToLower().Equals(@return.ToLower().Trim()))
                    {
                        string args = String.Join(',', m.GetParameters().Select(s => s.ParameterType.Name));
                        if(args.ToLower().Trim().Equals(@params.ToLower().Trim()))
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
