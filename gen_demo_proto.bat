cd tools
dotnet CraftNet.ProtoGen.dll --in=../samples/Demo.Model/Shared/10000_Gate.proto --tpl=proto_cs
dotnet CraftNet.ProtoGen.dll --in=../samples/Demo.Model/Shared/20000_World.proto --tpl=proto_cs
dotnet CraftNet.ProtoGen.dll --in=../samples/Demo.Model/Shared/InnerMessage.proto --tpl=memory_pack_cs

pause