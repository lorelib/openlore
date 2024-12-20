using Godot;

namespace OpenLore.resource_manager.pack_file;

public partial class PfsFile : Resource
{
    public uint Crc;
    public uint Size;
    public uint Offset;
    public byte[] FileBytes;
    public string Name;
    public string ArchiveName;

    public PfsFile(string archiveName, uint crc, uint size, uint offset, byte[] fileBytes)
    {
        Crc = crc;
        Size = size;
        Offset = offset;
        FileBytes = fileBytes;
        ArchiveName = archiveName;
    }
}