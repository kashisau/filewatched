using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace filewatched
{
  // A class that is used to scan the contents of a directory, making a list of
  // all the existing files and returning them as a flat List.
  class FileScanner
  {
    private string path;
    private List<string> files;
    private ILogger _logger;
    public FileScanner(string path, ILogger logger)
    {
      this._logger = logger;
      this.path = path;
    }

    public async Task<List<string>> Scan()
    {
      try 
      {
          var fileEnumeration = Task.Run(() =>
          {
            var watch = new Stopwatch();
            watch.Start();

            // Obtain the file system entries in the directory path.
            string[] directoryEntries =
                Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories); 

            watch.Stop();
            _logger.LogInformation($"Enumerated {directoryEntries.Length} in {watch.ElapsedMilliseconds/1000} seconds.");
            files = new List<string>(directoryEntries);
            return files;
          });
          return await fileEnumeration;
      }
      catch (ArgumentNullException) 
      {
          System.Console.WriteLine("Path is a null reference.");
      }
      catch (System.Security.SecurityException) 
      {
          System.Console.WriteLine("The caller does not have the " +
              "required permission.");
      }
      catch (ArgumentException) 
      {
          System.Console.WriteLine("Path is an empty string, " +
              "contains only white spaces, " + 
              "or contains invalid characters.");
      }
      catch (System.IO.DirectoryNotFoundException) 
      {
          System.Console.WriteLine("The path encapsulated in the " + 
              "Directory object does not exist.");
      }
      return null;
    }
  }
}