using System;

namespace Junya005.Nodevel
{
    /// <summary>
    /// グラフの開始点となるノード。
    /// </summary>
    [Serializable]
    public class StartNode : NodeBase
    {
        public override int OutputPortCount => 1;
    }
}

