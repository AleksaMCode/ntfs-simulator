using System.IO;

namespace FileSystem
{
    public interface INtfsFs
    {
        uint BytesPerCluster { get; }
        uint BytesPerSector { get; }
        byte SectorPerCluster { get; }
    }
}
