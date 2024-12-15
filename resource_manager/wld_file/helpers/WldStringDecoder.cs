using System.Text;

namespace OpenLore.resource_manager.wld_file.helpers;

// Lantern Extractor class
internal class WldStringDecoder
{
    public static readonly byte[] StringHashKey = { 0x95, 0x3a, 0xc5, 0x2a, 0x95, 0x7a, 0x95, 0x6a };

    public static string Decode(byte[] data)
    {
        for (var i = 0; i < data.Length; i++) data[i] ^= StringHashKey[i % 8];
        return Encoding.UTF8.GetString(data, 0, data.Length);
    }
}