# MyProxy

MyProxy allow us to add proxy or turn our objects in event listeners objects. 


## Installation

.NET CLI

```bash
dotnet add package Adr.MyProxy  --version 2.1.0
```

Nuget package manager

```bash
PM> Install-Package Adr.MyProxy -Version 2.1.0
```

packageReference

```bash
<PackageReference Include="Adr.MyProxy " Version="2.1.0" />
```

## Usage

**With this sample classes:**
```csharp
        public interface IPerson 
        {
            string Name { get; set; }
            void Talk(string msg);
            void Run();
            int AskAge();
            IEnumerable<IPerson> GetParents();
        }

        public class Person : IPerson
        {
            public string Name { get; set; }
            public int AskAge() => 23;
            public IEnumerable<IPerson> GetParents() => new List<IPerson> { new Person(), new Person() };

            public void Run() => Console.WriteLine($"{Name}, run !");
            public void Talk(string msg) => Console.WriteLine($"{Name} said: \"{msg}\"");
           
        }

```


**We can add event listeners**
```csharp
IPerson p = MyProxy.DynamicExtensions.CreateListener<Person>(); // create a instance with a Person with listeners and cast to interface type to use implemented listeners methods

            p.SetPropertyValue(nameof(IPerson.Name), "Adriano"); // set prop with reflection 

            // add listener
            p.When(nameof(p.AskAge), args =>
            {
                Console.WriteLine("Add 1 to age: 23 => 24");

                return ((int)args.Result!) + 1;
            });

            int age = p.AskAge(); // returns 24

            // add listener
            p.When(nameof(p.GetParents), args =>
            {
                Console.WriteLine("Add name to parents");

                List<IPerson>? parents = args.Result as List<IPerson>;

                if (parents != null && parents.Count == 2)
                {
                    parents[0].SetPropertyValue(nameof(IPerson.Name), "Father");
                    parents[1].SetPropertyValue(nameof(IPerson.Name), "Mother");
                }
                else return null!;

                return parents!;

            });

            var parets = p.GetParents(); // returns Person.Name = Father, Person.Name = Mother

            // add listener
            p.When(nameof(p.AskAge), args =>
            {
                Console.WriteLine($"{args.Sender!.GetPropertyValue(nameof(p.Name))} run from proxy!");

                return null!;
            });

            p.Run(); // write on console "Adriano run from proxy!"

```

**We can add before and after delegates to all methods in a object:**
```csharp
           IPerson p = new Person()
                .AddProxy<Person, IPerson>(
                new BeforeMethodCall(args => Console.WriteLine($"Berofe the method {args.Method!.Name}")), 
                new AfterMethodCall(args => {

                    Console.WriteLine($"After the method, we also can change the result");
                    return args.Result;
                }), 
                new ReplaceMethodCall(args => {

                    Console.WriteLine($"We can replace the method {args.Method!.Name} too");
                    return args.Method!.ReturnType.Equals(typeof(void)) ? null! : Activator.CreateInstance(args.Method.ReturnType)!;
                }), 
                new object[]{});;

            p.Name = "Adriano";

            p.Run();

            Console.Read();

```



## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)
