namespace ZombieLand.Environment
{
    /// <summary>
    /// Hard-coded ASCII map. Strings are read top-to-bottom; the first
    /// row corresponds to the highest Z value in the world.
    /// Legend:
    ///   '#' wall
    ///   '.' floor
    ///   'P' player spawn  (one)
    ///   'E' exit portal   (one)
    ///   'F' memory fragment spawn
    ///   'Z' zombie spawn
    /// All walls are isolated 2x2 pillars surrounded by floor, so the
    /// arena is always one fully connected walkable region.
    /// </summary>
    public static class MazeData
    {
        public static readonly string[] Layout =
        {
            "#################",
            "#P..............#",
            "#...##.....##...#",
            "#...##.....##...#",
            "#.......F.......#",
            "#...............#",
            "#...##.....##...#",
            "#.F.##..Z..##...#",
            "#...##.....##...#",
            "#......Z........#",
            "#...##.....##...#",
            "#...##..F..##...#",
            "#...##.....##...#",
            "#.....Z.....F...#",
            "#...............#",
            "#...##.....##...#",
            "#.F.##.....##...#",
            "#......E........#",
            "#################",
        };
    }
}
