using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Http.Resource
{
    public class ConfigurationResource<T> : Resource<T>
    {
        public ConfigurationResource(string name, Type resourceHandlerClass, int handlerPoolSize, IList<Action> actions)
        {
        }

        public override void DispatchToHandlerWith(Context context, Action.MappedParameters mappedParameters)
        {
            throw new NotImplementedException();
        }
    }
}
