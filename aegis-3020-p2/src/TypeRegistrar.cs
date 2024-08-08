using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace aegis_3020_p2.src
{
    public sealed class TypeRegistrar(IServiceCollection builder) : ITypeRegistrar
    {
        private readonly IServiceCollection _builder = builder;

        public ITypeResolver Build()
        {
            return new TypeResolver(_builder.BuildServiceProvider());
        }

        public void Register(Type service, Type implementation)
        {
            _builder.AddSingleton(service, implementation);
        }

        public void RegisterInstance(Type service, object implementation)
        {
            _builder.AddSingleton(service, implementation);
        }

        public void RegisterLazy(Type service, Func<object> factory)
        {
            _builder.AddSingleton(service, provider => factory());
        }
    }

    public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
    {
        private readonly IServiceProvider _provider = provider;

        public object? Resolve(Type? type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Type cannot be null");
            }

            var service = _provider.GetService(type);

            if (service == null)
            {
                throw new InvalidOperationException($"Service for type {type.FullName} not found.");
            }

            return service;
        }

        public void Dispose()
        {
            if (_provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
