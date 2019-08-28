using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Http.Resource.Sse;

namespace Vlingo.Http.Tests.Resource.Sse
{
    public class MockSseStreamResource : SseStreamResource
    {
        private ICompletesEventually _completes;

        private Request _request;

        public MockRequestResponseContext _requestResponseContext;
        
        public MockSseStreamResource(World world) : base(world)
        {
            _completes = world.CompletesFor(new BasicCompletes<string>(world.Stage.Scheduler));
            _requestResponseContext = new MockRequestResponseContext(new MockResponseSenderChannel());
        }

        public void NextRequest(Request request) => _request = request;

        protected override ICompletesEventually Completes => _completes;

        protected override Context Context => new Context(_requestResponseContext, _request, _completes);
    }
}