﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using Burrows.Configuration.Configurators;
using Burrows.Configuration.EndpointConfigurators;
using Burrows.Endpoints;
using System;
using Burrows.Exceptions;
using Magnum;
using Burrows.Util;

namespace Burrows.Configuration
{
    public static class EndpointCacheFactory
	{
		static readonly EndpointFactoryDefaultSettings _defaultSettings = new EndpointFactoryDefaultSettings();

		[NotNull]
		public static EndpointCache New([NotNull] Action<IEndpointFactoryConfigurator> configure)
		{
			Guard.AgainstNull(configure, "configure");

			var configurator = new EndpointFactoryConfigurator(_defaultSettings);

			configure(configurator);

			IConfigurationResult result = ConfigurationResult.CompileResults(configurator.Validate());

			try
			{
				IEndpointFactory endpointFactory = configurator.CreateEndpointFactory();

				EndpointCache endpointCache = new EndpointCache(endpointFactory);

				return endpointCache;
			}
			catch (Exception ex)
			{
				throw new ConfigurationException(result, "An exception was thrown during endpoint cache creation", ex);
			}
		}

		public static void ConfigureDefaultSettings([NotNull] Action<IEndpointFactoryDefaultSettingsConfigurator> configure)
		{
			Guard.AgainstNull(configure);

			var configurator = new EndpointFactoryDefaultSettingsConfigurator(_defaultSettings);

			configure(configurator);
		}
	}
}