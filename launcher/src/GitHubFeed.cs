using System;
using System.IO;
using System.Text.RegularExpressions;
static class GitHubFeed {
 public static string Resolve(string input){input=(input??"").Trim().TrimEnd('/');if(input.StartsWith("https://github.com/",StringComparison.OrdinalIgnoreCase))input=input.Substring(19);if(input.EndsWith(".git",StringComparison.OrdinalIgnoreCase))input=input.Substring(0,input.Length-4);if(!Regex.IsMatch(input,"^[A-Za-z0-9][A-Za-z0-9-]*/[A-Za-z0-9_.-]+$")||input.Contains(".."))throw new IOException("Enter a public GitHub repository as owner/repository or its HTTPS URL.");return "https://github.com/"+input+"/releases/latest/download/manifest.json";}
 public static void CheckBuild(string manifest,string download){var m=new Uri(manifest);var d=new Uri(download);string[] p=m.AbsolutePath.Trim('/').Split('/');if(m.Host!="github.com"||p.Length!=6||p[2]!="releases"||p[3]!="latest"||p[4]!="download"||p[5]!="manifest.json")throw new IOException("Invalid GitHub release feed.");if(d.Scheme!="https"||d.Host!="github.com"||!d.AbsolutePath.StartsWith("/"+p[0]+"/"+p[1]+"/releases/download/",StringComparison.OrdinalIgnoreCase))throw new IOException("Build URL must be a tagged release asset from the configured repository.");}
}
