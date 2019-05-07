﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Tiebreaking;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulation
{

    public class MeekVoteCount : AbstractRankedVoteCount
    {
        private decimal quota = 0.0m;
        private decimal surplus = 0.0m;
        private readonly int precision = 9;
        // Round up to precision
        private decimal RoundUp(decimal d)
        {
            // The reference rule says to round UP, so we need
            // to Round(weight + 10^(-precision) / 2).
            decimal r = 0.5m * Convert.ToDecimal(Math.Pow(10.0D,
                Convert.ToDouble(0 - precision)));
            return decimal.Round(d + r, precision);
        }

        // batchEliminator should be new RunoffBatchEliminator(tiebreaker, seats)
        public MeekVoteCount(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots,
            IBatchEliminator batchEliminator, int seats = 1, int precision = 9)
            : base(candidates, ballots, batchEliminator, seats)
        {
            this.precision = precision;
        }

        /// <inheritdoc/>
        protected override void InitializeCandidateStates(IEnumerable<Candidate> candidates)
        {
            // As per reference rule A
            foreach (Candidate c in candidates)
            {
                MeekCandidateState cs = new MeekCandidateState
                {
                    KeepFactor = 1,
                    State = CandidateState.States.hopeful
                };
                candidateStates[c] = cs;
            }
        }

        /// <inheritdoc/>
        public override void CountBallots()
        {
            // The s>=surplus check is skipped the first iteration.
            // We implement this by having surplus greater than the
            // number of whole ballots.
            surplus = Convert.ToDecimal(ballots.Count) + 1;
            const decimal omega = 0.000001m;
            IEnumerable<Candidate> elected;

            // B.1 skip all this if we're finished.
            if (CheckComplete())
                return;

            do
            {
                decimal s = surplus;
                bool kfStasis;

                DistributeVotes();
                quota = ComputeQuota();
                elected = GetWinners(quota);
                surplus = ComputeSurplus(quota);
                // B.2.e Counting continues at B1, next tabulation round.
                if (elected.Count() > 0)
                    break;
                // B.2.e Iteration complete, no election, go to B.3
                else if (surplus >= s || omega > surplus)
                    break;
                // B.2.f Update keep factors.  Also detect stasis.
                kfStasis = UpdateKeepFactors(quota);
                // Upon Stasis, we must eliminate candidates (B.3)
                // We know:
                //   - Elected < Seats
                //   - Elected + Hopeful > Seats
                // Therefor IsComplete() is false and will not change state.
                // Re-entering this body is guaranteed to return to this state.
                // Elimination is required.
                if (kfStasis)
                    break;
            } while (true);

            // Update our tiebreaker algorithms
            batchEliminator.UpdateTiebreaker(candidateStates);
        }

        /// <inheritdoc/>
        public override Dictionary<Candidate, CandidateState.States> GetTabulation()
        {
            List<Candidate> winners = null;
            if (!CheckComplete())
                winners = GetWinners(quota).ToList();
            else
            {
                CandidateState.States s = CandidateState.States.elected;
                // Reference rule step C
                // Count number of electeds
                Dictionary<Candidate, CandidateState.States> elected =
                    candidateStates.Where(x => x.Value.State == CandidateState.States.elected)
                    .ToDictionary(x => x.Key, x => s);
                // If we've filled all seats, defeat everyone else; else elect them all.
                if (elected.Count() == seats)
                    s = CandidateState.States.defeated;

                elected = candidateStates.Where(x => x.Value.State == CandidateState.States.hopeful)
                        .ToDictionary(x => x.Key, x => s);

                return elected;
            }

            if (winners.Count() > 0)
            {
                return candidateStates
                    .Where(x => winners.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => CandidateState.States.elected);
            }
            return batchEliminator.GetEliminationCandidates(candidateStates, surplus)
              .ToDictionary(x => x, x => CandidateState.States.defeated);
        }

        /// <inheritdoc/>
        protected override void SetStates(Dictionary<Candidate, CandidateState.States> candidates)
        {
            foreach (Candidate c in candidates.Keys)
            {
                if (!candidateStates.ContainsKey(c))
                    throw new ArgumentOutOfRangeException();
                MeekCandidateState cs = candidateStates[c] as MeekCandidateState;
                if (cs is null)
                    throw new InvalidOperationException("candidateState contains objects that aren't of type MeekCandidateState");
                cs.State = candidates[c];
                // Set KeepFactor to 0 for losers as per B.3
                if (cs.State == CandidateState.States.defeated)
                    cs.KeepFactor = 0.0m;
            }
        }

        public override decimal GetVoteCount(Candidate candidate)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<Candidate, decimal> GetVoteCounts()
        {
            throw new NotImplementedException();
        }

        // Reference rule B.1
        private bool CheckComplete()
        {
            int e = 0, h = 0;
            foreach (CandidateState c in candidateStates.Values)
            {
                if (c.State == CandidateState.States.elected)
                    e++;
                else if (c.State == CandidateState.States.hopeful)
                    h++;
            }
            // We're done counting.  Go to reference rule step C.
            if (e + h <= seats)
                return true;
            // Still not done.
            return false;
        }

        // Reference rule B.2.a Distribute Votes
        private void DistributeVotes()
        {
            // Zero the vote counts
            foreach (CandidateState c in candidateStates.Values)
                c.VoteCount = 0.0m;

            // Distribute the ballots among the votes.
            // Meek also considered an alternative formulation in which
            // voters would be allowed to indicate equal preference for
            // some candidates instead of a strict ordering; we have not
            // implemented this alternative.
            foreach (IRankedBallot b in ballots)
            {
                decimal weight = 1.0m;
                List<IRankedVote> votes = new List<IRankedVote>(b.Votes);
                votes.Sort();
                foreach (IRankedVote v in votes)
                {
                    MeekCandidateState cs = candidateStates[v.Candidate] as MeekCandidateState;
                    if (cs is null)
                        throw new InvalidOperationException("candidateState contains objects that aren't of type MeekCandidateState");
                    // Get value to transfer to this candidate, restricted to
                    // the specified precision
                    decimal value = weight * cs.KeepFactor;
                    weight = RoundUp(weight);

                    // Add this to the candidate's vote and remove from ballot's
                    //weight
                    cs.VoteCount += value;
                    weight -= value;

                    // Do this until weight hits zero, or we run out of rankings.
                    // Note:  Already-elected candidates have keep factors between
                    // 0 and 1; hopefuls all have 1; defeated will have 0.  The
                    // remaining voting power is either all transfered to the first
                    // hopeful candidate or exhausted as the ballot runs out of
                    // non-defeated candidates.
                    //
                    // We only hit 0 if a non-elected hopeful is on the ballot.
                    if (weight <= 0.0m)
                        break;
                }
            }
        }

        // Reference rule B.2.b
        private decimal ComputeQuota()
        {
            decimal p = Convert.ToDecimal(Math.Pow(10.0D, Convert.ToDouble(0 - precision)));
            decimal q = candidateStates.Sum(x => x.Value.VoteCount);
            q /= seats + 1;
            // truncate and add the precision digit
            q = decimal.Round(q - 0.5m * p) + p;
            return q;
        }

        // Reference rule B.2.c, returns null if no hopefuls get elected.
        private IEnumerable<Candidate> GetWinners(decimal quota)
        {
            List<Candidate> elected = new List<Candidate>();
            Dictionary<Candidate, decimal> hopefuls = candidateStates
                    .Where(x => x.Value.State == CandidateState.States.hopeful)
                    .ToDictionary(x => x.Key, x => x.Value.VoteCount);
            foreach (Candidate c in hopefuls.Keys)
            {
                // Elect hopefuls who made it
                if (hopefuls[c] >= quota)
                    elected.Add(c);
            }
            return elected;
        }

        // B.2.d compute surplus of elected candidates
        private decimal ComputeSurplus(decimal quota)
        {
            decimal s = 0.0m;
            // XXX:  does the reference rule mean total surplus not less
            // than zero, or only count candidates whose surplus is greater
            // than zero?
            foreach (Candidate c in candidateStates.Keys)
            {
                if (candidateStates[c].State == CandidateState.States.elected)
                    s += candidateStates[c].VoteCount - quota;
            }
            if (s < 0.0m)
                s = 0.0m;
            return s;
        }

        // B.2.f Update keep factors.  Returns true if stable state detected.
        private bool UpdateKeepFactors(decimal quota)
        {
            // If no KeepFactors change or if any Keepfactor increases,
            // we have a stable state and are in stasis.
            bool noChange = true;
            bool increase = false;
            foreach (MeekCandidateState c in candidateStates.Values)
            {
                decimal kf = c.KeepFactor;
                // XXX: does the reference rule intend we round up
                // between each operation, or multiply by (q/v) and
                // then round up?
                c.KeepFactor *= quota / c.VoteCount;
                c.KeepFactor = RoundUp(c.KeepFactor);
                if (kf != c.KeepFactor)
                    noChange = false;
                if (c.KeepFactor > kf)
                    increase = true;
            }

            // Stasis detect
            return (noChange || increase);
        }
    }
}
