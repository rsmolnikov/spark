﻿using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.Parsing;
using SparkSense.StatementCompletion.CompletionSets;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Operations;
using Spark.Parser.Markup;

namespace SparkSense.StatementCompletion
{
    public class CompletionSource : ICompletionSource
    {
        private bool _isDisposed;
        private ITextBuffer _textBuffer;
        private IProjectExplorer _projectExplorer;
        private ITextStructureNavigator _textNavigator;
        private IViewExplorer _viewExplorer;
        private ITrackingSpan _trackingSpan;
        private SnapshotPoint _triggerPoint;

        public CompletionSource(ITextBuffer textBuffer, ITextStructureNavigator textNavigator, IProjectExplorer projectExplorer)
        {
            _textBuffer = textBuffer;
            _textNavigator = textNavigator;
            _projectExplorer = projectExplorer;
            _viewExplorer = ViewExplorer.CreateFromActiveDocument(_projectExplorer);
        }
        public CompletionSource(ITextBuffer textBuffer, ITextStructureNavigator textNavigator)
        {
            _textBuffer = textBuffer;
            _textNavigator = textNavigator;
        }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            _triggerPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);

            if (!session.Properties.TryGetProperty(typeof(ITrackingSpan), out _trackingSpan))
                _trackingSpan = _triggerPoint.Snapshot.CreateTrackingSpan(new Span(_triggerPoint, 0), SpanTrackingMode.EdgeInclusive);

            var syntax = new SparkSyntax();
            Node currentNode = syntax.ParseNode(_textBuffer.CurrentSnapshot.GetText(), _triggerPoint);
            CompletionSet sparkCompletions = GetCompletionSetFor(currentNode);
            if (sparkCompletions == null) return;

            MergeSparkWithAllCompletionsSet(completionSets, sparkCompletions);
            completionSets.Add(sparkCompletions);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        #endregion

        private CompletionSet GetCompletionSetFor(Node node)
        {
            char currentCharacter = _textBuffer.CurrentSnapshot[_triggerPoint - 1];

            switch (currentCharacter)
            {
                case '<':
                    return SparkCompletionSetFactory.Create<SparkElementCompletionSet>(_viewExplorer, _trackingSpan);
                case '"':
                    return SparkCompletionSetFactory.Create<SparkAttributeCompletionSet>(_viewExplorer, _trackingSpan);
                default:
                    break;
            }

            if (node is ElementNode)
                return SparkCompletionSetFactory.Create<SparkElementCompletionSet>(_viewExplorer, _trackingSpan);
            if (node is AttributeNode)
                return SparkCompletionSetFactory.Create<SparkAttributeCompletionSet>(_viewExplorer, _trackingSpan);
            return null;
        }

        private static void MergeSparkWithAllCompletionsSet(IList<CompletionSet> completionSets, CompletionSet sparkCompletions)
        {

            CompletionSet allCompletionsSet;
            if (!TryExtractAllCompletionsSet(completionSets, out allCompletionsSet)) return;

            var mergedCompletionSet = new CompletionSet(
                                allCompletionsSet.Moniker,
                                allCompletionsSet.DisplayName,
                                allCompletionsSet.ApplicableTo,
                                GetCombinedSortedList(sparkCompletions, allCompletionsSet),
                                allCompletionsSet.CompletionBuilders);

            completionSets.Remove(allCompletionsSet);
            completionSets.Add(mergedCompletionSet);
        }

        private static bool TryExtractAllCompletionsSet(IList<CompletionSet> completionSets, out CompletionSet allCompletions)
        {
            allCompletions = null;
            foreach (var completionSet in completionSets)
            {
                if (completionSet.DisplayName != "All") continue;
                allCompletions = completionSet;
                return true;
            }
            return false;
        }

        private static List<Completion> GetCombinedSortedList(CompletionSet sparkCompletions, CompletionSet allCompletionsSet)
        {
            var combinedList = new List<Completion>();
            combinedList.AddRange(allCompletionsSet.Completions);
            combinedList.AddRange(sparkCompletions.Completions);
            combinedList.Sort((a, b) => a.DisplayText.CompareTo(b.DisplayText));
            return combinedList;
        }


    }
}