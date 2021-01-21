# NTFS simulator

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