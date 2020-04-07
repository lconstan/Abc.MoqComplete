﻿using System.Collections.Generic;
using System.Linq;
using Abc.MoqComplete.Extensions;
using Abc.MoqComplete.Services;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Features.Intellisense.CodeCompletion.CSharp;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExpectedTypes;
using JetBrains.ReSharper.Psi.Resources;
using JetBrains.ReSharper.Psi.Tree;

namespace Abc.MoqComplete.CompletionProvider
{
	[Language(typeof(CSharpLanguage))]
	public abstract class BaseCallbackMethodProvider : ItemsProviderOfSpecificContext<CSharpCodeCompletionContext>
	{
		protected abstract IMethod GetMockedMethodFromSetupMethod(ISolution solution, IInvocationExpression invocation);

		protected abstract IEnumerable<string> GetMockedMethodParameterTypes(ISolution solution, IInvocationExpression invocation);

		protected override bool IsAvailable(CSharpCodeCompletionContext context)
		{
			var codeCompletionType = context.BasicContext.CodeCompletionType;

			return codeCompletionType == CodeCompletionType.SmartCompletion || codeCompletionType == CodeCompletionType.BasicCompletion;
		}

		protected override bool AddLookupItems(CSharpCodeCompletionContext context, IItemsCollector collector)
		{
			var identifier = context.TerminatedContext.TreeNode as IIdentifier;
			var expression = identifier.GetParentSafe<IReferenceExpression>();

			if (expression == null)
			{
				return false;
			}

			if (!(expression.ConditionalQualifier is IInvocationExpression invocation))
			{
				return false;
			}

			var solution = context.BasicContext.Solution;
			var methodIdentifier = solution.GetComponent<IMoqMethodIdentifier>();

			if (methodIdentifier.IsMoqReturnMethod(invocation))
			{
				invocation = invocation.InvokedExpression?.FirstChild as IInvocationExpression;
			}

			var mockedMethod = GetMockedMethodFromSetupMethod(solution, invocation);

			if (mockedMethod == null || mockedMethod.Parameters.Count == 0)
			{
				return false;
			}

			var types = GetMockedMethodParameterTypes(solution, invocation);
			var variablesName = mockedMethod.Parameters.Select(p => p.ShortName);
			var proposedCallback = $"Callback<{string.Join(",", types)}>(({string.Join(",", variablesName)}) => {{}})";

			var item = CSharpLookupItemFactory.Instance.CreateKeywordLookupItem(context,
				proposedCallback,
				TailType.None,
				PsiSymbolsThemedIcons.Method.Id);

			item.SetInsertCaretOffset(-2);
			item.SetReplaceCaretOffset(-2);
			item.WithInitializedRanges(context.CompletionRanges, context.BasicContext);
			item.SetTopPriority();
			collector.Add(item);

			return true;
		}
	}
}