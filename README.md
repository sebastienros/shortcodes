## Basic Overview

Shortcodes processor for .NET with focus on performance and simplicity.

<br>

## Features

- Parses and renders shortcodes.
- Supports **async** shortcode to execute database queries and async operations more efficiently under load.
- Named and positioned arguments.

<br>

## Contents
- [Sample usage](#sample-usage)
- [Used by](#used-by)

<br>

## Sample usage

Don't forget to include the __using__ statement:

```c#
using Shortcodes;
```

### Predefined shortcodes

```c#
var processor = new ShortcodesProcessor(new NamedShortcodeProvider
{
    ["hello"] = (args, content) => new ValueTask<string>("Hello world!")
});

Console.WriteLine(await process.EvaluateAsync("This is an [hello]"));
```

Which results in 

```
This is an Hello world!
```

### Named arguments

Arguments can contain ay character, but need to be quoted either with `'` or `"` if they contain spaces.
Strings can use standard string escape sequences like `\u03A9` and `\n`.

```c#
var processor = new ShortcodesProcessor(new NamedShortcodeProvider
{
    ["bold"] = (args, content) => 
    {
        var text = args.Named("text");
        
        return new ValueTask<string>($"<b>{text}</b>");
    }
});

Console.WriteLine(await process.EvaluateAsync("[bold text='bold text' 1234]"));
```

### Content arguments

Shortcodes using opening and closing tags can access their inner content.

```c#
var processor = new ShortcodesProcessor(new NamedShortcodeProvider
{
    ["bold"] = (args, content) => 
    {
        return new ValueTask<string>($"<b>{content}</b>");
    }
});

Console.WriteLine(await process.EvaluateAsync("[bold]bold text[/bold]"));
```

For single tags, the content is `null`. It means that you can detect if a shortcode was
used with a closing tag, even if the inner content is empty.

### Positional arguments

If an argument doesn't have a name, an default index can be used.

```c#
var processor = new ShortcodesProcessor(new NamedShortcodeProvider
{
    ["bold"] = (args, content) => 
    {
        var text = args.NamedOrDefault("text");
        
        return new ValueTask<string>($"<b>{text}</b>");
    }
});

Console.WriteLine(await process.EvaluateAsync("[bold 'bold text']"));
```

```
<b>bold text</b>
```

Named and positional arguments can be mixed together. Each time an argument doesn't
have a name, the index is incremented.


```c#
var processor = new ShortcodesProcessor(new NamedShortcodeProvider
{
    ["bold"] = (args, content) => 
    {
        var text = args.At(0);
        
        return new ValueTask<string>($"<b>{text}</b>");
    }
});

Console.WriteLine(await process.EvaluateAsync("[bold id='a' 'some text']"));
```

```
<b>some text</b>
```

### Escaping tags

In case you want to render a shortcode instead of evaluating it, you can double the 
opening and closing braces.

```
[[bold] some text to show [/bold]]
```

Will then be rendered as 

```
[bold] some text to show [/bold]
```

And for single tags:

```
[[bold 'text']]
```

Will be rendered as 

```
[bold 'text']
```

In case several braces are used, and they are balanced, a single one will be escaped.

```
[[[bold 'text']]]
```

Will be rendered as:

```
[[bold 'text']]
```

Not that unbalanced braces won't be escaped.

```
[[[[bold 'text']]
```

Will be rendered as

```
[[[[bold 'text']]
```