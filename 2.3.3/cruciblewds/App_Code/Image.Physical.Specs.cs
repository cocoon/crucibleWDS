using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


public class Image_Physical_Specs
{
    public string image { get; set; }
    public HD_Physical_Specs[] hd { get; set; }
}

public class HD_Physical_Specs
{
    public string name { get; set; }
    public string size { get; set; }
    public string table { get; set; }
    public string boot { get; set; }
    public string lbs { get; set; }
    public string pbs { get; set; }
    public string active { get; set; }
    public string guid { get; set; }
    public Partition_Physical_Specs[] partition { get; set; }
}

public class Partition_Physical_Specs
{
    public string number { get; set; }
    public string start { get; set; }
    public string end { get; set; }
    public string size { get; set; }
    public string resize { get; set; }
    public string type { get; set; }
    public string used_mb { get; set; }
    public string fsid { get; set; }
    public string fstype { get; set; }
    public string uuid { get; set; }
    public string guid { get; set; }
    public string active { get; set; }
    public string size_override { get; set; }
    public VG_Physical_Specs vg { get; set; }
}

public class VG_Physical_Specs
{
    public string name { get; set; }
    public string size { get; set; }
    public string type { get; set; }
    public string pv { get; set; }
    public string uuid { get; set; }
    public LV_Physical_Specs[] lv { get; set; }
}
public class LV_Physical_Specs
{
    public string name { get; set; }
    public string size { get; set; }
    public string resize { get; set; }
    public string type { get; set; }
    public string vg { get; set; }
    public string used_mb { get; set; }
    public string fstype { get; set; }
    public string uuid { get; set; }
    public string active { get; set; }
    public string size_override { get; set; }
}


public class Partition_Resized_Client
{
    public string number { get; set; }
    public bool isBoot { get; set; }
    public string start { get; set; }
    public string size { get; set; }
    public string type { get; set; }
    public string fsid { get; set; }
    public string fstype { get; set; }
    public string uuid { get; set; }
    public string guid { get; set; }
    public bool partResized { get; set; }
}

public class Partition_Resized_Client_LVM
{
    public string name { get; set; }
    public string fstype { get; set; }
    public string size { get; set; }
    public string uuid { get; set; }
    public string vg { get; set; }
    public bool partResized { get; set; }
}

public class ExtendedPartition
{
    public long minSize_BLK { get; set; }
    public bool isOnlySwap { get; set; }
    public bool hasLogical { get; set; }
    public int logicalCount { get; set; }
}

public class VolumeGroup
{
    public string name { get; set; }
    public long minSize_BLK { get; set; }
    public bool hasLv { get; set; }
    public string pv { get; set; }
    public long agreedPVSize_BLK { get; set; }
}