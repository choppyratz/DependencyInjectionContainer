using Microsoft.VisualStudio.TestTools.UnitTesting;
using DependectInjectionContainer;
using System.Collections.Generic;

namespace DependencyInjectionContainerUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private DependencyProvider provider;
        [TestInitialize]
        public void SetUp()
        {
            var dependencies = new DependenciesConfiguration();
            dependencies.Registr<ITest1, Test1>(isSingleton: false);
            dependencies.Registr<ITest2<IRepository>, Test2<IRepository>>(isSingleton: false);
            dependencies.Registr<ITest3, Test3>(isSingleton: true);
            dependencies.Registr<ITest4, Test4>(isSingleton: false);
            dependencies.Registr(typeof(ITest5<>), typeof(Test5<>), isSingleton: false);
            dependencies.Registr<ITest6, Test61>(isSingleton: false);
            dependencies.Registr<ITest6, Test62>(isSingleton: false);
            dependencies.Registr<ITest7, Test7>(isSingleton: false);
            dependencies.Registr<ITest8, Test81>(isSingleton: false);
            dependencies.Registr<ITest8, Test82>(isSingleton: false);
            provider = new DependencyProvider(dependencies);
        }
        [TestMethod]
        public void TestSimpleDependencyResolve()
        {
            var service1 = provider.Resolve<ITest1>();
            Assert.IsNotNull(service1);
        }

        [TestMethod]
        public void TestGenericDependencyResolve()
        {
            var service1 = provider.Resolve<ITest2<IRepository>>();
            Assert.IsNotNull(service1);
        }

        [TestMethod]
        public void TestSingletoneLifeTime()
        {
            var service1 = provider.Resolve<ITest3>();
            var service12 = provider.Resolve<ITest3>();
            Assert.AreEqual(1, Test3.BornCount);
        }

        [TestMethod]
        public void TestDependencyPerTimeLifeTime()
        {
            var service1 = provider.Resolve<ITest4>();
            var service12 = provider.Resolve<ITest4>();
            Assert.AreEqual(2, Test4.BornCount);
        }

        [TestMethod]
        public void TestOpenGenericResolve()
        {
            var service1 = provider.Resolve<ITest5<MySqlRepository>>();
            Assert.IsNotNull(service1);
            Assert.AreEqual(service1.GetType().GetGenericArguments()[0].FullName, typeof(MySqlRepository).FullName);
        }

        [TestMethod]
        public void TestEnumerableReturnForDependencies()
        {
            var service1 = provider.Resolve<IEnumerable<ITest6>>();
            Assert.IsNotNull(service1);
            Assert.AreEqual(service1.GetType().GetInterface(typeof(IEnumerable<ITest6>).Name).FullName, typeof(IEnumerable<ITest6>).FullName);
        }

        [TestMethod]
        public void TestCounstructorPassingParams()
        {
            var service1 = provider.Resolve<ITest7>();
            IEnumerable<ITest6> p = (service1 as Test7).param;
            Assert.IsNotNull(p);
            Assert.AreEqual(p.GetType().GetInterface(typeof(IEnumerable<ITest6>).Name).FullName, typeof(IEnumerable<ITest6>).FullName);
        }

        [TestMethod]
        public void TestCounstructorPassingParamsRecursion()
        {
        }
    }

    interface ITest1 { }
    class Test1 : ITest1
    {
        
    }

    interface IRepository { }
    interface ITest2<TRepository>
    {
    }
    class Test2<TRepository> : ITest2<TRepository>
    {

    }

    interface ITest3 { }
    class Test3 : ITest3
    {
        public static int BornCount = 0;
        public Test3()
        {
            BornCount++;
        }
    }

    interface ITest4 { }
    class Test4 : ITest4
    {
        public static int BornCount = 0;
        public Test4()
        {
            BornCount++;
        }
    }

    interface ITest5<TRepository>
    {
    }

    class Test5<TRepository> : ITest5<TRepository>
    {

    }
    class MySqlRepository
    {

    }

    interface ITest6 { }
    class Test61 : ITest6
    {

    }

    class Test62 : ITest6
    {

    }

    interface ITest7 { }
    class Test7 : ITest7
    {
        public IEnumerable<ITest6> param;
        public Test7(IEnumerable<ITest6> repository)
        {
            param = repository;
        }
    }

    interface ITest8 { }

    class Test81 : ITest7
    {
        public Test81(ITest8 repository)
        {
            
        }
    }

    class Test82 : ITest7
    {
        public Test82(ITest8 repository)
        {

        }
    }
}
