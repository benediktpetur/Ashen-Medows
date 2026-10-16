using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
public partial class MigrationScene {
 long coopWireSequence,coopReceivedSequence=-1;byte[][] coopFrameParts;int coopPartsReceived;
 void SendCoopFrame(long peer,CoopFrame frame){byte[] bytes;using(var packed=new MemoryStream()){using(var zip=new DeflateStream(packed,CompressionLevel.Fastest,true)){var raw=Encoding.UTF8.GetBytes(Json.Write(frame));zip.Write(raw,0,raw.Length);}bytes=packed.ToArray();}
  const int packetSize=1000;int count=(bytes.Length+packetSize-1)/packetSize;long sequence=++coopWireSequence;
  for(int index=0;index<count;index++){int length=Math.Min(packetSize,bytes.Length-index*packetSize);var part=new byte[length];Array.Copy(bytes,index*packetSize,part,0,length);RpcId(peer,nameof(CoopFramePart),sequence,index,count,part);}
 }
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.UnreliableOrdered,TransferChannel=1)]
 void CoopFramePart(long sequence,int index,int count,byte[] payload){if(!CoopClient||!coopReady||count<1||count>64||index<0||index>=count||payload.Length>1000||sequence<coopReceivedSequence)return;
  if(sequence>coopReceivedSequence){coopReceivedSequence=sequence;coopFrameParts=new byte[count][];coopPartsReceived=0;}
  if(coopFrameParts==null||coopFrameParts.Length!=count||coopFrameParts[index]!=null)return;coopFrameParts[index]=payload;if(++coopPartsReceived!=count)return;
  try{using(var packed=new MemoryStream()){foreach(var part in coopFrameParts)packed.Write(part,0,part.Length);packed.Position=0;using(var zip=new DeflateStream(packed,CompressionMode.Decompress))using(var raw=new MemoryStream()){var buffer=new byte[4096];int read;while((read=zip.Read(buffer,0,buffer.Length))>0){if(raw.Length+read>524288)return;raw.Write(buffer,0,read);}CoopFrameUpdate(Encoding.UTF8.GetString(raw.ToArray()));}}}catch(InvalidDataException){}finally{coopFrameParts=null;}
 }
}
