using System;
using System.Text;

namespace Vlingo.Http.Tests
{
    public class ResponseParserTest
    {
        private string MultipleResponseBuilder(int amount)
        {
            var builder = new StringBuilder();

            /*for (var idx = 1; idx <= amount; ++idx)
            {
                var body = (idx % 2 == 0) ? uniqueJaneDoe() : uniqueJohnDoe();
                uniqueBodies.add(body);
                builder.append(this.createdResponse(body));
            }*/

            return builder.ToString();
        }
    }
}