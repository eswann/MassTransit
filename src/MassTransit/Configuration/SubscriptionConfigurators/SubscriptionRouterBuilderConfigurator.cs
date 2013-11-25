namespace MassTransit.SubscriptionConfigurators
{
	using System;
	using System.Collections.Generic;
	using Configurators;
	using SubscriptionBuilders;

    public interface ISubscriptionRouterBuilderConfigurator :
    IConfigurator
    {
        ISubscriptionRouterBuilder Configure(ISubscriptionRouterBuilder builder);
    }

	public class SubscriptionRouterBuilderConfigurator :
		ISubscriptionRouterBuilderConfigurator
	{
		readonly Action<ISubscriptionRouterBuilder> _configureCallback;

		public SubscriptionRouterBuilderConfigurator(Action<ISubscriptionRouterBuilder> configureCallback)
		{
			_configureCallback = configureCallback;
		}

		public IEnumerable<IValidationResult> Validate()
		{
			if (_configureCallback == null)
				yield return this.Failure("ConfigureCallback", "Callback cannot be null");
		}

		public ISubscriptionRouterBuilder Configure(ISubscriptionRouterBuilder builder)
		{
			_configureCallback(builder);

			return builder;
		}
	}
}