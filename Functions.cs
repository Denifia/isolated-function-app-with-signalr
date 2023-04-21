using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace isolatedWorkerSignalr;

public class Functions
{
    [Function("negotiate")]
    public SignalRConnectionInfo Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData request,
        [SignalRConnectionInfoInput(HubName = "MyHub")] SignalRConnectionInfo signalRConnectionInfo)
    {
        return signalRConnectionInfo;
    }

    [Function("broadcast-message1")]
    public Task<MultipleOutputBindings> BroadcastMessage1(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "broadcast-message")]
    HttpRequestData request)
    {
        return Task.FromResult(new MultipleOutputBindings
        {
            HttpResponseData = request.CreateResponse(HttpStatusCode.OK),
            SignalRMessageAction = new SignalRMessageAction("method1")
            {
                Arguments = new object[] { $"http triggered {DateTime.Now}" }
            }
        });
    }

    /// <remark>
    /// !! Should work but the output binding never triggers a message on the client
    /// </remark>
    [Function("broadcast-message2")]
    [SignalROutput(HubName = "MyHub")]
    public Task BroadcastMessage2(
        [SignalRTrigger("MyHub", "messages", "method2")]
        SignalRInvocationContext invocationContext)
    {
        return Task.FromResult(new SignalRMessageAction("method1")
        {
            Arguments = new object[] { $"signalR triggered {DateTime.Now}" }
        });
    }

    [Function("periodic-broadcast")]
    [SignalROutput(HubName = "MyHub")]
    public Task<SignalRMessageAction> PeriodicBroadcastMessage(
         [TimerTrigger("*/5 * * * * *")] TimerInfo timerInfo)
    {
        return Task.FromResult(new SignalRMessageAction("method1")
        {
            Arguments = new object[] { $"timer triggered {DateTime.Now}" }
        });
    }

    public class MultipleOutputBindings
    {
        [SignalROutput(HubName = "MyHub")]
        public SignalRMessageAction SignalRMessageAction { get; set; }
        public HttpResponseData HttpResponseData { get; set; }
    }
}