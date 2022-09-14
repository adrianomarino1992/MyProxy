
namespace MyProxy.Tests
{
    public class TestForDynamicExtensions
    {

        public TestForDynamicExtensions()
        {
            MyProxy.Objects.ProxyContainer.Container.UseTypeCache = false;
        }

        [Fact]
        public void TestForAddProxyMethod()
        {
            object before = false;
            object after = false;
            object replace = false;

            IPerson p = new Person("Adriano").AddProxy<Person, IPerson>(
                new Objects.Delegates.BeforeMethodCall(arg =>
                {
                    before = true;

                }),
                new Objects.Delegates.AfterMethodCall(arg =>
                {
                    after = true;

                    return arg.Result;

                }),
                new Objects.Delegates.ReplaceMethodCall(arg =>
                {
                    replace = true;

                    return 10;

                }));

            _ = p.AskAge();

            Assert.True((bool)before);
            Assert.True((bool)after);
            Assert.True((bool)replace);


        }


        [Fact]
        public void TestForAddProxyMethodWithVoidResult()
        {


            IPerson p = new Person("Adriano").AddProxy<Person, IPerson>(
                null,
                new Objects.Delegates.AfterMethodCall(arg =>
                {

                    return arg.Result;

                }),
                new Objects.Delegates.ReplaceMethodCall(arg =>
                {
                    return null;

                }));

            p.Run();

        }


        [Fact]
        public void TestForAddProxyMethodWithValueTypeResult()
        {

            IPerson p = new Person("Adriano").AddProxy<Person, IPerson>(
                null,
                new Objects.Delegates.AfterMethodCall(arg =>
                {
                    return arg.Result;

                }),
                new Objects.Delegates.ReplaceMethodCall(arg =>
                {

                    return 10;

                }));

            int r = p.AskAge();

            Assert.Equal(10, r);


        }


        [Fact]
        public void TestForAddProxyMethodWithRefTypeResult()
        {

            IPerson p = new Person("Adriano").AddProxy<Person, IPerson>(
                null,
                new Objects.Delegates.AfterMethodCall(arg =>
                {
                    return arg.Result;

                }),
                new Objects.Delegates.ReplaceMethodCall(arg =>
                {
                    IEnumerable<IPerson> list = new List<IPerson>()
                {
                    new Person("Person1"),
                    new Person("Person2")
                };

                    return list;

                }));

            IEnumerable<IPerson> people = p.GetParents();

            Assert.NotNull(people);
            Assert.NotEmpty(people);
            Assert.True(people.Count() == 2);
            Assert.True(people.First().Name == "Person1");
            Assert.True(people.Last().Name == "Person2");


        }
    }
}
