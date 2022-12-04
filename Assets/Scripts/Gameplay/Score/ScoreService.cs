using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Utility;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Score
{
    public class ScoreService : MonoBehaviour, IScoreService
    {
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Color comboLostColor;
        private readonly char[] scoreCharArray = new char[64];
        private readonly char[] comboCharArray = new char[64];

        private int currentCombo = 0;
        private int totalCombo = 1;
        private float currentScoreFull = 0;
        private float currentScorePartial = 0;
        private float comboRedmix = 0;
        private readonly UnorderedList<ScoreEvent> pendingScoreEvents = new UnorderedList<ScoreEvent>(20);

        private int CurrentScoreTotal => Mathf.RoundToInt(currentScoreFull + currentScorePartial);

        public void ProcessJudgement(JudgementResult result)
        {
            if (result.IsLost())
            {
                currentCombo = 0;
                if (Mathf.Approximately(comboRedmix, 0))
                {
                    comboRedmix = 1;
                }

                return;
            }

            comboRedmix = 0;
            currentCombo++;

            float scorePerNote =
                totalCombo != 0 ? (float)Values.MaxScore / totalCombo : 0;

            float scoreToAdd = 0;
            if (result.IsFar())
            {
                scoreToAdd = scorePerNote * Values.FarPenaltyMultipler;
            }
            else if (result.IsMax())
            {
                scoreToAdd = scorePerNote + 1;
            }
            else
            {
                scoreToAdd = scorePerNote;
            }

            pendingScoreEvents.Add(new ScoreEvent()
            {
                Timing = Services.Audio.Timing,
                Score = scoreToAdd,
            });
        }

        public void UpdateDisplay(int currentTiming)
        {
            comboRedmix = comboRedmix - (Time.deltaTime / Values.ComboLostFlashDuration);
            comboRedmix = Mathf.Max(comboRedmix, 0);
            SetCombo(currentCombo);

            currentScorePartial = 0;
            for (int i = pendingScoreEvents.Count - 1; i >= 0; i--)
            {
                ScoreEvent scoreEvent = pendingScoreEvents[i];

                if (currentTiming > scoreEvent.Timing + Values.ScoreModifyDelay)
                {
                    pendingScoreEvents.RemoveAt(i);
                    currentScoreFull += scoreEvent.Score;
                }
                else
                {
                    float partial = scoreEvent.Score * (currentTiming - scoreEvent.Timing) / (float)Values.ScoreModifyDelay;
                    currentScorePartial += partial;
                }
            }

            SetScore(CurrentScoreTotal);
        }

        public void ResetScoreTo(int currentCombo, int totalCombo)
        {
            this.currentCombo = currentCombo;
            this.totalCombo = totalCombo;
            SetCombo(currentCombo);

            pendingScoreEvents.Clear();
            currentScorePartial = 0;

            if (totalCombo == 0)
            {
                currentScoreFull = 0;
                SetScore(0);
            }
            else
            {
                float scorePerNote = (float)Values.MaxScore / totalCombo;
                currentScoreFull = scorePerNote * currentCombo;
            }

            SetScore(CurrentScoreTotal);
        }

        private void SetArray(char[] array, int number, out int length)
        {
            bool isNegative = number < 0;
            number = isNegative ? -number : number;

            int i = array.Length - 1;
            length = 0;
            while (i >= 0 && number > 0)
            {
                array[i] = (char)('0' + (number % 10));
                number /= 10;
                length++;
                i--;
            }

            if (isNegative)
            {
                if (i > 0)
                {
                    length++;
                }
                else
                {
                    i = 0;
                }

                array[i] = '-';
            }
        }

        private void SetScore(int score)
        {
            SetArray(scoreCharArray, score, out int length);
            for (int i = scoreCharArray.Length - 8; i < scoreCharArray.Length - length; i++)
            {
                scoreCharArray[i] = '0';
            }

            length = Mathf.Max(length, 8);
            scoreText.SetCharArray(scoreCharArray, scoreCharArray.Length - length, length);
        }

        private void SetCombo(int combo)
        {
            if (Mathf.Approximately(comboRedmix, 0))
            {
                comboText.color = Services.Skin.ComboColor;
                comboText.outlineColor = Services.Skin.ComboColor;

                SetArray(comboCharArray, combo, out int length);
                comboText.SetCharArray(comboCharArray, comboCharArray.Length - length, length);
            }
            else
            {
                Color comboColor = comboLostColor;
                comboColor.a = comboRedmix;
                comboText.color = comboColor;
                comboText.outlineColor = comboColor;
            }
        }
    }
}