namespace ArcCreate.Gameplay.Judgement
{
    /// <summary>
    /// Interface for providing input judgement services to internal (Gameplay) classes.
    /// </summary>
    public interface IJudgementService
    {
        /// <summary>
        /// Gets the world y coordinate of the sky input.
        /// </summary>
        /// <value>The world y coordinate.</value>
        float SkyInputY { get; }

        /// <summary>
        /// Add a judgement request to be processed later.
        /// </summary>
        /// <param name="request">The request details.</param>
        void Request(LaneTapJudgementRequest request);

        /// <summary>
        /// Add a judgement request to be processed later.
        /// </summary>
        /// <param name="request">The request details.</param>
        void Request(LaneHoldJudgementRequest request);

        /// <summary>
        /// Add a judgement request to be processed later.
        /// </summary>
        /// <param name="request">The request details.</param>
        void Request(ArcJudgementRequest request);

        /// <summary>
        /// Add a judgement request to be processed later.
        /// </summary>
        /// <param name="request">The request details.</param>
        void Request(ArcTapJudgementRequest request);

        /// <summary>
        /// Start processing user input. Should be done after all notes requested for judgements as necessary.
        /// </summary>
        /// <param name="currentTiming">The current timing.</param>
        void ProcessInput(int currentTiming);

        /// <summary>
        /// Reset judgement state.
        /// </summary>
        void ResetJudge();
    }
}