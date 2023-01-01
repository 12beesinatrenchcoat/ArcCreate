using ArcCreate.Compose.Components;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class ChartSkinUI : ChartMetadataUI
    {
        [SerializeField] private OptionsPanel side;
        [SerializeField] private OptionsPanel note;
        [SerializeField] private OptionsPanel particle;
        [SerializeField] private OptionsPanel accent;
        [SerializeField] private OptionsPanel track;
        [SerializeField] private OptionsPanel singleLine;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            side.SetValueWithoutNotify(chart.Skin?.Side);
            note.SetValueWithoutNotify(chart.Skin?.Note);
            particle.SetValueWithoutNotify(chart.Skin?.Particle);
            accent.SetValueWithoutNotify(chart.Skin?.Accent);
            track.SetValueWithoutNotify(chart.Skin?.Track);
            singleLine.SetValueWithoutNotify(chart.Skin?.SingleLine);
        }

        private new void Start()
        {
            base.Start();
            side.OnValueChanged += OnSide;
            note.OnValueChanged += OnNote;
            particle.OnValueChanged += OnParticle;
            accent.OnValueChanged += OnAccent;
            track.OnValueChanged += OnTrack;
            singleLine.OnValueChanged += OnSingleLine;
        }

        private void OnDestroy()
        {
            side.OnValueChanged -= OnSide;
            note.OnValueChanged -= OnNote;
            particle.OnValueChanged -= OnParticle;
            accent.OnValueChanged -= OnAccent;
            track.OnValueChanged -= OnTrack;
            singleLine.OnValueChanged -= OnSingleLine;
        }

        private void OnSide(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Side = value;
            Services.Gameplay.Skin.AlignmentSkin = value;
        }

        private void OnNote(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Note = value;
            Services.Gameplay.Skin.NoteSkin = value;
        }

        private void OnParticle(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Particle = value;
            Services.Gameplay.Skin.ParticleSkin = value;
        }

        private void OnAccent(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Accent = value;
            Services.Gameplay.Skin.AccentSkin = value;
        }

        private void OnTrack(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Track = value;
            Services.Gameplay.Skin.TrackSkin = value;
        }

        private void OnSingleLine(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.SingleLine = value;
            Services.Gameplay.Skin.SingleLineSkin = value;
        }

        private void CreateSkinObjectIfNull()
        {
            Target.Skin = Target.Skin ?? new SkinSettings();
        }
    }
}