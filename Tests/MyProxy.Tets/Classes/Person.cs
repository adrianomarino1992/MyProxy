namespace MyProxy.Tets.Classes
{
    public class Person : IPerson
    {
        public Person(string name)
        {
            Name = name;
        }

        public Person() { }

        public string Name { get; set; }
        public int AskAge() => 23;
        public IEnumerable<IPerson> GetParents() => new List<IPerson> { new Person("Father"), new Person("Mother") };
        public void Run() => Console.WriteLine($"{Name}, run !");
        public void Talk(string msg) => Console.WriteLine($"{Name} said: \"{msg}\"");

    }
}
