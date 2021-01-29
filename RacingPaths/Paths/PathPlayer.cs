using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace avaness.RacingPaths.Paths
{
    public class PathPlayer
    {
        private readonly List<Path> paths = new List<Path>();
        
        public void Update()
        {
            for (int i = paths.Count - 1; i >= 0; i--)
            {
                Path p = paths[i];
                if (p.IsEmpty)
                {
                    paths.RemoveAtFast(i);
                }
                else
                {
                    if (!p.Play())
                        paths.RemoveAtFast(i);
                }
            }
        }

        public void Play(params Path[] paths)
        {
            Play((IEnumerable<Path>)paths);
        }

        public void Play(IEnumerable<Path> paths)
        {
            this.paths.Clear();
            this.paths.AddRange(paths);
        }

        public void Clear()
        {
            paths.Clear();
        }
    }
}
