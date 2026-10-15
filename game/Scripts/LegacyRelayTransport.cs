using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

static class NetJson {
 public static string Write<T>(T value){using(var s=new MemoryStream()){new DataContractJsonSerializer(typeof(T)).WriteObject(s,value);return Encoding.UTF8.GetString(s.ToArray());}}
 public static T Read<T>(string value){using(var s=new MemoryStream(Encoding.UTF8.GetBytes(value)))return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(s);}
}
static class FrameCodec {
 public const int MaxFrame=1048576,MaxText=4194304;
 public static byte[] Pack(string text){byte[] raw=Encoding.UTF8.GetBytes(text);if(raw.Length>MaxText)throw new IOException("Packet too large.");byte[] payload=raw;byte flag=0;if(raw.Length>256)using(var buffer=new MemoryStream()){using(var zip=new DeflateStream(buffer,CompressionMode.Compress,true))zip.Write(raw,0,raw.Length);if(buffer.Length<raw.Length){payload=buffer.ToArray();flag=1;}}if(payload.Length+1>MaxFrame)throw new IOException("Frame too large.");byte[] frame=new byte[payload.Length+5];int n=payload.Length+1;frame[0]=(byte)(n>>24);frame[1]=(byte)(n>>16);frame[2]=(byte)(n>>8);frame[3]=(byte)n;frame[4]=flag;Buffer.BlockCopy(payload,0,frame,5,payload.Length);return frame;}
 static void Exact(Stream stream,byte[] buffer,int offset,int count){while(count>0){int n=stream.Read(buffer,offset,count);if(n<=0)throw new EndOfStreamException("Connection closed.");offset+=n;count-=n;}}
 public static string Read(Stream stream){byte[] header=new byte[4];Exact(stream,header,0,4);int length=(header[0]<<24)|(header[1]<<16)|(header[2]<<8)|header[3];if(length<1||length>MaxFrame)throw new IOException("Invalid frame length.");byte[] data=new byte[length];Exact(stream,data,0,length);if(data[0]==2){if(length!=1)throw new IOException("Invalid heartbeat.");return null;}if(data[0]==0){if(length-1>MaxText)throw new IOException("Packet too large.");return new UTF8Encoding(false,true).GetString(data,1,length-1);}if(data[0]!=1)throw new IOException("Unknown frame encoding.");using(var source=new MemoryStream(data,1,length-1))using(var zip=new DeflateStream(source,CompressionMode.Decompress))using(var output=new MemoryStream()){byte[] chunk=new byte[4096];int count;while((count=zip.Read(chunk,0,chunk.Length))>0){if(output.Length+count>MaxText)throw new IOException("Inflated packet too large.");output.Write(chunk,0,count);}return new UTF8Encoding(false,true).GetString(output.ToArray());}}
}
sealed class FramedConnection:IDisposable {
 TcpClient tcp;Stream stream;Action<string> receive;Action<string> closed;ConcurrentQueue<byte[]> reliable=new ConcurrentQueue<byte[]>();Dictionary<string,byte[]> pending=new Dictionary<string,byte[]>();object gate=new object();AutoResetEvent wake=new AutoResetEvent(false);volatile bool alive=true;long queued;int stopped,started;Thread readThread,writeThread;public long BytesSent,BytesReceived;
 public FramedConnection(TcpClient socket,Stream transport,Action<string> onReceive,Action<string> onClose){tcp=socket;stream=transport;receive=onReceive;closed=onClose;tcp.NoDelay=true;tcp.ReceiveTimeout=45000;tcp.SendTimeout=10000;readThread=new Thread(Read){IsBackground=true};writeThread=new Thread(Write){IsBackground=true};}
 public void Start(){if(Interlocked.Exchange(ref started,1)!=0)return;readThread.Start();writeThread.Start();}
 public void Send(string text,string key=null){SendPacked(FrameCodec.Pack(text),key);}
 public void SendPacked(byte[] frame,string key=null){if(!alive)return;if(key!=null){lock(gate){if(pending.Count>=32&&!pending.ContainsKey(key)){Fail("Too many pending streams.");return;}pending[key]=frame;}}else{if(reliable.Count>=80||Interlocked.Read(ref queued)+frame.Length>8388608){Fail("Connection cannot keep up.");return;}Interlocked.Add(ref queued,frame.Length);reliable.Enqueue(frame);}wake.Set();}
 void Read(){try{while(alive){string text=FrameCodec.Read(stream);if(text!=null){Interlocked.Add(ref BytesReceived,Encoding.UTF8.GetByteCount(text));receive(text);}}}catch(Exception ex){Fail(ex.Message);}}
 void Write(){try{DateTime last=DateTime.UtcNow;while(alive){wake.WaitOne(1000);byte[] frame;while(reliable.TryDequeue(out frame)){Interlocked.Add(ref queued,-frame.Length);stream.Write(frame,0,frame.Length);Interlocked.Add(ref BytesSent,frame.Length);last=DateTime.UtcNow;}byte[][] frames;lock(gate){frames=new List<byte[]>(pending.Values).ToArray();pending.Clear();}foreach(var bytes in frames){stream.Write(bytes,0,bytes.Length);Interlocked.Add(ref BytesSent,bytes.Length);last=DateTime.UtcNow;}if((DateTime.UtcNow-last).TotalSeconds>=10){byte[] ping={0,0,0,1,2};stream.Write(ping,0,ping.Length);last=DateTime.UtcNow;}stream.Flush();}}catch(Exception ex){Fail(ex.Message);}}
 void Fail(string why){if(Interlocked.Exchange(ref stopped,1)!=0)return;alive=false;tcp.Close();wake.Set();closed(why);}
 public void Dispose(){if(Interlocked.Exchange(ref stopped,1)!=0)return;alive=false;tcp.Close();wake.Set();}
}
[DataContract] public sealed class RelayConfig {
 [DataMember] public string Address="",CertificateSha256="";
 [DataMember] public int Port=28747;
 public bool Ready {get{return !string.IsNullOrWhiteSpace(Address)&&Port>0&&Port<65536&&CertificateSha256!=null&&CertificateSha256.Length==64;}}
 public static string PathName {get{return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"relay-client.json");}}
 public static RelayConfig Load(){try{return File.Exists(PathName)?NetJson.Read<RelayConfig>(File.ReadAllText(PathName)):new RelayConfig();}catch{return new RelayConfig();}}
 public static string Pin(X509Certificate certificate){using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(certificate.GetRawCertData())).Replace("-","");}
}
[DataContract] public sealed class RelayRoom {
 [DataMember] public string Code,Name,Phase,GameMode;
 [DataMember] public int Count,Round,Version;
 [DataMember] public bool Public,PasswordRequired;
}
[DataContract] public sealed class RelayMessage {
 [DataMember(EmitDefaultValue=false)] public string Op,Code,Name,Data,Phase,Error,GameMode;
 [DataMember(EmitDefaultValue=false)] public int Peer,Count,Round,Version;
 [DataMember(EmitDefaultValue=false)] public bool Public,PasswordRequired,Replace;
 [DataMember(EmitDefaultValue=false)] public RelayRoom[] Rooms;
}
sealed class RelayLink:IDisposable {
 public ConcurrentQueue<RelayMessage> Inbox=new ConcurrentQueue<RelayMessage>();public volatile bool Connected;FramedConnection wire;TcpClient tcp;volatile bool disposed;
 public void Connect(RelayConfig config){new Thread(()=>{try{if(config==null||!config.Ready)throw new IOException("Configure the relay server address and certificate fingerprint first.");tcp=new TcpClient();var wait=tcp.BeginConnect(config.Address,config.Port,null,null);try{if(!wait.AsyncWaitHandle.WaitOne(8000))throw new IOException("Relay did not respond.");tcp.EndConnect(wait);}finally{wait.AsyncWaitHandle.Close();}tcp.ReceiveTimeout=tcp.SendTimeout=10000;var ssl=new SslStream(tcp.GetStream(),false,(s,c,chain,error)=>c!=null&&string.Equals(RelayConfig.Pin(c),config.CertificateSha256,StringComparison.OrdinalIgnoreCase));ssl.AuthenticateAsClient(config.Address,null,SslProtocols.Tls12,false);if(disposed){tcp.Close();return;}wire=new FramedConnection(tcp,ssl,text=>Inbox.Enqueue(NetJson.Read<RelayMessage>(text)),why=>{Connected=false;if(!disposed)Inbox.Enqueue(new RelayMessage{Op="error",Error=why});});wire.Start();Connected=true;Inbox.Enqueue(new RelayMessage{Op="connected"});}catch(Exception ex){if(tcp!=null)tcp.Close();if(!disposed)Inbox.Enqueue(new RelayMessage{Op="error",Error="Relay: "+ex.Message});}}){IsBackground=true}.Start();}
 public void Send(RelayMessage message){if(wire!=null)wire.Send(NetJson.Write(message),message.Replace?"peer:"+message.Peer:null);}
 public void Dispose(){disposed=true;Connected=false;if(wire!=null)wire.Dispose();if(tcp!=null)tcp.Close();}
}
static class RoomPassword {
 public static string Challenge(){byte[] bytes=new byte[32];using(var random=RandomNumberGenerator.Create())random.GetBytes(bytes);return Convert.ToBase64String(bytes);}
 public static string Proof(string password,string nonce){if(string.IsNullOrEmpty(nonce))return "";using(var hash=SHA256.Create())using(var hmac=new HMACSHA256(hash.ComputeHash(Encoding.UTF8.GetBytes(password??""))))return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(nonce)));}
 public static bool Match(string actual,string expected){if(actual==null||expected==null||actual.Length!=expected.Length)return false;int diff=0;for(int i=0;i<actual.Length;i++)diff|=actual[i]^expected[i];return diff==0;}
}
