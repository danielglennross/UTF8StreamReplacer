# UTF8StreamReplacer

UTF8StreamReplacer is a class which decorates over a UTF8 byte stream allowing all occurances of specific strings/byte arrays to be replaced
when the stream is written & flushed.

- Strings/byte arrays can be replaced via simple matching (i.e. find/replace logic). 
- Strings/byte arrays that fall within specific delimiters can also be found. Here, a function is provided expressing how the value should be replaced.

# Example

Replace all occurances of a specific string / byte array when writing the stream:

```cs
// sring api
var simpleStringReplace = new UTf8StreamReplacer(stream, "ReplaceThis", "WithThis");

// byte[] api
var simpleByteReplace = new UTf8StreamReplacer(
    stream, Encoding.UTF8.GetBytes("ReplaceThis"), Encoding.UTF8.GetBytes("WithThis")
);
```

Replace all occurances of a string / byte array that fall within a specific delimiter(s) when writing the steam:

```cs
// string api

// single delimiter
var delimitedHashStringReplace = new UTf8StreamReplacer(stream, (str) =>
{
    // str will have appeared ##str## (UTf8 Byte encoded) within the stream
    // return a new string
}, "##");

// start/end delimiter
var delimitedCommentStringReplace = new UTf8StreamReplacer(stream, (str) =>
{
    // str will have appeared <!--str--> (UTf8 Byte encoded) within the stream
    // return a new string
}, "<!--", "-->");

// byte[] api

// single delimiter
var delimitedHashByteArrReplace = new UTf8StreamReplacer(stream, (byteArr) =>
{
    // byteArr will have appeared ##byteArr## (UTf8 Byte encoded) within the stream
    // return a new byte array
}, Encoding.UTF8.GetBytes("##"));

// start/end delimiter
var delimitedCommentStringReplace = new UTf8StreamReplacer(stream, (str) =>
{
    // byteArr will have appeared <!--byteArr--> (UTf8 Byte encoded) within the stream
    // return a new byte arr
}, Encoding.UTF8.GetBytes("<!--"), Encoding.UTF8.GetBytes("-->"));
```
