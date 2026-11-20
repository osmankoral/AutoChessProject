using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe class BackSetUpSystem : SystemMainThreadFilter<BackSetUpSystem.Filter>
    {

        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public BackSet* BackSet;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            filter.BackSet->Position = filter.Transform->Position;
            Log.Error(filter.Entity.ToString());
        }
    }
}
