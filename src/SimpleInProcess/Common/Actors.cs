using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULS.Core;

namespace SimpleInProcess.Common
{
    public partial class SubActor : NetworkActor
    {
        [Replicate]
        public string SubTestValue = "Hello World";
        [Replicate]
        public float X = 0.0f;
        [Replicate]
        public float Y = 0.0f;
        [Replicate]
        public float Z = 0.0f;

        public SubActor(INetworkOwner setNetworkOwner, long overrideUniqueId = -1)
            : base(setNetworkOwner, overrideUniqueId)
        {
        }
    }

    public partial class DerivedActor : Actor
    {
        [Replicate]
        public long DerivedTestValue = 0;

        public DerivedActor(INetworkOwner setNetworkOwner, long overrideUniqueId = -1)
            : base(setNetworkOwner, overrideUniqueId)
        {
        }
    }

    public partial class Actor : NetworkActor
    {
        [Replicate]
        public int CustomId { get; set; } = 0;
        [Replicate]
        public SubActor? RefToSubActor = null;

        public Actor(INetworkOwner setNetworkOwner, long overrideUniqueId = -1)
            : base(setNetworkOwner, overrideUniqueId)
        {
        }
    }
}
