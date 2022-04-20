using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ULS.Core;

namespace SimpleInProcess.Common
{
    public partial class SubActor : NetworkObject
    {
        [Replicate]
        private string _SubTestValue = "Hello World";
        [Replicate]
        private Vector3 _translation;

        public SubActor(INetworkOwner setNetworkOwner, long overrideUniqueId = -1)
            : base(setNetworkOwner, overrideUniqueId)
        {
            //
        }
    }

    public partial class DerivedActor : Actor
    {
        [Replicate]
        private long _DerivedTestValue = 0;

        [Replicate]
        private short _MyValue = 0;

        public DerivedActor(INetworkOwner setNetworkOwner, long overrideUniqueId = -1)
            : base(setNetworkOwner, overrideUniqueId)
        {
        }
    }

    public partial class Actor : NetworkObject
    {
        [Replicate(ReplicationStrategy = ReplicationStrategy.Immediate)]
        private int _CustomId = 0;
        [Replicate]
        private int _Counter = 0;
        [Replicate]
        private SubActor? _RefToSubActor = null;

        public Actor(INetworkOwner setNetworkOwner, long overrideUniqueId = -1)
            : base(setNetworkOwner, overrideUniqueId)
        {
        }
    }
}
