using System.Text;
using Lotr.Constants;
using Lotr.Entities.Humanoids;
using Lotr.Factions;
using Lotr.Items;
using Lotr.Quests;
using Lotr.Regions;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

namespace Lotr;

public class LotrModSystem : ModSystem
{
    public AlignmentSystem? Alignment { get; private set; }
    public QuestSystem?     Quests    { get; private set; }
    public RegionSystem?    Regions   { get; private set; }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.Logger.Notification($"{LotrConstants.LogPrefix} Initializing Middle-earth...");

        // Entity classes
        api.RegisterEntity("EntityHobbit",  typeof(EntityHobbit));
        api.RegisterEntity("EntityHuman",   typeof(EntityHuman));
        api.RegisterEntity("EntityElf",     typeof(EntityElf));
        api.RegisterEntity("EntityDwarf",   typeof(EntityDwarf));
        api.RegisterEntity("EntityGandalf", typeof(EntityGandalf));

        // Item classes
        api.RegisterItemClass("ItemLembas", typeof(ItemLembas));

        api.Logger.Notification($"{LotrConstants.LogPrefix} Classes registered.");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);

        Alignment = new AlignmentSystem(api);
        Quests    = new QuestSystem(api, Alignment);
        Quests.LoadQuests();
        Regions   = new RegionSystem(api);
        Regions.Load();

        // DefaultSpawnPosition is NOT available at SaveGameLoaded — use PlayerJoin instead
        api.Event.PlayerJoin += p =>
        {
            if (Regions.SpawnX == 0) Regions.CacheSpawn();
        };

        api.Logger.Notification($"{LotrConstants.LogPrefix} Alignment + Quest + Region systems started.");

        RegisterCommands(api);
    }

    private void RegisterCommands(ICoreServerAPI api)
    {
        var lotr = api.ChatCommands
            .Create("lotr")
            .WithDescription("LOTR mod commands")
            .RequiresPrivilege(Vintagestory.API.Server.Privilege.chat);

        // /lotr region
        lotr.BeginSubCommand("region")
            .WithDescription("Show which Middle-earth region you are in")
            .HandleWith(OnRegionCommand)
        .EndSubCommand();

        // /lotr alignment
        lotr.BeginSubCommand("alignment")
            .WithDescription("Show your faction alignment scores")
            .HandleWith(OnAlignmentCommand)
        .EndSubCommand();

        // /lotr setalignment <faction> <value>
        lotr.BeginSubCommand("setalignment")
            .WithDescription("Set alignment: /lotr setalignment <faction> <value>")
            .RequiresPrivilege(Vintagestory.API.Server.Privilege.controlserver)
            .WithArgs(api.ChatCommands.Parsers.Word("factionId"), api.ChatCommands.Parsers.Int("value"))
            .HandleWith(OnSetAlignmentCommand)
        .EndSubCommand();

        // /lotr quest list
        lotr.BeginSubCommand("quest")
            .WithDescription("Quest commands")
            .BeginSubCommand("list")
                .WithDescription("List available and active quests")
                .HandleWith(OnQuestList)
            .EndSubCommand()
            .BeginSubCommand("start")
                .WithDescription("Start a quest: /lotr quest start <questId>")
                .WithArgs(api.ChatCommands.Parsers.Word("questId"))
                .HandleWith(OnQuestStart)
            .EndSubCommand()
            .BeginSubCommand("complete")
                .WithDescription("Complete a quest (debug): /lotr quest complete <questId>")
                .RequiresPrivilege(Vintagestory.API.Server.Privilege.controlserver)
                .WithArgs(api.ChatCommands.Parsers.Word("questId"))
                .HandleWith(OnQuestComplete)
            .EndSubCommand()
            .BeginSubCommand("status")
                .WithDescription("Check quest status: /lotr quest status <questId>")
                .WithArgs(api.ChatCommands.Parsers.Word("questId"))
                .HandleWith(OnQuestStatus)
            .EndSubCommand()
        .EndSubCommand();
    }

    // ── Region command ───────────────────────────────────────────

    private TextCommandResult OnRegionCommand(TextCommandCallingArgs args)
    {
        if (Regions == null) return TextCommandResult.Error("Region system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var pos = player.Entity.Pos.XYZ;
        var (region, biome) = Regions.GetAt(pos.X, pos.Z);

        if (region == null)
        {
            double rx = pos.X - (Regions.SpawnX);
            double rz = pos.Z - (Regions.SpawnZ);
            return TextCommandResult.Success(
                $"You are in the Wilderness. No named region.\n  World pos: X={pos.X:F0} Z={pos.Z:F0}\n  Spawn-relative: X={rx:F0} Z={rz:F0}");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"=== {region.DisplayName} ===");
        sb.AppendLine($"  Region:  {region.Id}");
        sb.AppendLine($"  Faction: {region.FactionId.Replace("lotr:faction-", "")}");
        if (biome != null)
        {
            sb.AppendLine($"  Biome:   {biome.DisplayName}");
            sb.AppendLine($"  Temp:    {biome.TempMin}..{biome.TempMax} °C");
            sb.AppendLine($"  Rain:    {biome.RainMin:F2}..{biome.RainMax:F2}");
            sb.AppendLine($"  Fertility: {biome.FertilityMin:F2}..{biome.FertilityMax:F2}");
            if (biome.Flora.Count > 0)
                sb.AppendLine($"  Flora:   {string.Join(", ", biome.Flora)}");
        }
        sb.AppendLine($"  Pos:     X={pos.X:F0}  Y={pos.Y:F0}  Z={pos.Z:F0}");
        return TextCommandResult.Success(sb.ToString().TrimEnd());
    }

    // ── Alignment commands ───────────────────────────────────────

    private TextCommandResult OnAlignmentCommand(TextCommandCallingArgs args)
    {
        if (Alignment == null) return TextCommandResult.Error("Alignment system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var sb = new StringBuilder();
        sb.AppendLine("=== Middle-earth Alignment ===");
        foreach (var (fid, score) in Alignment.GetAllAlignments(player))
        {
            var tier = Alignment.GetTier(player, fid);
            var shortName = fid.Replace("lotr:faction-", "");
            sb.AppendLine($"  {shortName,-12} {score,6:+0;-0;0}  [{tier}]");
        }
        return TextCommandResult.Success(sb.ToString().TrimEnd());
    }

    private TextCommandResult OnSetAlignmentCommand(TextCommandCallingArgs args)
    {
        if (Alignment == null) return TextCommandResult.Error("Alignment system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        string factionId = (string)args[0];
        int value = (int)args[1];
        if (!factionId.StartsWith("lotr:")) factionId = $"lotr:faction-{factionId}";

        Alignment.SetAlignment(player, factionId, value);
        return TextCommandResult.Success($"Set {factionId} = {value} [{Alignment.GetTier(player, factionId)}]");
    }

    // ── Quest commands ───────────────────────────────────────────

    private TextCommandResult OnQuestList(TextCommandCallingArgs args)
    {
        if (Quests == null) return TextCommandResult.Error("Quest system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var sb = new StringBuilder();
        var available = Quests.GetAvailable(player);
        var active = Quests.GetActive(player);

        sb.AppendLine("=== Quests ===");
        if (active.Count > 0)
        {
            sb.AppendLine("Active:");
            foreach (var q in active) sb.AppendLine($"  [A] {q.Id.Replace("lotr:quest-", "")} — {q.Name}");
        }
        if (available.Count > 0)
        {
            sb.AppendLine("Available:");
            foreach (var q in available) sb.AppendLine($"  [ ] {q.Id.Replace("lotr:quest-", "")} — {q.Name}");
        }
        if (active.Count == 0 && available.Count == 0)
            sb.AppendLine("  No quests available right now.");

        return TextCommandResult.Success(sb.ToString().TrimEnd());
    }

    private TextCommandResult OnQuestStart(TextCommandCallingArgs args)
    {
        if (Quests == null) return TextCommandResult.Error("Quest system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        string questId = (string)args[0];
        if (!questId.StartsWith("lotr:")) questId = $"lotr:quest-{questId}";

        var (ok, msg) = Quests.StartQuest(player, questId);
        return ok ? TextCommandResult.Success(msg) : TextCommandResult.Error(msg);
    }

    private TextCommandResult OnQuestComplete(TextCommandCallingArgs args)
    {
        if (Quests == null) return TextCommandResult.Error("Quest system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        string questId = (string)args[0];
        if (!questId.StartsWith("lotr:")) questId = $"lotr:quest-{questId}";

        var (ok, msg) = Quests.CompleteQuest(player, questId);
        return ok ? TextCommandResult.Success(msg) : TextCommandResult.Error(msg);
    }

    private TextCommandResult OnQuestStatus(TextCommandCallingArgs args)
    {
        if (Quests == null) return TextCommandResult.Error("Quest system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        string questId = (string)args[0];
        if (!questId.StartsWith("lotr:")) questId = $"lotr:quest-{questId}";

        var state = Quests.GetState(player, questId);
        return TextCommandResult.Success($"{questId}: {state}");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
