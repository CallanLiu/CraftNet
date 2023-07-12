cd tools
dotnet XGFramework.ProtoGen.dll --in=../src/Demo.Model/Shared/10000_Gate.proto --tpl=proto_cs
dotnet XGFramework.ProtoGen.dll --in=../src/Demo.Model/Shared/20000_World.proto --tpl=proto_cs
dotnet XGFramework.ProtoGen.dll --in=../src/Demo.Model/Shared/InnerMessage.proto --tpl=memory_pack_cs

pause