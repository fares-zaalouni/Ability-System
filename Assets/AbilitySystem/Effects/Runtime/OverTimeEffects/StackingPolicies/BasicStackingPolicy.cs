using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public class BasicStackingPolicy : IStackingPolicy
    {
        private StackingBehavior _stackingBehavior;
        private bool _newInstance;
        private bool _stackIfSameSource;

        public BasicStackingPolicy(StackingBehavior stackingBehavior, bool stackIfSameSource, bool newInstance = false)
        {
            _stackingBehavior = stackingBehavior;
            _stackIfSameSource = stackIfSameSource;
            _newInstance = newInstance;
        }

        public bool HandleStacking(IAbilityTarget target, OverTimeEffect newEffect, OverTimeEffectGroup existingEffects)
        {
            bool addedNew = false;
            if(existingEffects.Effects == null || existingEffects.Effects.Count == 0)
            {
                existingEffects.AddEffect(newEffect);
                return true;
            }
            if (!_stackIfSameSource)
            {
                switch (_stackingBehavior)
                {
                    case StackingBehavior.StackAll:
                        existingEffects.AddStacksToAll(newEffect.Stacks);
                        break;
                    case StackingBehavior.StackNewest:
                        if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                            existingEffects.GetNewestEffect().AddStacks(newEffect.Stacks);
                        break;
                    case StackingBehavior.StackOldest:
                        if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                            existingEffects.GetOldestEffect().AddStacks(newEffect.Stacks);
                        break;
                    case StackingBehavior.None:
                        break;
                }
            }
            else
            {
                switch (_stackingBehavior)
                {
                    case StackingBehavior.StackAll:
                        if(!existingEffects.TryAddStacksBySource(newEffect.Context.Caster, newEffect.Stacks))
                        {
                            existingEffects.AddEffect(newEffect);
                            addedNew = true;
                        }
                        break;
                    case StackingBehavior.StackNewest:
                        if (existingEffects.TryGetNewestEffectFromSource(newEffect.Context.Caster, out var newestFromSource))
                            newestFromSource.AddStacks(newEffect.Stacks);
                        else
                        {
                            existingEffects.AddEffect(newEffect);
                            addedNew = true;
                        }
                        break;
                    case StackingBehavior.StackOldest:
                        if (existingEffects.TryGetOldestEffectFromSource(newEffect.Context.Caster, out var oldestFromSource))
                            oldestFromSource.AddStacks(newEffect.Stacks);
                        else
                        {
                            existingEffects.AddEffect(newEffect);
                            addedNew = true;
                        }
                        break;
                    case StackingBehavior.None:
                        break;
                }
            }


            

            if (_newInstance && !addedNew)
            {
                existingEffects.AddEffect(newEffect);
            }
            return _newInstance || addedNew;
        }
    }
}