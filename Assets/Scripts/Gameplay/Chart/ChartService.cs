using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class ChartService : MonoBehaviour, IChartService, IChartControl
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject tapPrefab;
        [SerializeField] private GameObject holdPrefab;
        [SerializeField] private GameObject arcPrefab;
        [SerializeField] private GameObject arcSegmentPrefab;
        [SerializeField] private GameObject arcTapPrefab;
        [SerializeField] private GameObject connectionLinePrefab;
        [SerializeField] private GameObject beatlinePrefab;
        [SerializeField] private GameObject timingGroupPrefab;

        [Header("Capacity")]
        [SerializeField] private int tapCapacity;
        [SerializeField] private int holdCapacity;
        [SerializeField] private int arcCapacity;
        [SerializeField] private int arcSegmentCapacity;
        [SerializeField] private int arcTapCapacity;
        [SerializeField] private int connectionLineCapacity;
        [SerializeField] private int beatlineCapacity;

        private readonly List<TimingGroup> timingGroups = new List<TimingGroup>();

        public int Timing
        {
            get => Services.Audio.Timing;
            set
            {
                Services.Audio.Timing = value;
                ResetJudge();
            }
        }

        public int ChartAudioOffset
        {
            get => Values.ChartAudioOffset;
            set
            {
                Values.ChartAudioOffset = value;
                ResetJudge();
            }
        }

        public float TimingPointDensityFactor
        {
            get => Values.TimingPointDensity;
            set
            {
                Values.TimingPointDensity = value;
                ResetJudge();
            }
        }

        public void ReloadSkin()
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.ReloadSkin();
            }
        }

        public void ResetJudge()
        {
            int currentCombo = 0;
            int timing = Services.Audio.Timing;
            int totalCombo = 0;
            InputMode inputMode = (InputMode)Settings.InputMode.Value;
            bool isAuto = inputMode == InputMode.Auto || inputMode == InputMode.AutoController;

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.ResetJudge();
                if (isAuto)
                {
                    currentCombo += tg.ComboAt(Services.Audio.Timing);
                }

                totalCombo += tg.TotalCombo();
            }

            Services.Score.ResetScoreTo(currentCombo, totalCombo);
            Services.Judgement.ClearRequests();
        }

        public IEnumerable<T> FindByTiming<T>(int timing)
            where T : ArcEvent
        {
            if (typeof(T) == typeof(ScenecontrolEvent))
            {
                foreach (var note in Services.Scenecontrol.FindByTiming(timing))
                {
                    yield return note as T;
                }
            }

            if (typeof(T) == typeof(CameraEvent))
            {
                foreach (var note in Services.Camera.FindByTiming(timing))
                {
                    yield return note as T;
                }
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.FindByTiming<T>(timing);
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<T> FindByEndTiming<T>(int endTiming)
            where T : LongNote
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.FindByEndTiming<T>(endTiming);
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<T> FindEventsWithinRange<T>(int from, int to)
            where T : ArcEvent
        {
            if (typeof(T) == typeof(ScenecontrolEvent))
            {
                foreach (var note in Services.Scenecontrol.FindWithinRange(from, to))
                {
                    yield return note as T;
                }
            }

            if (typeof(T) == typeof(CameraEvent))
            {
                foreach (var note in Services.Camera.FindWithinRange(from, to))
                {
                    yield return note as T;
                }
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.FindEventsWithinRange<T>(from, to);
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<T> GetAll<T>()
            where T : ArcEvent
        {
            if (typeof(T) == typeof(ScenecontrolEvent))
            {
                foreach (var note in Services.Scenecontrol.Events)
                {
                    yield return note as T;
                }
            }

            if (typeof(T) == typeof(CameraEvent))
            {
                foreach (var note in Services.Camera.Events)
                {
                    yield return note as T;
                }
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.GetEventType<T>();
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.Clear();
            }

            timingGroups.Clear();
        }

        public void LoadChart(ChartReader reader)
        {
            LoadChart(new ArcChart(reader));
            Timing = 0;
        }

        public void LoadChart(ArcChart chart)
        {
            Clear();

            Values.ChartAudioOffset = chart.AudioOffset;
            Values.TimingPointDensity = chart.TimingPointDensity;

            int i = 0;
            for (int j = 0; j < chart.TimingGroups.Count; j++)
            {
                ChartTimingGroup tg = chart.TimingGroups[j];
                GameObject go = Instantiate(timingGroupPrefab, transform);
                TimingGroup newTg = new TimingGroup(i);
                newTg.Load(tg, go.transform);
                timingGroups.Add(newTg);
            }

            Services.Camera.Load(chart.Cameras);
            Services.Scenecontrol.Load(chart.SceneControls);
            ResetJudge();
        }

        public void AddEvents(IEnumerable<ArcEvent> e)
        {
            foreach (var n in e)
            {
                if (n.TimingGroup >= timingGroups.Count)
                {
                    GetTimingGroup(n.TimingGroup);
                }
            }

            IEnumerable<CameraEvent> cameraEvents = e.Where(n => n is CameraEvent).Cast<CameraEvent>();
            IEnumerable<ScenecontrolEvent> scEvents = e.Where(n => n is ScenecontrolEvent).Cast<ScenecontrolEvent>();

            if (cameraEvents.Any())
            {
                Services.Camera.Add(cameraEvents);
            }

            if (scEvents.Any())
            {
                Services.Scenecontrol.Add(scEvents);
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.AddEvents(e.Where(n => n.TimingGroup == tg.GroupNumber));
            }
        }

        public void RemoveEvents(IEnumerable<ArcEvent> e)
        {
            IEnumerable<CameraEvent> cameraEvents = e.Where(n => n is CameraEvent).Cast<CameraEvent>();
            IEnumerable<ScenecontrolEvent> scEvents = e.Where(n => n is ScenecontrolEvent).Cast<ScenecontrolEvent>();

            if (cameraEvents.Any())
            {
                Services.Camera.Remove(cameraEvents);
            }

            if (scEvents.Any())
            {
                Services.Scenecontrol.Remove(scEvents);
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.RemoveEvents(e.Where(n => n.TimingGroup == tg.GroupNumber));
            }
        }

        public void UpdateEvents(IEnumerable<ArcEvent> e)
        {
            IEnumerable<CameraEvent> cameraEvents = e.Where(n => n is CameraEvent).Cast<CameraEvent>();
            IEnumerable<ScenecontrolEvent> scEvents = e.Where(n => n is ScenecontrolEvent).Cast<ScenecontrolEvent>();

            if (cameraEvents.Any())
            {
                Services.Camera.Change(cameraEvents);
            }

            if (scEvents.Any())
            {
                Services.Scenecontrol.Change(scEvents);
            }

            List<ArcEvent> tgChanged = e.Where(n => n.TimingGroupChanged).ToList();
            if (tgChanged.Count > 0)
            {
                tgChanged.Sort((a, b) => a.TimingGroupChangedFrom == b.TimingGroupChangedFrom
                                    ? a.TimingGroup.CompareTo(b.TimingGroup)
                                    : a.TimingGroupChangedFrom.CompareTo(b.TimingGroupChangedFrom));

                int from = tgChanged[0].TimingGroupChangedFrom;
                int to = tgChanged[0].TimingGroup;

                List<ArcEvent> currentTgChange = new List<ArcEvent>();
                for (int i = 0; i < tgChanged.Count; i++)
                {
                    ArcEvent n = tgChanged[i];
                    if (from == n.TimingGroupChangedFrom && to == n.TimingGroup)
                    {
                        currentTgChange.Add(n);
                    }
                    else
                    {
                        GetTimingGroup(from).RemoveEvents(currentTgChange);
                        GetTimingGroup(to).AddEvents(currentTgChange);
                        from = n.TimingGroupChangedFrom;
                        to = n.TimingGroup;
                        currentTgChange.Clear();
                        currentTgChange.Add(n);
                    }

                    n.ResetTimingGroupChangedFrom();
                }

                GetTimingGroup(from).RemoveEvents(currentTgChange);
                GetTimingGroup(to).AddEvents(currentTgChange);
            }

            List<ArcEvent> tgUnchanged = e.Where(n => !n.TimingGroupChanged).ToList();
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.UpdateEvents(tgUnchanged.Where(n => n.TimingGroup == tg.GroupNumber));
            }
        }

        public TimingGroup GetTimingGroup(int tg)
        {
            if (tg < 0)
            {
                return timingGroups[0];
            }

            while (tg >= timingGroups.Count)
            {
                TimingGroup newTg = new TimingGroup(timingGroups.Count);
                newTg.Load(transform);
                timingGroups.Add(newTg);
            }

            return timingGroups[tg];
        }

        public void UpdateChart(int currentTiming)
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.UpdateGroup(currentTiming);
            }
        }

        private void Awake()
        {
            Pools.New<TapBehaviour>(Values.TapPoolName, tapPrefab, transform, tapCapacity);
            Pools.New<HoldBehaviour>(Values.HoldPoolName, holdPrefab, transform, holdCapacity);
            Pools.New<ArcBehaviour>(Values.ArcPoolName, arcPrefab, transform, arcCapacity);
            Pools.New<ArcTapBehaviour>(Values.ArcTapPoolName, arcTapPrefab, transform, arcTapCapacity);
            Pools.New<ArcSegment>(Values.ArcSegmentPoolName, arcSegmentPrefab, transform, arcSegmentCapacity);
            Pools.New<LineRenderer>(Values.ConnectonLinePoolName, connectionLinePrefab, transform, connectionLineCapacity);
            Pools.New<Transform>(Values.BeatlinePoolName, beatlinePrefab, transform, beatlineCapacity);

            Settings.GlobalAudioOffset.OnValueChanged.AddListener(OnGlobalOffsetChange);
        }

        private void OnDestroy()
        {
            Pools.Destroy<TapBehaviour>(Values.TapPoolName);
            Pools.Destroy<HoldBehaviour>(Values.HoldPoolName);
            Pools.Destroy<ArcBehaviour>(Values.ArcPoolName);
            Pools.Destroy<ArcTapBehaviour>(Values.ArcTapPoolName);
            Pools.Destroy<ArcSegment>(Values.ArcSegmentPoolName);
            Pools.Destroy<LineRenderer>(Values.ConnectonLinePoolName);
            Pools.Destroy<Transform>(Values.BeatlinePoolName);

            Settings.GlobalAudioOffset.OnValueChanged.RemoveListener(OnGlobalOffsetChange);
        }

        private void OnGlobalOffsetChange(int offset)
        {
            ResetJudge();
        }
    }
}