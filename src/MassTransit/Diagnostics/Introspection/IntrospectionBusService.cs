// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Burrows.Diagnostics.Introspection
{
    using System.Linq;
    using Context;
    using Messages;

    public class IntrospectionBusService :
        IBusService,
        Consumes<IGetBusStatus>.Context
    {
        IServiceBus _bus;
        IServiceBus _controlBus;
        UnsubscribeAction _unsubscribe;

        public void Consume(IConsumeContext<IGetBusStatus> context)
        {
            IDiagnosticsProbe probe = _bus.Probe();

            context.Respond(new BusStatus
                {
                    Entries = probe.Entries
                        .Select(x => (IBusStatusEntry) new BusStatusEntry(x.Context, x.Key, x.Value))
                        .ToArray(),
                });
        }

        public void Dispose()
        {
        }

        public void Start(IServiceBus bus)
        {
            _bus = bus;
            _controlBus = bus.ControlBus;
            _unsubscribe = _controlBus.SubscribeInstance(this);
        }

        public void Stop()
        {
            _unsubscribe();
        }
    }
}