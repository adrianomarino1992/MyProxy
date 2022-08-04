namespace MyProxy.Tets.Classes
{
    public interface IPerson
    {
        string Name { get; set; }
        void Talk(string msg);
        void Run();
        int AskAge();
        IEnumerable<IPerson> GetParents();
    }
}
