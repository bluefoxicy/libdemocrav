﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MoonsetTechnologies.Voting.Tiebreaking
{
    public interface IAbstractTiebreakerMetadata
    {
        String Algorithm { get; }
        Type Factory { get; }
        String Description { get; }
    }
}
