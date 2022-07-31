namespace MyProxy.Objects
{
    public class MethodsInvokedCollection : List<MethodInvoked>
    {

        public MethodsInvokedCollection(List<MethodInvoked> list) : this()
        {
            AddRange(list);
        }

        public MethodsInvokedCollection()
        {

        }

    }

}
