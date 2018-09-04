using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rivals
{
    public class RivalsTask
    {
        private class ExpansionTracker
        {
            Map map;
            HashSet<Point> visited;
            Queue<OwnedLocation>[] fronts;

            private static readonly List<Size> possibleDirections = new List<Size>
            {
                new Size(-1, 0),
                new Size(0, -1),
                new Size(+1, 0),
                new Size(0, +1)
            };

            private IEnumerable<Point> AvailablePositions(Point pos) =>
               possibleDirections.Select(offs => pos + offs)
                                 .Where(p => map.InBounds(p) &&
                                             map.Maze[p.X, p.Y] == MapCell.Empty);

            private IEnumerable<OwnedLocation> PlacePlayers()
            {
                for (var i = 0; i < map.Players.Count(); i++)
                {
                    var pos = map.Players[i];
                    var loc = new OwnedLocation(i, pos, 0);
                    fronts[i].Enqueue(loc);
                    visited.Add(pos);
                    yield return loc;
                }
            }

            private IEnumerable<OwnedLocation> ExpandFront(Queue<OwnedLocation> front)
            {
                for (var count = front.Count(); count > 0; --count)
                {
                    OwnedLocation settle = front.Dequeue();
                    foreach (var pos in AvailablePositions(settle.Location)
                                        .Where(p => !visited.Contains(p)))
                    {
                        var newLoc = new OwnedLocation(settle.Owner, pos, settle.Distance + 1);
                        front.Enqueue(newLoc);
                        visited.Add(newLoc.Location);
                        yield return newLoc;
                    }
                }
            }

            private bool CanSomeoneExpand() =>
                fronts.Where(x => x.Any()).Any();

            public ExpansionTracker(Map map)
            {
                this.map = map;
                visited = new HashSet<Point>();
                fronts = map.Players.Select(p => new Queue<OwnedLocation>()).ToArray();
            }

            public IEnumerable<OwnedLocation> TrackExpansion()
            {
                foreach (var s in PlacePlayers())
                    yield return s;

                while (CanSomeoneExpand())
                    foreach (var front in fronts)
                        foreach (var s in ExpandFront(front))
                            yield return s;
            }
        }

        public static IEnumerable<OwnedLocation> AssignOwners(Map map)
        {
            foreach (var s in new ExpansionTracker(map).TrackExpansion())
                yield return s;
        }
    }
}
