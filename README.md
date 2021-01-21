# NTFS simulator
## Commands
Detailed explanation of the individual commands.

COMMAND | NAME | SYNOPSIS | DESCRIPTION | OPTIONS 
| --- | --- | --- | --- | :---:
MKDIR | mkdir - creates a new directory if it does not already exist. | **mkdir** *DIRECTORY* | Create the *DIRECTORY* specified by the operand | x
CREATE | create - creates a new file if it does not already exist. | **create** *FILE* | Update the access and modification times of each FILE to the current time.<br> FILE argument that does not exist is created empty. | x
PUT | put - "uploads" a file from Windows fs to in-memory fs | **put** *FILE* | Copies the file from specified path to RAM memory. | x
GET | get - "downloads" a file from in-memory fs to fs on Windows | **get** *SOURCE* *DEST* | Copies the SOURCE from specified RAM memory to a file specified by the DEST (path + name + extension). | x
LS |  ls - list directory contents | **ls** | List information about the current directory. Sort entries alphabetically. | x
CP | cp - copy file | **cp** *FILE* *DIRECTORY* | Copy SOURCE to DIRECTORY inside of the in-memory fs. | x
MV | mv -move file | **mv** *FILE* *DIRECTORY*  | Moves SOURCE to DIRECTORY inside of the in-memory fs. | x
CAT | cat - print file on the standard output  | **cat** *FILE* | Concatenate FILE to standard output. | x
RENAME | rename - rename file or directory | **rename** *SOURCE* *DEST* | Rename SOURCE to DEST. | x
ECHO | echo - write string(s) to file | **echo** *FILE* *STRING* | Echo the STRING to the FILE. Creates/overwrites the FILE. | x
RM | rm - remove files or directories | **rm** [*OPTION*] FILE | **rm** removes each specified file. By default, it does not remove directories. | <p align="justify"><b>-f</b><br>remove directories and their contents recursively</p>
STAT | stat - shows information about the file | **stat** *FILE* | Show detail information about the file including information from file headers and information about the MFT entry. | x
CD | cd - change the working directory | **cd** *DEST* <br>**cd** .. | Change the working directory of the current "shell execution environment".<br/><ul><li>If no *path* operand is given, error message "*Command has wrong number of arguments!*" will be displayed.</li><li>If the *path* operand is dot-dot, the current path will be changed to the previous subdirectory.</li></ul> | x
CLS | cls - reset the terminal | **cls** | Past inputs are deleted. | x
EXIT | exit - cause program termination | **exit** | Cause normal process termination. | x

## Limitations
<p align="justify">As I have stated previously, this is an in-memory implementation of simplified NTFS. Project is mainly focuses around MFT (<a href="https://en.wikipedia.org/wiki/NTFS#Master_File_Table">Master File Table</a>) and it ignores journaling, security descriptors and other similar topics. It also only supports <a href="https://en.wikipedia.org/wiki/NTFS#Resident_vs._non-resident_attributes">resident files</a>, which is why the <a href="https://en.wikipedia.org/wiki/B-tree">B-tree</a> storage of large file storage isn't implemented. All directory records reside entirely within MFT structure.
<ol>
    <li><p align="justify">Complete file system is located within a single binary file on the existing file system when not used. When used whole binary file is loaded into memory.</p></li>
    <li><p align="justify">File system consists of any number of files and directories organized in a maximum level depth of two (e.q. <i>root/dir1/file1)</i>.</p></li>
    <li><p align="justify">Access to files/directories on the file system is always done via absolute paths (e.q. <i>root/dir1/file1</i>).</p></li>
    <li><p align="justify">Maximum file system size is 20 MB, and the maximum file size on a file system is 64 KB.</p></li>
    <li><p align="justify">The contents of the file is kept in blocks of minimum size of 5 B, while keeping the fragmentation to a minimum.</p></li>
</ol>

## To-Do List
- [ ] Implement non-resident large file storage using B-tree.
  - [ ] Remove arbitrary file and file system limits.
- [ ] Implement journaling.
  - [ ] Implement file system logging.
- [ ] Implement MFT mirror.
- [ ] Implement Bitmaps and Bad clusers detection.
- [ ] Remove max. limit of file system depth.
- [ ] Implement file compression.
- [ ] Expand the available command list.
- [ ] Fix **rm** command.

## References
<ul>
    <li><p align="justify">Richard Russon, Yuval Fledel - <i>NTFS Documentation</i></p></li>
    <li><p align="justify">Michael Wilkinson - <i>NTFS Reference Sheet</i></p></li>
    <li><p align="justify">William Stallings - <i>Operating Systems - Internals and Design Principles</i></p></li>
    <li><p align="justify">Abraham Silberschatz, Peter Baer Galvin, Greg Gagne - <i>Operating System Concepts</i></p></li>
    <li><p align="justify">Andrew S. Tanenbaum, Herbert Bos - <i>Modern Operating Systems</i></p></li>
    <li><p align="justify">Harald Baier, Bjorn Roos - <i>Analysis of an NTFS File System</i></p></li>
    <li><p align="justify"><a href="http://technet.microsoft.com/en-us/library/cc781134(WS.10).aspx">How NTFS Works</a></p></li>
    <li><p align="justify"><a href="http://technet.microsoft.com/en-us/library/cc976808.aspx">The NTFS File System</a></p></li>
    <li><p align="justify"><a href="http://technet.microsoft.com/en-us/library/cc976796.aspx">Boot Sector</a></p></li>
    <li><p align="justify"><a href="https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-2000-server/cc976786(v=technet.10)">Master Boot Record</a></p></li>