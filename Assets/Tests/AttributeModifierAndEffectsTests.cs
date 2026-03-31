using System.Collections.Generic;
using System.Reflection;
using AbilitySystem.Attributes;
using AbilitySystem.Core;
using AbilitySystem.Effects;
using AbilitySystem.Targeting;
using NUnit.Framework;
using UnityEngine;
using Attribute = AbilitySystem.Attributes.Attribute;

namespace AbilitySystem.Tests.EditMode
{
    public class AttributeModifierAndEffectsTests
    {
        [Test]
        public void Attribute_Modifiers_AreAppliedByPriority()
        {
            var attribute = new Attribute(100f, "health");

            // Insert in reverse order on purpose; priority sorting should still apply multiply first, then add.
            attribute.AddModifier(new AddRuntimeModifier(priority: 10, amount: 10f));
            attribute.AddModifier(new MultiplyRuntimeModifier(priority: 1, factor: 2f));

            Assert.That(attribute.RuntimeValue, Is.EqualTo(210f).Within(0.0001f));
        }

        [Test]
        public void DOTEffect_ReapplyModifiers_IsIdempotent()
        {
            var caster = new FakeCasterWithAttributes();
            caster.AddAttribute(new Attribute(120f, "ability_power"));

            var context = new AbilityContext(caster);
            var target = new RecordingDamageTarget();

            var modifier = new AttributeModifier(
                priority: 1,
                applicationStrategy: ModifierApplicationStrategy.Runtime,
                source: ModifierSource.Caster,
                attributeName: "ability_power",
                percent: 0.1f);

            var modifiers = new List<IModifier> { modifier };

            var stackingPolicy = ScriptableObject.CreateInstance<BasicStackingPolicyDefinition>();
            var durationPolicy = ScriptableObject.CreateInstance<BasicDurationPolicyDefinition>();
            var definition = ScriptableObject.CreateInstance<TestOverTimeEffectDefinition>();

            try
            {
                definition.Initialize(stackingPolicy, durationPolicy);

                var dot = new DOTEffect(
                    definition,
                    damagePerTick: 50f,
                    duration: 5f,
                    tickInterval: 1f,
                    initialStacks: 1,
                    maxStacks: 5,
                    modifiers: modifiers,
                    context: context);

                dot.ApplyModifiers(target);
                dot.ApplyTickTo(target);
                var firstTickDamage = target.LastDamage;

                dot.ApplyModifiers(target);
                dot.ApplyTickTo(target);
                var secondTickDamage = target.LastDamage;

                Assert.That(firstTickDamage, Is.EqualTo(62f).Within(0.0001f));
                Assert.That(secondTickDamage, Is.EqualTo(firstTickDamage).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(durationPolicy);
                Object.DestroyImmediate(stackingPolicy);
            }
        }

        [Test]
        public void RegisterOverTimeEffect_WhenNewInstanceAdded_DurationPolicyTargetsOnlyPreExistingEffects()
        {
            var manager = CreateOverTimeEffectLifetimeManagerForTest();
            var target = new RecordingDamageTarget();
            var caster = new FakeCasterWithAttributes();

            var stackingPolicyDefinition = ScriptableObject.CreateInstance<InlineStackingPolicyDefinition>();
            var durationPolicyDefinition = ScriptableObject.CreateInstance<InlineDurationPolicyDefinition>();
            var definition = ScriptableObject.CreateInstance<TestOverTimeEffectDefinition>();
            var spyDurationPolicy = new SpyDurationPolicy();

            try
            {
                stackingPolicyDefinition.Initialize(new BasicStackingPolicy(StackingBehavior.None, stackIfSameSource: true, newInstance: true));
                durationPolicyDefinition.Initialize(spyDurationPolicy);
                definition.Initialize(stackingPolicyDefinition, durationPolicyDefinition);

                var existingEffect = new TestOverTimeEffect(definition, new AbilityContext(caster), duration: 5f);
                manager.RegisterOverTimeEffect(target, existingEffect);

                spyDurationPolicy.Reset();
                spyDurationPolicy.ExpectedExistingEffect = existingEffect;

                var newEffect = new TestOverTimeEffect(definition, new AbilityContext(caster), duration: 5f);
                manager.RegisterOverTimeEffect(target, newEffect);

                Assert.That(spyDurationPolicy.CallCount, Is.EqualTo(1));
                Assert.That(spyDurationPolicy.LastGroupContainsExistingEffect, Is.True);
                Assert.That(spyDurationPolicy.LastGroupContainsNewEffect, Is.False);
                Assert.That(spyDurationPolicy.LastGroupCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(durationPolicyDefinition);
                Object.DestroyImmediate(stackingPolicyDefinition);
                DestroyOverTimeEffectLifetimeManagerForTest(manager);
            }
        }

        [Test]
        [TestCase(StackingBehavior.None, false)]
        [TestCase(StackingBehavior.None, true)]
        [TestCase(StackingBehavior.StackAll, false)]
        [TestCase(StackingBehavior.StackAll, true)]
        [TestCase(StackingBehavior.StackNewest, false)]
        [TestCase(StackingBehavior.StackNewest, true)]
        [TestCase(StackingBehavior.StackOldest, false)]
        [TestCase(StackingBehavior.StackOldest, true)]
        public void BasicStackingPolicy_FirstApplyToEmptyGroup_AlwaysAddsNewEffect(StackingBehavior behavior, bool stackIfSameSource)
        {
            var policy = new BasicStackingPolicy(behavior, stackIfSameSource, newInstance: false);
            var stackingPolicyDefinition = ScriptableObject.CreateInstance<InlineStackingPolicyDefinition>();
            var durationPolicyDefinition = ScriptableObject.CreateInstance<InlineDurationPolicyDefinition>();
            var definition = ScriptableObject.CreateInstance<TestOverTimeEffectDefinition>();

            try
            {
                stackingPolicyDefinition.Initialize(new BasicStackingPolicy(StackingBehavior.None, stackIfSameSource: true, newInstance: true));
                durationPolicyDefinition.Initialize(new BasicDurationPolicy(DurationRefreshPolicy.None, RefreshPolicy.NeverRefresh));
                definition.Initialize(stackingPolicyDefinition, durationPolicyDefinition);

                var incomingEffect = new TestOverTimeEffect(definition, new AbilityContext(new FakeCasterWithAttributes()), duration: 5f);
                var group = new OverTimeEffectGroup();
                var added = policy.HandleStacking(target: null, newEffect: incomingEffect, existingEffects: group);

                Assert.That(added, Is.True);
                Assert.That(group.EffectCount, Is.EqualTo(1));
                Assert.That(group.GetOldestEffect(), Is.SameAs(incomingEffect));
            }
            finally
            {
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(durationPolicyDefinition);
                Object.DestroyImmediate(stackingPolicyDefinition);
            }
        }

        [Test]
        public void BasicDurationPolicy_ExtendAllWithSameSource_OnlyExtendsMatchingSource()
        {
            var casterA = new FakeCasterWithAttributes();
            var casterB = new FakeCasterWithAttributes();

            var stackingPolicyDefinition = ScriptableObject.CreateInstance<InlineStackingPolicyDefinition>();
            var durationPolicyDefinition = ScriptableObject.CreateInstance<InlineDurationPolicyDefinition>();
            var definition = ScriptableObject.CreateInstance<TestOverTimeEffectDefinition>();

            try
            {
                stackingPolicyDefinition.Initialize(new BasicStackingPolicy(StackingBehavior.None, stackIfSameSource: true, newInstance: true));
                durationPolicyDefinition.Initialize(new BasicDurationPolicy(DurationRefreshPolicy.None, RefreshPolicy.NeverRefresh));
                definition.Initialize(stackingPolicyDefinition, durationPolicyDefinition);

                var sourceEffect = new TestOverTimeEffect(definition, new AbilityContext(casterA), duration: 3f);
                var sameSourceExisting = new TestOverTimeEffect(definition, new AbilityContext(casterA), duration: 10f);
                var differentSourceExisting = new TestOverTimeEffect(definition, new AbilityContext(casterB), duration: 10f);

                var group = new OverTimeEffectGroup();
                group.AddEffect(sameSourceExisting);
                group.AddEffect(differentSourceExisting);

                var policy = new BasicDurationPolicy(DurationRefreshPolicy.ExtendAll, RefreshPolicy.RefreshIfSameSource);
                policy.HandleDuration(target: null, newEffect: sourceEffect, existingEffects: group);

                Assert.That(sameSourceExisting.RemainingDuration, Is.EqualTo(13f).Within(0.0001f));
                Assert.That(differentSourceExisting.RemainingDuration, Is.EqualTo(10f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(durationPolicyDefinition);
                Object.DestroyImmediate(stackingPolicyDefinition);
            }
        }

        [Test]
        public void DamageEffect_Instances_AreIsolated_PerCaster()
        {
            var target = new RecordingDamageTarget();

            var casterA = new FakeCasterWithAttributes();
            casterA.AddAttribute(new Attribute(100f, "ability_power"));

            var casterB = new FakeCasterWithAttributes();
            casterB.AddAttribute(new Attribute(300f, "ability_power"));

            // Mirror pipeline behavior: each application creates a new effect instance.
            CreateDamageEffect(casterA).ApplyTo(target);
            CreateDamageEffect(casterB).ApplyTo(target);
            CreateDamageEffect(casterA).ApplyTo(target);

            Assert.That(target.RecordedDamage.Count, Is.EqualTo(3));
            Assert.That(target.RecordedDamage[0], Is.EqualTo(60f).Within(0.0001f));
            Assert.That(target.RecordedDamage[1], Is.EqualTo(80f).Within(0.0001f));
            Assert.That(target.RecordedDamage[2], Is.EqualTo(60f).Within(0.0001f));

            DamageEffect CreateDamageEffect(ICaster caster)
            {
                return new DamageEffect(
                    damageAmount: 50f,
                    modifiers: new List<IModifier>
                    {
                        new AttributeModifier(1, ModifierApplicationStrategy.Runtime, ModifierSource.Caster, "ability_power", 0.1f)
                    },
                    context: new AbilityContext(caster));
            }
        }

        [Test]
        public void DamageEffect_ReusingSameInstance_CompoundsModifiers()
        {
            var target = new RecordingDamageTarget();
            var caster = new FakeCasterWithAttributes();
            caster.AddAttribute(new Attribute(100f, "ability_power"));

            var reusedEffect = new DamageEffect(
                damageAmount: 50f,
                modifiers: new List<IModifier>
                {
                    new AttributeModifier(1, ModifierApplicationStrategy.Runtime, ModifierSource.Caster, "ability_power", 0.1f)
                },
                context: new AbilityContext(caster));

            reusedEffect.ApplyTo(target);
            reusedEffect.ApplyTo(target);

            Assert.That(target.RecordedDamage.Count, Is.EqualTo(2));
            Assert.That(target.RecordedDamage[0], Is.EqualTo(60f).Within(0.0001f));
            Assert.That(target.RecordedDamage[1], Is.EqualTo(70f).Within(0.0001f));
        }

        private sealed class AddRuntimeModifier : IModifier
        {
            private readonly float _amount;
            public int Priority { get; }

            public AddRuntimeModifier(int priority, float amount)
            {
                Priority = priority;
                _amount = amount;
            }

            public void Apply(Attribute attribute)
            {
                attribute.AddToRuntime(_amount);
            }
        }

        private sealed class MultiplyRuntimeModifier : IModifier
        {
            private readonly float _factor;
            public int Priority { get; }

            public MultiplyRuntimeModifier(int priority, float factor)
            {
                Priority = priority;
                _factor = factor;
            }

            public void Apply(Attribute attribute)
            {
                attribute.SetRuntime(attribute.RuntimeValue * _factor);
            }
        }

        private sealed class FakeCasterWithAttributes : ICaster, IAttributeHolder
        {
            private readonly Dictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();

            public void AddAttribute(Attribute attribute)
            {
                _attributes[attribute.Name] = attribute;
            }

            public bool TryGetAttribute(string attributeName, out Attribute attribute)
            {
                return _attributes.TryGetValue(attributeName, out attribute);
            }

            public bool CanConsumeCost(IReadOnlyCollection<Attribute> costs)
            {
                return true;
            }

            public void ConsumeCost(IReadOnlyCollection<Attribute> costs)
            {
            }

            public void RegisterAttributes()
            {
            }

            public void GrantAbility(AbilityDefinition abilityDefinition)
            {
            }

            public void RemoveAbility(AbilityDefinition abilityDefinition)
            {
            }
        }

        private sealed class RecordingDamageTarget : IAbilityTarget, IDamageable, IAttributeHolder
        {
            private readonly Dictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();

            public List<float> RecordedDamage { get; } = new List<float>();
            public float LastDamage => RecordedDamage.Count == 0 ? 0f : RecordedDamage[RecordedDamage.Count - 1];

            public void AddAttribute(Attribute attribute)
            {
                _attributes[attribute.Name] = attribute;
            }

            public bool IsTargetable()
            {
                return true;
            }

            public void TakeDamage(float amount, ICaster source = null)
            {
                RecordedDamage.Add(amount);
            }

            public bool CanConsumeCost(IReadOnlyCollection<Attribute> costs)
            {
                return false;
            }

            public void ConsumeCost(IReadOnlyCollection<Attribute> costs)
            {
            }

            public bool TryGetAttribute(string attributeName, out Attribute attribute)
            {
                return _attributes.TryGetValue(attributeName, out attribute);
            }

            public void RegisterAttributes()
            {
            }
        }

        private sealed class TestOverTimeEffectDefinition : OverTimeEffectDefinition
        {
            public void Initialize(StackingPolicyDefinition stackingPolicy, DurationPolicyDefinition durationPolicy = null)
            {
                _duration = 5f;
                _tickInterval = 1f;
                _maxStacks = 5;
                _initialStacks = 1;
                _applyOnce = false;
                _stackingPolicy = stackingPolicy;
                _durationPolicy = durationPolicy;
            }

            public override IAbilityEffect CreateEffect(AbilityContext context)
            {
                throw new System.NotSupportedException("Test-only definition.");
            }
        }

        private sealed class TestOverTimeEffect : OverTimeEffect
        {
            public TestOverTimeEffect(OverTimeEffectDefinition definition, AbilityContext context, float duration)
                : base(definition, duration, tickInterval: 1f, stacks: 1, maxStacks: 10, context: context)
            {
            }

            public override void ApplyTickTo(IAbilityTarget target)
            {
            }

            public override void ApplyModifiers(IAbilityTarget target)
            {
            }
        }

        private sealed class InlineStackingPolicyDefinition : StackingPolicyDefinition
        {
            private IStackingPolicy _runtimePolicy;

            public void Initialize(IStackingPolicy runtimePolicy)
            {
                _runtimePolicy = runtimePolicy;
            }

            public override IStackingPolicy CreateRuntimeStackingPolicy()
            {
                return _runtimePolicy;
            }
        }

        private sealed class InlineDurationPolicyDefinition : DurationPolicyDefinition
        {
            private IDurationPolicy _runtimePolicy;

            public void Initialize(IDurationPolicy runtimePolicy)
            {
                _runtimePolicy = runtimePolicy;
            }

            public override IDurationPolicy CreateRuntimeDurationPolicy()
            {
                return _runtimePolicy;
            }
        }

        private sealed class SpyDurationPolicy : IDurationPolicy
        {
            public int CallCount { get; private set; }
            public int LastGroupCount { get; private set; }
            public bool LastGroupContainsNewEffect { get; private set; }
            public bool LastGroupContainsExistingEffect { get; private set; }
            public OverTimeEffect ExpectedExistingEffect { get; set; }

            public void HandleDuration(IAbilityTarget target, OverTimeEffect newEffect, OverTimeEffectGroup existingEffects)
            {
                CallCount++;
                LastGroupCount = existingEffects.EffectCount;
                LastGroupContainsNewEffect = existingEffects.Effects.Contains(newEffect);
                LastGroupContainsExistingEffect = ExpectedExistingEffect != null && existingEffects.Effects.Contains(ExpectedExistingEffect);
            }

            public void Reset()
            {
                CallCount = 0;
                LastGroupCount = 0;
                LastGroupContainsNewEffect = false;
                LastGroupContainsExistingEffect = false;
                ExpectedExistingEffect = null;
            }
        }

        private static OverTimeEffectLifetimeManager CreateOverTimeEffectLifetimeManagerForTest()
        {
            var instanceField = typeof(OverTimeEffectLifetimeManager).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            instanceField?.SetValue(null, null);

            var gameObject = new GameObject("Test_OverTimeEffectLifetimeManager");
            return gameObject.AddComponent<OverTimeEffectLifetimeManager>();
        }

        private static void DestroyOverTimeEffectLifetimeManagerForTest(OverTimeEffectLifetimeManager manager)
        {
            if (manager != null)
            {
                Object.DestroyImmediate(manager.gameObject);
            }

            var instanceField = typeof(OverTimeEffectLifetimeManager).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            instanceField?.SetValue(null, null);
        }
    }
}