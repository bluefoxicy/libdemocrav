﻿using MoonsetTechnologies.Voting.Tabulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    /// <summary>
    /// Mediates tabulation state messages between tabulators, tiebreakers,
    /// and related objects.
    /// </summary>
    public class TabulationMediator : TabulationMonitor
    {
        /// <summary>
        /// Creates Vote, Ballot, Candidate, and BallotSet objects shared by the tabulation objects.
        /// </summary>
        public BallotFactory BallotFactory { get; set; }

        /// <summary>
        /// Inform listeners of a tabulation beginning.
        /// </summary>
        /// <param name="tabulationDetails">Details of the tabulation.</param>
        public new void BeginTabulation(TabulationDetailsEventArgs tabulationDetails)
          => base.BeginTabulation(tabulationDetails);

        /// <summary>
        /// Inform listeners of a completed tabulation round.
        /// </summary>
        /// <param name="candidateStates">The current candidate states at the time of update.</param>
        /// <param name="note">A note to attach to the message.</param>
        public new void CompleteRound(Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
          => base.CompleteRound(candidateStates, note);

        /// <summary>
        /// Inform listeners of a tiebreaker state change.
        /// </summary>
        /// <param name="winPairs">The new pairs of candidates who will win a tiebreaker.</param>
        /// <param name="note">A note to attach to the message.</param>
        public new void ChangeTiebreakerState(Dictionary<Candidate, Dictionary<Candidate, bool>> winPairs,
            string note = null)
        {
            base.ChangeTiebreakerState(winPairs, note);
        }

        /// <summary>
        /// Inform listeners of a completed tabulation.
        /// </summary>
        /// <param name="candidateStates">The current candidate states at the time of update.</param>
        /// <param name="note">A note to attach to the message.</param>
        public new void CompleteTabulation(Dictionary<Candidate, CandidateState> candidateStates,
            string note = null)
          => base.CompleteTabulation(candidateStates, note);

        /// <summary>
        /// Inform listeners of a completed tabulation round.
        /// </summary>
        /// <param name="tabulationState">The current tabulation state at the time of update.</param>
        public new void CompleteRound(TabulationStateEventArgs tabulationState)
          => base.CompleteRound(tabulationState);
    }
}
