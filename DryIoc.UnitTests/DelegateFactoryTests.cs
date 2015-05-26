﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DelegateFactoryTests
    {
        [Test]
        public void Can_register_custom_delegates_without_ReflectionFactory()
        {
            var container = new Container();

            container.Register<ServiceLocator>();
            container.Register<A>();

            container.Register(Made.Of(r => ServiceInfo.Of<ServiceLocator>(), l => l.SingleInstanceFactory));
            container.Register(Made.Of(r => ServiceInfo.Of<ServiceLocator>(), l => l.MultiInstanceFactory));

            var getInstance = container.Resolve<SingleInstanceFactory>();
            Assert.IsNotNull(getInstance(typeof(A)));

            var getAllInstances = container.Resolve<MultiInstanceFactory>();
            Assert.AreEqual(1, getAllInstances(typeof(A)).Count());
        }

        public delegate object SingleInstanceFactory(Type serviceType);
        public delegate IEnumerable<object> MultiInstanceFactory(Type serviceType);

        public class ServiceLocator
        {
            public readonly SingleInstanceFactory SingleInstanceFactory;
            public readonly MultiInstanceFactory MultiInstanceFactory;

            private readonly IResolver _resolver;

            public ServiceLocator(IResolver resolver)
            {
                _resolver = resolver;
                SingleInstanceFactory = GetInstance;
                MultiInstanceFactory = GetAllInstances;
            }

            public object GetInstance(Type serviceType)
            {
                return _resolver.Resolve(serviceType);
            }

            public IEnumerable<object> GetAllInstances(Type serviceType)
            {
                return _resolver.ResolveMany<object>(serviceType);
            }
        }

        public class A { }

        [Test]
        public void Can_register_custom_delegates_as_method_groups()
        {
            var container = new Container();
            container.Register<ServiceLocator>();

            var ex = Assert.Throws<ContainerException>(() => 
            container.Register(Made.Of(r => ServiceInfo.Of<ServiceLocator>(), f => (SingleInstanceFactory)f.GetInstance)));

            Assert.AreEqual(Error.NotSupportedMadeExpression, ex.Error);
        }

        [Test]
        public void Given_Lambda_registration_Resolving_service_should_be_of_Lambda_provided_implementation()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service());

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_without_specified_service_type_should_use_implementation_type_as_service_type()
        {
            var container = new Container();
            container.RegisterDelegate(_ => new Service());

            Assert.Throws<ContainerException>(() => container.Resolve<IService>());

            var service = container.Resolve<Service>();
            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Func()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service());

            var func = container.Resolve<Func<IService>>();

            Assert.That(func(), Is.Not.Null.And.Not.SameAs(func()));
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Lazy()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service());

            var service = container.Resolve<Lazy<IService>>();

            Assert.That(service.Value, Is.Not.Null.And.SameAs(service.Value));
        }

        [Test]
        public void Given_lambda_registration_Injecting_it_as_dependency_should_work()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();

            var dependency = new Dependency();
            container.RegisterDelegate<IDependency>(_ => dependency);

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void While_registering_It_is_possible_to_resolve_lambda_parameters_from_container()
        {
            var container = new Container();
            container.Register<IDependency, Dependency>();
            container.RegisterDelegate(r => new ServiceWithDependency(r.Resolve<IDependency>()));

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());
        }

        [Test]
        public void Resolving_non_registered_dependency_inside_lambda_should_throw()
        {
            var container = new Container();
            container.RegisterDelegate(r => new ServiceWithDependency(r.Resolve<IDependency>()));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ServiceWithDependency>());
        }

        [Test]
        public void Possible_to_Register_delegate_with_runtime_type_of_service()
        {
            var container = new Container();

            container.RegisterDelegate(typeof (IService), _ => new Service());

            var service = container.Resolve<IService>();
            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolving_delegate_with_service_type_not_assignable_to_created_object_type_should_Throw()
        {
            var container = new Container();

            container.RegisterDelegate(typeof (IService), _ => "blah");

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        internal class MyClass
        {
            public IService MyService { get; set; }
        }
    }
}

