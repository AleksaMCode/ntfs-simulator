namespace FileSystem
{
    public interface ISaveObjects
    {
        int GetSaveLength();
        void Save(byte[] buffer, int offset);
    }
}
