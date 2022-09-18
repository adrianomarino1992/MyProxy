namespace MyProxy.Tests
{

    public class TestForCreateObjectLikeGeneric
    {
        [Fact]
        public void CreateACtorOfObjectLikeAConcreteObjectWithArgs()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            Assert.Equal("Adriano", p.Name);

        }


        [Fact]
        public void CreateACtorOfObjectLikeAConcreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>();
            m_TestObject(p);
        }

        [Fact]
        public void SetPropertyAttributedInConstructor()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            p.Name = "Changed";

            Assert.Equal("Changed", p.Name);


        }


        [Fact]
        public void CallVoidMethodFromCOncreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            p.Run();

        }

        [Fact]
        public void CallValueReturnMethodFromCOncreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            int age = p.AskAge();

            Assert.Equal(23, age);

        }


        [Fact]
        public void WhenDoWithValueReturnMethodFromCOncreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            p.When(nameof(p.AskAge)).Do(args => 25);

            int age = p.AskAge();

            Assert.Equal(25, age);

        }

        [Fact]
        public void WhenDoWithlRefReturnMethodFromCOncreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            p.When(nameof(p.GetParents)).Do(args =>
            {
                IEnumerable<IPerson> list = new List<IPerson>()
                {
                    new Person("Person1"),
                    new Person("Person2")
                };

                return list;

            });

            IEnumerable<IPerson> people = p.GetParents();

            Assert.NotNull(people);
            Assert.NotEmpty(people);
            Assert.True(people.Count() == 2);
            Assert.True(people.First().Name == "Person1");
            Assert.True(people.Last().Name == "Person2");

        }


        [Fact]
        public void CallRefReturnMethodFromCOncreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            IEnumerable<IPerson> people = p.GetParents();

            Assert.NotNull(people);
            Assert.NotEmpty(people);
            Assert.True(people.Count() == 2);
            Assert.True(people.First().Name == "Father");
            Assert.True(people.Last().Name == "Mother");

        }

        [Fact]
        public void CallGenericMethodFromConcreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            Person control = new Person
            {
                Name = "control person"
            };

            Person result = p.GenericMethod<Person>(control);

            Assert.Equal(control.Name, result.Name);

        }

        [Fact]
        public void CallGenericMethodFromConcreteObjectWithinWhenDOEvents()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            Person control = new Person
            {
                Name = "control person"
            };

            object boxInt = 0;

            p.WhenCallMethodWithThisParamtersType(nameof(p.GenericMethod), new Type[] { typeof(Person) }).Do(args => {

                boxInt = ((int)boxInt) + 1;

                return control;                

            });

            Person result = p.GenericMethod<Person>(control);

            string resultStr = p.GenericMethod<string>("Hello");

            Assert.Equal(control.Name, result.Name);
            Assert.Equal(1, (int)boxInt);
            Assert.Equal("Hello", resultStr);

        }

        private void m_TestObject(object p)
        {
            Assert.NotNull(p);
            Assert.IsAssignableFrom<Person>(p);
            Assert.IsAssignableFrom<MyProxy.Objects.Interfaces.IProxyType>(p);
        }
    }
}