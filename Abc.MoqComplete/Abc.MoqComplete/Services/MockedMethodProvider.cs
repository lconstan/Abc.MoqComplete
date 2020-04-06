﻿using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Abc.MoqComplete.Services
{
	[SolutionComponent]
	public class MockedMethodProvider : BaseMethodProvider, IMockedMethodProvider
	{
		private readonly IMoqMethodIdentifier _methodIdentifier;

		public MockedMethodProvider(IMoqMethodIdentifier methodIdentifier)
		{
			_methodIdentifier = methodIdentifier;
		}

		/// <inheritdoc />
		protected override bool IsSetupMethod(IInvocationExpression invocationExpression)
		{
			return _methodIdentifier.IsMoqSetupMethod(invocationExpression);
		}
	}
}