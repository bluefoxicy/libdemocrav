﻿using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public abstract class AbstractTiebreaker
    {
        private TabulationMediator mediator;
        protected BallotSet Ballots { get; set; }
        protected int Seats { get; set; }
        /// <summary>
        /// Fallback tiebreaker
        /// </summary>
        protected AbstractTiebreaker Tiebreaker { get; set; }
        /// <summary>
        /// The TabulationMediator used to coordinate with the tabulator and any TabulationMonitor clients.
        /// </summary>
        public TabulationMediator Mediator
        {
            protected get => mediator;
            set
            {
                if (!(mediator is null))
                {
                    mediator.RoundComplete -= HandleRoundComplete;
                    mediator.TabulationBegin -= HandleTabulationBegin;
                }
                mediator = value;
                mediator.RoundComplete += HandleRoundComplete;
                mediator.TabulationBegin += HandleTabulationBegin;
            }
        }
        /// <summary>
        /// True if batch elimination will not affect tiebreakers occurring after batch elimination.
        /// </summary>
        public abstract bool FullyInformed { get; }

        /// <summary>
        /// Identify the top candidate in a tie.
        /// </summary>
        /// <param name="candidates">The tied candidates.</param>
        /// <param name="ballotWeights">Weights of each ballot, or null if ballots are unweighted.</param>
        /// <returns>The ultimate winner of the tie.</returns>
        public Candidate GetTieWinner(IEnumerable<Candidate> candidates,
            Dictionary<Ballot, decimal> ballotWeights = null)
          => BreakTie(candidates, ballotWeights, true);

        /// <summary>
        /// Identify the bottom candidate in a tie.
        /// </summary>
        /// <param name="candidates">The tied candidates.</param>
        /// <param name="ballotWeights">Weights of each ballot, or null if ballots are unweighted.</param>
        /// <returns>The ultimate loser of the tie.</returns>
        public Candidate GetTieLoser(IEnumerable<Candidate> candidates,
            Dictionary<Ballot, decimal> ballotWeights = null)
          => BreakTie(candidates, ballotWeights, false);

        /// <summary>
        /// Breaks a tie.
        /// </summary>
        /// <param name="candidates">The tied candidates.</param>
        /// <param name="ballotWeights">Weights of each ballot, or null if ballots are unweighted.</param>
        /// <param name="findWinner">True if picking a winner, false if searching for exclusion.</param>
        /// <returns>Winner or loser.</returns>
        protected abstract Candidate BreakTie(IEnumerable<Candidate> candidates,
            Dictionary<Ballot, decimal> ballotWeights,
            bool findWinner);
        /// <summary>
        /// Called when the current vote counts are submitted for updating the tiebreaker.
        /// </summary>
        /// <param name="candidateStates">Candidate states for the tiebreaker</param>
        protected abstract void UpdateTiebreaker(Dictionary<Candidate, CandidateState> candidateStates);

        // Event handler for tiebreaker update event
        private void HandleRoundComplete(object sender, TabulationStateEventArgs e)
        {
            UpdateTiebreaker(e.CandidateStates);

            // FIXME:  Send an event reporting the state of the tiebreaker
        }

        // Event handler for tiebreaker update event
        private void HandleTabulationBegin(object sender, TabulationDetailsEventArgs e)
        {
            Ballots = e.Ballots;
            Seats = e.Seats;
            // FIXME:  Send an event reporting the state of the tiebreaker
        }

        public AbstractTiebreaker(AbstractTiebreaker tiebreaker = null)
        {
            Tiebreaker = tiebreaker;
        }

        public AbstractTiebreaker() : this(null)
        {

        }
    }
}
