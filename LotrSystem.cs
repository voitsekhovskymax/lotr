using Vintagestory.API.Common;

namespace Lotr;

public class LotrSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        api.Logger.Notification("LOTR Mod loaded.");
    }
}
