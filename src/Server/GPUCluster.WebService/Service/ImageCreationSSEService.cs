using System.Threading.Tasks;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace GPUCluster.WebService.Service
{
    public interface IImageCreationSSEService : IServerSentEventsService
    { }
    public class ImageCreationSSEService : ServerSentEventsService, IImageCreationSSEService
    {
        public ImageCreationSSEService(IOptions<ServerSentEventsServiceOptions<ServerSentEventsService>> options) : base(options)
        {
        }
    }
}