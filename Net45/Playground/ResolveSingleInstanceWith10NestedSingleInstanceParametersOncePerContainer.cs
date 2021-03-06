﻿using Autofac;
using BenchmarkDotNet.Attributes;
using DryIoc;
using NUnit.Framework;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    [TestFixture]
    public class ResolveSingleInstanceWith10NestedSingleInstanceParametersOncePerContainer
    {
        [Test]
        public void DryIoc_test()
        {
            Measure(PrepareDryIoc());
        }

        [Test, Explicit]
        public void DryIoc_test_1000_times()
        {
            for (var i = 0; i < 1000; i++)
            {
                Measure(PrepareDryIoc());
            }
        }

        public static global::DryIoc.IContainer PrepareDryIoc()
        {
            var container = new Container();

            container.Register<SingleInstance>(Reuse.Singleton);
            container.Register<Dependency1>(Reuse.Singleton);
            container.Register<Dependency2>(Reuse.Singleton);
            container.Register<Dependency3>(Reuse.Singleton);
            container.Register<Dependency4>(Reuse.Singleton);
            container.Register<Dependency5>(Reuse.Singleton);
            container.Register<Dependency6>(Reuse.Singleton);
            container.Register<Dependency7>(Reuse.Singleton);
            container.Register<Dependency8>(Reuse.Singleton);
            container.Register<Dependency9>(Reuse.Singleton);
            container.Register<Dependency10>(Reuse.Singleton);

            return container;
        }

        public static object Measure(global::DryIoc.IContainer container)
        {
            return container.Resolve<SingleInstance>();
        }

        [Test]
        public void Autofac_test()
        {
            Measure(PrepareAutofac());
        }

        public static IContainer PrepareAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SingleInstance>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency1>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency2>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency3>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency4>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency5>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency6>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency7>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency8>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency9>().AsSelf().SingleInstance();
            builder.RegisterType<Dependency10>().AsSelf().SingleInstance();

            return builder.Build();
        }

        public static object Measure(IContainer container)
        {
            return container.Resolve<SingleInstance>();
        }

        #region CUT

        internal class Dependency1
        {
            public Dependency1(Dependency2 d2) {}
        }

        internal class Dependency2
        {
            public Dependency2(Dependency3 d3) {}
        }

        internal class Dependency3
        {
            public Dependency3(Dependency4 d4) {}
        }

        internal class Dependency4
        {
            public Dependency4(Dependency5 d5) {}
        }

        internal class Dependency5
        {
            public Dependency5(Dependency6 d6) {}
        }

        internal class Dependency6
        {
            public Dependency6(Dependency7 d7) {}
        }

        internal class Dependency7
        {
            public Dependency7(Dependency8 d8) {}
        }

        internal class Dependency8
        {
            public Dependency8(Dependency9 d9) {}
        }

        internal class Dependency9
        {
            public Dependency9(Dependency10 d10) {}
        }

        internal class Dependency10 {}

        internal class SingleInstance
        {
            public SingleInstance(Dependency1 d1) {}
        }


        #endregion

        public class BenchmarkResolution
        {
            private IContainer _autofac = PrepareAutofac();

            private global::DryIoc.IContainer _dryioc = PrepareDryIoc();

            [Benchmark]
            public object BmarkAutofac()
            {
                return Measure(_autofac);
            }

            [Benchmark]
            public object BmarkDryIoc()
            {
                return Measure(_dryioc);
            }
        }

        public class BenchmarkRegistrationAndResolution
        {
            [Benchmark]
            public object BmarkAutofac()
            {
                return Measure(PrepareAutofac());
            }

            [Benchmark]
            public object BmarkDryIoc()
            {
                return Measure(PrepareDryIoc());
            }
        }
    }
}