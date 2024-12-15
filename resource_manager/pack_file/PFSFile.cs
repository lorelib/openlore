using Godot;

namespace OpenLore.resource_manager.pack_file;

public partial class PFSFile : Resource
{
    public uint Crc;
    public uint Size;
    public uint Offset;
    public byte[] FileBytes;
    public string Name;
    public string ArchiveName;

    public PFSFile(string archiveName, uint crc, uint size, uint offset, byte[] fileBytes)
    {
        Crc = crc;
        Size = size;
        Offset = offset;
        FileBytes = fileBytes;
        ArchiveName = archiveName;
    }
}