/// <summary>
/// 单例类 Lazy初始化
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    /// <summary>
    /// 对象锁(防止多线程访问单例对象创建多个实例)
    /// </summary>
    private static object o = new object();

    /// <summary>
    /// 实例
    /// </summary>
    private static T instance;

    /// <summary>
    /// 获取单例
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (o)
                {
                    if (instance == null)// 双检锁
                    {
                        instance = new T();
                    }
                }
            }
            return instance;
        }
    }
    //强制触发初始化
    public static void ForceInit()
    {
        //同伙获得Instance 让Instance 初始化
        T inst = Instance;
    }
}