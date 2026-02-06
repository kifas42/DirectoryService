using NJsonSchema.Generation;
using Shared;

namespace DirectoryService.Presentation;

public class EnvelopeSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.Type != typeof(Envelope<Errors>))
            return;

        if (!context.Schema.Properties.TryGetValue("errors", out var errorsProperty))
            return;

        var errorSchema = context.Resolver.GetSchema(typeof(Error), isIntegerEnumeration: false);

        errorsProperty.Item = errorSchema;
    }
}