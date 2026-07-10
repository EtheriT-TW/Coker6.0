
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayCreditDetailResponseDataDto
    {
        public string RtnMsg { get; set; }
        public class RtnValueDto
        {
            public int TradeID { get; set; }
            public int Amount { get; set; }
            public int ClsAmt { get; set; }
            public string AuthTime { get; set; }
            public string Status { get; set; }
        }
        public RtnValueDto RtnValue {  get; set; }
        public class CloseDataDto
        {
            public string Status { get; set; }
            public int Amount { get; set; }
            public string DateTime { get; set; }
        }
        [JsonConverter(typeof(SingleOrArrayConverter<CloseDataDto>))]
        public List<CloseDataDto> CloseData { get; set; } = new();

    }

    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Null)
            {
                return new List<T>();
            }

            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>(serializer) ?? new List<T>();
            }

            if (token.Type == JTokenType.Object && !token.HasValues)
            {
                return new List<T>();
            }

            var item = token.ToObject<T>(serializer);
            return item == null ? new List<T>() : new List<T> { item };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
