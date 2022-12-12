using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public class TapNoteGroup : ShortNoteGroup<Tap, TapBehaviour>
    {
        public override string PoolName => Values.TapPoolName;

        public override void SetupNotes()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                Tap tap = Notes[i];
                SetupConnection(tap);
            }
        }

        protected override void OnAdd(Tap note)
        {
            SetupConnection(note);
        }

        protected override void OnUpdate(Tap note)
        {
            SetupConnection(note);
        }

        protected override void OnRemove(Tap note)
        {
            RemoveConnection(note);
        }

        private void SetupConnection(Tap note)
        {
            RemoveConnection(note);

            IEnumerable<ArcTap> connectedArcTaps
                = Services.Chart.FindByTiming<ArcTap>(note.Timing);

            foreach (ArcTap arcTap in connectedArcTaps)
            {
                note.ConnectedArcTaps.Add(arcTap);
            }

            note.Rebuild();
        }

        private void RemoveConnection(Tap note)
        {
            foreach (ArcTap arcTap in note.ConnectedArcTaps)
            {
                arcTap.ConnectedTaps.Remove(note);
            }

            note.ConnectedArcTaps.Clear();
        }
    }
}