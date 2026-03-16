using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public class BasicStackingPolicy : IStackingPolicy
    {
        private DurationRefreshPolicy _durationRefreshPolicy;
        private StackingBehavior _stackingBehavior;
        private bool _newInstance;
        private bool _stackIfSameSource;

        public BasicStackingPolicy(DurationRefreshPolicy durationRefreshPolicy, StackingBehavior stackingBehavior, bool stackIfSameSource, bool newInstance = false)
        {
            _durationRefreshPolicy = durationRefreshPolicy;
            _stackingBehavior = stackingBehavior;
            _stackIfSameSource = stackIfSameSource;
            _newInstance = newInstance;
        }
        public bool HandleStacking(IAbilityTarget target, OverTimeEffect newEffect, OverTimeEffectGroup existingEffects)
        {
            bool addedNew = false;
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
                        if(!existingEffects.TryAddStacksBySource(newEffect.Source, newEffect.Stacks))
                        {
                            existingEffects.AddEffect(newEffect);
                            addedNew = true;
                        }
                        break;
                    case StackingBehavior.StackNewest:
                        if (existingEffects.TryGetNewestEffectFromSource(newEffect.Source, out var newestFromSource))
                            newestFromSource.AddStacks(newEffect.Stacks);
                        else
                        {
                            existingEffects.AddEffect(newEffect);
                            addedNew = true;
                        }
                        break;
                    case StackingBehavior.StackOldest:
                        if (existingEffects.TryGetOldestEffectFromSource(newEffect.Source, out var oldestFromSource))
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


            switch (_durationRefreshPolicy)
            {
                case DurationRefreshPolicy.RefreshAll:
                    existingEffects.RefreshAllDurations();
                    break;
                case DurationRefreshPolicy.RefreshNewest:
                    if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                        existingEffects.GetNewestEffect().RefreshDuration();
                    break;
                case DurationRefreshPolicy.RefreshOldest:
                    if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                        existingEffects.GetOldestEffect().RefreshDuration();
                    break;
                case DurationRefreshPolicy.ExtendAll:
                    existingEffects.ExtendAllDurations(newEffect.TotalDuration);
                    break;
                case DurationRefreshPolicy.ExtendNewest:
                    if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                        existingEffects.GetNewestEffect().ExtendDuration(newEffect.TotalDuration);
                    break;
                case DurationRefreshPolicy.ExtendOldest:
                    if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                        existingEffects.GetOldestEffect().ExtendDuration(newEffect.TotalDuration);
                    break;
                case DurationRefreshPolicy.None:
                    break;
            }

            if (_newInstance && !addedNew)
            {
                existingEffects.AddEffect(newEffect);
            }
            return _newInstance || addedNew;
        }
    }
}