﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Ballots;
using MoonsetTechnologies.Voting.Utility;

namespace MoonsetTechnologies.Voting.Tabulation
{
    public abstract class AbstractSingleTransferableVoteTabulator : AbstractTabulator
    {
        public AbstractSingleTransferableVoteTabulator(TabulationMediator mediator,
            IEnumerable<ITabulatorSetting> tabulatorSettings)
            : base(mediator, tabulatorSettings)
        {
        }
    }
}
