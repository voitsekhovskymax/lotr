namespace Lotr.Factions;

public enum AlignmentTier
{
    Nemesis    = 0,  // -1000..-750
    Hated      = 1,  // -749..-500
    Hostile    = 2,  // -499..-250
    Unfriendly = 3,  // -249..-1
    Friendly   = 4,  // 0..249
    Honored    = 5,  // 250..499
    Revered    = 6,  // 500..749
    Exalted    = 7   // 750..1000
}
