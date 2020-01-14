using System;
using System.Collections.Generic;

[Serializable]
public class ServerFiles
{
  public string ServerPath { get; set; }
  public List<string> Files { get; set; }
}
