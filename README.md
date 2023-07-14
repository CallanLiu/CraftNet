# CraftNet

一个极简、高效的 `C# Game Server Framework`，用于快速简单的开发`分布式游戏`服务端程序，支持`单进程多线程`与`多进程单线程`的部署方式。


## 框架特性
* Actor模式的一种实现
* 灵活的运行模式(多线程运行、单线程开发)
* 逻辑热重载
* 高性能的内网通信，使用`Kestrel`
* 可共享的Actor调度器，方便处理`Scene`与`Player`或`Room`与`Player`的关系
* 优雅的Rpc调用


![](https://github.com/Yinmany/XGFramework/raw/main/doc/framework.png)

