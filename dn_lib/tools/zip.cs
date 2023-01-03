using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;

namespace dn_lib.tools {
  public class zip {

    public static GZipStream open_zip(string zip_file) { 
      FileStream outFile = new FileStream(zip_file, FileMode.Create, FileAccess.Write, FileShare.None);
      return new GZipStream(outFile, CompressionMode.Compress);
    }

    public static void add_zip_folder(GZipStream str, string in_dir, string rel_path_zip = "", List<string> out_folders = null) {
      string[] files = Directory.GetFiles(in_dir, "*.*", SearchOption.AllDirectories);
      int dlen = in_dir[in_dir.Length - 1] == Path.DirectorySeparatorChar ? in_dir.Length : in_dir.Length + 1;
      foreach (string fp in files) {
        string folder = Path.GetDirectoryName(fp);
        if (out_folders != null && out_folders.FirstOrDefault(s => folder.ToLower().StartsWith(s.ToLower())) != null) 
          continue;
        add_zip_file(str, in_dir, fp.Substring(dlen), rel_path_zip != "" ? Path.Combine(rel_path_zip, fp.Substring(dlen)) : "");
      }
    }

    public static void zip_folder(string in_dir, string out_file) {
      GZipStream str = open_zip(out_file);
      try {
        add_zip_folder(str, in_dir);
      } finally { str.Close(); }
    }

    public static void unzip(string zip_file, string out_folder, string likes = "") {
      using (FileStream inFile = new FileStream(zip_file, FileMode.Open, FileAccess.Read, FileShare.None))
      using (GZipStream zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
        while (unzip_file(out_folder, zipStream, likes)) ;
    }

    #region internals

    public static void add_zip_file(GZipStream str, string in_dir, string rel_path, string rel_path_zip = "") {
      // Compress file name
      char[] chars = rel_path_zip != "" ? rel_path_zip.ToCharArray() : rel_path.ToCharArray();
      str.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
      foreach (char c in chars)
        str.Write(BitConverter.GetBytes(c), 0, sizeof(char));

      // Compress file content
      byte[] bytes = File.ReadAllBytes(Path.Combine(in_dir, rel_path));
      str.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
      str.Write(bytes, 0, bytes.Length);
    }

    protected static bool unzip_file(string sdir, GZipStream zip_str, string likes = "") {
      //Decompress file name
      byte[] bytes = new byte[sizeof(int)];
      int Readed = zip_str.Read(bytes, 0, sizeof(int));
      if (Readed < sizeof(int))
        return false;

      int iNameLen = BitConverter.ToInt32(bytes, 0);
      bytes = new byte[sizeof(char)];
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < iNameLen; i++) {
        zip_str.Read(bytes, 0, sizeof(char));
        char c = BitConverter.ToChar(bytes, 0);
        sb.Append(c);
      }
      string sFileName = sb.ToString();
      //if (progress != null)
      //    progress(sFileName);

      //Decompress file content
      bytes = new byte[sizeof(int)];
      zip_str.Read(bytes, 0, sizeof(int));
      int iFileLen = BitConverter.ToInt32(bytes, 0);

      bytes = new byte[iFileLen];
      zip_str.Read(bytes, 0, bytes.Length);

      if (likes != "") {
        string[] splitted = likes.Split(';');
        foreach (string split in splitted)
          if (!strings.like(sFileName, split)) return true;
      }

      string sFilePath = Path.Combine(sdir, sFileName);
      string sFinalDir = Path.GetDirectoryName(sFilePath);
      if (!Directory.Exists(sFinalDir))
        Directory.CreateDirectory(sFinalDir);

      using (FileStream outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        outFile.Write(bytes, 0, iFileLen);

      return true;
    }

    #endregion
  }
}



