namespace CraftNet.ProtoGen;

public static class Proto2CS
{
    internal static readonly char[] splitChars = { ' ', '\t' };

    public static ProtoGenData Gen(string protoText)
    {
        ProtoGenData genData = new ProtoGenData();
        foreach (string line in protoText.Split('\n'))
        {
            string newline = line.Trim();

            if (newline == "")
                continue;

            // 命名空间引用
            if (newline.StartsWith("//@using"))
            {
                string tmp = newline.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                genData.UsingList.Add(tmp);
                continue;
            }

            // 起始opcode
            if (newline.StartsWith("//@opcode"))
            {
                string tmp = newline.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                if (!ushort.TryParse(tmp, out ushort opcode))
                {
                    Console.WriteLine($"opcode起始定义无效，无法转换为整数。:{newline}");
                    return null;
                }

                genData.Opcode = opcode;
                continue;
            }

            // proto的选项
            if (newline.StartsWith("option csharp_namespace"))
            {
                // cs命名空间
                if ("csharp_namespace".IndexOf(newline, StringComparison.Ordinal) != 0)
                {
                    // "Namespace";
                    string ns = newline.Split('=')[1];
                    ns                = ns.Remove(ns.Length - 1).Replace("\"", "");
                    genData.Namespace = ns.Trim();
                    continue;
                }
            }

            // 开始解析消息
            if (newline.StartsWith("message"))
            {
                genData.Push(new ProtoMessage(line));
                continue;
            }

            // 进入消息解析
            if (genData.TryPeek() is ProtoMessage messageDefine)
            {
                if (newline == "{")
                    continue;

                if (newline == "}")
                {
                    genData.Pop(); // 弹出自己

                    if (genData.TryPeek() is ProtoElement elementDefine)
                    {
                        elementDefine.AddChild(messageDefine);
                    }
                    else
                    {
                        genData.Elems.Add(messageDefine);
                    }

                    continue;
                }

                if (newline.Trim().StartsWith("//"))
                {
                    messageDefine.AddChild(new ProtoComment { Comment = newline, TabCount = 2 });
                    continue;
                }

                if (TryParseEnum())
                    continue;

                messageDefine.AddChild(new ProtoMessageField(newline));
                continue;
            }

            // 注释
            if (newline.StartsWith("//"))
            {
                genData.Elems.Add(new ProtoComment { Comment = newline, TabCount = 1 });
                continue;
            }

            if (TryParseEnum())
                continue;

            bool TryParseEnum()
            {
                // 开始解析枚举
                if (newline.StartsWith("enum"))
                {
                    genData.Push(new ProtoEnum(line));
                    return true;
                }

                if (genData.TryPeek() is ProtoEnum protoElementDefine)
                {
                    if (newline == "{")
                        return true;

                    if (newline == "}")
                    {
                        genData.Pop(); // 弹出自己

                        if (genData.TryPeek() is ProtoElement elementDefine)
                        {
                            elementDefine.AddChild(protoElementDefine);
                        }
                        else
                        {
                            genData.Elems.Add(protoElementDefine);
                        }

                        return true;
                    }

                    protoElementDefine.AddChild(new ProtoEnumField(newline));
                    return true;
                }

                return false;
            }
        }

        return genData;
    }
}