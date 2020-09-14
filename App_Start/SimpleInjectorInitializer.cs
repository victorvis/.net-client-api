[assembly: WebActivator.PostApplicationStartMethod(typeof(SaeClient.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace SaeClient.App_Start
{
    using SaeClient.Services;
    using SaeClient.Services.Interfaces;
    using SimpleInjector;
    using SimpleInjector.Integration.Web;
    using SimpleInjector.Integration.Web.Mvc;
    using System.Reflection;
    using System.Web.Mvc;

    public static class SimpleInjectorInitializer
    {
        /// <summary>Initialize the container and register it as MVC3 Dependency Resolver.</summary>
        public static void Initialize()
        {
            var container = new Container();

            try
            {
                container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

                InitializeContainer(container);

                container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
                container.Verify();

                DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            }
            catch (System.Exception e)
            {
                container.Dispose();
                throw;
            }
        }

        private static void InitializeContainer(Container container)
        {
            container.Register<IApiSaeService, ApiSaeService>(Lifestyle.Scoped);
        }
    }
}