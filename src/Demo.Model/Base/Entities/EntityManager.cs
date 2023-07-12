using CraftNet;

namespace Demo.Entities;

/// <summary>
/// 实体系统
/// </summary>
public interface IEntityManager : ISystemTypeId, ISystemBase<IEntityManager>
{
    /// <summary>
    /// 创建实体
    /// </summary>
    /// <returns></returns>
    Entity Create();

    /// <summary>
    /// 销毁实体
    /// </summary>
    /// <param name="entity"></param>
    void Destroy(Entity entity);
}

// public class EntityManager : IEntityManager
// {
//     
// }