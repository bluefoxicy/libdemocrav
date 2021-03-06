using MoonsetTechnologies.Voting.Analytics;
using MoonsetTechnologies.Voting.Tabulation;
using MoonsetTechnologies.Voting.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonsetTechnologies.Voting;
using MoonsetTechnologies.Voting.Ballots;

namespace MoonsetTechnologies.Voting.Utility
{ 
    // General algorithm:
    //   if CondorcetSet is One Candidate
    //     Winner is Candidate in CondorcetSet
    //   else
    //     Eliminate Candidates not in RetainSet
    //     Eliminate Candidate with Fewest Votes

    // Variants as (CondorcetSet, RetainSet):
    //   Tideman's Alternative:  (schwartz, smith)
    //   Tideman's Alternative Schwartz:  (schwartz, schwartz)
    //   Tideman's Alternative Smith:  (smith, smith)
    public class TidemansAlternativeTabulatorFactory : AbstractTabulatorFactory<TidemansAlternativeTabulator>
    {
        protected TopCycle.TopCycleSets condorcetSet = TopCycle.TopCycleSets.schwartz;
        protected TopCycle.TopCycleSets retainSet = TopCycle.TopCycleSets.smith;

        public TidemansAlternativeTabulatorFactory()
            : base()
        {
        }
    }

    public class TidemansAlternativeSmithTabulatorFactory : TidemansAlternativeTabulatorFactory
    {
        public TidemansAlternativeSmithTabulatorFactory() : base()
        {
            condorcetSet = retainSet = TopCycle.TopCycleSets.smith;
        }
    }

    public class TidemansAlternativeSchwartzTabulatorFactory : TidemansAlternativeTabulatorFactory
    {
        public TidemansAlternativeSchwartzTabulatorFactory() : base()
        {
            condorcetSet = retainSet = TopCycle.TopCycleSets.schwartz;
        }
    }

}