using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public class BasicDurationPolicy : IDurationPolicy
    {
        private DurationRefreshPolicy _durationRefreshPolicy;
        private RefreshPolicy _refreshPolicy;

        public BasicDurationPolicy(DurationRefreshPolicy durationRefreshPolicy, RefreshPolicy refreshPolicy)
        {
            _durationRefreshPolicy = durationRefreshPolicy;
            _refreshPolicy = refreshPolicy;
        }

        public void HandleDuration(IAbilityTarget target, OverTimeEffect newEffect, OverTimeEffectGroup existingEffects)
        {
            if (_refreshPolicy == RefreshPolicy.NeverRefresh)
            {
                return;
            }
            else if (_refreshPolicy == RefreshPolicy.RefreshIfSameSource)
            {
                switch (_durationRefreshPolicy)
                {
                    case DurationRefreshPolicy.RefreshAll:
                        existingEffects.RefreshDurationsBySource(newEffect.Context.Caster);
                        break;
                    case DurationRefreshPolicy.RefreshNewest:
                        if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                            if (existingEffects.TryGetNewestEffectFromSource(newEffect.Context.Caster, out var newestEffect))
                                newestEffect.RefreshDuration();
                        break;
                    case DurationRefreshPolicy.RefreshOldest:
                        if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                            if (existingEffects.TryGetOldestEffectFromSource(newEffect.Context.Caster, out var oldestEffect))
                                oldestEffect.RefreshDuration();
                        break;
                    case DurationRefreshPolicy.ExtendAll:
                        existingEffects.ExtendDurationsBySource(newEffect.Context.Caster, newEffect.TotalDuration);
                        break;
                    case DurationRefreshPolicy.ExtendNewest:
                        if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                            if (existingEffects.TryGetNewestEffectFromSource(newEffect.Context.Caster, out var newestEffect))
                                newestEffect.ExtendDuration(newEffect.TotalDuration);
                        break;
                    case DurationRefreshPolicy.ExtendOldest:
                        if (existingEffects.Effects != null && existingEffects.Effects.Count > 0)
                            if (existingEffects.TryGetOldestEffectFromSource(newEffect.Context.Caster, out var oldestEffect))
                                oldestEffect.ExtendDuration(newEffect.TotalDuration);
                        break;
                    case DurationRefreshPolicy.None:
                        break;
                }
            }
            else
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
        }
    }
}