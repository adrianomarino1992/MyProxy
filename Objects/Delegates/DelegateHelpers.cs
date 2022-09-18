namespace MyProxy.Objects.Delegates
{
    public static class DelegateHelpers
    {

        public static bool IsNull(Delegate @delegate)
        {
            return @delegate == null;
        }
        
    }
}