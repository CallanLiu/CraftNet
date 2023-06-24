cd tools
dotnet XGFramework.ProtoGen.dll --in=../src/Demo.Model/Shared/OuterMessage.proto --tpl=proto_cs.tpl
dotnet XGFramework.ProtoGen.dll --in=../src/Demo.Model/Shared/InnerMessage.proto --tpl=memorypack_cs.tpl

pause