using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


public class HD_Checksum
{
    public string hdNumber { get; set; }
    public string path { get; set; }
    public File_Checksum[] fc { get; set; }
}

public class File_Checksum
{
    public string fileName { get; set; }
    public string checksum { get; set; }
    
}