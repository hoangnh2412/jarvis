// using Jarvis.WebApi.Monitoring.Interfaces;
// using OpenTelemetry.Trace;

// namespace Jarvis.WebApi.Monitoring;

// public class ElasticsearchTraceInstrumentation : ITraceInstrumentation
// {
//     private readonly OTLPOption _options;

//     public ElasticsearchTraceInstrumentation(
//         OTLPOption options)
//     {
//         _options = options;
//     }

//     public TracerProviderBuilder AddInstrumentation(TracerProviderBuilder builder)
//     {
//         builder.AddElasticsearchClientInstrumentation(options =>
//         {
//             options.ParseAndFormatRequest = true;
//             options.SuppressDownstreamInstrumentation = true;
//             options.SetDbStatementForRequest = false;
//         });
//         return builder;
//     }
// }