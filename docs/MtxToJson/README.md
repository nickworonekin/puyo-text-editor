# MtxToJson
MtxToJson converts MTX files to JSON and visa-versa. The resulting JSON files can be edited in your favorite text editor.

## Usage
```
MtxToJson [arguments] [options]
```

### Arguments

One or more files to convert.

### Options

**-f &lt;format&gt;** or **--format &lt;format&gt;**

The output format. Can be one of the following values:
* auto (default)
* mtx
* json

When this option is set to auto, the output format will be determined by the file extension of the input file.

**-v &lt;version&gt;** or **--version &lt;version&gt;**

The MTX version. Can be one of the following values:
* auto (default)
* 1
* 2

Use 1 for MTX files used in the following games:
* Puyo Puyo! 15th Anniversary
* Puyo Puyo 7
* Puyo Puyo!! 20th Anniversary

Use 2 for MTX files used in the following games:
* Puyo Puyo!! Quest
* Puyo Puyo Tetris
* Puyo Puyo Chronicle
* Puyo Puyo Champions/eSports

When this option is set to auto, the version will be 1 if the fnt or fpd options are set, otherwise it will be 2.

**--fnt &lt;fnt&gt;**

The FNT file to use. This is only required when version is 1 and fpd is not set.

**--fpd &lt;fpd&gt;**

The FPD file to use. This is only required when version is 1 and fnt is not set.

**--64bit**

When converting to MTX and version is 2, specifies offsets are 64-bit integers. Only set this when the MTX files are being used on the PC, PS4, Switch, or Xbox One version of the corresponding game. This may also be set in the JSON file, via the Has64BitOffsets property.

**-o &lt;output&gt;** or **--output &lt;output&gt;**

The output filename. Defaults to the input filename with an appropiate extension.

## Editing the JSON files
The MTX-converted JSON files are structured as follows:
```json
[
    [
        "Totally!{wait:30} That hairstyle\nis like so outdated.{arrow}{clear}You see anyone else\nsporting your goofy hair?{arrow}",
        "This {color:0}easygoing boy{/color}\nwith a faint smell\nof beetles!{arrow}"
    ],
    [
        "This clownfish ain't \"herring\" a word\nI'm sayin'.{arrow}",
        "That'd mean he's sleeping too... And\nhe and I are definitely not sleeping\ntogether.{arrow}"
    ]
]
```

### Escape Sequences

The following escape sequences are supported in each string in the JSON file.

| Sequence      | Description |
|---------------|-------------|
| **\n**        | New line
| **{arrow}**   | Shows the arrow and waits for user input before proceeding.
| **{clear}**   | Erases all currently displayed characters.
| **{color:n}** | Changes the color of the following characters to the color specified by n. n can be any number 0-65535.
| **{/color}**  | Changes the color of the following characters to the default color.
| **{speed:n}** | Sets the text speed to the value specified by n. Similar to wait but applies to all the following characters. n can be any number 0-65535.
| **{wait:n}**  | Waits an amount of time specified by n before showing the following characters. n can be any number 0-65535.