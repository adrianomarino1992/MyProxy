using System.Reflection;
using System.Reflection.Emit;

namespace MyProxy.Objects
{
    public class ProxyContainer
    {

        private AssemblyBuilder _assemblyBuilder;

        public bool UseTypeCache { get; set; }
        public AssemblyBuilder AssemblyBuilder => _assemblyBuilder;

        private ModuleBuilder _moduleBuilder;

        private Dictionary<Type, Type> _runtimeTypes;        

        public Type? GetType(Type target)
        {
            if (!UseTypeCache)
                return null;

            return _runtimeTypes.TryGetValue(target, out Type? type) ? type : null;
        }
              

        internal void AddType(Type target, Type @new)
        {
            if (!UseTypeCache)
                return;

            _runtimeTypes.Add(target, @new);
        }

        private static ProxyContainer? _container;
        public static ProxyContainer Container
        {
            get
            {
                if (_container == null)
                    _container = new ProxyContainer();

                return _container;
            }
        }
        public ModuleBuilder ModuleBuilder => _moduleBuilder;

        public static List<MethodInfo> Methods = new List<MethodInfo>();

        private ProxyContainer()
        {
            AssemblyName aName = new AssemblyName("myProxyDymAssembly");
            UseTypeCache = true;
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("myProxyDymModule");
            _runtimeTypes = new Dictionary<Type, Type>();
        }
    }

}

