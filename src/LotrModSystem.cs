using System.Text;
using Lotr.Constants;
using Lotr.Entities;
using Lotr.Entities.AI;
using Lotr.Entities.Humanoids;
using Lotr.Entities.Creatures;
using Lotr.Factions;
using Lotr.Items;
using Lotr.Network;
using Lotr.Quests;
using Lotr.Regions;
using Lotr.Systems.FactionGuardAlert;
using Lotr.Systems.NpcMorale;
using Lotr.Systems.World;
using Lotr.UI;
using Lotr.Utilities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace Lotr;

public class LotrModSystem : ModSystem
{
    // Server-side systems
    public AlignmentSystem?        Alignment  { get; private set; }
    public QuestSystem?            Quests     { get; private set; }
    public RegionSystem?           Regions    { get; private set; }
    public AlignmentDecaySystem?   Decay      { get; private set; }
    public CombatAlignmentHandler? Combat     { get; private set; }
    public FactionAggroSystem?     Aggro      { get; private set; }
    public DisguiseSystem?         Disguise   { get; private set; }
    public BountySystem?           Bounty     { get; private set; }
    public AlignmentPerksSystem?   Perks      { get; private set; }
    public FactionGuardAlertSystem? GuardAlert { get; private set; }
    public NpcMoraleSystem?        Morale     { get; private set; }
    public LotrWorldConfigSystem?  WorldConfig { get; private set; }

    // Client-side
    private GuiDialogFactions?    _factionsDialog;
    private IClientNetworkChannel? _clientChannel;

    // Network
    private IServerNetworkChannel? _serverChannel;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.Logger.Notification($"{LotrConstants.LogPrefix} Initializing Middle-earth...");

        api.RegisterEntity("EntityHobbit",   typeof(EntityHobbit));
        api.RegisterEntity("EntityHuman",    typeof(EntityHuman));
        api.RegisterEntity("EntityElf",      typeof(EntityElf));
        api.RegisterEntity("EntityDwarf",    typeof(EntityDwarf));
        api.RegisterEntity("EntityGandalf",  typeof(EntityGandalf));
        api.RegisterEntity("EntityOrc",      typeof(EntityOrc));
        api.RegisterEntity("EntityGoblin",   typeof(EntityGoblin));
        api.RegisterEntity("EntityTroll",    typeof(EntityTroll));
        api.RegisterEntity("EntityEnt",      typeof(EntityEnt));
        api.RegisterEntity("EntityBalrog",   typeof(EntityBalrog));
        api.RegisterEntity("EntityCreature", typeof(EntityCreature));

        api.RegisterItemClass("ItemLembas", typeof(ItemLembas));

        // Phase 8: AI task registration — method TBD (see StartServerSide)

        api.Logger.Notification($"{LotrConstants.LogPrefix} Classes registered.");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);

        // Core systems
        Alignment = new AlignmentSystem(api);
        Quests    = new QuestSystem(api, Alignment);
        Quests.LoadQuests();
        Regions   = new RegionSystem(api);
        Regions.Load();

        // Faction & alignment systems
        Disguise = new DisguiseSystem(Alignment);
        Combat   = new CombatAlignmentHandler(api, Alignment);
        Aggro    = new FactionAggroSystem(Alignment, Disguise);
        Decay    = new AlignmentDecaySystem(api, Alignment);
        Bounty   = new BountySystem(api, Alignment);
        Perks    = new AlignmentPerksSystem(Alignment);

        // Phase 8: AI tasks auto-discovered by VS from assembly — code = class name minus "AiTask" prefix, lowercased

        // Phase 8: guard alert & morale systems
        GuardAlert  = new FactionGuardAlertSystem(api);
        Morale      = new NpcMoraleSystem(api);
        WorldConfig = new LotrWorldConfigSystem(api);
        WorldConfig.Start();

        // Wire up entity kill event
        EntityRaceBase.Killed += (entity, damage) => Combat.OnEntityKilled(entity, damage);
        EntityRaceBase.Killed += (entity, damage) => Morale.OnEntityKilled(entity, damage);

        // Network channel — server side
        _serverChannel = api.Network
            .RegisterChannel(LotrConstants.NetworkChannel)
            .RegisterMessageType<AlignmentQueryPacket>()
            .RegisterMessageType<AlignmentResponsePacket>()
            .SetMessageHandler<AlignmentQueryPacket>(OnAlignmentQueryReceived);

        new EntityAssetValidator(api).ValidateAll();

        api.Event.PlayerJoin += OnPlayerJoin;

        api.Logger.Notification($"{LotrConstants.LogPrefix} All faction/alignment systems started.");

        RegisterCommands(api);
    }

    private void OnPlayerJoin(IServerPlayer player)
    {
        if (Regions?.SpawnX == 0) Regions.CacheSpawn();

        // Restore disguise state
        if (Disguise != null && Alignment != null)
        {
            var data = Alignment.LoadData(player);
            Disguise.RestoreFromSave(player, data);
        }
    }

    private void OnAlignmentQueryReceived(IServerPlayer player, AlignmentQueryPacket packet)
    {
        if (Alignment == null || Bounty == null || Alignment == null) return;

        var entries = new System.Collections.Generic.List<FactionEntry>();
        foreach (var fid in LotrConstants.Factions.All)
        {
            int score = Alignment.GetAlignment(player, fid);
            entries.Add(new FactionEntry
            {
                FactionId = fid,
                Score     = score,
                Tier      = AlignmentSystem.ScoreToTier(score).ToString()
            });
        }

        var (hasBounty, bFaction, bAmount, _) = Bounty.GetBountyStatus(player);
        var data = Alignment.LoadData(player);

        var response = new AlignmentResponsePacket
        {
            Entries         = entries,
            HasBounty       = hasBounty,
            BountyFaction   = bFaction,
            BountyAmount    = bAmount,
            IsDisguised     = data.IsDisguised,
            DisguisedFaction = data.DisguisedFactionId
        };

        _serverChannel?.SendPacket(response, player);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);

        _factionsDialog = new GuiDialogFactions(api);

        _clientChannel = api.Network
            .RegisterChannel(LotrConstants.NetworkChannel)
            .RegisterMessageType<AlignmentQueryPacket>()
            .RegisterMessageType<AlignmentResponsePacket>()
            .SetMessageHandler<AlignmentResponsePacket>(OnAlignmentResponseReceived);
    }

    private void OnAlignmentResponseReceived(AlignmentResponsePacket packet)
    {
        _factionsDialog?.OpenWith(packet);
    }

    // ── Commands ─────────────────────────────────────────────────

    private void RegisterCommands(ICoreServerAPI api)
    {
        var lotr = api.ChatCommands
            .Create("lotr")
            .WithDescription("LOTR mod commands")
            .RequiresPrivilege(Vintagestory.API.Server.Privilege.chat);

        lotr.BeginSubCommand("region")
            .WithDescription("Show which Middle-earth region you are in")
            .HandleWith(OnRegionCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("alignment")
            .WithDescription("Show your faction alignment scores")
            .HandleWith(OnAlignmentCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("factions")
            .WithDescription("Show faction standings (also opens GUI via /lotrfactions)")
            .HandleWith(OnAlignmentCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("setalignment")
            .WithDescription("Set alignment: /lotr setalignment <faction> <value>")
            .RequiresPrivilege(Vintagestory.API.Server.Privilege.controlserver)
            .WithArgs(api.ChatCommands.Parsers.Word("factionId"), api.ChatCommands.Parsers.Int("value"))
            .HandleWith(OnSetAlignmentCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("disguise")
            .WithDescription("Disguise as faction: /lotr disguise <factionId>")
            .WithArgs(api.ChatCommands.Parsers.Word("factionId"))
            .HandleWith(OnDisguiseCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("undisguise")
            .WithDescription("Remove disguise")
            .HandleWith(OnUndisguiseCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("perks")
            .WithDescription("Show your active faction perks")
            .HandleWith(OnPerksCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("bounty")
            .WithDescription("Check your bounty status")
            .HandleWith(OnBountyCommand)
        .EndSubCommand();

        lotr.BeginSubCommand("ui")
            .WithDescription("Open LOTR UI panels")
            .BeginSubCommand("factions")
                .WithDescription("Open faction standings window")
                .HandleWith(OnUiFactions)
            .EndSubCommand()
        .EndSubCommand();

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

    // ── Command handlers ─────────────────────────────────────────

    private TextCommandResult OnRegionCommand(TextCommandCallingArgs args)
    {
        if (Regions == null) return TextCommandResult.Error("Region system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var pos = player.Entity.Pos.XYZ;
        var (region, biome) = Regions.GetAt(pos.X, pos.Z);

        if (region == null)
        {
            double rx = pos.X - Regions.SpawnX;
            double rz = pos.Z - Regions.SpawnZ;
            string mapDbg = Regions.GetMapDebugInfo(pos.X, pos.Z);
            return TextCommandResult.Success(
                $"You are in the Wilderness.\n  World pos: X={pos.X:F0} Z={pos.Z:F0}\n  Spawn-relative: X={rx:F0} Z={rz:F0}\n  Map: {mapDbg}");
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
            if (biome.Flora.Count > 0)
                sb.AppendLine($"  Flora:   {string.Join(", ", biome.Flora)}");
        }
        sb.AppendLine($"  Pos:     X={pos.X:F0}  Y={pos.Y:F0}  Z={pos.Z:F0}");
        return TextCommandResult.Success(sb.ToString().TrimEnd());
    }

    private TextCommandResult OnAlignmentCommand(TextCommandCallingArgs args)
    {
        if (Alignment == null) return TextCommandResult.Error("Alignment system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var sb = new StringBuilder();
        sb.AppendLine("=== Middle-earth Alignment ===");
        foreach (var (fid, score) in Alignment.GetAllAlignments(player))
        {
            var tier      = AlignmentSystem.ScoreToTier(score);
            var shortName = fid.Replace("lotr:faction-", "");
            sb.AppendLine($"  {shortName,-20} {score,6:+0;-0;0}  [{tier}]");
        }

        // Show disguise / bounty status
        var data = Alignment.LoadData(player);
        if (data.IsDisguised)
            sb.AppendLine($"  [Disguised as {data.DisguisedFactionId.Replace("lotr:faction-", "")}]");
        if (data.HasActiveBounty)
            sb.AppendLine($"  [WANTED by {data.BountyFactionId.Replace("lotr:faction-", "")} — {data.BountyAmount} silver]");

        sb.AppendLine("(Open visual dialog: /lotr ui factions)");
        return TextCommandResult.Success(sb.ToString().TrimEnd());
    }

    private TextCommandResult OnSetAlignmentCommand(TextCommandCallingArgs args)
    {
        if (Alignment == null) return TextCommandResult.Error("Alignment system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        string factionId = (string)args[0];
        int value        = (int)args[1];
        if (!factionId.StartsWith("lotr:")) factionId = $"lotr:faction-{factionId}";

        Alignment.SetAlignment(player, factionId, value);
        return TextCommandResult.Success($"Set {factionId} = {value} [{Alignment.GetTier(player, factionId)}]");
    }

    private TextCommandResult OnDisguiseCommand(TextCommandCallingArgs args)
    {
        if (Disguise == null) return TextCommandResult.Error("Disguise system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        string factionId = (string)args[0];
        var (ok, msg) = Disguise.Activate(player, factionId);
        return ok ? TextCommandResult.Success(msg) : TextCommandResult.Error(msg);
    }

    private TextCommandResult OnUndisguiseCommand(TextCommandCallingArgs args)
    {
        if (Disguise == null) return TextCommandResult.Error("Disguise system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var (_, msg) = Disguise.Deactivate(player);
        return TextCommandResult.Success(msg);
    }

    private TextCommandResult OnPerksCommand(TextCommandCallingArgs args)
    {
        if (Perks == null) return TextCommandResult.Error("Perks system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");
        return TextCommandResult.Success(Perks.GetPerkSummary(player));
    }

    private TextCommandResult OnUiFactions(TextCommandCallingArgs args)
    {
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");
        OnAlignmentQueryReceived(player, new AlignmentQueryPacket());
        return TextCommandResult.Success("");
    }

    private TextCommandResult OnBountyCommand(TextCommandCallingArgs args)
    {
        if (Bounty == null) return TextCommandResult.Error("Bounty system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var (hasBounty, faction, amount, expiry) = Bounty.GetBountyStatus(player);
        if (!hasBounty) return TextCommandResult.Success("No active bounties on you.");

        string name     = faction.Replace("lotr:faction-", "");
        var expiryDate  = System.DateTimeOffset.FromUnixTimeSeconds(expiry);
        return TextCommandResult.Success(
            $"WANTED by {name} — {amount} silver — expires {expiryDate:HH:mm:ss} UTC");
    }

    // ── Quest commands ───────────────────────────────────────────

    private TextCommandResult OnQuestList(TextCommandCallingArgs args)
    {
        if (Quests == null) return TextCommandResult.Error("Quest system not running.");
        if (args.Caller.Player is not IServerPlayer player) return TextCommandResult.Error("Server only.");

        var sb        = new StringBuilder();
        var available = Quests.GetAvailable(player);
        var active    = Quests.GetActive(player);

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

    // ── Dispose ──────────────────────────────────────────────────

    public override void Dispose()
    {
        EntityModelCache.Clear();
        EntityRaceBase.Killed -= null;
        base.Dispose();
    }
}
