# CraftNet

一个极简、高效的 `C# Game Server Framework`，用于快速简单的开发`分布式游戏`服务端程序，支持`单进程多线程`与`多进程单线程`的部署方式。


## 框架说明
* 以`App`为单位开发各种功能服，【登录服】【网关服】【游戏服】【世界服】等等
* 一个进程随意运行任意`App`，开发时所有`App`在一个进程运行，部署时按需运行
* 灵活的调度方式，同个进程中所有`App`可使用同个线程调度器(单线程模型)，也可各个`App`使用不同的调度器(多线程模型)
* 基于Actor模型，可让任意对象成为Actor，知道ActorId就能向其发送消息，不论它在远程或本地
* 基于Asp.NetCore框架，使用高性能的`Kestrel`支持多种协议`Http`、`WebSocket`、`Tcp`，免费获得一个Http服务器...

![](https://github.com/Yinmany/XGFramework/raw/main/doc/framework.png)