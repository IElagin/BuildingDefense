using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay
{
    // Course EntityHealthPresenter (L8); the game has Player and Enemies teams instead of MainHero.
    public class EntityHealthPresenter : IPresenter
    {
        private BarWithText _bar;
        private Entity _entity;
        private ReactiveVariable<Teams> _team;
        private ReactiveVariable<float> _health;
        private ReactiveVariable<float> _maxHealth;

        private List<IDisposable> _disposables = new();

        public EntityHealthPresenter(
            Entity entity,
            BarWithText bar)
        {
            _entity = entity;
            _bar = bar;
        }

        public BarWithText Bar => _bar;

        public void Initialize()
        {
            _health = _entity.CurrentHealth;
            _maxHealth = _entity.MaxHealth;
            _team = _entity.Team;

            _disposables.Add(_health.Subscribe(OnHealthChanged));
            _disposables.Add(_maxHealth.Subscribe(OnMaxHealthChanged));
            _disposables.Add(_team.Subscribe(OnTeamChanged));

            UpdateHealth();
            UpdateFillerColorBy(_team.Value);
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();
        }

        private void OnTeamChanged(Teams oldValue, Teams newTeam) => UpdateFillerColorBy(newTeam);

        private void OnMaxHealthChanged(float oldValue, float newValue) => UpdateHealth();

        private void OnHealthChanged(float oldValue, float newValue) => UpdateHealth();

        private void UpdateHealth()
        {
            _bar.UpdateText(_health.Value.ToString("0"));
            _bar.UpdateSlider(_health.Value / _maxHealth.Value);
        }

        private void UpdateFillerColorBy(Teams team)
        {
            if (team == Teams.Player)
                _bar.SetFillerColor(Color.green);
            else if (team == Teams.Enemies)
                _bar.SetFillerColor(Color.red);
        }
    }
}
