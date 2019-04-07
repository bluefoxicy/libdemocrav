﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting
{
    public interface IBallot
    {
        IEnumerable<IVote> Votes { get; }
    }

    public interface IRankedBallot : IBallot
    {
        new IEnumerable<IRankedVote> Votes { get; }
    }

    public interface IVote
    {
        Candidate Candidate { get; }
    }

    public interface IRankedVote : IVote
    {
        /// <summary>
        /// The ordinal value, with lower indicating more preferred.
        /// </summary>
        int Value { get; }
        /// <summary>
        /// Check if this vote is ranked higher in preference to (vote).
        /// </summary>
        /// <param name="vote">The vote to compare.</param>
        /// <returns>true if this vote is ranked higher in preference.  false on tie or loss.</returns>
        bool Beats(IRankedVote vote);
    }

    /// <summary>
    /// A Vote object.  Allows placing a value on a vote.
    /// Immutable.
    /// </summary>
    public class RankedVote : IRankedVote, IEquatable<RankedVote>
    {
        public Candidate Candidate { get; }
        public int Value { get; }

        public RankedVote(Candidate candidate, int value)
        {
            Candidate = candidate;
            Value = value;
        }

        public virtual bool Beats(IRankedVote vote) => Value > vote.Value;

        public virtual bool Equals(RankedVote v)
        {
            if (v is null)
                return false;
            else if (ReferenceEquals(this, v))
                return true;
            return Candidate.Equals(v.Candidate) && Value.Equals(v.Value);
        }

        public override bool Equals(object obj) => Equals(obj as RankedVote);

        public override int GetHashCode() => HashCode.Combine(Candidate, Value);
    }

}
