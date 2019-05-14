﻿using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Utility
{
    public class InstantRunoffVotingTabulatorFactory : AbstractTabulatorFactory
    {
        public InstantRunoffVotingTabulatorFactory()
        : base()
        {
            //SetTiebreaker(new TiebreakerFactory<LastDifferenceTiebreaker>());
        }

        public override Ballot CreateBallot(IEnumerable<Vote> votes)
        {
            throw new NotImplementedException();
        }

        public override AbstractTabulator CreateTabulator()
        {
            TabulationMediator mediator = new TabulationMediator();

            InstantRunoffVotingTabulator t = new InstantRunoffVotingTabulator(mediator, tiebreakerFactory);
            return t;
        }

        public override Vote CreateVote(Candidate candidate, decimal value)
        {
            throw new NotImplementedException();
        }
    }
}
