using NJsonSchema;
using NJsonSchema.Generation;
using Shared;

namespace DirectoryService.Presentation;

public class EnvelopeSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType != typeof(Envelope<Error>))
        {
            return;
        }

        if (!context.Schema.Properties.TryGetValue("error", out JsonSchemaProperty? errorProperty))
        {
            return;
        }

        JsonSchema errorSchema = context.Resolver.GetSchema(typeof(Error), false);

        errorProperty.Item = errorSchema;
    }
}