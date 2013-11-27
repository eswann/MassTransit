﻿namespace MassTransit.Saga.Pipeline
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
	/// Determines if a message property is compared to the CorrelationId of the saga and if so
	/// returns an expression that can be used to return that id
	/// </summary>
	/// <typeparam name="TSaga"></typeparam>
	/// <typeparam name="TMessage"></typeparam>
	public class CorrelationExpressionToSagaIdVisitor<TSaga, TMessage> :
		Util.ExpressionVisitor
	{
		Expression<Func<TMessage, Guid>> _result;

		public Expression<Func<TMessage, Guid>> Build(Expression<Func<TSaga, TMessage, bool>> expression)
		{
			_result = null;

			Visit(expression);

			return _result;
		}

		protected override Expression VisitBinary(BinaryExpression b)
		{
			if (b == null)
				return null;

			var left = b.Left as MemberExpression;
			var right = b.Right as MemberExpression;
			if(left != null && right != null && left.Expression != null && right.Expression != null)
			{
				if(left.Expression.NodeType == ExpressionType.Parameter && left.Expression.Type == typeof(TMessage) &&
					right.Expression.NodeType == ExpressionType.Parameter && right.Expression.Type == typeof(TSaga))
				{
					EvaluateMessageToSaga(left, right);
				}

				if (right.Expression.NodeType == ExpressionType.Parameter && right.Expression.Type == typeof(TMessage) &&
					left.Expression.NodeType == ExpressionType.Parameter && left.Expression.Type == typeof(TSaga))
				{
					EvaluateMessageToSaga(right, left);
				}
			}

			return base.VisitBinary(b);
		}

		void EvaluateMessageToSaga(MemberExpression messageExpression, MemberExpression sagaExpression)
		{
			if (sagaExpression.Member.MemberType == MemberTypes.Property && sagaExpression.Member.Name == "CorrelationId")
			{
				_result = Expression.Lambda<Func<TMessage, Guid>>(messageExpression,
					(ParameterExpression) messageExpression.Expression);
			}
		}
	}
}