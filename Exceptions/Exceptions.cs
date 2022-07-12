using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProxy.Exceptions
{
    public class InvalidTypeException : Exception
    {
        public Type Type { get; }
        public override string Message { get; }
        public InvalidTypeException(Type type, string msg = "")
        {

            Message = msg;

            if (string.IsNullOrEmpty(msg))
            {
                Message = $"The type {type.Name} must be a interface type";
            }

            Type = type;

        }
    }

    public class FailCastException : Exception
    {
        public Type Type { get; }
        public override string Message { get; }
        public FailCastException(Type type, Type target, string msg = "")
        {

            Message = msg;

            if (string.IsNullOrEmpty(msg))
            {
                Message = $"Can not cast a object of type {type.Name} to {target.Name}";
            }

            Type = type;

        }
    }

    public class MethodNotFoundException : Exception
    {
        public Type Type { get; }
        public override string Message { get; }
        public MethodNotFoundException(Type type, string name, Type[] @params = null, Type @return = null, string msg = "")
        {
            @params = @params ?? new Type[] { };
            @return = @return ?? typeof(void);

            Message = msg;

            if (string.IsNullOrEmpty(msg))
            {

                Message = $"The type {type.Name} do not have any method {(@return == null ? typeof(void).Name : @return.Name)} {name}({string.Join(",", @params.Select(s => s.Name))}) ";
            }

            Type = type;

        }
    }

}
