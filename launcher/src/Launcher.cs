using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
[DataContract] sealed class Feed { [DataMember] public string Version,Url,Sha256,Executable,Notes; [DataMember] public long Bytes; }
[DataContract] sealed class Settings { [DataMember] public string Repository="",ManifestUrl="",InstallRoot=""; }
static class Data {
 public static T Read<T>(string s){using(var m=new MemoryStream(Encoding.UTF8.GetBytes(s)))return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(m);}
 public static string Write<T>(T o){using(var m=new MemoryStream()){new DataContractJsonSerializer(typeof(T)).WriteObject(m,o);return Encoding.UTF8.GetString(m.ToArray());}}
 public static void Atomic(string path,string value){string temp=path+".tmp";File.WriteAllText(temp,value);if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}
}
sealed class Installer {
 public string Root;public const long Limit=4L*1024*1024*1024;
 public Installer(string root){Root=Path.GetFullPath(root);Directory.CreateDirectory(Root);}
 public string State{get{return Path.Combine(Root,"current.json");}}
 public Feed Current(){try{return Data.Read<Feed>(File.ReadAllText(State));}catch{return null;}}
 public static void Validate(Feed f){Version v;if(f==null||!Version.TryParse(f.Version,out v)||v.Major<0)throw new IOException("Release needs a numeric version, for example 0.2.0.");Uri u;if(!Uri.TryCreate(f.Url,UriKind.Absolute,out u)||u.Scheme!="https")throw new IOException("Build download must use HTTPS.");if(f.Sha256==null||f.Sha256.Length!=64||!System.Text.RegularExpressions.Regex.IsMatch(f.Sha256,"^[a-fA-F0-9]{64}$"))throw new IOException("Invalid SHA256.");if(f.Bytes<1||f.Bytes>Limit)throw new IOException("Build exceeds download limit.");SafePath("C:\\package",f.Executable);if(!f.Executable.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))throw new IOException("Missing game executable.");}
 public static string SafePath(string root,string relative){if(string.IsNullOrWhiteSpace(relative)||Path.IsPathRooted(relative)||relative.Contains(":")||relative.Contains("\\")||relative.IndexOf('\0')>=0)throw new IOException("Unsafe package path.");string full=Path.GetFullPath(Path.Combine(root,relative.Replace('/',Path.DirectorySeparatorChar))),prefix=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;if(!full.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new IOException("Package path escapes install folder.");return full;}
 public string GamePath(Feed f){return SafePath(Path.Combine(Root,"versions",f.Version),f.Executable);}
 public void Extract(string zip,string stage,Feed feed){Directory.CreateDirectory(stage);long total=0;var seen=new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);using(var archive=ZipFile.OpenRead(zip)){if(archive.Entries.Count>50000)throw new IOException("Too many package files.");foreach(var e in archive.Entries){if((e.ExternalAttributes>>16&0xF000)==0xA000)throw new IOException("Package links are not allowed.");string dest=SafePath(stage,e.FullName);if(!seen.Add(dest))throw new IOException("Duplicate package path.");if(e.FullName.EndsWith("/")){Directory.CreateDirectory(dest);continue;}total+=e.Length;if(total>Limit*2)throw new IOException("Expanded package too large.");Directory.CreateDirectory(Path.GetDirectoryName(dest));e.ExtractToFile(dest,false);}}if(!File.Exists(SafePath(stage,feed.Executable)))throw new IOException("Game executable is missing from package.");}
 public void Install(Feed f,string zip){Validate(f);if(new FileInfo(zip).Length!=f.Bytes)throw new IOException("Download size mismatch.");string hash;using(var sha=SHA256.Create())using(var stream=File.OpenRead(zip))hash=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","");if(!hash.Equals(f.Sha256,StringComparison.OrdinalIgnoreCase))throw new IOException("Download checksum mismatch.");string versions=Path.Combine(Root,"versions");Directory.CreateDirectory(versions);string stage=Path.Combine(versions,"stage-"+Guid.NewGuid().ToString("N")),dest=Path.Combine(versions,f.Version);try{Extract(zip,stage,f);if(Directory.Exists(dest))throw new IOException("This version already exists. Publish a new version number.");Directory.Move(stage,dest);Data.Atomic(State,Data.Write(f));}finally{if(Directory.Exists(stage))Directory.Delete(stage,true);}}
}
