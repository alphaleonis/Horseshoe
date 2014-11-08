# Horseshoe 

* [Introduction](#introduction)
* [Tag Types](#tag-types)
	* [Module](#Module)
	* [Template Declaration](#template-declaration)
	* [Substitution](#substitution)
	* [Conditional Block](#conditional-block)
	* [Iteration](#iteration)
* [Whitespace Control](#whitespace-control)

## Introduction

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

* [Module](#Module)
* [Template Declaration](#template-declaration)
* [Substitution](#substitution)
* [Conditional Block](#conditional-block)
* [Iteration](#iteration)

### Module

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

### Substitution

The most basic tag type is the property substitution tag. A `{{name}}` tag in a template will expand to the property named `name` in the data context.  If no such property is available, a compile time error will occur. 

These tags may only appear within a [template declaration](#template-declaration).

All variables are HTML escaped by default. If you want to return unescaped HTML, use triple curly braces: `{{{name}}}`.

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

### Conditional Block

A conditional block is a block whose contents may or may not be output depending on the value of a variable. It translates to an `if`-statement in TypeScript.

The format of a conditional block is:
```html
{{#if variable}}
...
{{/if}}
```

or with an else-clause:

```html
{{#if variable}}
...
{{#else}}
...
{{/if}}
```

where `variable` is the name of a property or variable that should be evaluated. This variable will be tested for truthiness, and the contents of the block is output if the value is truthy. If the name of the variable is prefixed with a bang (`!`) the condition is inverted, i.e. the contents of the block will be output if the value of the variable is falsey. 

Note that the expression after the `#if` in the tag can only be a single name of a variable or property of the data context, and is not an arbitrary TypeScript expression. Such logic should be placed in the view model or similar layer.  

An `{{#else}}` tag may be used inside the block, and the contents from the else clause to the end will be output if the first part is not.
 
For example:
```html
{{#if author}}
   <h1>{{firstName}} {{lastName}}</h1>
{{#else}}
    <h1>Unknown Author</h1>
{{/if}}
```

### Iteration

Iteration blocks outputs its contents multiple times, once for each item in a list provided. The format is:
```html
{{#foreach type variable in array}}
...
{{/foreach}}
```
Where `array` is the name of a variable or property in the data context of an array type, `type` is the element type of the array and `variable` is the name of the variable that will be assigned each item of the array in order.

For example, given the model:
```typescript
class AuthorListViewModel {
    authors: string[] = ['Stephen King', 'J.K. Rowling', 'Agatha Christie']    
}
```

and the template:

```html
{{#template myTemplate : AuthorListViewModel}}		
	<h1>Authors</h1>
    <ul>
		{{#foreach string name in authors}}
			<li>{{name}}</li>
		{{/foreach}}
	</ul>
{{/template}}
```
would result in:
```html
<h1>Authors</h1>
<ul>
	<li>Stephen King</li>
	<li>J.K. Rowling</li>
	<li>Agatha Christie</li>
</ul>
```
when rendered.

## Whitespace Control

Normally all whitespace within a template is preserved. However to enable nice formatting of templates, but to strip whitespace in the generated output each block tag may specify to trim all leading and/or trailing whitespace. This is done by writing a tilde (`~`) after the opening curly braces or before the closing curly braces.

For example the template fragment:
```html
{{#foreach item in items}}
   {{item.name}},
   {{item.value}}
{{/foreach}}
```
where items is the array `[ {name: 'A'; value: '1'},  {name: 'B'; value: '2'}, ]` would output 
```html
   A,
   1

   B,
   2
```

Specifying trimming in the template like this:
```html
{{#foreach item in items~}}
   {{item.name}},
   {{~item.value~}}
{{/foreach}}
```

Would instead produce:
`A,1B,2`

### Inverting the Whitespace Control Character Meaning
If the `{{#template}}` declaration tag is specifies to remove leading whitespace, this instead has the effect on inverting the meaning of the whitespace control character throughout the template. So in our previous example if we wanted all leading and trailing whitespace to be removed from all tags per default we could have written it as:
```html
{{~#template Mytemplate : MyViewModel}}
	{{#foreach item in items}}
	   {{item.name}},
	   {{item.value}}
	{{/foreach}}
{{/template}}
```
Which also would have produced the output:
`A,1B,2`