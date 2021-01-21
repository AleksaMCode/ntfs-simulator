# NTFS simulator

##Commands
Detailed explanation of the individual commands.

COMMAND | NAME | SYNOPSIS | DESCRIPTION | OPTIONS 
| --- | --- | --- | --- | :---:
MKDIR | mkdir - creates a new directory if it does not already exist. | **mkdir** *directory* | Create the directorie specified by the operand | x
CREATE | create - creates a new fle if it does not already exist. | **create** filename | | x
PUT | | | | x
GET | | | | x
LS | | | | x
MOVE | | | | x
RENAME | | | | x
ECHO | | | | x
CP | | | | x
CAT | | | | x
RM | | | | x
STAT | stat - shows information about the file | **stat** *filename* | Show detail information about the file including information from file headers and information about the MFT entry. | x
CD | cd - change the working directory | **cd** *path*<br>**cd** .. | Change the working directory of the current "shell execution environment".<br/><ul><li>If no *path* operand is given, error message "*Command has wrong number of arguments!*" will be displayed.</li><li>If the *path* operand is dot-dot, the current path will be changed to the previous subdirectory.</li></ul> | x
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
  - [ ] Remove file and file system limits.
- [ ] Implement journaling.
- [ ] Implement file system logging.
- [ ] Implement MFT mirror.
- [ ] Implement Bitmaps and Bad clusers detection.
- [ ] Remove max. limit of file system depth.