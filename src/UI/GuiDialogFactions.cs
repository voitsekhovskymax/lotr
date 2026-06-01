using System.Text;
using Lotr.Constants;
using Lotr.Network;
using Vintagestory.API.Client;

namespace Lotr.UI;

// Client-side dialog showing player faction standings.
// Opened by /lotr ui factions after server sends AlignmentResponsePacket.
public class GuiDialogFactions : GuiDialog
{
    public override string ToggleKeyCombinationCode => null!;

    public GuiDialogFactions(ICoreClientAPI capi) : base(capi) { }

    public void OpenWith(AlignmentResponsePacket data)
    {
        ComposeDialog(data);
        if (!IsOpened()) TryOpen();
        else SingleComposer.ReCompose();
    }

    private void ComposeDialog(AlignmentResponsePacket data)
    {
        double pad = GuiStyle.ElementToDialogPadding;

        // Fixed content area: y=30 clears title bar, 480 wide, 440 tall for 41 factions
        ElementBounds textBounds = ElementBounds.Fixed(0, 30, 480, 440);
        ElementBounds btnBounds  = ElementBounds.Fixed(180, 476, 120, 28);

        // bgBounds must know its children for FitToChildren to work
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(pad);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(textBounds, btnBounds);

        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
            .WithAlignment(EnumDialogArea.CenterMiddle);

        SingleComposer = capi.Gui.CreateCompo("lotr-factions", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar("Middle-earth: Faction Standings", () => TryClose())
            .AddRichtext(BuildVtml(data), CairoFont.WhiteSmallText(), textBounds)
            .AddSmallButton("Close", () => { TryClose(); return true; }, btnBounds)
            .Compose();
    }

    private static string BuildVtml(AlignmentResponsePacket data)
    {
        var sb = new StringBuilder();

        if (data.IsDisguised)
        {
            string dName = data.DisguisedFaction.Replace("lotr:faction-", "");
            sb.Append($"<font color=\"#ffcc00\"><strong>DISGUISED as: {dName}</strong></font><br>");
        }

        if (data.HasBounty)
        {
            string bName = data.BountyFaction.Replace("lotr:faction-", "");
            sb.Append($"<font color=\"#ff4444\"><strong>WANTED by {bName} — {data.BountyAmount} silver</strong></font><br>");
        }

        if (data.IsDisguised || data.HasBounty) sb.Append("<br>");

        sb.Append("<font color=\"#888888\">Faction — Score — Tier</font><br>");
        sb.Append("<font color=\"#555555\">────────────────────────────────────</font><br>");

        foreach (var entry in data.Entries)
        {
            string color = LotrConstants.AlignmentColors.ForScore(entry.Score);
            string name  = entry.FactionId.Replace("lotr:faction-", "");
            string score = $"{entry.Score:+0;-0;0}";
            sb.Append($"<font color=\"{color}\">{name}  {score}  [{entry.Tier}]</font><br>");
        }

        return sb.ToString();
    }
}
