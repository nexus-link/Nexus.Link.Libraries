# Nexus.Link.Libraries.Core

## Decoupling

### Schema versions

Suppose that a producer is putting items on a queue that a consumer is reading. If the schema for the data that is stored on the queue is changed, we might have items on the queue from both schemes. This is interfaces, types and logic to handle that.

- IVersionedSchema. By implementing this interface your data type is prepared for handling schema versions.
- AnynymousSchema. A minimal type that implements IVersionedSchema.

This is how it works. If you have the data types QueueEnvelopeV1 and QueueEnvelopeV2 that both inherits from IVersionedSchema, then you can use the following code to deserialize data:

```csharp
var json = Queue.ReadOneItem();
var probe = JsonConvert.DeserializeObject<AnonymousSchema>(json);
if (!probe.SchemaVersion.HasValue)
{
  // Error handling
  // ... or treat this as version 0 (before schema versions was introduced)?
}
switch (probe.SchemaVersion.Value)
{
  case 1:
    var envelopeV1 = JsonConvert.DeserializeObject<QueueEnvelopeV1);
	...
	break;
  case 2:
    var envelopeV2 = JsonConvert.DeserializeObject<QueueEnvelopeV2);
	...
	break;
  default:
    // Error handling
	break;
}
```

The next level of this where different data types (with different versions) are stored at the same place. For instance if your queue could contain different data altogether. This is interfaces, types and logic to handle that.

- INamedSchema. By implementing this interface your data type is prepared for handling schema versions of named types.
- NamedSchema. A minimal type that implements INamedSchema.


This is how it works. If you have the data types ItemType1 and ItemType2 that both inherits from INamedSchema, then you can use the following code to deserialize data:

```csharp
var json = Queue.ReadOneItem();
var probe = JsonConvert.DeserializeObject<NamedSchema>(json);
if (!probe.SchemaName == null)
{
  // Error handling
}
switch (probe.SchemaName.Value)
{
  case "ItemType1":
    var item1 = JsonConvert.DeserializeObject<ItemType1);
	...
	break;
  case ItemType2:
    var item2 = JsonConvert.DeserializeObject<ItemType2);
	...
	break;
  default:
    // Error handling
	break;
}
```

A named schema can also have versions, where you the typically would have nested switch statements in the examples above.

We provide a convenience class, `SchemaParser`, that can do the deserialization for you. You just tell the class which names and versions you have, and it will take care of the dirty work for you.

```csharp
var schemaParser = new SchemaParser()
  .Add(typeof(DataAnonymous))
  .Add("DataType1", 1, typeof(DataType1Version1))
  .Add("DataType2", 1, typeof(DataType2Version1))
  .Add("DataType2", 2, typeof(DataType2Version2));
var json = Queue.ReadOneItem();
var success = schemaParser.TryParse(json, out var schemaName, out var schemaVersion, out var data);
if (data is DataType1Version1)
{
  ...
}
else if (data is DataType2Version1) {
{
  ...
}
```
