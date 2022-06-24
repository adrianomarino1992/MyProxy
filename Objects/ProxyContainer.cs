using System.Reflection;
using System.Reflection.Emit;

namespace MyProxy.Objects
{
    public class ProxyContainer
    {
        private AssemblyBuilder _assemblyBuilder;

        public AssemblyBuilder AssemblyBuilder => _assemblyBuilder;

        private ModuleBuilder _moduleBuilder;

        public ModuleBuilder ModuleBuilder => _moduleBuilder;

        public static List<MethodInfo> Methods = new List<MethodInfo>();
                

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


        private ProxyContainer()
        {
            AssemblyName aName = new AssemblyName("myProxyDymAssembly");
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("myProxyDymModule");
        }
    }

}

