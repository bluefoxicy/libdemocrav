﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoonsetTechnologies.Voting.Analytics;

namespace MoonsetTechnologies.Voting.Tabulators
{
    /// <summary>
    /// Computes the Smith and Schwartz sets.
    /// </summary>
    class TopCycle
    {
        protected List<Candidate> smithSet;
        protected List<Candidate> schwartzSet;
        public IEnumerator<Candidate> SmithSet => smithSet.GetEnumerator();
        public IEnumerator<Candidate> SchwartzSet => schwartzSet.GetEnumerator();

        public TopCycle(IEnumerable<Candidate> candidates, IEnumerable<IRankedBallot> ballots)
            : this(new PairwiseGraph(candidates, ballots))
        {
            
        }

        public TopCycle(PairwiseGraph graph)
        {
            ComputeSets(graph);
        }

        /// <summary>
        /// Compute Smith and Schwartz sets with Tarjan's Algorithm.
        /// </summary>
        /// <param name="graph">The pairwise graph.</param>
        private void ComputeSets(PairwiseGraph graph)
        {
            Dictionary<Candidate, int> linkId = new Dictionary<Candidate, int>();
            Dictionary<Candidate, int> nodeId = new Dictionary<Candidate, int>();
            Stack<Candidate> s = new Stack<Candidate>();
            int i = 0;

            void dfs(Candidate c, bool isSmith)
            {
                // Skip this node when isSmith is false and the node is not in the
                // Smith Set, of which the Schwartz Set is a subset.
                if (!isSmith && !smithSet.Contains(c))
                // Only search if not yet visited.
                if (nodeId.ContainsKey(c))
                    return;
                // put onto the stack
                s.Push(c);
                // Set the node's linkId and nodeId, then increment i
                nodeId[c] = i;
                linkId[c] = i;
                i++;

                // Visit each neighbor
                List<Candidate> neighbors = new List<Candidate>(graph.Wins(c));
                if (isSmith)
                    neighbors.AddRange(graph.Ties(c));
                foreach (Candidate d in graph.Wins(c))
                {
                    // Visit first so it will be on the stack when we do the next check,
                    // unless it's already visited and thus won't be on the stack.
                    dfs(d, isSmith);
                    // It's on the stack, so set linkId to the low-link value
                    if (s.Contains(d))
                        linkId[c] = Math.Min(linkId[c], linkId[d]);
                }
                // We've visited all neighbors, did we find a SCC?
                if (linkId[c] == nodeId[c])
                {
                    // Remove all associated members of the SCC
                    while (linkId[s.Peek()] == linkId[c])
                        s.Pop();
                }
            }

            List<Candidate> getSet(bool isSmith)
            {
                // Visit each node in the graph as a starting point.
                foreach (Candidate c in graph.Candidates)
                    dfs(c, false);

                // Find every SCC that cannot be reached by any other SCC.
                // In the Smith Set, this is one SCC; in the Schwartz Set,
                // we may have several.
                Dictionary<(int, int), bool> reachable = new Dictionary<(int, int), bool>();
                List<int> dominating = new List<int>();
                List<Candidate> output = new List<Candidate>();

                // Special thanks to https://stackoverflow.com/a/55526085/5601193
                foreach (Candidate k in linkId.Keys)
                {
                    foreach(Candidate l in linkId.Keys)
                    {
                        foreach (Candidate m in linkId.Keys)
                        {
                            // The SCC containing (l) can reach the SCC containing (m) if
                            //  - (l) defeats (m)
                            //  - (l) ties with (m) and it's the Smith Set
                            //  - (l) is already known to reach (m)
                            //  - (l) can reach (k) and (k) can reach (m)
                            reachable[(linkId[l], linkId[m])] =
                                graph.Wins(l).Contains(m) ||
                                (isSmith && graph.Ties(l).Contains(m)) ||
                                reachable[(linkId[l], linkId[m])] ||
                                (reachable[(linkId[l], linkId[k])] && reachable[(linkId[k], linkId[m])]);
                        }
                    }
                }

                // Time to find all dominating SCCs
                dominating.AddRange(linkId.Values.Distinct());
                foreach (int j in linkId.Values.Distinct())
                {
                    foreach (int k in linkId.Values.Distinct())
                    {
                        // Reaching itself doesn't count
                        if (j == k)
                            continue;
                        // If we can reach j from k, j is not dominating
                        if (reachable[(k, j)])
                            dominating.Remove(j);
                    }
                }

                // Select all the candidates from the SCCs
                foreach (Candidate c in linkId.Keys)
                {
                    if (dominating.Contains(linkId[c]))
                        output.Add(c);
                }

                return output;
            }

            // Must compute SmithSet first
            smithSet = getSet(true);
            schwartzSet = getSet(false);
        }
    }
}
