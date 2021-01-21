using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystem.Boot;
using FileSystem.Attributes;
using FileSystem.Attributes.Indexroot;

namespace FileSystem
{
    public class NtfsFileSystem : INtfsFs
    {
        private Stream diskStream = new MemoryStream();
        private BootSector boot;
        private Stream mftStream = new MemoryStream();
        public List<FileRecordSegmentHeader> mftHeaderRecord = new List<FileRecordSegmentHeader>();

        private uint sectorsPerRecord;
        public uint BytesPerFileRecord { get; set; }
        public uint BytesPerCluster { get { return (uint)boot.BIOSParameterBlock.BytesPerSector * boot.BIOSParameterBlock.SectorPerCluster; } }
        public uint BytesPerSector { get { return boot.BIOSParameterBlock.BytesPerSector; } }
        public byte SectorPerCluster { get { return boot.BIOSParameterBlock.SectorPerCluster; } }
        public ulong TotalSectors { get { return boot.ExtraBPB.TotalSectors; } }
        public ulong TotalClusters { get { return boot.ExtraBPB.TotalSectors / boot.BIOSParameterBlock.SectorPerCluster; } }
        //public Stream DiskStream { get { return diskStream; } set { diskStream = value; } }
        public uint BytesOccupied { get; set; }
        //public uint BytesRemaining { get; set; }

        public FileRecordSegmentHeader Root { get; set; }

        public VolumeName VolName { get; set; }
        //public VolumeInformation VolInfo { get; set; }
        public uint VolumeSize { get; set; }

        public uint CurrentMftRecordNumber { get; set; }
        public uint FileRecordCount { get; set; }
        public uint DirectoryRecordCount { get; set; }
        private byte[] buffer;

        public NtfsFileSystem(Stream disk)
        {
            diskStream = disk;
            //ReadBoot();
            //ReadMFT();
        }

        public NtfsFileSystem() : base() { }

        public NtfsFileSystem(string name) : base()
        {
            DirectoryRecordCount = 1;       // root folder
            FileRecordCount = CurrentMftRecordNumber = 0;
            BytesPerFileRecord = 64_000;    // 64 KB
            VolumeSize = 20_000_000;        // 20 MB
            BytesOccupied = 0;

            CreateBoot();
            CreateRoot(name);

            VolName = new VolumeName();
            char[] arr = name.ToCharArray();
            Array.Reverse(arr);
            VolName.Name = arr.ToString();
            BytesOccupied += (uint)VolName.Name.Length;

            //BytesRemaining = VolumeSize - BytesOccupied;
        }

        private void CreateRoot(string name)
        {
            Root = new FileRecordSegmentHeader();
            Root.CreateFileRecordHeader(Enums.MFTEntryFlags.FileNameIndexPresent, null);

            StandardInformation stdInfo = new StandardInformation();
            stdInfo.CreateStandInfoFile(FileAttributes.Normal);
            Root.UsedEntrySize += stdInfo.RecordLength;

            IndexRoot indRoot = new IndexRoot();
            //Root.UsedEntrySize += (uint)indRoot.numberOfChildren * 8; // reference is 8 B

            FileName fileName = new FileName();
            fileName.CreateFileNameFile(name);
            fileName.RealSize = fileName.AllocatedSize = 0;
            Root.UsedEntrySize += fileName.RecordLength;

            Root.attributes.Add(stdInfo);
            Root.attributes.Add(fileName);
            Root.attributes.Add(indRoot);

            BytesOccupied += Root.UsedEntrySize;
        }

        private void CreateBoot()
        {
            boot = new BootSector();
            byte[] bootSector = boot.DataWritter();
            BytesOccupied += (uint)bootSector.Length;

            var memory = new MemoryStream();
            memory.Write(bootSector, 0, bootSector.Length);
            memory.Flush();
            memory.Seek(0, SeekOrigin.Begin);
            memory.CopyTo(diskStream);
            //diskStream.Position = bootSector.Length;
        }

        public byte[] Save()
        {
            CreateDiskStream();
            var memory = new MemoryStream();
            diskStream.Seek(0, SeekOrigin.Begin);
            diskStream.CopyTo(memory);
            memory.Seek(0, SeekOrigin.Begin);
            return memory.ToArray();
        }

        private void CreateMftStream()
        {
            var memory = new MemoryStream();

            foreach (var mft in mftHeaderRecord)
            {
                byte[] dat = mft.DataWritter();
                memory.Write(dat, 0, dat.Length);
                memory.Flush();
                memory.Position = 0;
                memory.CopyTo(mftStream);
             }
        }

        public void CreateDiskStream()
        {
            var memory = new MemoryStream();

            // $MFT
            CreateMftStream();
            mftStream.Position = 0;
            mftStream.CopyTo(diskStream);

            // $Root
            byte[] rootSec = Root.DataWritter();
            memory.Write(rootSec, 0, rootSec.Length);
            memory.Flush();
            memory.Position = 0;
            memory.CopyTo(diskStream);

            // $Volume
            byte[] volume = new byte[VolName.Name.Length + 4];
            Array.Copy(Encoding.ASCII.GetBytes(VolName.Name), 0, volume, 0, VolName.Name.Length);
            Array.Copy(BitConverter.GetBytes(VolumeSize), 0, volume, VolName.Name.Length, 4);
            memory.Write(volume, 0, volume.Length);
            memory.Flush();
            memory.Position = 0;
            memory.CopyTo(diskStream);

            byte[] emptySpace = new byte[VolumeSize - BytesOccupied];
            memory.Write(emptySpace, 0, emptySpace.Length);
            memory.Flush();
            memory.Position = 0;
            memory.CopyTo(diskStream);
        }

        public byte[] searchData(string name) //in all FS
        {
            foreach (var node in mftHeaderRecord)
                if (((FileName)node.attributes.ElementAt(1)).Filename.Equals(name))
                    return ((Data)node.attributes.ElementAt(2)).DataBytes;

            return null;
        }

        public byte[] searchDataInDirectory(FileRecordSegmentHeader dir, string name)
        {
            foreach (var child in ((IndexRoot)dir.attributes.ElementAt(2)).Children)
                if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(name))
                    return ((Data)child.attributes.ElementAt(2)).DataBytes;

            return null;
        }

        public FileRecordSegmentHeader findParent(string name)
        {
            if (((FileName)Root.attributes.ElementAt(1)).Filename.Equals(name))
                return Root;

            foreach (var node in mftHeaderRecord)
                if (((FileName)node.attributes.ElementAt(1)).Filename.Equals(name))
                    return node;

            return null;
        }

        public FileRecordSegmentHeader findParentSubNode(string name)
        {
            foreach (var child in ((IndexRoot)Root.attributes.ElementAt(2)).Children)
                if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(name))
                    return child;

            return null;
        }

        public FileRecordSegmentHeader findParentSubNodes(string subNode1, string subNode2)
        {
            foreach (var child in ((IndexRoot)findParentSubNode(subNode1).attributes.ElementAt(2)).Children)
                if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(subNode2))
                    return child;

            return null;
        }

        public FileRecordSegmentHeader FindFolder(FileRecordSegmentHeader folder, string searchDir)
        {
            foreach (var child in ((IndexRoot)folder.attributes.ElementAt(2)).Children)
                if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(searchDir))
                    return child;

            return null;
        }

        public void changeName(FileRecordSegmentHeader folder, string oldName, string newName)
        {
            foreach (var node in ((IndexRoot)folder.attributes.ElementAt(2)).Children)
                if (((FileName)node.attributes.ElementAt(1)).Filename.Equals(oldName))
                {
                    if ((newName.Length > oldName.Length && (node.TotalEntrySize - node.UsedEntrySize) > newName.Length) || newName.Length <= oldName.Length)
                    {
                        node.UsedEntrySize += (uint)(newName.Length - oldName.Length);

                        string theName;
                        if (node.attributes.ElementAt(2) is Data)
                        {
                            string[] split = ((FileName)node.attributes.ElementAt(1)).Filename.Split('.');
                            split[0] = newName;
                            theName = split[0] + "." + split[1];
                        }
                        else
                            theName = newName;

                        FileName fn = new FileName();
                        fn.CreateFileNameFile(theName);
                        fn.CreationTime = ((FileName)node.attributes.ElementAt(1)).CreationTime;
                        fn.AllocatedSize = ((FileName)node.attributes.ElementAt(1)).AllocatedSize;
                        fn.RealSize = ((FileName)node.attributes.ElementAt(1)).RealSize;
                        node.attributes[1] = fn;
                    }
                    break;
                }
        }

        public void printData(FileRecordSegmentHeader folder, string name)
        {
            foreach (var node in ((IndexRoot)folder.attributes.ElementAt(2)).Children)
                if (((FileName)node.attributes.ElementAt(1)).Filename.Equals(name))
                {
                    Console.WriteLine();
                    Console.WriteLine("About the MFT entry (STANDARD_INFORMATION + FILE_NAME + DATA):");
                    Console.WriteLine("Entry was created : {0}", ((StandardInformation)node.attributes.ElementAt(0)).CreationTime);
                    Console.WriteLine("Entry was last modified : {0}", ((StandardInformation)node.attributes.ElementAt(0)).ModifiedTime);
                    Console.WriteLine("Entry was last accessed : {0}", ((StandardInformation)node.attributes.ElementAt(0)).AccessTime);
                    Console.WriteLine("MFT for Entry was last changed : {0}", ((StandardInformation)node.attributes.ElementAt(0)).MFTChangeTime);
                    Console.WriteLine();

                    Console.WriteLine("About the DATA:");
                    Console.WriteLine("File was created : {0}", ((FileName)node.attributes.ElementAt(1)).CreationTime);
                    Console.WriteLine("File was last modified : {0}", ((FileName)node.attributes.ElementAt(1)).ModifiedTime);
                    Console.WriteLine("File was last accessed : {0}", ((FileName)node.attributes.ElementAt(1)).AccessTime);
                    Console.WriteLine("MFT for file was last changed : {0}", ((FileName)node.attributes.ElementAt(1)).MFTChangeTime);
                    Console.WriteLine("Real size : {0}", ((FileName)node.attributes.ElementAt(1)).RealSize);
                    Console.WriteLine("Allocated size : {0}", ((FileName)node.attributes.ElementAt(1)).AllocatedSize);
                    Console.WriteLine();

                    Console.WriteLine("Starting from the MFT header for this file (starting address is a local 0):");
                    Console.WriteLine("STANDARD_INFORMATION offset: 24B");
                    Console.WriteLine("FILE_NAME offset: {0}B", 24 + ((StandardInformation)node.attributes.ElementAt(0)).RecordLength);
                    Console.WriteLine("DATA offset: {0}B", 24 + ((StandardInformation)node.attributes.ElementAt(0)).RecordLength + ((FileName)node.attributes.ElementAt(1)).RecordLength);
                    Console.WriteLine();

                    break;
                }
        }

        //private void ReadBoot()
        //{
        //    byte[] data = new byte[512];
        //    diskStream.Seek(0, SeekOrigin.Begin);
        //    diskStream.Read(data, 0, data.Length);

        //    boot = BootSector.DataReader(data, data.Length, 0);         // parsing boot

        //    BytesPerFileRecord = boot.ExtenedBPB.ClustersPerMFTRecord;
        //    sectorsPerRecord = boot.ExtenedBPB.ClustersPerMFTRecord / boot.BIOSParameterBlock.BytesPerSector;
        //}

        //private void ReadMFT()
        //{
        //    buffer = new byte[BytesPerFileRecord];
        //    diskStream.Seek((long)(boot.ExtenedBPB.MFTClusterNumber * BytesPerCluster), SeekOrigin.Begin);
        //    diskStream.Read(buffer, 0, buffer.Length);

        //    FileRecordSegmentHeader record = FileRecordSegmentHeader.ReadFileRecord(buffer, 0, boot.BIOSParameterBlock.BytesPerSector, sectorsPerRecord);
        //    mftHeaderRecord = record;

        //    // mftStream = new NtfsDiskStream(diskStream, BytesPerCluster);
        //    CurrentMftRecordNumber = 0;
        //    FileRecordCount = (uint)(mftStream.Length / BytesPerFileRecord);
        //}

        public FileRecordSegmentHeader ReadNextRecord()
        {
            uint newPosition = CurrentMftRecordNumber * BytesPerFileRecord;
            if (mftStream.Position != newPosition)
                mftStream.Seek(newPosition, SeekOrigin.Begin);

            if (mftStream.Read(buffer, 0, buffer.Length) == 0)
                return null;

            FileRecordSegmentHeader record = FileRecordSegmentHeader.ReadFileRecord(buffer, 0, boot.BIOSParameterBlock.BytesPerSector, sectorsPerRecord);
            CurrentMftRecordNumber = record.MFTNumber + 1;

            return record;
        }

        public IEnumerable<FileRecordSegmentHeader> GetRecord()
        {
            while (true)
            {
                FileRecordSegmentHeader record = ReadNextRecord();

                if (record == null)
                    break;

                yield return record;
            }
        }
    }
}
