Dson.NET
========

DSON (Doge Serialized Object Notation): a data-interchange format that is easy for Shiba Inu dogs to read and write.

DSON spec: [dogeon.org](http://dogeon.org/)

Dson.NET pacakage: [Newtonsoft.Dson](https://www.nuget.org/packages/Newtonsoft.Dson/)

```csharp
var data = new
{
    hello = "world",
    people = new[] { "James", "Brendan", "Amy" }
};

string dson = DsonConvert.SerializeObject(data, Formatting.Indented);
// such
//   "hello" is "world",
//   "people" is so
//     "James" and
//     "Brendan" and
//     "Amy"
//   many
// wow
```