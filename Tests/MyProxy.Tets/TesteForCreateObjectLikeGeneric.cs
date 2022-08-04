namespace MyProxy.Tets
{

    public class TesteForCreateObjectLikeGeneric
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
        public void CallRefReturnMethodFromCOncreteObject()
        {
            IPerson p = global::MyProxy.DynamicExtensions.CreateObjectLike<Person>(new Type[] { typeof(string) }, new object[] { "Adriano" });

            m_TestObject(p);

            IEnumerable<IPerson> people =  p.GetParents();

            Assert.NotNull(people);
            Assert.NotEmpty(people);
            Assert.True(people.Count() == 2);
            Assert.True(people.First().Name == "Father");
            Assert.True(people.Last().Name == "Mother");

        }

        private void m_TestObject(object p)
        {
            Assert.NotNull(p);
            Assert.IsAssignableFrom<Person>(p);
            Assert.IsAssignableFrom<MyProxy.Objects.Interfaces.IProxyType>(p);
        }
    }
}