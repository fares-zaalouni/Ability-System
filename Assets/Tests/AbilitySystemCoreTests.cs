using System.Collections.Generic;
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace AbilitySystem.Tests.EditMode
{
    public class AbilityCoreTests
    {
        [Test]
        public void AbilityRunner_Next_ExecutesActionsInOrder_AndCompletes()
        {
            var executionOrder = new List<string>();
            var context = new AbilityContext(null);
            var actions = new List<IAbilityAction>
            {
                new RecordingAction("A", executionOrder, autoAdvance: true),
                new RecordingAction("B", executionOrder, autoAdvance: true)
            };

            var runner = new AbilityRunner(actions, context);
            var completed = false;
            runner.OnCompleted += () => completed = true;

            runner.Next();

            CollectionAssert.AreEqual(new[] { "A", "B" }, executionOrder);
            Assert.That(completed, Is.True);
        }

        [Test]
        public void AbilityRunner_Cancel_OnSustainedAction_WithCancelAftermath_RaisesCancelled()
        {
            var context = new AbilityContext(null);
            var sustainedAction = new FakeSustainedAction(
                isCancellable: true,
                isInterruptible: false,
                cancelAftermath: SustainedActionEndAftermath.Cancel,
                interruptAftermath: SustainedActionEndAftermath.None,
                cancelResult: true,
                interruptResult: false);

            var runner = new AbilityRunner(new List<IAbilityAction> { sustainedAction }, context);
            var cancelled = false;
            runner.OnCancelled += () => cancelled = true;

            runner.Next();
            runner.Cancel();

            Assert.That(sustainedAction.CancelCalled, Is.True);
            Assert.That(cancelled, Is.True);
        }

        [Test]
        public void AbilityRunner_Interrupt_WithNoneAftermath_AdvancesToNextAction()
        {
            var context = new AbilityContext(null);
            var executionOrder = new List<string>();
            var sustainedAction = new FakeSustainedAction(
                isCancellable: false,
                isInterruptible: true,
                cancelAftermath: SustainedActionEndAftermath.None,
                interruptAftermath: SustainedActionEndAftermath.None,
                cancelResult: false,
                interruptResult: true);

            var actions = new List<IAbilityAction>
            {
                sustainedAction,
                new RecordingAction("AfterInterrupt", executionOrder, autoAdvance: false)
            };

            var runner = new AbilityRunner(actions, context);
            var interrupted = false;
            runner.OnInterrupted += () => interrupted = true;

            runner.Next();
            runner.Interrupt();

            Assert.That(sustainedAction.InterruptCalled, Is.True);
            CollectionAssert.AreEqual(new[] { "AfterInterrupt" }, executionOrder);
            Assert.That(interrupted, Is.False);
        }

        [Test]
        public void AbilityContext_SetAndTryGet_RoundTripsTypedData()
        {
            var context = new AbilityContext(null);

            context.Set(42);

            var found = context.TryGet<int>(out var value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void AbilityContext_Fork_CopiesTargetsAndBlackboardIndependently()
        {
            var original = new AbilityContext(null);
            original.Set(5);
            original.SetTargets(new List<IAbilityTarget> { new FakeTarget() });

            var fork = original.Fork();

            original.Set(99);
            original.SetTargets(new List<IAbilityTarget>());

            var foundInFork = fork.TryGet<int>(out var forkValue);

            Assert.That(foundInFork, Is.True);
            Assert.That(forkValue, Is.EqualTo(5));
            Assert.That(fork.Targets.Count, Is.EqualTo(1));
        }

        [Test]
        public void AbilityContext_SetRuntimeSignal_CanBeRetrievedByDefinition()
        {
            var context = new AbilityContext(null);
            var signalDefinition = ScriptableObject.CreateInstance<SignalDefinition>();
            var signal = new RuntimeSignal();

            try
            {
                context.SetRuntimeSignal(signalDefinition, signal);

                var found = context.TryGetRuntimeSignal(signalDefinition, out var storedSignal);

                Assert.That(found, Is.True);
                Assert.That(storedSignal, Is.SameAs(signal));
            }
            finally
            {
                Object.DestroyImmediate(signalDefinition);
            }
        }

        private sealed class RecordingAction : IAbilityAction
        {
            private readonly string _name;
            private readonly List<string> _executionOrder;
            private readonly bool _autoAdvance;

            public RecordingAction(string name, List<string> executionOrder, bool autoAdvance)
            {
                _name = name;
                _executionOrder = executionOrder;
                _autoAdvance = autoAdvance;
            }

            public void Execute(AbilityContext context, AbilityRunner runner)
            {
                _executionOrder.Add(_name);
                if (_autoAdvance)
                    runner.Next();
            }
        }

        private sealed class FakeSustainedAction : SustainedAction
        {
            private readonly bool _cancelResult;
            private readonly bool _interruptResult;

            public bool CancelCalled { get; private set; }
            public bool InterruptCalled { get; private set; }

            public FakeSustainedAction(
                bool isCancellable,
                bool isInterruptible,
                SustainedActionEndAftermath cancelAftermath,
                SustainedActionEndAftermath interruptAftermath,
                bool cancelResult,
                bool interruptResult)
                : base(isCancellable, isInterruptible, cancelAftermath, interruptAftermath)
            {
                _cancelResult = cancelResult;
                _interruptResult = interruptResult;
            }

            public override void Execute(AbilityContext context, AbilityRunner runner)
            {
            }

            public override bool Cancel(AbilityContext context)
            {
                CancelCalled = true;
                return _cancelResult;
            }

            public override bool Interrupt(AbilityContext context)
            {
                InterruptCalled = true;
                return _interruptResult;
            }
        }

        private sealed class FakeTarget : IAbilityTarget
        {
            public bool IsTargetable()
            {
                return true;
            }
        }
    }
}