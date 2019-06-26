using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Http.Resource
{
    public class Action
    {
        public class MappedParameters
        {
            public int ActionId { get; private set; }
            public IList<MappedParameter> Mapped { get; private set; }
        }

        public class MappedParameter
        {
            public object Value { get; set; }
        }
    }
}
