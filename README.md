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

Arguments need to be quoted either with `'` or `"`.

```c#
var processor = new ShortcodesProcessor(new NamedShortcodeProvider
{
    ["bold"] = (args, content) => 
    {
        var text = arts.Named("text");
        
        return new ValueTask<string>($"<b>{text}</b>");
    }
});

Console.WriteLine(await process.EvaluateAsync("[bold text='bold text']"));
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

### Positional arguments

If an argument doesn't have a name, an default index can be used.

```c#
var processor = new ShortcodesProcessor(new NamedShortcodeProvider
{
    ["bold"] = (args, content) => 
    {
        var text = arts.NamedOrDefault("text");
        
        return new ValueTask<string>($"<b>{text}</b>");
    }
});

Console.WriteLine(await process.EvaluateAsync("[bold 'bold text']"));
```