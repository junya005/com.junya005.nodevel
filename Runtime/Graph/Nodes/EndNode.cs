using System;

namespace Junya005.Nodevel
{
    /// <summary>
    /// グラフの終了点となるノード。
    /// </summary>
    [Serializable]
    public class EndNode : NodeBase
    {
        public override int OutputPortCount => 0;
    }
}

