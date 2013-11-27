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
namespace Burrows.Pipeline.Configuration
{
    using Context;
    using Exceptions;
    using Sinks;

    public class InboundMessageRouterConfigurator
	{
		readonly IPipelineSink<IConsumeContext> _sink;

		public InboundMessageRouterConfigurator(IPipelineSink<IConsumeContext> sink)
		{
			_sink = sink;
		}

		public MessageRouter<IConsumeContext<TOutput>> FindOrCreate<TOutput>()
			where TOutput : class
		{
			var scope = new InboundMessageRouterConfiguratorScope<IConsumeContext<TOutput>>();
			_sink.Inspect(scope);

			return scope.OutputRouter ?? ConfigureRouter<TOutput>(scope.InputRouter);
		}

		static MessageRouter<IConsumeContext<TOutput>> ConfigureRouter<TOutput>(MessageRouter<IConsumeContext> inputRouter)
			where TOutput : class
		{
			if (inputRouter == null)
				throw new PipelineException("The input router was not found");

			var router = new MessageRouter<IConsumeContext<TOutput>>();

			var translator = new InboundConvertMessageSink<TOutput>(router);

			inputRouter.Connect(translator);

			return router;
		}
	}
}