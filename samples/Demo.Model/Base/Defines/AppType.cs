namespace Demo;

/// <summary>
/// 定义要使用的App类型
/// </summary>
public enum AppType
{
    Login = 1 << 0,
    Gate  = 1 << 1,
    Game  = 1 << 2,
    World = 1 << 3
}