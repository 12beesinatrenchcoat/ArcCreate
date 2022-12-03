﻿using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;

namespace ArcCreate.Gameplay.Data
{
    public class ArcChart
    {
        public ArcChart(ChartReader reader)
        {
            AudioOffset = reader.AudioOffset;
            TotalTimingGroups = reader.TimingGroups.Count;
            TimingPointDensity = reader.TimingPointDensity;

            TimingGroups = reader.TimingGroups
                .Select(prop => new ChartTimingGroup { Properties = prop })
                .ToList();

            foreach (RawEvent e in reader.Events)
            {
                switch (e.Type)
                {
                    case RawEventType.Timing:
                        var timing = e as RawTiming;
                        TimingGroups[timing.TimingGroup].Timings.Add(
                            new TimingEvent()
                            {
                                TimingGroup = timing.TimingGroup,
                                Timing = timing.Timing,
                                Divisor = timing.Divisor,
                                Bpm = timing.Bpm,
                            });
                        break;

                    case RawEventType.Tap:
                        var tap = e as RawTap;
                        TimingGroups[tap.TimingGroup].Taps.Add(
                            new Tap()
                            {
                                TimingGroup = tap.TimingGroup,
                                Timing = tap.Timing,
                                Lane = tap.Lane,
                            });
                        break;

                    case RawEventType.Hold:
                        var hold = e as RawHold;
                        TimingGroups[hold.TimingGroup].Holds.Add(
                            new Hold()
                            {
                                TimingGroup = hold.TimingGroup,
                                EndTiming = hold.EndTiming,
                                Timing = hold.Timing,
                                Lane = hold.Lane,
                            });
                        break;

                    case RawEventType.Arc:
                        var raw = e as RawArc;
                        Arc arc = new Arc()
                        {
                            TimingGroup = raw.TimingGroup,
                            Color = raw.Color,
                            EndTiming = raw.EndTiming,
                            IsVoid = raw.IsTrace,
                            LineType = raw.LineType.ToArcLineType(),
                            Timing = raw.Timing,
                            XEnd = raw.XEnd,
                            XStart = raw.XStart,
                            YEnd = raw.YEnd,
                            YStart = raw.YStart,
                            Sfx = raw.Sfx,
                        };

                        if (raw.ArcTaps != null)
                        {
                            arc.IsVoid = true;
                            foreach (int t in raw.ArcTaps)
                            {
                                ArcTap arctap = new ArcTap() { Timing = t, Arc = arc };
                                arc.ArcTaps.Add(arctap);
                                TimingGroups[raw.TimingGroup].ArcTaps.Add(arctap);
                            }
                        }

                        TimingGroups[raw.TimingGroup].Arcs.Add(arc);
                        break;

                    case RawEventType.Camera:
                        var camera = e as RawCamera;
                        Cameras.Add(
                            new CameraEvent
                            {
                                TimingGroup = camera.TimingGroup,
                                Timing = camera.Timing,
                                Move = camera.Move,
                                Rotate = camera.Rotate,
                                CameraType = camera.CameraType.ToCameraType(),
                                Duration = camera.Duration,
                            });
                        break;

                    case RawEventType.SceneControl:
                        var sc = e as RawSceneControl;
                        SceneControls.Add(
                            new ScenecontrolEvent()
                            {
                                Timing = sc.Timing,
                                TimingGroup = sc.TimingGroup,
                                Typename = sc.SceneControlTypeName,
                                Arguments = sc.Arguments,
                            });
                        break;
                }
            }

            if (reader.Events.Count != 0)
            {
                LastEventTiming = reader.Events.Last().Timing;
            }
        }

        public int AudioOffset { get; private set; }

        public float TimingPointDensity { get; private set; }

        public int TotalTimingGroups { get; private set; }

        public int LastEventTiming { get; private set; } = 0;

        public List<ChartTimingGroup> TimingGroups { get; private set; }

        public List<CameraEvent> Cameras { get; private set; } = new List<CameraEvent>();

        public List<ScenecontrolEvent> SceneControls { get; private set; } = new List<ScenecontrolEvent>();
    }
}
