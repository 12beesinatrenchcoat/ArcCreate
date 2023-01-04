using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Timeline
{
    public class TimelineService : MonoBehaviour, ITimelineService
    {
        [SerializeField] private WaveformDisplay waveformDisplay;

        [Header("Markers")]
        [SerializeField] private Marker timingMarker;
        [SerializeField] private Marker returnTimingMarker;

        [Header("Playback control")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pauseHighlight;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject playHighlight;
        [SerializeField] private Button playReturnButton;
        [SerializeField] private GameObject playReturnHighlight;
        [SerializeField] private Button stopButton;

        private bool shouldFocusWaveformView = false;
        private bool isWaveformDraggingThisFrame = true;

        public int ViewFromTiming => waveformDisplay.ViewFromTiming;

        public int ViewToTiming => waveformDisplay.ViewToTiming;

        private bool IsPlaying => Services.Gameplay?.Audio.IsPlaying ?? false;

        private void Awake()
        {
            timingMarker.OnValueChanged += OnTimingMarker;
            waveformDisplay.OnWaveformDrag += OnWaveformDrag;
            waveformDisplay.OnWaveformScroll += OnWaveformScroll;
            pauseButton.onClick.AddListener(Pause);
            playButton.onClick.AddListener(Play);
            playReturnButton.onClick.AddListener(PlayReturn);
            stopButton.onClick.AddListener(Stop);
        }

        private void OnDestroy()
        {
            timingMarker.OnValueChanged -= OnTimingMarker;
            waveformDisplay.OnWaveformDrag -= OnWaveformDrag;
            waveformDisplay.OnWaveformScroll -= OnWaveformScroll;
            pauseButton.onClick.RemoveListener(Pause);
            playButton.onClick.RemoveListener(Play);
            playReturnButton.onClick.RemoveListener(PlayReturn);
            stopButton.onClick.RemoveListener(Stop);
        }

        private void Update()
        {
            if (IsPlaying && !isWaveformDraggingThisFrame)
            {
                timingMarker.SetTiming(Services.Gameplay.Audio.Timing);

                if (shouldFocusWaveformView)
                {
                    waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.Timing / 1000f);
                }
            }

            isWaveformDraggingThisFrame = false;
        }

        private void Pause()
        {
            Services.Gameplay.Audio.Pause();
            timingMarker.SetTiming(Services.Gameplay.Audio.Timing);
            waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.Timing / 1000f);
            pauseHighlight.SetActive(true);
            playHighlight.SetActive(false);
            playReturnHighlight.SetActive(false);
        }

        private void Play()
        {
            if (!IsPlaying)
            {
                Services.Gameplay.Audio.ResumeImmediately();
            }
            else
            {
                Services.Gameplay.Audio.SetReturnOnPause(false);
            }

            pauseHighlight.SetActive(false);
            playHighlight.SetActive(true);
            playReturnHighlight.SetActive(false);
            shouldFocusWaveformView = true;
            returnTimingMarker.gameObject.SetActive(false);
        }

        private void PlayReturn()
        {
            if (!IsPlaying)
            {
                Services.Gameplay.Audio.ResumeReturnableImmediately();
            }
            else
            {
                Services.Gameplay.Audio.SetReturnOnPause(true, Services.Gameplay.Audio.Timing);
            }

            pauseHighlight.SetActive(false);
            playHighlight.SetActive(false);
            playReturnHighlight.SetActive(true);
            shouldFocusWaveformView = true;
            returnTimingMarker.gameObject.SetActive(true);
            returnTimingMarker.SetTiming(Services.Gameplay.Audio.Timing);
        }

        private void Stop()
        {
            Services.Gameplay.Audio.Stop();
            pauseHighlight.SetActive(true);
            playHighlight.SetActive(false);
            playReturnHighlight.SetActive(false);
            timingMarker.SetTiming(Services.Gameplay.Audio.Timing);
            waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.Timing / 1000f);
        }

        private void OnWaveformDrag(float x)
        {
            timingMarker.SetDragPosition(x);
            Services.Gameplay.Audio.Timing = timingMarker.Timing;
            Services.Gameplay.Audio.SetResumeAt(timingMarker.Timing);
            isWaveformDraggingThisFrame = true;
        }

        private void OnWaveformScroll()
        {
            shouldFocusWaveformView = false;
        }

        private void OnTimingMarker(Marker marker, int timing)
        {
            Services.Gameplay.Audio.Timing = timing;
            Services.Gameplay.Audio.SetResumeAt(timing);
            isWaveformDraggingThisFrame = true;
        }
    }
}