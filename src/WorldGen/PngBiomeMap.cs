using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.WorldGen;

/// <summary>
/// Reads a PNG map where each pixel represents a 512x512 block region.
/// Pixel color → region ID via colormap.json.
///
/// PNG coordinate system:
///   pixel (mapCenterPixelX, mapCenterPixelZ) = world spawn (0, 0) relative
///   pixel (px, pz) = spawn-relative world pos ((px - cx) * scale, (pz - cz) * scale)
/// </summary>
public class PngBiomeMap
{
    readonly ICoreServerAPI api;

    // Raw pixel data [z * width + x] = packed 0xRRGGBB
    int[]? pixels;
    int mapWidth, mapHeight;
    int centerPixelX, centerPixelZ;
    int pixelsPerBlock; // how many blocks one pixel covers (512)

    // color int → regionId
    readonly Dictionary<int, string> colorToRegion = new();

    public bool IsLoaded => pixels != null;

    public PngBiomeMap(ICoreServerAPI api)
    {
        this.api = api;
    }

    public void Load()
    {
        // ── Load colormap.json ───────────────────────────────────────
        var cmAssets = api.Assets.GetMany("worldgen/colormap.json", "lotr");
        if (cmAssets.Count == 0)
        {
            api.Logger.Warning("[LOTR] PngBiomeMap: colormap.json not found.");
            return;
        }

        var cmText = cmAssets[0].ToText();
        var cm = JsonConvert.DeserializeObject<ColormapFile>(cmText);
        if (cm == null) { api.Logger.Warning("[LOTR] PngBiomeMap: colormap.json parse failed."); return; }

        pixelsPerBlock = cm.PixelsPerBlock > 0 ? cm.PixelsPerBlock : 512;
        centerPixelX   = cm.MapCenterPixelX;
        centerPixelZ   = cm.MapCenterPixelZ;

        foreach (var kv in cm.Colors)
        {
            int rgb = Convert.ToInt32(kv.Key, 16);
            colorToRegion[rgb] = kv.Value;
        }

        // ── Load map.png ─────────────────────────────────────────────
        // Assets.GetMany returns IAsset; we need raw bytes
        var pngAssets = api.Assets.GetMany("worldgen/map.png", "lotr");
        if (pngAssets.Count == 0)
        {
            api.Logger.Warning("[LOTR] PngBiomeMap: map.png not found.");
            return;
        }

        byte[] pngBytes = pngAssets[0].Data;
        if (!DecodePng(pngBytes, out mapWidth, out mapHeight, out pixels!))
        {
            api.Logger.Warning("[LOTR] PngBiomeMap: failed to decode map.png.");
            return;
        }

        api.Logger.Notification($"[LOTR] PngBiomeMap loaded {mapWidth}x{mapHeight}, {colorToRegion.Count} region colors, scale={pixelsPerBlock} blocks/px");
    }

    /// <summary>
    /// Returns the region ID for spawn-relative world coordinates, or null for wilderness.
    /// </summary>
    public string? GetRegionId(double relX, double relZ)
    {
        if (pixels == null) return null;

        int px = centerPixelX + (int)Math.Round(relX / pixelsPerBlock);
        int pz = centerPixelZ + (int)Math.Round(relZ / pixelsPerBlock);

        if (px < 0 || px >= mapWidth || pz < 0 || pz >= mapHeight) return null;

        int rgb = pixels[pz * mapWidth + px];
        if (rgb == 0) return null; // black = wilderness

        return colorToRegion.TryGetValue(rgb, out var id) ? id : null;
    }

    // ── JSON model ────────────────────────────────────────────────────

    class ColormapFile
    {
        [JsonProperty("pixelsPerBlock")]   public int PixelsPerBlock  { get; set; } = 512;
        [JsonProperty("mapCenterPixelX")] public int MapCenterPixelX { get; set; } = 64;
        [JsonProperty("mapCenterPixelZ")] public int MapCenterPixelZ { get; set; } = 64;
        [JsonProperty("colors")]          public Dictionary<string, string> Colors { get; set; } = new();
    }

    // ── Minimal PNG decoder (RGB/RGBA only, no interlace) ─────────────

    static bool DecodePng(byte[] data, out int width, out int height, out int[] pixels)
    {
        width = height = 0; pixels = Array.Empty<int>();
        try
        {
            // Validate signature
            if (data.Length < 8) return false;
            // PNG signature: 137 80 78 71 13 10 26 10
            if (data[0] != 137 || data[1] != 80 || data[2] != 78 || data[3] != 71) return false;

            int pos = 8;
            int w = 0, h = 0, bitDepth = 0, colorType = 0;
            var idatChunks = new List<byte[]>();
            int[] palette = Array.Empty<int>(); // PLTE for indexed PNG

            while (pos + 12 <= data.Length)
            {
                int len  = ReadInt32BE(data, pos); pos += 4;
                string ct = System.Text.Encoding.ASCII.GetString(data, pos, 4); pos += 4;

                if (ct == "IHDR")
                {
                    w = ReadInt32BE(data, pos);
                    h = ReadInt32BE(data, pos + 4);
                    bitDepth  = data[pos + 8];
                    colorType = data[pos + 9];
                }
                else if (ct == "PLTE")
                {
                    // Each entry is 3 bytes (R, G, B); len must be divisible by 3
                    int entries = len / 3;
                    palette = new int[entries];
                    for (int i = 0; i < entries; i++)
                        palette[i] = (data[pos + i * 3] << 16) | (data[pos + i * 3 + 1] << 8) | data[pos + i * 3 + 2];
                }
                else if (ct == "IDAT")
                {
                    var chunk = new byte[len];
                    Array.Copy(data, pos, chunk, 0, len);
                    idatChunks.Add(chunk);
                }
                else if (ct == "IEND") break;

                pos += len + 4; // skip data + CRC
            }

            if (w == 0 || h == 0 || idatChunks.Count == 0) return false;
            // Support 8-bit RGB (2), RGBA (6), and indexed/palette (3)
            if (bitDepth != 8 || (colorType != 2 && colorType != 6 && colorType != 3)) return false;
            if (colorType == 3 && palette.Length == 0) return false;

            // Combine IDAT chunks and decompress
            int totalLen = 0;
            foreach (var c in idatChunks) totalLen += c.Length;
            byte[] compressed = new byte[totalLen];
            int off = 0;
            foreach (var c in idatChunks) { Array.Copy(c, 0, compressed, off, c.Length); off += c.Length; }

            // Decompress (skip 2-byte zlib header)
            byte[] raw;
            using (var ms = new MemoryStream(compressed, 2, compressed.Length - 2))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            using (var out_ = new MemoryStream())
            {
                ds.CopyTo(out_);
                raw = out_.ToArray();
            }

            pixels = new int[w * h];

            if (colorType == 3)
            {
                // Indexed PNG: 1 byte per pixel = palette index, filter byte per row
                byte[] prev = new byte[w];
                for (int y = 0; y < h; y++)
                {
                    int rowStart = y * (w + 1);
                    int filterType = raw[rowStart];
                    byte[] row = new byte[w];
                    Array.Copy(raw, rowStart + 1, row, 0, w);

                    ApplyFilter(filterType, row, prev, 1);

                    for (int x = 0; x < w; x++)
                        pixels[y * w + x] = palette[row[x]];

                    prev = row;
                }
            }
            else
            {
                // RGB (2) or RGBA (6): 3 or 4 bytes per pixel
                int channels = colorType == 6 ? 4 : 3;
                int stride = w * channels;
                byte[] prev = new byte[stride];

                for (int y = 0; y < h; y++)
                {
                    int rowStart = y * (stride + 1);
                    int filterType = raw[rowStart];
                    byte[] row = new byte[stride];
                    Array.Copy(raw, rowStart + 1, row, 0, stride);

                    ApplyFilter(filterType, row, prev, channels);

                    for (int x = 0; x < w; x++)
                    {
                        int i = x * channels;
                        pixels[y * w + x] = (row[i] << 16) | (row[i + 1] << 8) | row[i + 2];
                    }

                    prev = row;
                }
            }

            width = w; height = h;
            return true;
        }
        catch { return false; }
    }

    static void ApplyFilter(int filter, byte[] row, byte[] prev, int bpp)
    {
        switch (filter)
        {
            case 0: break; // None
            case 1: // Sub
                for (int i = bpp; i < row.Length; i++)
                    row[i] = (byte)(row[i] + row[i - bpp]);
                break;
            case 2: // Up
                for (int i = 0; i < row.Length; i++)
                    row[i] = (byte)(row[i] + prev[i]);
                break;
            case 3: // Average
                for (int i = 0; i < row.Length; i++)
                {
                    int a = i >= bpp ? row[i - bpp] : 0;
                    row[i] = (byte)(row[i] + (a + prev[i]) / 2);
                }
                break;
            case 4: // Paeth
                for (int i = 0; i < row.Length; i++)
                {
                    int a = i >= bpp ? row[i - bpp] : 0;
                    int b = prev[i];
                    int c2 = i >= bpp ? prev[i - bpp] : 0;
                    row[i] = (byte)(row[i] + PaethPredictor(a, b, c2));
                }
                break;
        }
    }

    static int PaethPredictor(int a, int b, int c)
    {
        int p = a + b - c;
        int pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    static int ReadInt32BE(byte[] d, int i)
        => (d[i] << 24) | (d[i+1] << 16) | (d[i+2] << 8) | d[i+3];
}
