using NJsonSchema.Generation;
using Shared;

namespace DirectoryService.Presentation;

public class EnvelopeSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType != typeof(Envelope<Error>))
            return;

        if (!context.Schema.Properties.TryGetValue("error", out var errorProperty))
            return;

        var errorSchema = context.Resolver.GetSchema(typeof(Error), isIntegerEnumeration: false);

        errorProperty.Item = errorSchema;
    }
}