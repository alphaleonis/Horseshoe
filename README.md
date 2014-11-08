# Horseshoe 

## Description

Horseshoe is a Visual Studio extension used to compile templates into [TypeScript](http://www.typescriptlang.org/) files that can be used for generating HTML or similar files.  It works by expanding tags in a template using values provided as an input object, called the *data context*, to the generated function. 

The benefit of having the template compiled into TypeScript at design time, as opposed to having the template parsed and transformed at runtime is strong type-checking as well as some performance benefit.


### Horseshoe Files

Horeshoe template definitions files have the `.hrs` extension.They should have the custom tool `HorseshoeGenerator` specified in Visual Studio to enable generation of a TypeScript file. For convenience, an item template is created in Visual Studio when the extension is installed, so you can add a new Horseshoe template file to your project by right-clicking your project and selecting "Add->New item..." and searching for "Horseshoe". 

Whenever the `.hrs` file is saved, the corresponding `.ts` file is generated as a sub item in the project. 

### File Structure

A Horseshoe file consists of one or more template declarations, optionally enclosed in a module declaration. Each template will generate a TypeScript class with a static method named `render` accepting a single input object, the type of which is defined in the template declaration.  
 
## Tag Types

Tags are indicated by double curly-braces, much like [Mustache](http://mustache.github.io/) or [Handlebars.js](http://handlebarsjs.com/). For example `{{name}}` is a tag, which will be expanded at runtime to the property `` name`` of the data context. 

There are also block tags with special functions, for example `{{#template}}`. Each such tag starts with a double curly brace followed by the hash sign (`#`) and the keyword denoting the tag. The block is then terminated by a closing tag, much like HTML tags, appearing for example as `{{/template}}`.  

### The Module Tag

Templates can be grouped in modules, which will translate directly into a TypeScript module. Module tags can occur only at the outermost level of a Horseshoe document.

For example the block:
```html
{{#module MyModuleName}}
...
{{/module}}
```

Will result in the following TypeScript:
```TypeScript
module MyModuleName {
	...
}
```

### Template Declaration

Every template in a Horseshoe document must be enclosed in a `{{#template}}` block. The format of this tag is as follows:
```html
{{#template TemplateName : DataContextType}}
...
{{/template}}
```

Where `TemplateName` is the name of the generated class, and `DataContextType` is the type of the input parameter to the generated `render` function.

For example the template above would be translated into something like the following:
```TypeScript
class TemplateName {
	static render(dataContext: DataContextType) : string {
		...
	}
} 
```

Template blocks must be declared at the top-level or within a `module` block.

### Property Substitution
The most basic tag type is the property substitution tag. A `{{name}}` tag in a template will expand to the property named `name` in the data context.  If no such property is available, a compile time error will occur. These tags may only appear within a [template declaration](#template-declaration).

All variables are HTML escaped by default. If you want to return unescaped HTML, use triple curly braces: `{{{name}}}`.

#### Example

Consider the following (partial) template:
```html
<ul>
   <li>{{name}}</li>
   <li>{{age}}</li>
   <li>{{description}}</li>
   <li>{{{description}}}</li>
</ul>
```

Given the following input:
```typescript
class MyViewModel {
    name: string = "Joe Smith";
	age: number = 24;
	description: string = "<b>Awesome!</b>";
}
```

When rendered at runtime would produce:
```html
<ul>
   <li>Joe Smith</li>
   <li>24</li>
   <li>&lt;b&gt;Awesome!&lt;/b&gt;</li>
   <li><b>Awesome!</b></li>
</ul>
```
