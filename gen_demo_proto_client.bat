set PROTO_DIR=../src/Demo.Model/Shared
set OUT_DIR=../../CraftNet.DemoClient/Assets/Scripts/Gen/
cd tools
dotnet CraftNet.ProtoGen.dll --in=%PROTO_DIR%/10000_Gate.proto --out=%OUT_DIR% --tpl=proto_cs



pause