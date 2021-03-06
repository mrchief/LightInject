﻿namespace LightInject.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using LightInject;
    using LightInject.SampleLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConstructorInjectionTests
    {
        [TestMethod]
        public void GetInstance_KnownDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(Bar));
            container.Register(typeof(IFoo), typeof(FooWithDependency));
            var instance = (FooWithDependency)container.GetInstance<IFoo>();
            Assert.IsInstanceOfType(instance.Bar, typeof(Bar));
        }

        [TestMethod]
        public void GetInstance_UnKnownDependency_ThrowsException()
        {
            var container = CreateContainer();
            container.Register<IFoo, FooWithDependency>();
            ExceptionAssert.Throws<InvalidOperationException>(
                () => container.GetInstance<IFoo>(), e=> e.InnerException.Message == ErrorMessages.UnknownConstructorDependency);
        }

        [TestMethod]
        public void GetInstance_GenericDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>();
            container.Register(typeof(IFoo<>), typeof(FooWithGenericDependency<>));
            var instance = (FooWithGenericDependency<IBar>)container.GetInstance<IFoo<IBar>>();
            Assert.IsInstanceOfType(instance.Dependency, typeof(Bar));
        }

        [TestMethod]
        public void GetInstance_OpenGenericDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar<>), typeof(Bar<>));
            container.Register(typeof(IFoo<>), typeof(FooWithOpenGenericDependency<>));
            var instance = (FooWithOpenGenericDependency<int>)container.GetInstance<IFoo<int>>();
            Assert.IsInstanceOfType(instance.Dependency, typeof(Bar<int>));
        }

        [TestMethod]
        public void GetInstance_OpenGenericDependencyWithRequestLifeCycle_InjectsSameDependenciesForSingleRequest()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar<>), typeof(Bar<>), LifeCycleType.Request);
            container.Register(typeof(IFoo<>), typeof(FooWithSameOpenGenericDependencyTwice<>));
            var instance = (FooWithSameOpenGenericDependencyTwice<int>)container.GetInstance<IFoo<int>>();
            Assert.AreEqual(instance.Bar1, instance.Bar2);
        }

        [TestMethod]
        public void GetInstance_DependencyWithTransientLifeCycle_InjectsTransientDependency()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>();
            container.Register<IFoo, FooWithDependency>();
            var instance1 = (FooWithDependency)container.GetInstance<IFoo>();
            var instance2 = (FooWithDependency)container.GetInstance<IFoo>();
            Assert.AreNotEqual(instance1.Bar, instance2.Bar);
        }

        [TestMethod]
        public void GetInstance_DependencyWithRequestLifeCycle_InjectsTransientDependency()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>(LifeCycleType.Request);
            container.Register<IFoo, FooWithDependency>();
            var instance1 = (FooWithDependency)container.GetInstance<IFoo>();
            var instance2 = (FooWithDependency)container.GetInstance<IFoo>();
            Assert.AreNotEqual(instance1.Bar, instance2.Bar);
        }

        [TestMethod]
        public void GetInstance_DependencyWithSingletonLifeCycle_InjectsSingleonDependency()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>(LifeCycleType.Singleton);
            container.Register<IFoo, FooWithDependency>();
            var instance1 = (FooWithDependency)container.GetInstance<IFoo>();
            var instance2 = (FooWithDependency)container.GetInstance<IFoo>();
            Assert.AreEqual(instance1.Bar, instance2.Bar);
        }

        [TestMethod]
        public void GetInstance_DependencyWithSingletonLifeCycle_CallsDependencyConstructorOnlyOnce()
        {
            var container = CreateContainer();
            Bar.InitializeCount = 0;
            container.Register<IBar>(c => new Bar(), LifeCycleType.Singleton);            
            container.Register<IFoo>(c => new FooWithDependency(c.GetInstance<IBar>()));
            container.GetInstance<IFoo>();
            container.GetInstance<IFoo>();
            Assert.AreEqual(1, Bar.InitializeCount);
        }
        
        [TestMethod]
        public void GetInstance_DependencyWithTransientLifeCycle_InjectsTransientDependenciesForSingleRequest()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>();
            container.Register<IFoo, FooWithSameDependencyTwice>();
            var instance = (FooWithSameDependencyTwice)container.GetInstance<IFoo>();
            Assert.AreNotEqual(instance.Bar1, instance.Bar2);
        }

        [TestMethod]
        public void GetInstance_DependencyWithSingletonLifeCycle_InjectsSingletonDependenciesForSingleRequest()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>(LifeCycleType.Singleton);
            container.Register<IFoo, FooWithSameDependencyTwice>();
            var instance = (FooWithSameDependencyTwice)container.GetInstance<IFoo>();
            Assert.AreEqual(instance.Bar1, instance.Bar2);
        }

        [TestMethod]
        public void GetInstance_DependencyWithRequestLifeCycle_InjectsSameDependencyForSingleClass()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>(LifeCycleType.Request);
            container.Register<IFoo, FooWithSameDependencyTwice>();
            var instance = (FooWithSameDependencyTwice)container.GetInstance<IFoo>();
            Assert.AreEqual(instance.Bar1, instance.Bar2);
        }

        [TestMethod]
        public void GetInstance_DependencyWithRequestLifeCycle_InjectsSameDependencyThroughoutObjectGraph()
        {
            var container = CreateContainer();
            container.Register<IBar, BarWithSampleServiceDependency>();
            container.Register<ISampleService, SampleService>(LifeCycleType.Request);
            container.Register<IFoo, FooWithSampleServiceDependency>();
            var instance = (FooWithSampleServiceDependency)container.GetInstance<IFoo>();
            Assert.AreSame(((BarWithSampleServiceDependency)instance.Bar).SampleService, instance.SampleService);
        }

        [TestMethod]
        public void GetInstance_DependencyWithRequestLifeCycle_InjectsTransientDependenciesForMultipleRequest()
        {
            var container = CreateContainer();
            container.Register<IBar, Bar>(LifeCycleType.Request);
            container.Register<IFoo, FooWithSameDependencyTwice>();
            var instance1 = (FooWithSameDependencyTwice)container.GetInstance<IFoo>();
            var instance2 = (FooWithSameDependencyTwice)container.GetInstance<IFoo>();
            Assert.AreNotEqual(instance1.Bar1, instance2.Bar2);
        }

        [TestMethod]
        public void GetInstance_ValueTypeDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register(42);
            container.Register<IFoo, FooWithValueTypeDependency>();
            var instance = (FooWithValueTypeDependency)container.GetInstance<IFoo>();
            Assert.AreEqual(42, instance.Value);
        }

        [TestMethod]
        public void GetInstance_EnumDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register(Encoding.UTF8);
            container.Register<IFoo, FooWithEnumDependency>();
            var instance = (FooWithEnumDependency)container.GetInstance<IFoo>();
            Assert.AreEqual(Encoding.UTF8, instance.Value);
        }

        [TestMethod]
        public void GetInstance_ReferenceTypeDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register("SomeValue");
            container.Register<IFoo, FooWithReferenceTypeDependency>();
            var instance = (FooWithReferenceTypeDependency)container.GetInstance<IFoo>();
            Assert.AreEqual("SomeValue", instance.Value);
        }

        [TestMethod]
        public void GetInstance_RequestLifeCycle_CallConstructorsOnDependencyOnlyOnce()
        {
            var container = CreateContainer();
            Bar.InitializeCount = 0;
            container.Register(typeof(IBar), typeof(Bar), LifeCycleType.Request);
            container.Register(typeof(IFoo), typeof(FooWithSameDependencyTwice));
            container.GetInstance<IFoo>();
            Assert.AreEqual(1, Bar.InitializeCount);
        }

        [TestMethod]
        public void GetInstance_MultipleContructors_UsesConstructorWithMostParameters()
        {
            var container = CreateContainer();
            container.Register(typeof(IFoo), typeof(FooWithMultipleConstructors));
            container.Register(typeof(IBar), typeof(Bar));
            var foo = (FooWithMultipleConstructors)container.GetInstance<IFoo>();
            Assert.IsNotNull(foo.Bar);
        }

        [TestMethod]
        public void GetInstance_FuncDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(Bar));
            container.Register(typeof(IFoo), typeof(FooWithFuncDependency));
            var instance = (FooWithFuncDependency)container.GetInstance<IFoo>();
            Assert.IsInstanceOfType(instance.GetBar(), typeof(Bar));
        }

        [TestMethod]
        public void GetInstance_NamedFuncDependency_InjectsDependency()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(Bar), "SomeBar");
            container.Register(typeof(IFoo), typeof(FooWithNamedFuncDependency));
            var instance = (FooWithNamedFuncDependency)container.GetInstance<IFoo>();
            Assert.IsInstanceOfType(instance.GetBar("SomeBar"), typeof(Bar));
        }

        [TestMethod]
        public void GetInstance_IEnumerableDependency_InjectsAllInstances()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(Bar));
            container.Register(typeof(IBar), typeof(AnotherBar), "AnotherBar");
            container.Register(typeof(IFoo), typeof(FooWithEnumerableDependency));
            var instance = (FooWithEnumerableDependency)container.GetInstance<IFoo>();
            Assert.AreEqual(2, instance.Bars.Count());
        }

        [TestMethod]
        public void GetInstance_CompositeDependency_InjectsOnlyOtherImplementations()
        {
            var container = CreateContainer();
            container.Register(typeof(IFoo), typeof(Foo), "Foo");
            container.Register(typeof(IFoo), typeof(AnotherFoo), "AnotherFoo");
            container.Register(typeof(IFoo), typeof(FooWithEnumerableIFooDependency));            
            var instance = (FooWithEnumerableIFooDependency)container.GetInstance<IFoo>();
            Assert.IsInstanceOfType(instance.FooList.First(), typeof(Foo));
            Assert.IsInstanceOfType(instance.FooList.Last(), typeof(AnotherFoo));
        }

        [TestMethod]
        public void GetInstance_SecondLevelCompositeDependency_InjectsOnlyOtherImplementations()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(BarWithFooDependency));
            container.Register(typeof(IFoo), typeof(Foo), "Foo");
            container.Register(typeof(IFoo), typeof(AnotherFoo), "AnotherFoo");
            container.Register<IFoo>(f => new FooWithEnumerableIFooDependency(f.GetAllInstances<IFoo>()));
                
            var instance = (BarWithFooDependency)container.GetInstance<IBar>();
            Assert.IsInstanceOfType(((FooWithEnumerableIFooDependency)instance.Foo).FooList.First(), typeof(Foo));
            Assert.IsInstanceOfType(((FooWithEnumerableIFooDependency)instance.Foo).FooList.Last(), typeof(AnotherFoo));            
        }

        [TestMethod]
        public void GetInstance_RecursiveDependency_ThrowsException()
        {
            var container = CreateContainer();
            container.Register(typeof(IFoo), typeof(FooWithRecursiveDependency));
            ExceptionAssert.Throws<InvalidOperationException>(
                () => container.GetInstance<IFoo>(), ex => ex.InnerException.InnerException.Message == ErrorMessages.RecursiveDependency);
        }

        [TestMethod]
        public void GetInstance_SecondLevelRecursiveDependency_ThrowsException()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(BarWithFooDependency));
            container.Register(typeof(IFoo), typeof(FooWithRecursiveDependency));
            ExceptionAssert.Throws<InvalidOperationException>(
                () => container.GetInstance<IBar>(), ex => ex.InnerException.InnerException.InnerException.Message == ErrorMessages.RecursiveDependency);
        }

        [TestMethod]
        public void GetInstance_RequestLifeCycle_FirstIEnumerableAndArgumentAreSame()
         {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(Bar), LifeCycleType.Request);
            container.Register(typeof(IFoo), typeof(FooWithEnumerableAndRegularDependency));
            var instance = (FooWithEnumerableAndRegularDependency)container.GetInstance<IFoo>();
            Assert.AreSame(instance.Bar, instance.Bars.First());
        }

        [TestMethod]
        public void GetInstance_SingletonLifeCycle_FirstIEnumerableAndArgumentAreSame()
        {
            var container = CreateContainer();
            container.Register(typeof(IBar), typeof(Bar), LifeCycleType.Singleton);
            container.Register(typeof(IFoo), typeof(FooWithEnumerableAndRegularDependency));
            var instance = (FooWithEnumerableAndRegularDependency)container.GetInstance<IFoo>();
            Assert.AreSame(instance.Bar, instance.Bars.First());
        }

        private static IServiceContainer CreateContainer()
        {
            return new ServiceContainer();
        }
    }
}