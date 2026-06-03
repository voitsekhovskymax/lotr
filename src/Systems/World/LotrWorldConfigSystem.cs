using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Systems.World;

/// <summary>
/// Enforces Middle-earth world configuration settings.
/// Applied on every world load so players cannot accidentally enable
/// mechanics that break Middle-earth immersion.
/// </summary>
public class LotrWorldConfigSystem(ICoreServerAPI api)
{
    const int RecommendedMapSize = 512_000;

    public void Start()
    {
        api.Event.SaveGameLoaded += OnSaveGameLoaded;
        api.Event.PlayerJoin     += OnPlayerJoin;
    }

    void OnSaveGameLoaded()
    {
        var config = api.WorldManager.SaveGame.WorldConfiguration;

        // Temporal storms contradict Middle-earth lore — always off
        config.SetString("temporalStorms", "off");

        // Vanilla pre-history / mankind lore is replaced by Middle-earth lore
        config.SetString("loreContent", "false");

        api.Logger.Notification("[LOTR] World config enforced: temporalStorms=off, loreContent=false");

        LogSizeWarningIfNeeded();
    }

    void OnPlayerJoin(IServerPlayer player)
    {
        int x = api.WorldManager.MapSizeX;
        int z = api.WorldManager.MapSizeZ;
        if (x < RecommendedMapSize || z < RecommendedMapSize)
        {
            player.SendMessage(
                Vintagestory.API.Config.GlobalConstants.GeneralChatGroup,
                $"[LOTR] This world is {x}×{z} blocks — smaller than the recommended " +
                $"{RecommendedMapSize}×{RecommendedMapSize} for full Middle-earth coverage. " +
                $"Create a new world and set Map Size X/Z to {RecommendedMapSize}.",
                EnumChatType.OwnMessage);
        }
    }

    void LogSizeWarningIfNeeded()
    {
        int x = api.WorldManager.MapSizeX;
        int z = api.WorldManager.MapSizeZ;
        if (x < RecommendedMapSize || z < RecommendedMapSize)
            api.Logger.Warning(
                $"[LOTR] World size {x}×{z} is smaller than recommended {RecommendedMapSize}×{RecommendedMapSize}. " +
                $"Create a new world with Map Size X={RecommendedMapSize}, Z={RecommendedMapSize}.");
        else
            api.Logger.Notification($"[LOTR] World size {x}×{z} ✓");
    }
}
