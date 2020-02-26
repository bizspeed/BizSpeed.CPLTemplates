# BizSpeed.CPL Templates

## Document Schema
### doc
```xml
<doc [pagewidth="x"]>
...
</doc>
```
- width: page width in inches

This is the container for all other printable elements. In general, the top level elements can appear in any order. It sets up the page width, the default font, the form feed mechanism (JOURNAL), and begins the first "page"

### img
```xml
<img align source width />
```
- align: left|center
- source: Base-64 encoded representation of the image to be displayed
- width: percentage of the total page width

This tag uses the CPCL EG command

### section
```xml
<section>
...
</section>
```

This tag can be used to emit something like a `<div />` in HTML. It can contain any combination of the following tags:
- `<text />`
- `<b />`
- `<br />`
- `<h1 />`

### br
```xml
<br />
```

This tag simply emits a newline to show a blank line

### text
```xml
<text>ipsum lorem</text>
```

This tag just emits the text contained in the tag

### b
```xml
<b>ipsum lorem</b>
```

This tag will emit the text contained in a bold font

### h1
```xml
<h1>ipsum lorem</h1>
```

This tag will emit the text contained in a large font

### line
```xml
<line width="100" />
```
- width: the number of pixels wide to draw the line

This tag will draw a horizontal line across the page

### textline
```xml
<textline character="-" width="10" />
```
- character: the ASCII character to repeat. Defaults to "-".
- width: the number of times to repeat the character specified. Defaults to 65 (the number of characters in a 4" wide page)

This tag will emit a "line" of characters

### grid
```xml
<grid>
    <columns>
        <column width="30" />
        <column width="30" />
    </columns>
    <rows>
        <row>
            <cell [align="left|right"] [colspan="n"]>...</cell>
            ...
        </row>
    </rows>
</grid>
```

This tag will emit a simple tabular output to assist in performing some amount of controlled layout. The width attribute in the `<column />` element is expressed in characters. The content in the `<cell />` tag can be one of
* `<text />`
* `<b />`
* `<line />`
* `<textline />`

The colspan attribute of the `<cell />` element will stretch to the right as in HTML.
