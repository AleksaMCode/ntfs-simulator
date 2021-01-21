using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using FileSystem.Attributes;
using FileSystem.Attributes.Indexroot;

namespace FileSystem
{
    public class Program
    {
        private enum choice { mkdir, create, put, get, ls, cp, mv, rename, echo, cat, rm, stat, cd, cd_remove, cls, exit, wrong_command };
        //private static readonly string[] choices = { "mkdir", "create", "put", "get", "ls", "cp", "mv", "rename", "echo", "cat", "rm", "start", "cd", "cd..", "exit" };
        private static string message1 = "Press Enter to continue.";
        private static readonly List<Tuple<string, int, int>> choices = new List<Tuple<string, int, int>> // <command name, min.arg, max.arg> 
        {
            Tuple.Create("mkdir", 1, 2),   // mkdir name
            Tuple.Create("create", 1, 1),  // create filename sa *.extension
            Tuple.Create("put", 1, 1),     // put location sa filename-om w/ *.extension
            Tuple.Create("get", 2, 2),     // get location sa filename-om w/ *.extension filename sa *.extension
            Tuple.Create("ls", 0, 0),      // ls
            Tuple.Create("cp", 2, 2),      // cp filename.extension location
            Tuple.Create("mv", 2, 2),      // mv filename.extension location
            Tuple.Create("rename", 2, 2),  // rename filename.extension/directoryname newname
            Tuple.Create("echo", 2, 2),    // echo filename.txt text
            Tuple.Create("cat", 1, 1),     // cat filename.txt
            Tuple.Create("rm", 1, 1),      // rm name
            Tuple.Create("stat", 1, 1),    // stat filename w/ *.extension
            Tuple.Create("cd", 1, 1),      // cd location
            Tuple.Create("cd..", 0, 0),    // cd..
            Tuple.Create("cls", 0, 0),     // cld
            Tuple.Create("exit", 0, 0)     // exit
        };

        //static void MessageWrite(string str)
        //{
        //    Console.Write(">" + "{0} {1}\n>", str, message1);
        //    Console.ReadLine();
        //}

        static void CreateDirectory(FileRecordSegmentHeader parentFolder, FileRecordSegmentHeader childFolder, string name, NtfsFileSystem fs)
        {
            childFolder.CreateFileRecordHeader(Enums.MFTEntryFlags.FileNameIndexPresent, parentFolder);

            StandardInformation stdInfo = new StandardInformation();
            stdInfo.CreateStandInfoFile(FileAttributes.Normal);
            childFolder.UsedEntrySize += stdInfo.RecordLength;

            IndexRoot indRoot = new IndexRoot();
            //childFolder.UsedEntrySize += indRoot.RecordLength;

            FileName fileName = new FileName();
            fileName.CreateFileNameFile(name);
            fileName.RealSize = fileName.AllocatedSize = 0;
            childFolder.UsedEntrySize += fileName.RecordLength;

            if (childFolder.UsedEntrySize >= (fs.VolumeSize - fs.BytesOccupied + 8))
            {
                Console.WriteLine("Volume is full!");
                Save(fs.VolName.Name, fs.Save());
                Environment.Exit(0);
            }

            childFolder.attributes.Add(stdInfo);
            childFolder.attributes.Add(fileName);
            childFolder.attributes.Add(indRoot);

            ((IndexRoot)parentFolder.attributes.ElementAt(2)).Children.Add(childFolder);
            ((IndexRoot)parentFolder.attributes.ElementAt(2)).numberOfChildren++;
            childFolder.parent = parentFolder;

            fs.BytesOccupied += childFolder.UsedEntrySize + 8;
            fs.DirectoryRecordCount++;
            parentFolder.UsedFolderSize += childFolder.UsedEntrySize;
        }

        static void CreateFile(FileRecordSegmentHeader parentFolder, FileRecordSegmentHeader childFile, string name, NtfsFileSystem fs)
        {
            childFile.CreateFileRecordHeader(Enums.MFTEntryFlags.FileRecordSegmentInUse, parentFolder);

            StandardInformation stdInfo = new StandardInformation();
            stdInfo.CreateStandInfoFile(FileAttributes.Normal);
            childFile.UsedEntrySize += stdInfo.RecordLength;

            FileName fileName = new FileName();
            fileName.CreateFileNameFile(name);
            childFile.UsedEntrySize += fileName.RecordLength;

            fileName.AllocatedSize = childFile.TotalEntrySize - childFile.UsedEntrySize;
            fileName.RealSize = 0;
            childFile.UsedEntrySize += (uint)fileName.RealSize;

            Data data = new Data();

            childFile.attributes.Add(stdInfo);
            childFile.attributes.Add(fileName);
            childFile.attributes.Add(data);

            if (childFile.TotalEntrySize < (fs.VolumeSize - fs.BytesOccupied) + 8) // +8 for reference in folder
            {
                fs.mftHeaderRecord.Add(childFile);

                ((FileSystem.Attributes.Indexroot.IndexRoot)parentFolder.attributes.ElementAt(2)).Children.Add(childFile);
                ((FileSystem.Attributes.Indexroot.IndexRoot)parentFolder.attributes.ElementAt(2)).numberOfChildren++;
                childFile.parent = parentFolder;
                fs.FileRecordCount++;
                fs.BytesOccupied += 8;  // one child added
                fs.BytesOccupied += childFile.TotalEntrySize;
                parentFolder.UsedFolderSize += childFile.TotalEntrySize;
            }
            else
            {
                Console.WriteLine("Volume is full!");
                Save(fs.VolName.Name, fs.Save());
                Environment.Exit(0);
            }
        }

        static void Save(string fsName, byte[] data)
        {
            using (FileStream stream = new FileStream(fsName + ".bin", FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                    writer.Write(data);
            }
        }

        static void Main(string[] args)
        {
            string command, fsName, subDir1, subDir2, currentDir;
            subDir1 = subDir2 = string.Empty;

            while (true)
            {
                Console.Clear();
                Console.Write(">" + "Enter Filesystem name!\n>");
                command = Console.ReadLine();
                //command.Substring(command.IndexOf(">"));
                if (Regex.IsMatch(command, @"^[A-Za-z0-9]+$"))
                {
                    fsName = command;
                    break;
                }
                else
                {
                    Console.Write(">" + "FS name is not valid!" + message1);
                    Console.ReadLine();
                }
            }
            NtfsFileSystem ntfsFS = new NtfsFileSystem(fsName);
            currentDir = fsName;
            var currectFolder = ntfsFS.Root;

            Console.Clear();
            while (true)
            {
                Console.Write("{0}:\\{1}{2}>", fsName, subDir1 == string.Empty ? subDir1 : subDir1 + "\\", subDir2);
                string[] textCatch = Console.ReadLine().Split('"');     // e.q. textCatch[1] is "text for input."
                string[] tokens = (textCatch[0].ToLower()).Split(' ');  //string[] tokens = command.Split(' ');
                command = tokens[0];
                command = command.Trim();
                bool toContinue = true;
                choice selected = choice.wrong_command;

                {
                    int i = 0;
                    foreach (var ch in choices)
                    {
                        if (command.Equals(choices[i++].Item1))
                            selected = (choice)(--i);

                        if (ch.Item1.Equals(command) && (tokens.Length - 1 >= ch.Item2 && tokens.Length - 1 <= ch.Item3))
                        {
                            toContinue = false;
                            break;
                        }
                    }
                }

                if (toContinue)
                {
                    Console.Write("{0}:\\>{1} {2}\n", fsName, "Command has wrong number of arguments!", message1);
                    //Console.ReadLine();
                    continue;
                }

                //for (int i = 0; i < (int)choice.cd_remove; ++i)
                //    if (command.Equals(choices[i].Item1))
                //    {
                //        selected = (choice)i;
                //        break;
                //    }

                if (selected != choice.wrong_command)
                {

                    switch (selected)
                    {
                        case choice.mkdir:
                            {
                                if (!(subDir1 != String.Empty && subDir2 != String.Empty))
                                {
                                    FileRecordSegmentHeader mkFolder = new FileRecordSegmentHeader();
                                    CreateDirectory(currectFolder, mkFolder, tokens[1], ntfsFS);
                                }
                                else
                                    Console.WriteLine("Allowed directory depth is 2. Can't make {0} directory!", tokens[1]);

                                break;
                            }
                        case choice.create:
                            {
                                FileRecordSegmentHeader createFile = new FileRecordSegmentHeader();
                                CreateFile(currectFolder, createFile, tokens[1], ntfsFS);

                                break;
                            }
                        case choice.put:
                            {
                                //if (Directory.Exists(tokens[1]))
                                using (var stream = new FileStream(tokens[1], FileMode.Open))
                                {
                                    if (stream.Length <= 64_000)
                                    {
                                        try
                                        {
                                            FileRecordSegmentHeader header = new FileRecordSegmentHeader();
                                            FileRecordSegmentHeader parent = currectFolder;

                                            header.CreateFileRecordHeader(Enums.MFTEntryFlags.FileRecordSegmentInUse, parent);

                                            StandardInformation stdInfo = new StandardInformation();
                                            stdInfo.CreateStandInfoFile(FileAttributes.Normal);
                                            header.UsedEntrySize += stdInfo.RecordLength;

                                            FileName fileName = new FileName();
                                            fileName.CreateFileNameFile(Path.GetFileName(tokens[1]));
                                            header.UsedEntrySize += fileName.RecordLength;

                                            if (stream.Length <= (header.TotalEntrySize - header.UsedEntrySize))
                                            {
                                                fileName.AllocatedSize = header.TotalEntrySize - header.UsedEntrySize;
                                                fileName.RealSize = (ulong)stream.Length;
                                                header.UsedEntrySize += (uint)fileName.RealSize;

                                                Data data = new Data();
                                                var mem = new MemoryStream();
                                                stream.CopyTo(mem);
                                                data.DataBytes = mem.ToArray();

                                                header.attributes.Add(stdInfo);
                                                header.attributes.Add(fileName);
                                                header.attributes.Add(data);

                                                if (header.TotalEntrySize < (ntfsFS.VolumeSize - ntfsFS.BytesOccupied) + 8) // +8 for reference in folder
                                                {
                                                    ntfsFS.mftHeaderRecord.Add(header);

                                                    ((FileSystem.Attributes.Indexroot.IndexRoot)parent.attributes.ElementAt(2)).Children.Add(header);
                                                    ((FileSystem.Attributes.Indexroot.IndexRoot)parent.attributes.ElementAt(2)).numberOfChildren++;
                                                    header.parent = parent;

                                                    ntfsFS.FileRecordCount++;
                                                    ntfsFS.BytesOccupied += 8;  // one child added
                                                    ntfsFS.BytesOccupied += header.TotalEntrySize;
                                                    parent.UsedFolderSize += header.TotalEntrySize;
                                                }
                                                else
                                                {
                                                    Save(fsName, ntfsFS.Save());
                                                    throw new Exception("Volume is full!");
                                                }
                                            }
                                            else
                                                Console.WriteLine("File is too large!");
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            Environment.Exit(0);
                                        }
                                    }
                                    else
                                        Console.WriteLine("File is larger than 64 KB!");
                                }
                                //else
                                //    Console.WriteLine("Directory '{0}' doesn't exist!", tokens[1]);

                                break;
                            }
                        case choice.get:
                            {
                                //FileInfo fileInfo = new FileInfo(tokens[1]);
                                //using (FileStream fs = fileInfo.Create())
                                //{
                                byte[] data = ntfsFS.searchDataInDirectory(currectFolder, tokens[2]);
                                if (data != null)
                                    File.WriteAllBytes(tokens[1], data);
                                else
                                    Console.WriteLine("File is not located in the current folder!");

                                break;
                            }
                        case choice.ls:
                            {
                                Console.WriteLine();
                                foreach (var child in ((FileSystem.Attributes.Indexroot.IndexRoot)currectFolder.attributes.ElementAt(2)).Children)
                                {
                                    if (child.Flags == Enums.MFTEntryFlags.FileRecordSegmentInUse)
                                        Console.WriteLine(((StandardInformation)child.attributes.ElementAt(0)).CreationTime.ToString() + "    " + "    " +
                                           child.UsedEntrySize.ToString("N1") + " " + ((FileName)child.attributes.ElementAt(1)).Filename);
                                    else
                                        Console.WriteLine(((StandardInformation)child.attributes.ElementAt(0)).CreationTime.ToString() + "    " + "<DIR>" +
                                            "     " + ((FileName)child.attributes.ElementAt(1)).Filename);
                                }
                                Console.WriteLine();
                                break;
                            }
                        case choice.cp:
                            {
                                try
                                {
                                    // get the file
                                    FileRecordSegmentHeader fileToCpy = null;
                                    FileRecordSegmentHeader header = new FileRecordSegmentHeader();
                                    FileRecordSegmentHeader parent = null;

                                    foreach (var child in ((FileSystem.Attributes.Indexroot.IndexRoot)currectFolder.attributes.ElementAt(2)).Children)
                                        if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(tokens[1]))
                                        {
                                            fileToCpy = child;
                                            break;
                                        }

                                    if (fileToCpy != null)
                                    {
                                        string[] splitDestination = tokens[2].Split('\\');
                                        if (splitDestination.Length > 2)
                                            Console.WriteLine("Moving failed!");
                                        else
                                        {
                                            FileRecordSegmentHeader moveFolder = null;
                                            FileRecordSegmentHeader tmp = null;
                                            foreach (var child in ((FileSystem.Attributes.Indexroot.IndexRoot)currectFolder.parent.attributes.ElementAt(2)).Children)
                                                if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(splitDestination[0]))
                                                {
                                                    tmp = moveFolder = child;
                                                    break;
                                                }

                                            if (moveFolder != null)
                                            {
                                                if (splitDestination.Length == 2)
                                                {
                                                    foreach (var child in ((FileSystem.Attributes.Indexroot.IndexRoot)tmp.attributes.ElementAt(2)).Children)
                                                        if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(splitDestination[1]))
                                                        {
                                                            moveFolder = child;
                                                            break;
                                                        }

                                                    if ((tmp != moveFolder && splitDestination.Length == 2) || (tmp == moveFolder && splitDestination.Length == 1))
                                                    {
                                                        //((FileSystem.Attributes.Indexroot.IndexRoot)currectFolder.attributes.ElementAt(2)).Children.Remove(moveFolder);
                                                        //((FileSystem.Attributes.Indexroot.IndexRoot)moveFolder.attributes.ElementAt(2)).Children.Add(moveFolder);
                                                        //moveFolder.parent = moveFolder;                                                                                                                                                          

                                                        parent = moveFolder;
                                                        header.CreateFileRecordHeader(Enums.MFTEntryFlags.FileRecordSegmentInUse, parent);

                                                        StandardInformation stdInfo = new StandardInformation();
                                                        stdInfo.CreateStandInfoFile(FileAttributes.Normal);
                                                        header.UsedEntrySize += stdInfo.RecordLength;

                                                        FileName fileName = new FileName();
                                                        fileName.CreateFileNameFile(((FileName)fileToCpy.attributes.ElementAt(1)).Filename);
                                                        header.UsedEntrySize += fileName.RecordLength;

                                                        fileName.AllocatedSize = header.TotalEntrySize - header.UsedEntrySize;
                                                        fileName.RealSize = (ulong)((FileName)fileToCpy.attributes.ElementAt(1)).RealSize;
                                                        header.UsedEntrySize += (uint)fileName.RealSize;

                                                        Data data = new Data();
                                                        Array.Copy(((Data)fileToCpy.attributes.ElementAt(2)).DataBytes, 0, data.DataBytes, 0, ((Data)fileToCpy.attributes.ElementAt(2)).DataBytes.Length);


                                                        header.attributes.Add(stdInfo);
                                                        header.attributes.Add(fileName);
                                                        header.attributes.Add(data);

                                                        if (header.TotalEntrySize < (ntfsFS.VolumeSize - ntfsFS.BytesOccupied) + 8) // +8 for reference in folder
                                                        {
                                                            ntfsFS.mftHeaderRecord.Add(header);

                                                            ((FileSystem.Attributes.Indexroot.IndexRoot)parent.attributes.ElementAt(2)).Children.Add(header);
                                                            ((FileSystem.Attributes.Indexroot.IndexRoot)parent.attributes.ElementAt(2)).numberOfChildren++;
                                                            header.parent = parent;

                                                            ntfsFS.FileRecordCount++;
                                                            ntfsFS.BytesOccupied += 8;  // one child added
                                                            ntfsFS.BytesOccupied += header.TotalEntrySize;
                                                            parent.UsedFolderSize += header.TotalEntrySize;
                                                        }
                                                        else
                                                        {
                                                            Save(fsName, ntfsFS.Save());
                                                            throw new Exception("Volume is full!");
                                                        }
                                                    }

                                                    else if (tmp == moveFolder && splitDestination.Length == 2)
                                                        Console.WriteLine("Copying failed!");
                                                }
                                            }
                                            else
                                                Console.WriteLine("Copying failed!");
                                        }
                                    }
                                    else
                                        Console.WriteLine("Copying failed!");

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    Environment.Exit(0);
                                }

                                break;
                            }
                        case choice.mv:
                            {
                                string[] splitDestination = tokens[2].Split('\\');
                                if (splitDestination.Length > 2)
                                    Console.WriteLine("Moving failed!");
                                else
                                {
                                    FileRecordSegmentHeader movingFile = null;
                                    foreach (var child in ((FileSystem.Attributes.Indexroot.IndexRoot)currectFolder.attributes.ElementAt(2)).Children)
                                        if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(tokens[1]))
                                        {
                                            movingFile = child;
                                            break;
                                        }

                                    if (movingFile != null)
                                    {
                                        FileRecordSegmentHeader moveFolder = null;
                                        FileRecordSegmentHeader tmp = null;
                                        foreach (var child in ((FileSystem.Attributes.Indexroot.IndexRoot)currectFolder.parent.attributes.ElementAt(2)).Children)
                                            if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(splitDestination[0]))
                                            {
                                                tmp = moveFolder = child;
                                                break;
                                            }

                                        if (moveFolder != null)
                                        {
                                            if (splitDestination.Length == 2)
                                            {
                                                foreach (var child in ((FileSystem.Attributes.Indexroot.IndexRoot)tmp.attributes.ElementAt(2)).Children)
                                                    if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(splitDestination[1]))
                                                    {
                                                        moveFolder = child;
                                                        break;
                                                    }
                                            }
                                            if ((tmp != moveFolder && splitDestination.Length == 2) || (tmp == moveFolder && splitDestination.Length == 1))
                                            {
                                                ((FileSystem.Attributes.Indexroot.IndexRoot)currectFolder.attributes.ElementAt(2)).Children.Remove(movingFile);
                                                ((FileSystem.Attributes.Indexroot.IndexRoot)moveFolder.attributes.ElementAt(2)).Children.Add(movingFile);
                                                moveFolder.parent = moveFolder;
                                            }
                                            else if (tmp == moveFolder && splitDestination.Length == 2)
                                                Console.WriteLine("Moving failed!");
                                        }
                                        else
                                            Console.WriteLine("Moving failed!");
                                    }
                                    else
                                        Console.WriteLine("Moving failed!");
                                }

                                break;
                            }
                        case choice.rename:
                            {
                                ntfsFS.changeName(currectFolder, tokens[1], tokens[2]);
                                break;
                            }
                        case choice.echo:
                            {
                                try
                                {
                                    if (textCatch[1].Length < 64_000)
                                    {
                                        FileRecordSegmentHeader header = new FileRecordSegmentHeader();
                                        FileRecordSegmentHeader parent = currectFolder;

                                        header.CreateFileRecordHeader(Enums.MFTEntryFlags.FileRecordSegmentInUse, parent);

                                        StandardInformation stdInfo = new StandardInformation();
                                        stdInfo.CreateStandInfoFile(FileAttributes.Normal);
                                        header.UsedEntrySize += stdInfo.RecordLength;

                                        FileName fileName = new FileName();
                                        fileName.CreateFileNameFile(tokens[1]);
                                        header.UsedEntrySize += fileName.RecordLength;

                                        if (textCatch.Length <= (header.TotalEntrySize - header.UsedEntrySize))
                                        {
                                            fileName.AllocatedSize = header.TotalEntrySize - header.UsedEntrySize;
                                            fileName.RealSize = (ulong)textCatch.Length;
                                            header.UsedEntrySize += (uint)fileName.RealSize;

                                            Data data = new Data();
                                            //Array.Copy(textCatch, 0, data.DataBytes, 0, textCatch.Length);
                                            data.DataBytes = Encoding.ASCII.GetBytes(textCatch[1]);

                                            header.attributes.Add(stdInfo);
                                            header.attributes.Add(fileName);
                                            header.attributes.Add(data);

                                            if (header.TotalEntrySize < (ntfsFS.VolumeSize - ntfsFS.BytesOccupied) + 8) // +8 for reference in folder
                                            {
                                                ntfsFS.mftHeaderRecord.Add(header);

                                                ((FileSystem.Attributes.Indexroot.IndexRoot)parent.attributes.ElementAt(2)).Children.Add(header);
                                                ((FileSystem.Attributes.Indexroot.IndexRoot)parent.attributes.ElementAt(2)).numberOfChildren++;
                                                header.parent = parent;

                                                ntfsFS.FileRecordCount++;
                                                ntfsFS.BytesOccupied += 8;  // one child added
                                                ntfsFS.BytesOccupied += header.TotalEntrySize;
                                                parent.UsedFolderSize += header.TotalEntrySize;
                                            }
                                            else
                                            {
                                                Save(fsName, ntfsFS.Save());
                                                throw new Exception("Volume is full!");
                                            }
                                        }
                                        else
                                            Console.WriteLine("File is too large!");
                                    }
                                    else
                                        Console.WriteLine("Text file is larger than 64 KB!");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    Environment.Exit(0);
                                }

                                break;
                            }
                        case choice.cat:
                            {
                                if (tokens[1].Contains(".txt"))
                                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(ntfsFS.searchDataInDirectory(currectFolder, tokens[1])));
                                else
                                    Console.WriteLine("File is not *.txt!");
                                break;
                            }
                        case choice.rm:
                            {
                                FileRecordSegmentHeader deleteThisChild = null;
                                foreach (var child in ((IndexRoot)currectFolder.attributes.ElementAt(2)).Children)
                                    if (((FileName)child.attributes.ElementAt(1)).Filename.Equals(tokens[1]))
                                    {
                                        deleteThisChild = child;
                                        break;
                                    }


                                if (deleteThisChild != null)
                                {
                                    ntfsFS.BytesOccupied -= 8;
                                    ntfsFS.BytesOccupied -= deleteThisChild.TotalEntrySize;
                                    currectFolder.UsedFolderSize = deleteThisChild.TotalEntrySize;

                                    if (deleteThisChild.Flags == Enums.MFTEntryFlags.FileRecordSegmentInUse)
                                        ntfsFS.FileRecordCount--;
                                    else
                                        ntfsFS.DirectoryRecordCount--;

                                    ((IndexRoot)currectFolder.attributes.ElementAt(2)).Children.Remove(deleteThisChild);
                                    deleteThisChild.parent = null;
                                }
                                else
                                    Console.WriteLine("File/Folder not found!");

                                break;
                            }
                        case choice.stat:
                            {
                                ntfsFS.printData(currectFolder, tokens[1]);
                                break;
                            }
                        case choice.cd:
                            {
                                if (!currentDir.Equals(tokens[1]))
                                {
                                    var currectFolderNew = ntfsFS.FindFolder(currectFolder, tokens[1]);
                                    if (currectFolderNew != null)
                                    {
                                        currectFolder = currectFolderNew;
                                        if (currentDir.Equals(fsName))
                                            currentDir = subDir1 = tokens[1];
                                        else if (currentDir.Equals(subDir1))
                                            currentDir = subDir2 = tokens[1];
                                    }
                                    else
                                        Console.WriteLine("Directory '{0}' doesn't exist in '{1}'!", tokens[1], currentDir);
                                }
                                else
                                    Console.WriteLine("This is not possible!");

                                break;
                            }
                        case choice.cd_remove:
                            {
                                if (currentDir == fsName)
                                {
                                    // Save NtfsFileSystem to *.dat file
                                    Save(fsName, ntfsFS.Save());
                                    currentDir = string.Empty;
                                    Environment.Exit(0);
                                }
                                else if (subDir2 != string.Empty)
                                {
                                    var parentFolder = currectFolder.parent;
                                    currectFolder = parentFolder;

                                    subDir2 = string.Empty;
                                    currentDir = subDir1;
                                }
                                else
                                {
                                    currectFolder = ntfsFS.Root;
                                    subDir1 = string.Empty;
                                    currentDir = fsName;
                                }

                                break;
                            }
                        case choice.cls:
                            {
                                Console.Clear();
                                break;
                            }
                        case choice.exit:
                            {
                                Save(fsName, ntfsFS.Save());
                                Environment.Exit(0);
                                break;
                            }
                    }
                }
                else
                {
                    Console.Write("{0}:\\>{1} {2}\n", fsName, "Unknown command.", message1);
                    continue;
                }
            }
        }
    }
}
