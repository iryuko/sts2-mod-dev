using System;

namespace TemplateMod
{
    // 这是占位入口文件，不代表已经验证的 STS2 真实入口接口。
    // 接入真实游戏时，需要根据已确认的加载方式替换类名、方法签名和初始化流程。
    public static class Entry
    {
        public static void Initialize()
        {
            Console.WriteLine("TemplateMod 占位入口：等待接入真实 STS2 mod 加载流程。");
        }
    }
}
