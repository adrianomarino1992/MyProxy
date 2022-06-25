using MyRefs.Exceptions;
using MyRefs.Extensions;
using System.Dynamic;

namespace MyProxy.Objects
{
    public  class DymObject : DynamicObject
    {
        private List<object> _aggregateList;

        public Dictionary<string, object?> Properties;

        public DymObject(params object[] objects)
        {
            _aggregateList = new List<object>();

            Properties = new Dictionary<string, object?>();

            foreach (object o in objects)
            {
                if (!_aggregateList.Any(s => s.GetType() == o.GetType()))
                    _aggregateList.Add(o);
            }
            
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            foreach (var item in _aggregateList)
            {
                try
                {
                    result = item.GetPropertyValue(binder.Name);
                    return true;

                }
                catch (PropertyNotFoundException)
                {
                    continue;
                }
            }

            foreach (var item in _aggregateList)
            {
                try
                {
                    result = item.GetFieldValue(binder.Name);
                    return true;

                }
                catch (FieldNotFoundException)
                {
                    continue;
                }
            }


            if (Properties.ContainsKey(binder.Name))
                result = Properties[binder.Name];
            else
                result = null;


            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            foreach (var item in _aggregateList)
            {
                try
                {
                    item.SetPropertyValue(binder.Name, value);
                    return true;

                }
                catch (PropertyNotFoundException)
                {
                    continue;
                }
            }

            foreach (var item in _aggregateList)
            {
                try
                {
                    item.SetFieldValue(binder.Name, value);
                    return true;

                }
                catch (FieldNotFoundException)
                {
                    continue;
                }
            }

            if (Properties.ContainsKey(binder.Name))
                Properties[binder.Name] = value;
            else
                Properties.Add(binder.Name, value);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            foreach (var item in _aggregateList)
            {
                try
                {
                    result = item.CallMethodByReflection(binder.Name, args);
                    return true;

                }
                catch (MissingMethodException)
                {
                    continue;
                }
            }

            result = null;
            return false;
        }
    }
}
