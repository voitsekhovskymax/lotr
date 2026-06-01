using System;
using System.Collections.Generic;
using System.Text;
using Lotr.Constants;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Utilities;

/// <summary>
/// Validates all lotr: entity JSON assets at server startup.
/// Runs automatically via LotrModSystem and logs warnings for any missing
/// required fields — works for all races (hobbits, humans, orcs, elves, dwarves, etc.)
/// as new entity JSON files are added they are validated automatically.
/// </summary>
public class EntityAssetValidator(ICoreServerAPI api)
{
    private static readonly string[] RequiredAnimations = ["idle", "walk", "hurt", "die"];

    public void ValidateAll()
    {
        var assets = api.Assets.GetMany("entities/", "lotr");
        int ok = 0, warn = 0;

        foreach (var asset in assets)
        {
            try
            {
                var jObj = JObject.Parse(Encoding.UTF8.GetString(asset.Data));
                ValidateJson(asset.Location.ToString(), jObj, ref ok, ref warn);
            }
            catch (Exception ex)
            {
                api.Logger.Warning($"{LotrConstants.LogPrefix} [Validator] {asset.Location}: parse error — {ex.Message}");
                warn++;
            }
        }

        api.Logger.Notification($"{LotrConstants.LogPrefix} Entity validation: {ok} OK, {warn} warnings.");
    }

    private void ValidateJson(string loc, JObject jObj, ref int ok, ref int warn)
    {
        var issues = new List<string>();

        // 1. Code must carry lotr: prefix
        string code = jObj["code"]?.Value<string>() ?? "";
        if (!code.StartsWith("lotr:"))
            issues.Add($"code missing 'lotr:' prefix (found: '{code}')");

        // 2. Hitbox must be positive
        float hx = jObj["hitboxSize"]?["x"]?.Value<float>() ?? 0f;
        float hy = jObj["hitboxSize"]?["y"]?.Value<float>() ?? 0f;
        if (hx <= 0f || hy <= 0f)
            issues.Add($"hitboxSize invalid ({hx:F2}, {hy:F2})");

        // 3. Client animations — all four required
        var animations = jObj["client"]?["animations"] as JArray;
        if (animations == null)
        {
            issues.Add("missing client.animations");
        }
        else
        {
            var definedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var anim in animations)
                if (anim["code"]?.Value<string>() is { } c) definedCodes.Add(c);

            foreach (string required in RequiredAnimations)
                if (!definedCodes.Contains(required))
                    issues.Add($"missing animation '{required}'");
        }

        // 4. Faction attribute
        string faction = jObj["attributes"]?["faction"]?.Value<string>() ?? "";
        if (string.IsNullOrEmpty(faction))
            issues.Add("missing attributes.faction");

        if (issues.Count > 0)
        {
            foreach (var issue in issues)
                api.Logger.Warning($"{LotrConstants.LogPrefix} [Validator] {loc}: {issue}");
            warn++;
        }
        else
        {
            ok++;
        }
    }
}
