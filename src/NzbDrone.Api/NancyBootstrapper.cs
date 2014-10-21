﻿using NLog;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using NzbDrone.Api.ErrorManagement;
using NzbDrone.Api.Extensions.Pipelines;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using TinyIoC;

namespace NzbDrone.Api
{
    public class NancyBootstrapper : TinyIoCNancyBootstrapper
    {
        private readonly TinyIoCContainer _tinyIoCContainer;
        private readonly Logger _logger;

        public NancyBootstrapper(TinyIoCContainer tinyIoCContainer)
        {
            _tinyIoCContainer = tinyIoCContainer;
            _logger =  NzbDroneLogger.GetLogger();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            _logger.Info("Starting NzbDrone API");

            if (RuntimeInfoBase.IsProduction)
            {
                DiagnosticsHook.Disable(pipelines);
            }

            RegisterPipelines(pipelines);

            container.Resolve<DatabaseTarget>().Register();
            container.Resolve<IEventAggregator>().PublishEvent(new ApplicationStartedEvent());

            ApplicationPipelines.OnError.AddItemToEndOfPipeline(container.Resolve<NzbDroneErrorPipeline>().HandleException, false);
        }

        private void RegisterPipelines(IPipelines pipelines)
        {
            var pipelineRegistrars = _tinyIoCContainer.ResolveAll<IRegisterNancyPipeline>();

            foreach (var registerNancyPipeline in pipelineRegistrars)
            {
                registerNancyPipeline.Register(pipelines);
            }
        }

        protected override TinyIoCContainer GetApplicationContainer()
        {
            return _tinyIoCContainer;
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"password" }; }
        }

        protected override byte[] FavIcon
        {
            get
            {
                return null;
            }
        }
    }
}