using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Exceptions;
using POGOProtos.Networking.Envelopes;
using System;
using System.Timers;

namespace PokemonGo.RocketAPI.Extensions
{
    public static class HttpClientExtensions
    {
        static Timer _timer = new Timer();
        public static async Task<TResponsePayload> PostProtoPayload<TRequest, TResponsePayload>(this System.Net.Http.HttpClient client,
            string url, RequestEnvelope requestEnvelope) where TRequest : IMessage<TRequest>
            where TResponsePayload : IMessage<TResponsePayload>, new()
        {
            await Task.Delay(300);
            Request:
            try
            {
                Debug.WriteLine($"Requesting {typeof(TResponsePayload).Name}");
                var response = await PostProto<TRequest>(client, url, requestEnvelope);
                if (response.Returns.Count == 0)
                    throw new InvalidResponseException();

                //Decode payload
                //todo: multi-payload support
                var payload = response.Returns[0];
                var parsedPayload = new TResponsePayload();
                parsedPayload.MergeFrom(payload);
                return parsedPayload;
            }
            catch(Exception e)
            {
                await Task.Delay(500);
                goto Request;
            }
        }

        public static async Task<ResponseEnvelope> PostProto<TRequest>(this System.Net.Http.HttpClient client, string url,
            RequestEnvelope requestEnvelope) where TRequest : IMessage<TRequest>
        {
            //Encode payload and put in envelop, then send
            var data = requestEnvelope.ToByteString();
            var result = await client.PostAsync(url, new ByteArrayContent(data.ToByteArray()));

            //Decode message
            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            var decodedResponse = new ResponseEnvelope();
            decodedResponse.MergeFrom(codedStream);

            return decodedResponse;
        }
    }
}