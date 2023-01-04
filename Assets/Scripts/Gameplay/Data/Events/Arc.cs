using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class Arc : LongNote, ILongNote<ArcBehaviour>, IArcJudgementReceiver
    {
        private ArcBehaviour instance;
        private bool highlight = false;
        private bool judgementRequestSent = false;
        private bool hasBeenHitOnce = false;
        private int flashCount = 0;
        private float arcGroupAlpha = 1;
        private int highlightUntil = int.MinValue;
        private Arc firstArcOfGroup;

        // Avoid infinite recursion
        private bool recursivelyCalled = false;

        public float XStart { get; set; }

        public float YStart { get; set; }

        public float XEnd { get; set; }

        public float YEnd { get; set; }

        public int Color { get; set; }

        public bool IsTrace { get; set; }

        public string Sfx { get; set; }

        public ArcLineType LineType { get; set; }

        public List<ArcTap> ArcTaps { get; set; } = new List<ArcTap>();

        public HashSet<Arc> NextArcs { get; } = new HashSet<Arc>();

        public HashSet<Arc> PreviousArcs { get; } = new HashSet<Arc>();

        public bool IsAssignedInstance => instance != null;

        public float SegmentLength => ArcFormula.CalculateArcSegmentLength(EndTiming - Timing);

        public bool IsFirstArcOfGroup => PreviousArcs.Count == 0;

        public bool ShouldDrawHeightIndicator => !IsTrace && (YStart != YEnd || IsFirstArcOfGroup);

        public float ArcCapSize => IsTrace ? Values.TraceCapSize : Values.ArcCapSize;

        public bool Highlight
        {
            get => highlight;
            set
            {
                highlight = value;
                if (instance != null)
                {
                    instance.Highlight = value;
                }
            }
        }

        public override ArcEvent Clone()
        {
            Arc arc = new Arc()
            {
                Timing = Timing,
                EndTiming = EndTiming,
                XStart = XStart,
                XEnd = XEnd,
                LineType = LineType,
                YStart = YStart,
                YEnd = YEnd,
                Color = Color,
                IsTrace = IsTrace,
                TimingGroup = TimingGroup,
            };
            foreach (var arctap in ArcTaps)
            {
                arc.ArcTaps.Add(arctap.Clone() as ArcTap);
            }

            return arc;
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            Arc n = newValues as Arc;
            XStart = n.XStart;
            XEnd = n.XEnd;
            LineType = n.LineType;
            YStart = n.YStart;
            YEnd = n.YEnd;
            Color = n.Color;
            IsTrace = n.IsTrace;
            TimingGroup = n.TimingGroup;
        }

        public void AssignInstance(ArcBehaviour instance)
        {
            this.instance = instance;
            instance.SetData(this);
            ReloadSkin();
            RebuildSegments();
        }

        public ArcBehaviour RevokeInstance()
        {
            var result = instance;
            instance = null;
            return result;
        }

        public void ResetJudge()
        {
            ResetJudgeTimings();
            Highlight = false;
            highlightUntil = int.MinValue;
            judgementRequestSent = false;
            arcGroupAlpha = 1;
        }

        public void Rebuild()
        {
            if (IsFirstArcOfGroup)
            {
                SetGroupFirst(this);
            }

            RecalculateJudgeTimings();
            RebuildSegments();
        }

        public void ReloadSkin()
        {
            var (normal, highlight, arcCap, heightIndicatorColor) = Services.Skin.GetArcSkin(this);
            instance.SetSkin(normal, highlight, arcCap, heightIndicatorColor);
        }

        public void UpdateJudgement(int currentTiming, GroupProperties groupProperties)
        {
            if (!IsTrace && currentTiming >= Timing && Timing < EndTiming && !judgementRequestSent)
            {
                RequestJudgement(currentTiming);
                judgementRequestSent = true;
            }
        }

        public void UpdateInstance(int currentTiming, double currentFloorPosition, GroupProperties groupProperties)
        {
            if (instance == null)
            {
                return;
            }

            float z = ZPos(currentFloorPosition);

            Vector3 pos = (groupProperties.FallDirection * z) + new Vector3(WorldXAt(Timing), WorldYAt(Timing), 0);
            Quaternion rot = groupProperties.RotationIndividual;
            Vector3 scl = groupProperties.ScaleIndividual;
            instance.SetTransform(pos, rot, scl);
            instance.UpdateSegmentsPosition(currentFloorPosition, groupProperties.FallDirection);

            float alpha = 1;
            if (!IsTrace)
            {
                Highlight = currentTiming <= highlightUntil;
                if (Highlight)
                {
                    flashCount = (flashCount + 1) % Values.ArcFlashCycle;
                    if (flashCount == 0)
                    {
                        alpha = Values.FlashArcAlphaScalar;
                    }

                    if (currentTiming <= EndTiming)
                    {
                        Services.Camera.AddTiltToCamera(WorldXAt(currentTiming));
                    }
                }
                else
                {
                    if (currentTiming >= firstArcOfGroup.Timing)
                    {
                        alpha = Values.MissedArcAlphaScalar;
                    }
                }

                alpha *= Values.MaxArcAlpha;
            }
            else
            {
                alpha = EndTiming - Timing <= 1 ? Values.MaxArcAlpha / 2 : Values.MaxArcAlpha;
            }

            Color color = groupProperties.Color;
            color.a *= Mathf.Min(alpha, arcGroupAlpha);
            instance.SetColor(color);

            if (hasBeenHitOnce || IsTrace)
            {
                instance.ClipTo(currentTiming, currentFloorPosition, currentTiming, currentFloorPosition);
            }
            else
            {
                instance.ClipTo(currentTiming, currentFloorPosition, Timing, FloorPosition);
            }

            if (Highlight && currentTiming >= Timing && currentTiming <= EndTiming)
            {
                Services.Particle.PlayLongParticle(
                    this,
                    new Vector3(WorldXAt(currentTiming), WorldYAt(currentTiming), 0));
            }
        }

        public void ProcessArcJudgement(int offset)
        {
            int currentTiming = Services.Audio.ChartTiming;
            judgementRequestSent = false;

            float x = WorldXAt(currentTiming);
            float y = WorldYAt(currentTiming);
            Vector3 currentPos = new Vector3(x, y);

            if (offset >= Values.HoldLostLateJudgeWindow)
            {
                SetGroupHighlight(int.MinValue);

                int missCount = UpdateCurrentJudgePointTiming(currentTiming - Values.HoldLostLateJudgeWindow);
                if (missCount > 0)
                {
                    Services.Score.ProcessJudgement(JudgementResult.LostLate, missCount);
                    Services.Particle.PlayTextParticle(currentPos, JudgementResult.LostLate);
                }

                RequestJudgement(currentTiming);
                judgementRequestSent = true;
                return;
            }

            if (currentTiming <= EndTiming)
            {
                int hitCount = UpdateCurrentJudgePointTiming(currentTiming + Values.FarJudgeWindow);
                if (hitCount > 0)
                {
                    Services.Score.ProcessJudgement(JudgementResult.Max, hitCount);
                    Services.Particle.PlayTextParticle(currentPos, JudgementResult.Max);
                }

                Services.Particle.PlayLongParticle(firstArcOfGroup, currentPos);
                SetGroupHighlight(currentTiming + Values.HoldHighlightPersistDuration);
                hasBeenHitOnce = true;

                RequestJudgement(currentTiming);
                judgementRequestSent = true;
            }
        }

        public int CompareTo(INote<ArcBehaviour> other)
        {
            LongNote note = other as LongNote;
            if (note.Timing == Timing)
            {
                return EndTiming.CompareTo(note.EndTiming);
            }

            return Timing.CompareTo(note.EndTiming);
        }

        public float WorldXAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return ArcFormula.ArcXToWorld(XStart);
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcXToWorld(ArcFormula.X(XStart, XEnd, p, LineType));
        }

        public float WorldYAt(int timing)
        {
            if (EndTiming == Timing)
            {
                return ArcFormula.ArcYToWorld(YStart);
            }

            float p = Mathf.Clamp((float)(timing - Timing) / (EndTiming - Timing), 0, 1);
            return ArcFormula.ArcYToWorld(ArcFormula.Y(YStart, YEnd, p, LineType));
        }

        private void RebuildSegments()
        {
            if (instance == null)
            {
                return;
            }

            instance.RebuildSegments();
        }

        private void RequestJudgement(int currentTiming)
        {
            Services.Judgement.Request(new ArcJudgementRequest()
            {
                ExpireAtTiming = currentTiming + Values.HoldLostLateJudgeWindow,
                AutoAtTiming = currentTiming,
                Arc = this,
                Receiver = this,
            });
        }

        private void SetGroupHighlight(int highlightUntil)
        {
            if (recursivelyCalled)
            {
                return;
            }

            recursivelyCalled = true;
            foreach (Arc arc in NextArcs)
            {
                arc.SetGroupHighlight(highlightUntil);
            }

            this.highlightUntil = highlightUntil;
            recursivelyCalled = false;
        }

        private void SetGroupFirst(Arc arc)
        {
            if (recursivelyCalled)
            {
                return;
            }

            recursivelyCalled = true;
            foreach (Arc next in NextArcs)
            {
                next.SetGroupFirst(arc);
            }

            firstArcOfGroup = arc;
            recursivelyCalled = false;
        }
    }
}
